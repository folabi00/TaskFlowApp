using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Core.Commons;
using TaskFlow.Core.Models;
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
        private readonly IValidator<CreateUserDTO> _createUserValidator;
        private readonly IValidator<UpdateUserDTO> _updateValidator;

        public UserController(IUserService userService, IValidator<CreateUserDTO> createUserValidator, IValidator<UpdateUserDTO> updateValidator)
        {
            _userService = userService;
            _createUserValidator = createUserValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var response = new BaseResponse();
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest(new BaseNullResponse { ResponseMessage = "Invalid input parameters" });
            }
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
            if (id == Guid.Empty)
            {
                return BadRequest(new BaseNullResponse() { ResponseMessage = "Invalid input parameters" });
            }
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
            var validationResult = await _createUserValidator.ValidateAsync(user);
            if (!validationResult.IsValid)
            {
                var errorResponse = validationResult.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Error = e.ErrorMessage
                });
                response.ResponseMessage = "One or more validation error occured"; 
                response.Result = errorResponse;
                return BadRequest(response);
            }
            var result = await _userService.CreateUser(user);            
            if (result != null)
            {
                response.ResponseMessage = "User creation initiated";
                response.Result = result;
                return Created("Create User",response);
            }
            return BadRequest(new BaseNullResponse());

        }
        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUser(UpdateUserDTO user )
        {
            var response = new BaseResponse();
            var validationResult = await _updateValidator.ValidateAsync(user);
            if (!validationResult.IsValid)
            {
                var errorResponse = validationResult.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Error = e.ErrorMessage
                });
                response.ResponseMessage = "One or more validation error occured";
                response.Result = errorResponse;
                return BadRequest(response);
            }
            var result = await _userService.UpdateUser(user);
            if (result != null)
            {
                if (string.IsNullOrEmpty(result?.RegistrationNumber))
                {
                    response.ResponseMessage = $"User {user.Id}'s details not found";
                    response.Result = user;
                    return NotFound(response);
                }
                response.ResponseMessage = $"User {user.Id}'s details updated";
                response.Result = user;
                return Ok(response);
            }
            return BadRequest(new BaseNullResponse());


        }
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id, string RegistrationNumber)
        {
            var response = new BaseResponse();
            if (id == Guid.Empty || RegistrationNumber.IsNullOrEmpty())
            {
                return BadRequest(new BaseNullResponse() { ResponseMessage = "Invalid input parameters" });
            }
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
            if(userId == Guid.Empty || token.IsNullOrEmpty())
            {
                return BadRequest(new BaseNullResponse() { ResponseMessage = "Invalid input parameters" });
            }
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
