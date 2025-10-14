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
    }
}
