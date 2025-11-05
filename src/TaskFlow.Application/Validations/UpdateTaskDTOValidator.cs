using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Validations
{
    public class UpdateTaskDTOValidator : AbstractValidator<UpdateTaskRequestDto>
    {
        public UpdateTaskDTOValidator()
        {
            RuleFor(t => t.TaskName).NotEmpty().WithMessage("Task Name cannot be Empty")
                .NotNull().WithMessage("Task Name cannot be Null")
                .Length(3, 50).WithMessage("Task Name must be between 3 and 50 Characters")
                .Matches("^[a-zA-Z0-9-]+$").WithMessage("Invalid Character found in Name");


            RuleFor(t => t.TaskDescription).NotEmpty().WithMessage("Task Description cannot be Empty")
                .NotNull().WithMessage("Task Description cannot be Null")
                .MaximumLength(150).WithMessage("Maximum character exceeded for Description")
                .Matches("^[a-zA-Z0-9-]+$").WithMessage("Invalid Character found in Description");

            RuleFor(t => t.TaskStatus).NotEmpty().WithMessage("Task Status cannot be Empty")
                .NotNull().WithMessage("Task Status cannot be Null")
                .IsInEnum().WithMessage("Status must be a valid enum value.");

            RuleFor(t => t.TaskCompletionStatus).NotEmpty().WithMessage("Task Status cannot be Empty")
                .NotNull().WithMessage("Task Status cannot be Null")
                .IsInEnum().WithMessage("Status must be a valid enum value.");

        }
    }
}
