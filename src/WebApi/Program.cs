using Asp.Versioning.ApiExplorer;
using Serilog;
using Serilog.AspNetCore;
using TaskFlow.WebApi.Extensions;
using TaskFlow.WebApi.Middlewares;
using Microsoft.OpenApi;

namespace TaskFlow.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {           
            var builder = WebApplication.CreateBuilder(args);
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

            
            // Add services to the container.
            builder.Services.AddCustomServices(builder.Configuration);
            builder.Host.UseSerilog();
            var app = builder.Build();

            //var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

            Log.Information("Application starting...");
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow API V1");
                    options.SwaggerEndpoint("/swagger/v2/swagger.json", "TaskFlow API V2");
                });
            }
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseMiddleware<RequestResponseLogMiddleware>();

            app.UseRateLimiter();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            try
            {
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly!");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
