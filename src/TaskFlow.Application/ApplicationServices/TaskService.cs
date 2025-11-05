using Azure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Core.Commons;
using TaskFlow.Core.Models;
using Task = System.Threading.Tasks.Task;
using TaskEntity = TaskFlow.Core.Models.Task;
using TaskEntityStatus = TaskFlow.Core.Enums.TaskStatus;

namespace TaskFlow.Application.ApplicationServices
{
    public class TaskService : ITaskService
    {
        private const string ClassName = "TaskService";
        private readonly ITaskRepository _taskRepository;
        private readonly ILogger<TaskService> _logger;
        public TaskService(ILogger<TaskService> logger, ITaskRepository taskRepository)
        {
            _logger = logger;
            _taskRepository = taskRepository;
        }
        public async Task<TaskResponseDto> CreateTask(CreateTaskRequestDto taskDto)
        {
            string methodName = nameof(CreateTask);
            var response = new TaskResponseDto();
            try
            {

                TaskEntity task = new TaskEntity()
                {
                    TaskName = taskDto.TaskName,
                    TaskDescription = taskDto.TaskDescription,
                    UserId = taskDto.UserId,
                    TaskStatus = TaskEntityStatus.Initiated,
                    UpdatedAt = DateTimeOffset.UtcNow

                };
                await _taskRepository.CreateTaskAsync(task);

                response.Id = task.Id;
                response.TaskName = task.TaskName;
                response.TaskDescription = task.TaskDescription;
                response.TaskStatus = task.TaskStatus;
                response.User = task.User.Email;
                response.UserId = taskDto.UserId;
                response.TaskCompletionStatus = task.TaskCompletionStatus;
                response.CreatedAt = task.CreatedAt;
                response.UpdatedAt = task.UpdatedAt;
                response.CompletedAt = task.CompletedAt;
                response.DaysRunning = task.DaysRunning;
                response.CompletedInDays = task.CompletedInDays;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : Something went wrong while creating task, some task details may be missing", ex.Message);
                return response;
            }
        }

        public async Task<bool> DeleteTask(long id)
        {
            string methodName = "DeleteTask";
            try
            {
                var status = await _taskRepository.DeleteTaskAsync(id);
                if (status)
                {
                    _logger.LogInformation($"[{ClassName}] [{methodName}] : Task {id} delete status is {status.ToString().ToUpper()}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : Error occured while trying to delete task ", ex.Message);
                throw;
            }
        }
        public async Task<PaginatedResponse<TaskDTO>> GetAllTasks(int pageNumber, int pageSize)
        {
            string methodName = "GetAllTasks";
            PaginatedResponse<TaskDTO> taskDTO = new();
            List<TaskDTO> tasks = new List<TaskDTO>();
            try
            {
                var returnedTasks = await _taskRepository.GetAllTasksAsync(pageNumber, pageSize);
                if (returnedTasks.Count() > 0)
                {
                    foreach (var task in returnedTasks)
                    {
                        var newTask = new TaskDTO()
                        {
                            TaskName = task.TaskName,
                            TaskDescription = task.TaskDescription,
                            TaskStatus = task.TaskStatus,
                            TaskCompletionStatus = task.TaskCompletionStatus,
                            UserId = (Guid)task.UserId,
                            DaysRunning = task.DaysRunning,
                            CreatedAt = task.CreatedAt,
                            UpdatedAt = task.UpdatedAt,
                            CompletedAt = task.CompletedAt,
                            CompletedInDays = task.CompletedInDays
                        };
                        tasks.Add(newTask);

                    }
                    taskDTO.Items = tasks;
                    taskDTO.TotalCount = tasks.Count();
                    return taskDTO;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : Something went wrong while fetching all task", ex.Message);
                throw;
            }
            return taskDTO;
        }
        public async Task<PaginatedResponse<TaskDTO>> GetAllTaskPerUser(Guid userId, int pageNumber, int pageSize)
        {
            string methodName = "GetAllTasks";
            PaginatedResponse<TaskDTO> taskDTO = new();
            List<TaskDTO> tasks = new List<TaskDTO>();
            try
            {
                var returnedTasks = await _taskRepository.GetAllTasksPerUserPaginated(pageNumber, pageSize, userId);
                if (returnedTasks.Count() > 0)
                {
                    foreach (var task in returnedTasks)
                    {
                        var newTask = new TaskDTO()
                        {
                            TaskName = task.TaskName,
                            TaskDescription = task.TaskDescription,
                            TaskStatus = task.TaskStatus,
                            TaskCompletionStatus = task.TaskCompletionStatus,
                            UserId = (Guid)task.UserId,
                            DaysRunning = task.DaysRunning,
                            CreatedAt = task.CreatedAt,
                            UpdatedAt = task.UpdatedAt,
                            CompletedAt = task.CompletedAt,
                            CompletedInDays = task.CompletedInDays
                        };
                        tasks.Add(newTask);

                    }
                    taskDTO.Items = tasks;
                    taskDTO.TotalCount = tasks.Count();
                    return taskDTO;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : Something went wrong while fetching all task for User {userId}", ex.Message);
                throw;
            }
            return taskDTO;
        }
               

        public async Task<TaskEntity> GetTaskById(long taskID)
        {
            string methodName = "GetTaskById";
            var task = new TaskEntity() { TaskStatus = TaskEntityStatus.Null };
            try
            {
                var returnedTask = await _taskRepository.GetTasksByIdAsync(taskID);
                if (returnedTask != null && returnedTask.TaskStatus != TaskEntityStatus.Null)
                {
                    return returnedTask;
                }
                return task;
            }
            catch(Exception ex)
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : Something went wrong while fetching  task {taskID}", ex.Message);
                throw;
            }
        }

        public async Task<TaskResponseDto> UpdateTask(long taskId, UpdateTaskRequestDto task)
        {
            string methodName = nameof(UpdateTask);
            var response = new TaskResponseDto() { TaskStatus = TaskEntityStatus.Null};
            try
            {
                var modifiedTask = await _taskRepository.UpdateTaskAsync(taskId, task);
                if(modifiedTask.TaskStatus != TaskEntityStatus.Null)
                {
                    response.Id = modifiedTask.Id;
                    response.TaskName = modifiedTask.TaskName;
                    response.TaskStatus = modifiedTask.TaskStatus;
                    response.TaskDescription = modifiedTask.TaskDescription;
                    response.TaskCompletionStatus = modifiedTask.TaskCompletionStatus;
                    response.User = modifiedTask.User.Email;
                    response.DaysRunning = modifiedTask.DaysRunning;
                    response.CompletedInDays = modifiedTask.CompletedInDays;
                    response.CreatedAt = modifiedTask.CreatedAt;
                    response.UpdatedAt = modifiedTask.UpdatedAt;
                    response.CompletedAt = modifiedTask.CompletedAt;
                }
                return response;
            }
            catch(Exception ex)
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : Something went wrong while updating  task {taskId}", ex.Message);
                throw;
            }
        }

        public async Task<bool> CloseTask(long id, CloseTaskRequestDto task)
        {
            string methodName = nameof(CloseTask);
            try
            {
                var result = await _taskRepository.CompleteTaskStatusAsync(task.TaskCompletionStatus, id);
                if(result.ToString().ToLower() == "success")
                {
                    return true;    
                }
                return false;
            }
            catch(Exception ex)
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : Something went wrong while closing  task {id}", ex.Message);
                throw;
            }
        }
    }
}
