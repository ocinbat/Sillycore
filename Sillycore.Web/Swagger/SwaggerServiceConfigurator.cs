using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Anetta.ServiceConfiguration;
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
                    SwaggerConfiguration swaggerConfiguration = new SwaggerConfiguration();
                    configuration.Bind("Sillycore:Swagger", swaggerConfiguration);

                    c.SwaggerDoc(swaggerConfiguration.Version, swaggerConfiguration.GetSwaggerInfo());

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