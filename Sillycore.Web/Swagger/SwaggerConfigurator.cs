using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Sillycore.Web.Abstractions;
using Swashbuckle.AspNetCore.Swagger;

namespace Sillycore.Web.Swagger
{
    public class SwaggerConfigurator : IApplicationConfigurator
    {
        public int Order => 10701;
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            if (SillycoreAppBuilder.Instance.DataStore.Get<bool>(Constants.UseSwagger))
            {
                SwaggerConfiguration swaggerConfiguration = new SwaggerConfiguration();
                configuration.Bind("Sillycore:Swagger", swaggerConfiguration);

                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/swagger/{swaggerConfiguration.Version}/swagger.json", swaggerConfiguration.Version);
                });
            }
        }
    }
}