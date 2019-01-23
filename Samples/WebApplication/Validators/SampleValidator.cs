using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using WebApplication.Domain;

namespace WebApplication.Validators
{
    public class SampleValidator : AbstractValidator<Sample>
    {
        public SampleValidator(IService service)
        {
            RuleFor(x => x.Id).NotNull();
            RuleFor(x => x.Name).Length(1, 10);
        }
    }
}
