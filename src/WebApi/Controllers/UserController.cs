using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Core.Commons;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiVersion(1.0)]
    [EnableRateLimiting("policy")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var response = new BaseResponse();

            var users = await _userService.GetAllUsers(pageNumber,pageSize);
            if (users != null)
            {
                if(users.TotalCount > 0)
                {
                    response.ResponseMessage = "Users request successful";
                    response.Result = users;
                    return Ok(response);
                }
                response.ResponseMessage = $"{users.TotalCount} user(s) found";
                response.Result = users;
                return Ok(response);
            }
            return BadRequest(new BaseNullResponse());
        }
        
        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetUserByID(Guid id)
        {
            var response = new BaseResponse();
            var user = await _userService.GetUserById(id);
            if (user != null)
            {
                if(user.RegistrationNumber != null)
                {
                    response.ResponseMessage = $"User {user.RegistrationNumber} found";
                    response.Result = user;
                    return Ok(response);
                }
                response.ResponseMessage = $"Request successful, {user.RegistrationNumber} found";
                response.Result = user;
                return Ok(response);
            }
            return BadRequest(new BaseNullResponse());
        }
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser(CreateUserDTO user)
        {
            var response = new BaseResponse();
            var result = await _userService.CreateUser(user);            
            if (result != null)
            {
                if (result.RegistrationNumber == "01")
                {
                    response.ResponseMessage = "Email Already Exist";
                    response.Result = result;
                    return BadRequest(response);
                }
                else if (result.RegistrationNumber == "02")
                {
                    response.ResponseMessage = "Email Validation Error";
                    response.Result = result;
                    return BadRequest(response);
                }
                response.ResponseMessage = "User creation initiated";
                response.Result = result;
                return Created("Create User",response);
            }
            return BadRequest(new BaseNullResponse());

        }
        [HttpPatch("update-user")]
        public async Task<IActionResult> UpdateUser(UserDTO user)
        {
            var response = new BaseResponse();
            var result = await _userService.UpdateUser(user);
            if (result != null)
            {
                if (string.IsNullOrEmpty(result?.RegistrationNumber))
                {
                    response.ResponseMessage = $"User {user.RegistrationNumber}'s details not found";
                    response.Result = user;
                    return NotFound(response);
                }
                response.ResponseMessage = $"User {user.RegistrationNumber}'s details updated";
                response.Result = user;
                return Ok(response);
            }
            return BadRequest(new BaseNullResponse());


        }
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id, string RegistrationNumber)
        {
            var response = new BaseResponse();
            var result = await _userService.DeleteUser(id, RegistrationNumber);
            if (result == true)
            {
                response.ResponseMessage = $"User {RegistrationNumber}'s details Deleted";
                response.Result = null;
                return Accepted(response);
            }
            return BadRequest(new BaseNullResponse());
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token)
        {
            var response = new BaseResponse();
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId.ToString()))
            {
                return BadRequest(new BaseNullResponse());
            }
            var result = await _userService.ConfirmEmail(userId, token);
            if (result == true)
            {
                response.ResponseMessage = $"User Email verification successful";
                response.Result = null;
                return Ok(response);                
                //return Redirect("");
            }
            return BadRequest(new BaseNullResponse() { Result = result});

        }

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail( Guid userId)
        {
            var response = new BaseResponse();
            if (string.IsNullOrEmpty(userId.ToString()))
            {
                return BadRequest(new BaseNullResponse());
            }
            var result = await _userService.ResendConfirmation(userId);
            if (result == true)
            {
                response.ResponseMessage = $"User Email successfully validated";
                response.Result = null;
                return Ok(response);
                //TO-DO: remove google link and replace with appropriate url
                //return Redirect("https://google.com");
            }
            return BadRequest(new BaseNullResponse() { ResponseMessage = "Something went wrong while sending account confirmation email"});
        }
    }
}
