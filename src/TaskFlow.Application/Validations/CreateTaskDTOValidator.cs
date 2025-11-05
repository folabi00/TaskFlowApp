using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Validations
{
    public class CreateTaskDTOValidator : AbstractValidator<CreateTaskRequestDto>
    {
        public CreateTaskDTOValidator()
        {
            RuleFor(t => t.TaskName).NotEmpty().WithMessage("Task Name cannot be Empty")
                .NotNull().WithMessage("Task Name cannot be Null")
                .Length(3, 50).WithMessage("Task Name must be between 3 and 50 Characters");

            RuleFor(t => t.TaskDescription).NotEmpty().WithMessage("Task Description cannot be Empty")
                .NotNull().WithMessage("Task Description cannot be Null")
                .MaximumLength(150).WithMessage("Maximum character exceeded for Description")
                .Matches("^[a-zA-Z0-9-]+$\r\n").WithMessage("Invalid Character found in Description");

            RuleFor(t => t.UserId).NotEmpty().WithMessage("Task Name cannot be Empty")
                .NotNull().WithMessage("Task Name cannot be Null")
                .Must(id => id != Guid.Empty).WithMessage("UserId must be a valid Guid");

        }
    }
}
