using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Core.Enums;
using TaskStatus = TaskFlow.Core.Enums.TaskStatus;

namespace TaskFlow.Core.Models
{
    public class Task
    {
        public long Id { get; set; }
        public string? TaskName { get; set; }
        public string? TaskDescription { get; set; }
        public required TaskStatus TaskStatus{ get; set; }
        public TaskCompletionStatus? TaskCompletionStatus{ get; set; }
        public int DaysRunning { get; set; }
        public int CompletedInDays { get; set; } 
        public Guid? UserId { get; set; }
                
    }
    public enum Status { Initiated = 0, Ongoing = 1, Paused = 2, Completed = 3 }
}
