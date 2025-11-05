using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;
using TaskFlow.Core.Commons;
using TaskFlow.Core.Enums;
using TaskFlow.Core.Models;
using Task = System.Threading.Tasks.Task;
using TaskEntity = TaskFlow.Core.Models.Task;
using TaskEntityStatus = TaskFlow.Core.Enums.TaskStatus;

namespace TaskFlow.Application.Interfaces
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskEntity>> GetAllTasksAsync(int pageNumber, int pageSize);
        Task<TaskEntity> GetTasksByIdAsync(long id);
        Task CreateTaskAsync(TaskEntity task);
        Task<TaskEntity> UpdateTaskAsync(long taskId, UpdateTaskRequestDto taskDto);
        Task<DatabaseActionResult> CompleteTaskStatusAsync(TaskCompletionStatus taskStatus, long taskId);
        Task<bool> DeleteTaskAsync(long id);
        Task<IEnumerable<TaskEntity>> GetAllTasksPerUser(Guid UserId);
        Task<IEnumerable<TaskEntity>> GetAllTasksPerUserPaginated(int pageNumber, int pageSize, Guid UserId);

    }
}
