using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using TaskFlow.Application.Interfaces;
using TaskFlow.Infrastructure.Services;
using TaskFlow.Application.ApplicationServices;
using TaskFlow.Infrastructure.Persistence.Data;
using TaskFlow.Infrastructure.Helpers;
using TaskFlow.Infrastructure.Persistence.Repositories;

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
            services.AddScoped<IUserRegistrationNumberGenerator, UserRegistrationNumberGenerator>();
            services.AddScoped<IHashingService, HashingUtility>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddMemoryCache();

            return services;                
        }
    }
}
