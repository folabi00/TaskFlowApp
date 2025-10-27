using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Threading.RateLimiting;
using TaskFlow.Application.ApplicationServices;
using TaskFlow.Application.Interfaces;
using TaskFlow.Infrastructure.Helpers;
using TaskFlow.Infrastructure.Persistence.Data;
using TaskFlow.Infrastructure.Persistence.Repositories;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.WebApi.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new MediaTypeApiVersionReader("v"); //Accept: application/json;v=1
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = false;
            });
            services.AddControllers();
            //services.AddEndpointsApiExplorer();
            services.ConfigureOptions<ConfigureSwaggerOptions>();
            services.AddSwaggerGen();
            

            services.AddDbContext<AppDBContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), 
                sql => sql.MigrationsAssembly("TaskFlow.Infrastructure")));
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<IEmailService, EmailSender>();
            services.AddScoped<IUserRegistrationNumberGenerator, UserRegistrationNumberGenerator>();
            services.AddScoped<IHashingService, HashingUtility>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRoleRepository, RoleRepository>();

            services.AddMemoryCache();

            services.AddRateLimiter(options =>
            {
                options.AddPolicy("policy", context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: ip,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 3,
                            Window = TimeSpan.FromSeconds(10),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        });
                });
            });

            return services;                
        }
    }
}
