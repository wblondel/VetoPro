namespace VetoPro.Contracts.DTOs.Auth;

/// <summary>
/// DTO pour la création d'un compte de connexion (login/mot de passe).
/// Sera imbriqué optionnellement dans ContactCreateDto.
/// </summary>
public class AccountCreateDto
{
    public string LoginEmail { get; set; }
    public string Password { get; set; }
}