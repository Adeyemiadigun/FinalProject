
using Application.Dtos;
using FluentValidation;

namespace Application.Validation
{
    public class UpdateStudentStatusDtoValidator : AbstractValidator<UpdateStudentStatusDto>
    {
        public UpdateStudentStatusDtoValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(s => s == "Active".ToLower() || s == "Inactive".ToLower())
                .WithMessage("Status must be either 'Active' or 'Inactive'.");
        }
    }
}
