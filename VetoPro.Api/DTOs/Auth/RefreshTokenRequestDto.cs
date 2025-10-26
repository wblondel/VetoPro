using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs.Auth;

/// <summary>
/// DTO pour la demande d'un nouvel Access Token en utilisant un Refresh Token.
/// </summary>
public class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "Le Refresh Token est obligatoire.")]
    public string RefreshToken { get; set; }
}