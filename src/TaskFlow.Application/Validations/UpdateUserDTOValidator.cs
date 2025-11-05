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
    public class UpdateUserDTOValidator : AbstractValidator<UpdateUserDTO>
    {
        private readonly IUserRepository _repository;
        public UpdateUserDTOValidator(IUserRepository repository)
        {
            _repository = repository;

            RuleFor(u => u.Id).NotEmpty().WithMessage("Input valid Id")
                .Must(IsValidUserId).WithMessage("User Id must be a valid Unique Identifier");

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

        }

        private bool IsValidUserId(Guid id)
        {           
            return id != Guid.Empty;
        }

        private bool IsValidUserId( Guid id, CancellationToken cancellationToken)
        {
            return true;
        }
    }
}
