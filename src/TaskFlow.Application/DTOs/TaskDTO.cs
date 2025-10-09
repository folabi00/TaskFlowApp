using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Core.Models;

namespace TaskFlow.Application.DTOs
{
    public class TaskDTO
    {
        public string? TaskName { get; set; }
        public string? TaskDescription { get; set; }
        public required Status TaskStatus { get; set; }
        public int DaysRunning { get; set; }
        public int CompletedInDays { get; set; }
        public Guid UserId { get; set; }
    }
}
