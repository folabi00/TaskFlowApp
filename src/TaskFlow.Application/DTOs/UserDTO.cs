using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Core.Models;

namespace TaskFlow.Application.DTOs
{
    public class UserDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        //public string? Password { get; set; }
        public bool EmailConfirmed { get; set; } = false;
        public string? RegistrationNumber { get; set; }
        public bool HasCurrentTask { get; set; }
        public string? CurrentTask { get; set; }    
        public int TotalTasksAssigned { get; set; }
        public int TotalTasksCompleted { get; set; }
        public int TotalTasksFailed { get; set; }
    }
    public class CreateUserDTO
    {
        public required string? FirstName { get; set; }
        public required string? LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class CreateUserResultDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        //public string? Password { get; set; }
        public bool EmailConfirmed { get; set; } = false;

        public string? RegistrationNumber { get; set; }
        public string? CurrentTask { get; set; }


    }
}

