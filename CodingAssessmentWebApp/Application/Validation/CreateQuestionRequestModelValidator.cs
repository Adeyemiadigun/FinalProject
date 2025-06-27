using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using Domain.Enum;
using FluentValidation;

namespace Application.Validation
{
    public class CreateQuestionRequestModelValidator : AbstractValidator<CreateQuestionRequestModel>
    {
        public CreateQuestionRequestModelValidator()
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
