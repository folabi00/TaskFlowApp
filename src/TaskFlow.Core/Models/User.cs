using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Core.Enums;

namespace TaskFlow.Core.Models
{
    public class User
    {
        public required Guid Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }       
        public required byte[] PasswordHash { get; set; }
        public required byte[] PasswordSalt { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? RegistrationNumber { get; set;} = null;
        public UserStatus UserStatus { get; set; }
        public Role? Role { get; set; }
        public int? RoleId { get; set; }
        public bool HasCurrentTask {  get; set; }
        public string? CurrentTask { get; set; }
        public int TotalTasksAssigned { get; set; } 
        public int TotalTasksCompleted { get; set; }
        public int TotalTasksFailed { get; set; }
        public bool IsDeleted { get; set; }
       
        public ICollection<UserConfirmationToken> UserConfirmationTokens { get; set; } = new List<UserConfirmationToken>();
        public ICollection<Task> Tasks { get; set; } = new List<Task>();






    }
}
