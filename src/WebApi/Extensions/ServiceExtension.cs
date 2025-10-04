using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using TaskFlow.Core.Data;
using TaskFlow.Core.Interfaces;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.WebApi.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();


            services.AddDbContext<AppDBContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), 
                sql => sql.MigrationsAssembly("TaskFlow.WebApi")));
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<IEmailService, EmailSender>();

            services.AddMemoryCache();

            return services;                
        }
    }
}
