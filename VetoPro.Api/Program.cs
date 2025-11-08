using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Data;
using VetoPro.Api.Entities;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VetoPro.Api.Middleware;
using VetoPro.Contracts.DTOs.Catalogs;
using VetoPro.Contracts.Validators.Clinical;
using Microsoft.OpenApi.Models;
using Serilog;

namespace VetoPro.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Serilog configuration (must be done first)
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                optional: true)
            .Build();
        
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        try
        {
            Log.Information("Starting VetoPro API...");
            
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();

            var webAppCORS = "WebAppCORS";
            
            // --- Adding Services ---
            Log.Information("Configuring services...");
            
            builder.Services.AddExceptionHandler<ErrorHandlingMiddleware>(); // Register Exception Handler Service
            builder.Services.AddProblemDetails(); 
            builder.Services.AddControllers();
            builder.Services.AddValidatorsFromAssemblyContaining<AppointmentCreateDtoValidator>(); // Fluent validation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                var apiAssembly = Assembly.GetExecutingAssembly();
                var apiXmlFilename = $"{apiAssembly.GetName().Name}.xml";
                var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFilename);
                options.IncludeXmlComments(apiXmlPath);

                var contractsAssembly = typeof(SpeciesDto).Assembly;
                var contractsXmlFilename = $"{contractsAssembly.GetName().Name}.xml";
                var contractsXmlPath = Path.Combine(AppContext.BaseDirectory, contractsXmlFilename);

                if (File.Exists(contractsXmlPath))
                {
                    options.IncludeXmlComments(contractsXmlPath);
                }

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Bearer scheme.",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer" // doit correspondre au nom de la définition ci-dessus
                            }
                        },
                        new string[] { } // la liste des scopes (vide pour JWT)
                    }
                });
            });
            
            // Configuration Entity Framework
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<VetoProDbContext>(options => options.UseSqlite(connectionString));
            
            // Registering ASP.NET Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 6;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                })
                .AddEntityFrameworkStores<VetoProDbContext>()
                .AddDefaultTokenProviders();
            
            // Configuration JWT
            var jwtKey = builder.Configuration["Jwt:Key"];
            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Secret key is missing.");
            }

            // Ajout des services d'authentification et autorisation
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true; // sauvegarde le token dans le contexte
                    options.RequireHttpsMetadata = false; // mettre à true en production
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true, // vérifier l'expiration
                        ValidateIssuerSigningKey = true, // vérifier la signature

                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

                        // Gestion du clock skew
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddAuthorization();
            
            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: webAppCORS,
                    policy =>
                    {
                        policy.WithOrigins(
                                "https://www.vetopro.com",
                                "https://app.vetopro.com")
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .WithExposedHeaders("X-Pagination");

                        if (builder.Environment.IsDevelopment())
                        {
                            policy.WithOrigins("http://localhost:5256", "https://localhost:7008")
                                .AllowAnyMethod()
                                .AllowAnyHeader()
                                .WithExposedHeaders("X-Pagination");
                        }
                    });
            });

            // AutoMapper
            //builder.Services.AddAutoMapper(typeof(Program));
            
            builder.Services.AddRouting(options => options.LowercaseUrls = true); // Make URLs lowercase
            
            Log.Information("Services configuration finished.");
            
            // --- Building the application ---
            Log.Information("Building the application (builder.Build())...");
            var app = builder.Build();
            
            // --- Configuring the Middleware Pipeline ---
            Log.Information("Configuring the middleware pipeline...");
            
            app.UseExceptionHandler(); // This delegates unhandled exceptions to ErrorHandlingMiddleware
            
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                Log.Information("Swagger UI and documentation enabled (Development mode).");
            }
            
            app.UseHttpsRedirection();
            app.UseAuthentication(); // authentification MUST BE BEFORE authorization
            app.UseAuthorization();
            app.UseCors(webAppCORS);
            app.MapControllers(); // map routes to controllers
            
            Log.Information("Middleware pipeline configured.");
            
            // --- Seeding the database ---
            Log.Information("Starting database seeding...");
            await app.SeedDatabaseAsync();
            Log.Information("Database seeding finished.");
            
            // --- Starting the application ---
            Log.Information("Starting application...");
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "L'application n'a pas pu démarrer.");
        }
        finally
        {
            // assure que tous les logs sont écrits avant que l'app ne se ferme'
            await Log.CloseAndFlushAsync();
        }

    }
}