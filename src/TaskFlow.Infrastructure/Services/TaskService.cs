using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Core.Interfaces;

namespace TaskFlow.Infrastructure.Services
{
    public class TaskService : ITaskService
    {
        public Task<IEnumerable<Core.Models.Task>> AssignTask(Core.Models.Task task, string userID)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Core.Models.Task>> CreateTask(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Core.Models.Task>> DeleteTask(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Core.Models.Task>> GetAllTasks()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Core.Models.Task>> GetTaskById(long taskID)
        {
            throw new NotImplementedException();
        }

        public Task<Core.Models.Task> UpdateTask(Core.Models.Task task)
        {
            throw new NotImplementedException();
        }
    }
}
