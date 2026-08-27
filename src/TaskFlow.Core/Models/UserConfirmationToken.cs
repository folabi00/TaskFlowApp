using System;

namespace TaskFlow.Core.Models
{
    public class UserConfirmationToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string TokenHash { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset ExpiresAt { get; set; }
        public bool IsUsed { get; set; }

        // Renamed for code quality; mapped to existing DB column in configuration.
        public string TokenPurpose { get; set; } = "Email Confirmation";
    }
}
