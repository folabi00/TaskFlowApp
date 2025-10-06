using Serilog;
using Serilog.AspNetCore;
using TaskFlow.WebApi.Extensions;

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

            Log.Information("Application starting...");
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

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
