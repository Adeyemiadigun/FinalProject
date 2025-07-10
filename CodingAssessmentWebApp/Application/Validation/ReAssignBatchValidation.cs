using Application.Dtos;
using FluentValidation;

namespace Application.Validation
{
    public class ReAssignBatchValidator : AbstractValidator<ReAssignBatch>
    {
        public ReAssignBatchValidator()
        {
            RuleFor(x => x.BatchId)
                .NotEmpty().WithMessage("Batch ID is required.")
                .Must(id => id != Guid.Empty).WithMessage("Batch ID must be a valid GUID.");
        }
    }
}
