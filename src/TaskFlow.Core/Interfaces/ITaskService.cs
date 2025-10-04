using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Core.Models;

namespace TaskFlow.Core.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<Models.Task>> GetAllTasks();
        Task<IEnumerable<Models.Task>> GetTaskById(long taskID);
        Task<IEnumerable<Models.Task>> CreateTask(string name);
        Task<Models.Task> UpdateTask(Models.Task task);
        Task<IEnumerable<Models.Task>> DeleteTask(string name);
        Task<IEnumerable<Models.Task>> AssignTask(Models.Task task, string userID);



    }
}
