using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Validations
{
    public class LoginDTOValidator : AbstractValidator<LoginDTO>
    {
        public LoginDTOValidator()
        {
            RuleFor(u => u.Email).NotEmpty().WithMessage("Email cannot be empty")
                .EmailAddress().WithMessage("Must be a valid Email Address")
                .Must(email => email.Contains("@")).WithMessage("Email domain identifier is invalid");

            RuleFor(u => u.Password).NotEmpty().WithMessage("Password cannot be empty")
                .NotNull().WithMessage("Password cannot be Null")
                .MinimumLength(8).WithMessage("Password must be greater than 8 characters");
        }
    }
}
