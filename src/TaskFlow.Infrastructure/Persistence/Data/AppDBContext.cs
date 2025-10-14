using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Core.Models;
using Task = TaskFlow.Core.Models.Task;

namespace TaskFlow.Infrastructure.Persistence.Data
{
    public class AppDBContext: DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
            
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<UserConfirmationToken> UserConfirmationTokens { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.RegistrationNumber).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u =>u.Email).IsUnique();
            modelBuilder.Entity<User>(u => { u.HasOne(r => r.Role).WithMany().HasForeignKey(r => r.RoleId).OnDelete(DeleteBehavior.Restrict);
                u.Property(u => u.UserStatus).HasConversion<string>();
            
            });

            modelBuilder.Entity<UserConfirmationToken>(u => { u.HasKey(t => t.Id); u.HasIndex(t => new { t.UserId, t.TokenHash }); 
                u.HasOne(t => t.User).WithMany(u => u.UserConfirmationTokens).HasForeignKey(t => t.UserId);
                u.Property(t => t.TokenHash).IsRequired(); });

            modelBuilder.Entity<Role>(u => { u.HasKey(r => r.Id); u.HasIndex(r => r.RoleName); } );

            base.OnModelCreating(modelBuilder);
        }
    }
}
