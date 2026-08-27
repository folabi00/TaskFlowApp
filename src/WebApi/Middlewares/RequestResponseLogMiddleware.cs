using System;
using System.Diagnostics;
using System.Text.Json;
using TaskFlow.Application.Interfaces;
using TaskFlow.Core.Models;
using TaskFlow.Infrastructure.Services;
using Task = System.Threading.Tasks.Task;

namespace TaskFlow.WebApi.Middlewares
{
    public class RequestResponseLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLogMiddleware> _logger;
        private readonly IMessagePublisher<LogEntry> _messagePublisher;
        public RequestResponseLogMiddleware(ILogger<RequestResponseLogMiddleware> logger, RequestDelegate next, IMessagePublisher<LogEntry> messagePublisher)
        {
            _logger = logger;
            _next = next;
            _messagePublisher = messagePublisher;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var timer = Stopwatch.StartNew();
            var log = new LogEntry()
            {
                Id = Guid.NewGuid().ToString(),
                TraceId = httpContext.TraceIdentifier,
                Method = httpContext.Request.Method,
                Path = httpContext.Request.Path,
                BaseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}"    
            };
            httpContext.Request.EnableBuffering();
            using (var streamReader = new StreamReader(httpContext.Request.Body, leaveOpen: true))
            {
                var requestBody = await streamReader.ReadToEndAsync();
                log.RequestBody = requestBody;
                httpContext.Request.Body.Position = 0;
            }

            var initialResponseBody = httpContext.Response.Body;
            await using var memStream =  new MemoryStream();
            httpContext.Response.Body = memStream;

            await _next(httpContext);

            memStream.Position = 0;
            using (var streamReader = new StreamReader(httpContext.Response.Body, leaveOpen: true))
            {
                var responseBody = await streamReader.ReadToEndAsync();
                log.ResponseBody = responseBody;
                httpContext.Response.Body.Position = 0;
            }

            memStream.Position = 0;
            await memStream.CopyToAsync(initialResponseBody);
            httpContext.Response.Body = initialResponseBody;

            timer.Stop();
            log.Date = DateTimeOffset.UtcNow;
            log.ElapsedTimeMs = timer.ElapsedMilliseconds;
            log.ResponpseCode = httpContext.Response.StatusCode;

            _logger.LogInformation($"Pushing Request-Response log for in DB logging");

            await _messagePublisher.PublishAsync(log);            

        }
    }
}
