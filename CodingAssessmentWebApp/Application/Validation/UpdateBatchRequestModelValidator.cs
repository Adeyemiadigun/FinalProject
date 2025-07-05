using Application.Dtos;
using FluentValidation;


namespace Application.Validation
{
  
    public class UpdateBatchRequestModelValidator : AbstractValidator<UpdateBatchRequestModel>
    {
        public UpdateBatchRequestModelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Batch name is required.")
                .MaximumLength(100).WithMessage("Batch name must be less than 100 characters.");

            RuleFor(x => x.BatchNumber)
                .NotEmpty().WithMessage("Batch number is required.");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required.")
                .LessThan(x => x.EndDate.Value).When(x => x.EndDate.HasValue)
                .WithMessage("Start date must be before end date.");
        }
    }

}
