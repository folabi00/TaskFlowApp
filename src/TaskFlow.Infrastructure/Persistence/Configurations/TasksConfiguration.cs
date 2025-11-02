using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Core.Models;
using Task = TaskFlow.Core.Models.Task;


namespace TaskFlow.Infrastructure.Persistence.Configurations
{
    public class TasksConfiguration : IEntityTypeConfiguration<Task>
    {
        public void Configure(EntityTypeBuilder<Task> builder)
        {
            builder.ToTable("Tasks");
            builder.HasKey(t => t.Id);
            builder.HasIndex(t =>  t.UserId);
            builder.HasOne(r => r.User).WithMany(u => u.Tasks).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.Property(u => u.TaskStatus).HasConversion<string>().HasMaxLength(20);
            builder.Property(u => u.TaskCompletionStatus).HasConversion<string>().HasMaxLength(20);

        }
    }
}
