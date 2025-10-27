using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace TaskFlow.WebApi.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private const string ClassName = "ExceptionMiddleware";

        public ExceptionMiddleware(RequestDelegate next, IHostEnvironment environment, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _environment = environment;
            _logger = logger;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            string methodName = nameof(InvokeAsync);
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{ClassName}] [{methodName}] : An error {ex.Message} occured ");
                context.Response.StatusCode = ex switch
                {
                    InvalidOperationException => StatusCodes.Status400BadRequest,
                    NullReferenceException => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status500InternalServerError
                };
                var statusCode = context.Response.StatusCode;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = statusCode,
                    Title = "An internal error occured",
                    Type = ex.GetType().Name,
                    Detail = _environment.IsDevelopment() ? ex.Message : "Something went wrong while processing request"
                });
            }
        }
    }
}
