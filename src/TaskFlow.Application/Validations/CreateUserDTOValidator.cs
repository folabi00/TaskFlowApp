using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Application.Validations
{
    public class CreateUserDTOValidator : AbstractValidator<CreateUserDTO>
    {
        private readonly IUserRepository _repository;
        public CreateUserDTOValidator(IUserRepository repository)
        {
            _repository = repository;

            RuleFor(u => u.FirstName).NotEmpty().WithMessage("First Name cannot be empty")
                .Length(3, 50).WithMessage("First Name must be between 3 to 50 characters")
                .Must(name => name.All(char.IsLetter))
                .WithMessage("First Name must contain only letters.");

            RuleFor(u => u.LastName).NotEmpty().WithMessage("Last Name cannot be empty")
                .Length(3, 50).WithMessage("Last Name must be between 3 to 50 characters")
                .Must(name => name.All(char.IsLetter))
                .WithMessage("Last Name must contain only letters.");

            RuleFor(u => u.Email).NotEmpty().WithMessage("Email cannot be empty")
                .EmailAddress().WithMessage("Must be a valid Email Address")
                .MustAsync(async (email, ct) => await _repository.IsEmailUnique(email, ct)).WithMessage("Email must be unique");

            RuleFor(u => u.Password).NotEmpty().WithMessage("Password cannot be empty")
                .MinimumLength(8).WithMessage("Password must be greater than 8 characters");      
        }

        
        
    }
}
