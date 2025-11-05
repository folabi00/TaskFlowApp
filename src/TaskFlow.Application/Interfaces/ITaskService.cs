using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;
using TaskFlow.Core.Commons;
using TaskFlow.Core.Models;
//using Task = TaskFlow.Core.Models.Task;
using Task = System.Threading.Tasks.Task;
using TaskEntity = TaskFlow.Core.Models.Task;
using TaskEntityStatus = TaskFlow.Core.Enums.TaskStatus;

namespace TaskFlow.Application.Interfaces

{
    public interface ITaskService
    {
        Task<PaginatedResponse<TaskDTO>> GetAllTasks(int pageNumber, int pageSize);
        Task<PaginatedResponse<TaskDTO>> GetAllTaskPerUser(Guid userId, int pageNumber, int pageSize);
        Task<TaskEntity> GetTaskById(long taskID);
        Task<TaskResponseDto> UpdateTask(long taskId, UpdateTaskRequestDto task);
        Task<bool> DeleteTask(long id);
        Task<TaskResponseDto> CreateTask(CreateTaskRequestDto taskDto);
        Task<bool> CloseTask(long id, CloseTaskRequestDto task);


    }
}
