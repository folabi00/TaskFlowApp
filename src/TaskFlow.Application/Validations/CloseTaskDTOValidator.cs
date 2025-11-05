using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Validations
{
    public class CloseTaskDTOValidator : AbstractValidator<CloseTaskRequestDto>
    {
        public CloseTaskDTOValidator()
        {
            RuleFor(t => t.TaskStatus).NotEmpty().WithMessage("Task Status cannot be Empty")
               .NotNull().WithMessage("Task Status cannot be Null")
               .IsInEnum().WithMessage("Status must be a valid enum value.");

            RuleFor(t => t.TaskCompletionStatus).NotEmpty().WithMessage("Task Status cannot be Empty")
                .NotNull().WithMessage("Task Status cannot be Null")
                .IsInEnum().WithMessage("Status must be a valid enum value.");
        }
    }
}
