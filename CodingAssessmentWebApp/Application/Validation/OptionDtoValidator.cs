using Application.Dtos;
using FluentValidation;

namespace Application.Validation
{
    public class OptionDtoValidator : AbstractValidator<OptionDto>
    {
        public OptionDtoValidator()
        {
            RuleFor(x => x.OptionText).NotEmpty();
        }
    }

}
