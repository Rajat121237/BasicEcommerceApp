using eCommerce.Core.DTO;
using FluentValidation;

namespace eCommerce.Core.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address format");

        RuleFor(request => request.Password).NotEmpty().WithMessage("Password is required");

        RuleFor(request => request.PersonName).NotEmpty().WithMessage("Person name is required")
            .Length(1, 50).WithMessage("Person Name should be 1 to 50 characters long");

        RuleFor(request => request.Gender).IsInEnum().WithMessage("Invalid gender option");
    }
}
