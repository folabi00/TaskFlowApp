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
    public class UsersConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasIndex(u => u.RegistrationNumber).IsUnique();
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasOne(r => r.Role).WithMany().HasForeignKey(r => r.RoleId).OnDelete(DeleteBehavior.Restrict);
            builder.Property(u => u.UserStatus).HasConversion<string>().HasMaxLength(20);
        }
    }
}
