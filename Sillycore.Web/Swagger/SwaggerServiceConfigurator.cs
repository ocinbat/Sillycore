using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Sillycore.Abstractions;
using Swashbuckle.AspNetCore.Swagger;

namespace Sillycore.Web.Swagger
{
    public class SwaggerServiceConfigurator : IServiceConfigurator
    {
        public void Configure(IServiceCollection services, IConfiguration configuration)
        {
            if (SillycoreAppBuilder.Instance.DataStore.Get<bool>(Constants.UseSwagger))
            {
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new Info { Title = SillycoreAppBuilder.Instance.DataStore.Get<string>(Constants.ApplicationName), Version = "v1" });
                    c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = "header",
                        Type = "apiKey"
                    });
                    c.DescribeAllEnumsAsStrings();
                    c.DescribeStringEnumsInCamelCase();
                    c.DescribeAllParametersInCamelCase();
                    c.IgnoreObsoleteActions();
                    c.IgnoreObsoleteProperties();

                    string filePath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, $"{SillycoreAppBuilder.Instance.DataStore.Get<string>(Constants.ApplicationName)}.xml");

                    if (File.Exists(filePath))
                    {
                        c.IncludeXmlComments(filePath);
                    }
                });
            }
        }
    }
}