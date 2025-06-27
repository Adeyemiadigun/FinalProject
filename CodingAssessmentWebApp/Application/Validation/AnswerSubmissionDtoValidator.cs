using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using FluentValidation;

namespace Application.Validation
{
    public class AnswerSubmissionDtoValidator : AbstractValidator<AnswerSubmissionDto>
    {
        public AnswerSubmissionDtoValidator()
        {
            RuleFor(x => x.Answers).NotEmpty();
            RuleForEach(x => x.Answers).SetValidator(new QuestionAnswersValidator());
        }
    }

    public class QuestionAnswersValidator : AbstractValidator<QuestionAnswers>
    {
        public QuestionAnswersValidator()
        {
            RuleFor(x => x.QuestionId).NotEqual(Guid.Empty);
            RuleFor(x => x.SubmittedAnswer).NotEmpty().When(x => x.SelectedOptionIds == null || !x.SelectedOptionIds.Any());
        }
    }

}
