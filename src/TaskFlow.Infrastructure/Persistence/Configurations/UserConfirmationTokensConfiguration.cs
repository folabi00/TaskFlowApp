using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            builder.HasOne(t => t.User).WithMany(u => u.UserConfirmationTokens).HasForeignKey(t => t.UserId);
            builder.Property(t => t.TokenHash).IsRequired();
        }
    }
}
