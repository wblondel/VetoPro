using FluentValidation;
using VetoPro.Contracts.DTOs.Auth; // Correct DTO namespace

namespace VetoPro.Contracts.Validators.Auth; // Correct Validator namespace

public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Le Refresh Token est obligatoire.");
    }
}