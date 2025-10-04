using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Core.Models
{
    public class Task
    {
        public long Id { get; set; }
        public string? TaskName { get; set; }
        public string? TaskDescription { get; set; }
        public required Status TaskStatus{ get; set; }
        public int DaysRunning { get; set; }
        public int CompletedInDays { get; set; } 
        public Guid? UserId { get; set; }
                
    }
    public enum Status { Initiated = 0, Ongoing = 1, Paused = 2, Completed = 3 }
}
