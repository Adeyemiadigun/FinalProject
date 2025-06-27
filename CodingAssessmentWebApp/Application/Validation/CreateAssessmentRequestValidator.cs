using Application.Dtos;
using FluentValidation;

namespace Application.Validation
{
    public class CreateAssessmentRequestModelValidator : AbstractValidator<CreateAssessmentRequestModel>
    {
        public CreateAssessmentRequestModelValidator()
        {
            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.TechnologyStack).NotEmpty();
            RuleFor(x => x.DurationInMinutes).GreaterThan(0);
            RuleFor(x => x.StartDate).LessThan(x => x.EndDate);
            RuleFor(x => x.PassingScore).InclusiveBetween(0, 100);
            RuleForEach(x => x.AssignedStudentIds)
                .NotEqual(Guid.Empty).WithMessage("Invalid student ID found.");
        }
    }
}
