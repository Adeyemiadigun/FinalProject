using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using FluentValidation;

namespace Application.Validation
{
    public class AiQuestionGenerationRequestDtoValidator : AbstractValidator<AiQuestionGenerationRequestDto>
    {
        public AiQuestionGenerationRequestDtoValidator()
        {
            RuleFor(x => x.QuestionType).IsInEnum();
            RuleFor(x => x.TechnologyStack).NotEmpty();
            RuleFor(x => x.Difficulty).NotEmpty();
            RuleFor(x => x.Topic).NotEmpty();
        }
    }
}
