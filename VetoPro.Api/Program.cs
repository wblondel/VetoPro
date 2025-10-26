using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetoPro.Api.Data;
using VetoPro.Api.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Get the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configuration Entity Framework
builder.Services.AddDbContext<VetoProDbContext>(options =>
    options.UseSqlite(connectionString));

// Registering ASP.NET Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        // Configurez vos options de mot de passe, etc.
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<VetoProDbContext>();

// Ajout des services d'authentification et autorisation
builder.Services.AddAuthentication();
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

// Seed just after the app is built and before it starts
await DataSeeder.SeedDatabaseAsync(app);

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

// Lance l'application
app.Run();
