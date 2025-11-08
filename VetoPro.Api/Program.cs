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
            Log.Information("Démarrage de l'API VetoPro...");
            
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();
            
            // Register Exception Handler Service
            builder.Services.AddExceptionHandler<ErrorHandlingMiddleware>();
            builder.Services.AddProblemDetails();

            // Make URLs lowercase
            builder.Services.AddRouting(options => options.LowercaseUrls = true);

            // Add services to the container.
            builder.Services.AddControllers();

            // Swagger/OpenAPI
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

            // Fluent validation
            builder.Services.AddValidatorsFromAssemblyContaining<AppointmentCreateDtoValidator>();

            // Get the connection string from appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Configuration Entity Framework
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

            // AutoMapper
            //builder.Services.AddAutoMapper(typeof(Program));

            // CORS pour les applications clientes
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(corsBuilder =>
                {
                    corsBuilder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            // Construction de l'application
            var app = builder.Build();

            app.UseExceptionHandler(); // This delegates unhandled exceptions to ErrorHandlingMiddleware

            // Utiliser Swagger uniquement en environnement de développement
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // IMPORTANT : L'authentification DOIT être avant l'autorisation
            app.UseAuthentication();
            app.UseAuthorization();

            // Map les routes vers les contrôleurs
            app.MapControllers();

            // CORS pour les applications clientes
            app.UseCors();

            // Seed just after the app is built and before it starts
            await app.SeedDatabaseAsync();

            // Lance l'application
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