using Asp.Versioning;
using Azure;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Core.Commons;
using TaskFlow.Core.Models;

namespace TaskFlow.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiVersion(1.0)]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<LoginDTO> _validator;
        public AuthController(IAuthService authService, IValidator<LoginDTO> validator)
        {
            _authService = authService;
            _validator = validator;
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var response = new BaseResponse();
            var validationResult = await _validator.ValidateAsync(loginDTO);
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
            var result = await _authService.ValidateUserLogin(loginDTO.Email, loginDTO.Password);
            if (result.ResponseMessage == "Success")
            {
                response.ResponseMessage = "Login Successful";
                response.Result = result;
                return Ok(response);
            }
            return BadRequest(result);
        }   
    }
}
