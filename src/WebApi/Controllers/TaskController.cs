using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Core.Commons;
using TaskFlow.Infrastructure.Persistence.Data;

namespace TaskFlow.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiVersion(2.0)]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly AppDBContext _dbContext;
        public TaskController()
        {
            
        }

        [HttpGet("get-all-tasks")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            return Ok(new BaseNullResponse());
        }
    }
}
