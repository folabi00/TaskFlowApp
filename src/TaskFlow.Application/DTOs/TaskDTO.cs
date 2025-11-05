using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Core.Enums;
using TaskFlow.Core.Models;
using TaskStatus = TaskFlow.Core.Enums.TaskStatus;

namespace TaskFlow.Application.DTOs
{
    public class TaskDTO
    {
        public string? TaskName { get; set; }
        public string? TaskDescription { get; set; }
        public required TaskStatus TaskStatus { get; set; }
        public TaskCompletionStatus? TaskCompletionStatus { get; set; }
        public int DaysRunning { get; set; }
        public int CompletedInDays { get; set; }
        public Guid UserId { get; set; }
        public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
    }
    public class CreateTaskRequestDto
    {
        public string? TaskName { get; set; }
        public string? TaskDescription { get; set; }
        public Guid UserId { get; set; }
    }
    public class TaskResponseDto
    {
        public long Id { get; set; }
        public string? TaskName { get; set; }
        public string? TaskDescription { get; set; }
        public TaskStatus TaskStatus { get; set; }
        public TaskCompletionStatus? TaskCompletionStatus { get; set; }
        public int DaysRunning { get; set; }
        public int CompletedInDays { get; set; }
        public Guid? UserId { get; set; }
        public string? User {  get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
    }
    public class UpdateTaskRequestDto
    {
        public string? TaskName { get; set; }
        public string? TaskDescription { get; set; }
        public required TaskStatus TaskStatus { get; set; }
        public TaskCompletionStatus? TaskCompletionStatus { get; set; }
    }
    public class CloseTaskRequestDto
    {
        public TaskStatus TaskStatus { get; set; }
        public TaskCompletionStatus TaskCompletionStatus { get; set; } 
    }
}
