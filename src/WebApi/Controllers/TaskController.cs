using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Core.Commons;
using TaskStatus = TaskFlow.Core.Enums.TaskStatus;
using TaskFlow.Infrastructure.Persistence.Data;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiVersion(1.0)]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet("get-all-tasks")]
        public async Task<IActionResult> GetAllTask([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var response = new BaseResponse();

            var tasks = await _taskService.GetAllTasks(pageNumber, pageSize);
            if (tasks != null)
            {
                if (tasks.TotalCount > 0)
                {
                    response.ResponseMessage = "Request successful";
                    response.Result = tasks;
                    return Ok(response);
                }
                response.ResponseMessage = $"{tasks.TotalCount} user(s) found";
                response.Result = tasks;
                return Ok(response);
            }
            return BadRequest(new BaseNullResponse());
        }
        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetTaskByID(long id)
        {
            var response = new BaseResponse();
            var task = await _taskService.GetTaskById(id);
            if (task != null)
            {
                if (task.TaskStatus != TaskStatus.Null)
                {
                    response.ResponseMessage = $"Task {task.TaskName} found";
                    response.Result = task;
                    return Ok(response);
                }
                response.ResponseMessage = $"Invalid Task Status";
                response.Result = task;
                return BadRequest(response);
            }
            return BadRequest(new BaseNullResponse());
        }
        [HttpPost("create-task")]
        public async Task<IActionResult> CreateTask(CreateTaskRequestDto user)
        {
            var response = new BaseResponse();
            var result = await _taskService.CreateTask(user);
            if (result != null && result.TaskStatus != TaskStatus.Null)
            {                
                response.ResponseMessage = "Task creation initiated";
                response.Result = result;
                return CreatedAtAction("Create User", response);
            }
            return BadRequest(new BaseNullResponse());

        }
        [HttpPut("update-task")]
        public async Task<IActionResult> UpdateTask(long id, UpdateTaskRequestDto user)
        {
            var response = new BaseResponse();
            var result = await _taskService.UpdateTask(id, user);
            if (result != null)
            {
                if (result.TaskStatus == TaskStatus.Null)
                {
                    response.ResponseMessage = $"Task details may be null or not found";
                    response.Result = result;
                    return BadRequest(response);
                }
                response.ResponseMessage = $"Task with {id} updated successfully";
                response.Result = result;
                return Ok(response);
            }
            return NotFound(new BaseNullResponse());


        }
        [HttpPatch("close-task")]
        public async Task<IActionResult> CloseTask(long id, CloseTaskRequestDto user)
        {
            var response = new BaseResponse();
            if(id == 0)
            {
                return NotFound(new BaseResponse() { ResponseMessage = "Invalid Task Id"});
            }
            var result = await _taskService.CloseTask(id, user);
            if (result)
            {                
                response.ResponseMessage = $"Task with {id} closed successfully";
                response.Result = result;
                return Ok(response);
            }            
            response.ResponseMessage = $"Something went wrong, Task details may be null or not found";
            response.Result = result;
            return BadRequest(response);

        }
        [HttpDelete("delete-task/{id}")]
        public async Task<IActionResult> DeleteTask(long id)
        {
            var response = new BaseResponse();
            var result = await _taskService.DeleteTask(id);
            if (result)
            {
                response.ResponseMessage = $"Task {id} Deleted";
                response.Result = null;
                return Accepted(response);
            }
            return BadRequest(new BaseNullResponse());
        }
    }
}
