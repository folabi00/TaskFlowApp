using Microsoft.EntityFrameworkCore;
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
using TaskFlow.Core.Enums;
using TaskFlow.Core.Models;
using TaskFlow.Infrastructure.Persistence.Data;
using Task = System.Threading.Tasks.Task;
using TaskEntity = TaskFlow.Core.Models.Task;
using TaskEntityStatus = TaskFlow.Core.Enums.TaskStatus;

namespace TaskFlow.Infrastructure.Persistence.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDBContext _dbContext;
        private readonly ILogger<TaskRepository> _logger;
        private const string ClassName = nameof(TaskRepository);

        public TaskRepository(AppDBContext dbContext, ILogger<TaskRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task CreateTaskAsync(TaskEntity task)
        {
            string methodName = nameof(CreateTaskAsync);
            try
            {
                await _dbContext.Tasks.AddAsync(task);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"[{ClassName}] [{methodName}] : Task {task.Id} created for User {task.User.Email} at {DateTimeOffset.UtcNow} ");

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{ClassName}] [{methodName}] : An Error occured, unable to create task with Id {task.Id}", ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteTaskAsync(long id)
        {
            string methodName = nameof(DeleteTaskAsync);
            var task = await _dbContext.Tasks.FindAsync(id);
            _dbContext.Tasks.Remove(task!);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"[{ClassName}] [{methodName}] : Task {task.TaskName} deleted  at {DateTimeOffset.UtcNow} ");

            return true;
        }


        public async Task<IEnumerable<TaskEntity>> GetAllTasksAsync(int pageNumber, int pageSize)
        {
            string methodName = nameof(GetAllTasksAsync);
            var startIndex = (pageNumber - 1) * pageSize;
            var returnedTasks = await _dbContext.Tasks.OrderBy(u => u.Id).Skip(startIndex).Take(pageSize).ToListAsync();
            _logger.LogInformation($"[{ClassName}] [{methodName}] : {returnedTasks.Count} number of tasks found");

            return returnedTasks;
        }
        public async Task<IEnumerable<TaskEntity>> GetAllTasksPerUser(Guid userId)
        {
            string methodName = nameof(GetAllTasksPerUser);
            var returnedTasks = await _dbContext.Tasks.Where(u => u.UserId == userId).ToListAsync();
            _logger.LogInformation($"[{ClassName}] [{methodName}] : {returnedTasks.Count} number of tasks found");

            return returnedTasks;
        }
        public async Task<IEnumerable<TaskEntity>> GetAllTasksPerUserPaginated(int pageNumber, int pageSize , Guid userId)
        {
            string methodName = nameof(GetAllTasksPerUserPaginated);
            var startIndex = (pageNumber - 1) * pageSize;
            var userTasks = await _dbContext.Tasks.Where(u => u.UserId == userId).ToListAsync();
            var returnedTasks = userTasks.OrderBy(u => u.Id).Skip(startIndex).Take(pageSize);
            //var returnedTasks = await _dbContext.Tasks.OrderBy(u => u.UserId == userId).Skip(startIndex).Take(pageSize).ToListAsync();
            _logger.LogInformation($"[{ClassName}] [{methodName}] : {userTasks.Count} number of tasks found");

            return returnedTasks;
        }

        public async Task<TaskEntity> GetTasksByIdAsync(long id)
        {
            string methodName = nameof(GetTasksByIdAsync);
            var task = await _dbContext.Tasks.FindAsync(id);
            _logger.LogInformation($"[{ClassName}] [{methodName}] : {task.TaskName} found");

            return task;
        }

        public async Task<TaskEntity> UpdateTaskAsync(long taskId, UpdateTaskRequestDto taskDto)
        {
            string methodName = nameof(UpdateTaskAsync);
            var returnedTask = _dbContext.Tasks.Find(taskId);
            if (returnedTask != null)
            {
                if(returnedTask.TaskStatus != TaskEntityStatus.Completed)
                {
                    returnedTask.TaskStatus = TaskEntityStatus.InProgress;
                    returnedTask.TaskName = taskDto.TaskName;
                    returnedTask.TaskDescription = taskDto.TaskDescription;
                    returnedTask.UpdatedAt = DateTimeOffset.UtcNow;
                    returnedTask.DaysRunning = (DateTimeOffset.UtcNow - returnedTask.CreatedAt).Days;

                    await _dbContext.SaveChangesAsync();
                    return returnedTask;
                }
                else
                {
                    _logger.LogError($"[{ClassName}] [{methodName}] : Error unable to update a completed task");
                    return new TaskEntity() { TaskStatus = TaskEntityStatus.Null };
                }
            }
            _logger.LogError($"[{ClassName}] [{methodName}] : Task is null or not Found");
            return new TaskEntity() { TaskStatus = TaskEntityStatus.Null };
        }

        public async Task<DatabaseActionResult> CompleteTaskStatusAsync(TaskCompletionStatus taskStatus, long taskId)
        {
            string methodName = nameof( CompleteTaskStatusAsync);
            var returnedTask = _dbContext.Tasks.Find(taskId);
            if (returnedTask != null)
            {
                returnedTask.TaskStatus = TaskEntityStatus.Completed;
                returnedTask.TaskCompletionStatus = taskStatus;
                returnedTask.CompletedInDays = (DateTimeOffset.UtcNow - returnedTask.CreatedAt).Days;
                returnedTask.CompletedAt = DateTimeOffset.UtcNow;
                
                await _dbContext.SaveChangesAsync();
                return DatabaseActionResult.Success;
            }
            _logger.LogError($"[{ClassName}] [{methodName}] : Task is null or not Found");
            return DatabaseActionResult.Failed;
        }
    }
}
