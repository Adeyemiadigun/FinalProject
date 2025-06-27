using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos;
using FluentValidation;

namespace Application.Validation
{
    public class BulkRegisterUserRequestModelValidator : AbstractValidator<BulkRegisterUserRequestModel>
    {
        public BulkRegisterUserRequestModelValidator()
        {
            RuleFor(x => x.Users).NotEmpty();
            RuleForEach(x => x.Users).SetValidator(new RegisterUserRequestModelValidator());
        }
    }

}
