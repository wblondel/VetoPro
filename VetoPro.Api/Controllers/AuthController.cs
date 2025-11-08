using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VetoPro.Api.Data;
using VetoPro.Contracts.DTOs.Auth;
using VetoPro.Api.Entities;
using FluentValidation;
using VetoPro.Contracts.Validators.Auth;

namespace VetoPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    VetoProDbContext context,
    IConfiguration config,
    IValidator<RegisterDto> registerValidator,
    IValidator<LoginDto> loginValidator,
    IValidator<RefreshTokenRequestDto> refreshTokenValidator)
    : ControllerBase
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager = roleManager;

    /// <summary>
    /// POST: api/auth/register
    /// Inscrit un nouvel utilisateur (Client/Propriétaire).
    /// </summary>
    [AllowAnonymous] // Tout le monde peut s'inscrire
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        var validationResult = await registerValidator.ValidateAsync(registerDto);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        
        // Vérifier si l'utilisateur existe déjà
        if (await userManager.FindByEmailAsync(registerDto.Email) != null)
        {
            return Conflict("Un compte avec cet e-mail existe déjà.");
        }

        // Commencer une transaction : L'utilisateur ET le contact doivent être créés
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // 1. Créer le ApplicationUser (Identity)
            var newUser = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                EmailConfirmed = true // Confirmer auto pour le dev
            };

            var identityResult = await userManager.CreateAsync(newUser, registerDto.Password);

            if (!identityResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { errors = identityResult.Errors.Select(e => e.Description) });
            }

            // 2. Assigner le rôle "Client"
            await userManager.AddToRoleAsync(newUser, "Client");

            // 3. Créer le Contact (Profil)
            var newContact = new Contact
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                IsOwner = true, // Par défaut, un nouvel inscrit est propriétaire
                IsClient = true, // Et client
                IsStaff = false,
                UserId = newUser.Id // Lier le compte de connexion
            };

            context.Contacts.Add(newContact);
            await context.SaveChangesAsync(); // Sauvegarder le contact

            // 4. Valider la transaction
            await transaction.CommitAsync();

            // 5. Générer les tokens et la réponse
            var authResponse = await GenerateTokensAndResponse(newUser, newContact);
            
            return Ok(authResponse);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Erreur interne lors de l'inscription: {ex.Message}");
        }
    }

    /// <summary>
    /// POST: api/auth/login
    /// Connecte un utilisateur existant.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        var validationResult = await loginValidator.ValidateAsync(loginDto);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        
        // Trouver l'utilisateur
        var user = await userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            return Unauthorized("Email ou mot de passe incorrect.");
        }
        
        // Trouver le profil Contact associé
        var contact = await context.Contacts.FirstOrDefaultAsync(c => c.UserId == user.Id);
        if (contact == null)
        {
            // Cas rare où un utilisateur existe sans profil contact
            return StatusCode(500, "Compte utilisateur trouvé mais profil de contact manquant.");
        }

        // Générer les tokens et la réponse
        var authResponse = await GenerateTokensAndResponse(user, contact);
        
        return Ok(authResponse);
    }

    /// <summary>
    /// POST: api/auth/refresh-token
    /// Demande un nouvel Access Token en utilisant un Refresh Token (Rotation).
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto requestDto)
    {
        var validationResult = await refreshTokenValidator.ValidateAsync(requestDto);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        
        // Trouver l'ancien refresh token dans la BDD
        var oldRefreshToken = await context.RefreshTokens
            .Include(rt => rt.User) // Charger l'utilisateur associé
            .FirstOrDefaultAsync(rt => rt.Token == requestDto.RefreshToken);

        if (oldRefreshToken == null)
        {
            return Unauthorized("Token d'actualisation non valide.");
        }

        // 2. Vérifier s'il est révoqué ou expiré
        if (oldRefreshToken.IsRevoked)
        {
            return Unauthorized("Token d'actualisation révoqué.");
        }
        if (oldRefreshToken.ExpiresUtc < DateTime.UtcNow)
        {
            return Unauthorized("Token d'actualisation expiré.");
        }

        // 3. L'ancien token est valide. Récupérer l'utilisateur.
        var user = oldRefreshToken.User;
        var contact = await context.Contacts.FirstOrDefaultAsync(c => c.UserId == user.Id);
        
        if (contact == null)
        {
            return StatusCode(500, "Utilisateur valide mais profil de contact manquant.");
        }

        // 4. Générer de nouveaux tokens ET révoquer l'ancien (Token Rotation)
        var authResponse = await GenerateTokensAndResponse(user, contact, oldRefreshToken);
        
        return Ok(authResponse);
    }
    
    /// <summary>
    /// POST: api/auth/revoke-token
    /// Révoque un Refresh Token (Déconnexion).
    /// </summary>
    [AllowAnonymous] // Doit être anonyme car l'Access Token peut être expiré
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequestDto requestDto)
    {
        var validationResult = await refreshTokenValidator.ValidateAsync(requestDto);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToDictionary()));
        }
        
        var refreshToken = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == requestDto.RefreshToken);

        if (refreshToken == null)
        {
            return NotFound("Token d'actualisation non trouvé.");
        }

        refreshToken.IsRevoked = true;
        await context.SaveChangesAsync();

        return NoContent(); // 204 No Content (Déconnexion réussie)
    }


    // --- MÉTHODES PRIVÉES (HELPERS) ---

    /// <summary>
    /// Génère un Access Token (JWT) et un Refresh Token (BDD).
    /// Gère la rotation (révocation de l'ancien token) si fourni.
    /// </summary>
    private async Task<AuthResponseDto> GenerateTokensAndResponse(ApplicationUser user, Contact contact, RefreshToken? oldRefreshToken = null)
    {
        // 1. Générer l'Access Token (JWT)
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = GenerateAccessToken(user, roles);

        // 2. Générer le Refresh Token
        var newRefreshToken = GenerateRefreshToken(user.Id);
        await context.RefreshTokens.AddAsync(newRefreshToken);

        // 3. Révoquer l'ancien token (Rotation)
        if (oldRefreshToken != null)
        {
            oldRefreshToken.IsRevoked = true;
        }

        // 4. Sauvegarder les changements (nouveau token + révocation de l'ancien)
        await context.SaveChangesAsync();

        // 5. Créer la réponse
        return new AuthResponseDto
        {
            UserId = user.Id.ToString(),
            Email = user.Email!,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            Roles = roles,
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token
        };
    }

    /// <summary>
    /// Génère un Access Token (JWT) pour un utilisateur.
    /// </summary>
    private string GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // ID unique du token
        };

        // Ajouter les rôles aux claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = config["Jwt:Issuer"],
            Audience = config["Jwt:Audience"],
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(config["Jwt:AccessTokenDurationInMinutes"])),
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Génère un Refresh Token (objet entité).
    /// </summary>
    private RefreshToken GenerateRefreshToken(Guid userId)
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var tokenString = Convert.ToBase64String(randomNumber);

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = tokenString,
            UserId = userId,
            ExpiresUtc = DateTime.UtcNow.AddDays(Convert.ToDouble(config["Jwt:RefreshTokenDurationInDays"])),
        };
    }
}