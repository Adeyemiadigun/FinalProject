using Application.Dtos;
using FluentValidation;

namespace Application.Validation
{
    public class RegisterUserRequestModelValidator : AbstractValidator<RegisterUserRequestModel>
    {
        public RegisterUserRequestModelValidator()
        {
            RuleFor(x => x.FullName).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Passwords do not match");
        }
    }

}
