using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using FluentValidation;

namespace Application.Validation
{
    public class UpdateAssessmentRequestModelValidator : AbstractValidator<UpdateAssessmentRequestModel>
    {
        public UpdateAssessmentRequestModelValidator()
        {
            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.TechnologyStack).NotEmpty();
            RuleFor(x => x.DurationInMinutes).GreaterThan(0);
            RuleFor(x => x.StartDate).LessThan(x => x.EndDate);
            RuleFor(x => x.PassingScore).InclusiveBetween(0, 100);
        }
    }

}
