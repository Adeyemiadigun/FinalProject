using Application.Dtos;
using Domain.Enum;
using FluentValidation;

namespace Application.Validation
{
    public class UpdateQuestionDtoValidator : AbstractValidator<UpdateQuestionDto>
    {
        public UpdateQuestionDtoValidator()
        {
            RuleFor(x => x.QuestionText).NotEmpty();
            RuleFor(x => x.QuestionType).IsInEnum();
            RuleFor(x => x.Marks).GreaterThan(0);
            RuleForEach(x => x.Options).SetValidator(new OptionDtoValidator()).When(x => x.QuestionType == QuestionType.MCQ);
            RuleForEach(x => x.TestCases).SetValidator(new CreateTestCaseDtoValidator()).When(x => x.QuestionType == QuestionType.Coding);
            RuleFor(x => x.Answer).SetValidator(new CreateAnswerDtoValidator());
        }
    }

}
