using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Core.Models;

namespace TaskFlow.Infrastructure.Persistence.Configurations
{
    public class UserConfirmationTokensConfiguration : IEntityTypeConfiguration<UserConfirmationToken>
    {
        public void Configure(EntityTypeBuilder<UserConfirmationToken> builder)
        {
            builder.ToTable("UserConfirmationTokens");
            builder.HasKey(t => t.Id);
            builder.HasIndex(t => new { t.UserId, t.TokenHash });

            builder.HasOne(t => t.User)
                .WithMany(u => u.UserConfirmationTokens)
                .HasForeignKey(t => t.UserId);

            builder.Property(t => t.TokenHash).IsRequired();

            // Keep compatibility with existing schema typo.
            builder.Property(t => t.TokenPurpose)
                .HasColumnName("TokenPurposee")
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}
