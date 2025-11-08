namespace VetoPro.Contracts.DTOs.Auth;

/// <summary>
/// DTO pour la demande d'un nouvel Access Token en utilisant un Refresh Token.
/// </summary>
public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; }
}