using Application.Dtos;
using FluentValidation;

namespace Application.Validation
{
    public class CreateTestCaseDtoValidator : AbstractValidator<CreateTestCaseDto>
    {
        public CreateTestCaseDtoValidator()
        {
            RuleFor(x => x.Input).NotEmpty();
            RuleFor(x => x.ExpectedOutput).NotEmpty();
            RuleFor(x => x.Weight).InclusiveBetween(0, 1);
        }
    }

}
