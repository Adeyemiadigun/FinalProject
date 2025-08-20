using Application.Dtos;
using FluentValidation;

namespace Application.Validation
{

    public class UpdateUserRequestModelValidator : AbstractValidator<UpdateUserRequestModel>
    {
        public UpdateUserRequestModelValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            When(x => !string.IsNullOrWhiteSpace(x.CurrentPassword) || !string.IsNullOrWhiteSpace(x.NewPassword), () =>
            {
                RuleFor(x => x.CurrentPassword)
                    .NotEmpty().WithMessage("Current password is required when changing password.")
                    .MinimumLength(6).WithMessage("Current password must be at least 6 characters.");

                RuleFor(x => x.NewPassword)
                    .NotEmpty().WithMessage("New password is required when changing password.")
                    .MinimumLength(6).WithMessage("New password must be at least 6 characters.")
                    .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from the current password.");
            });
        }
    }

}
