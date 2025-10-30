using System.ComponentModel.DataAnnotations;

namespace VetoPro.Contracts.DTOs.Auth;

/// <summary>
/// DTO pour la demande de connexion (login).
/// </summary>
public class LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}