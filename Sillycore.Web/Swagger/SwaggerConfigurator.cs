using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Sillycore.Web.Abstractions;

namespace Sillycore.Web.Swagger
{
    public class SwaggerConfigurator : IApplicationConfigurator
    {
        public int Order => 10701;
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            if (SillycoreAppBuilder.Instance.DataStore.Get<bool>(Constants.UseSwagger))
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", SillycoreAppBuilder.Instance.DataStore.Get<string>(Constants.ApplicationName));
                });
            }
        }
    }
}