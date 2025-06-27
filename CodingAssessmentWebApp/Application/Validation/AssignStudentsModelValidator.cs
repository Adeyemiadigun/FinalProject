using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using FluentValidation;

namespace Application.Validation
{
    public class AssignStudentsModelValidator : AbstractValidator<AssignStudentsModel>
    {
        public AssignStudentsModelValidator()
        {
            RuleFor(x => x.StudentIds).NotEmpty();
            RuleForEach(x => x.StudentIds).NotEqual(Guid.Empty).WithMessage("Invalid student ID found.");
        }
    }
}
