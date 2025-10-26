using System.ComponentModel.DataAnnotations;

namespace VetoPro.Api.DTOs;

/// <summary>
/// DTO pour la création d'un compte de connexion (login/mot de passe).
/// Sera imbriqué optionnellement dans ContactCreateDto.
/// </summary>
public class AccountCreateDto
{
    [Required(ErrorMessage = "L'e-mail de connexion est obligatoire.")]
    [EmailAddress]
    public string LoginEmail { get; set; }

    [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
    [MinLength(6, ErrorMessage = "Le mot de passe doit faire au moins 6 caractères.")]
    public string Password { get; set; }
}