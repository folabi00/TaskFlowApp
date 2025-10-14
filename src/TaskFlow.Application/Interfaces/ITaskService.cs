using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Core.Models;
using Task = TaskFlow.Core.Models.Task;

namespace TaskFlow.Application.Interfaces

{
    public interface ITaskService
    {
        Task<IEnumerable<Task>> GetAllTasks();
        Task<IEnumerable<Task>> GetTaskById(long taskID);
        Task<IEnumerable<Task>> CreateTask(string name);
        Task<Task> UpdateTask(Task task);
        Task<IEnumerable<Task>> DeleteTask(string name);
        Task<IEnumerable<Task>> AssignTask(Task task, string userID);


    }
}
