using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;


namespace TaskFlow.Infrastructure.Helpers
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var desc in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(desc.GroupName, new OpenApiInfo
                {
                    Title = "TASKFLOW API",
                    Version = desc.ApiVersion.ToString(),
                    Description = $"API documentation for version {desc.ApiVersion}",
                    Contact = new OpenApiContact
                    {
                        Name = "Your Name",
                        Email = "you@example.com"
                    }
                });
            }
        }
    }

}
