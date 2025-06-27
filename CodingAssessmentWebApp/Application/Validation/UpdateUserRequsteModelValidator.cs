using Application.Dtos;
using FluentValidation;

namespace Application.Validation
{
    public class UpdateUserRequsteModelValidator : AbstractValidator<UpdateUserRequsteModel>
    {
        public UpdateUserRequsteModelValidator()
        {
            RuleFor(x => x.Id).NotEqual(Guid.Empty);
            RuleFor(x => x.FullName).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }

}
