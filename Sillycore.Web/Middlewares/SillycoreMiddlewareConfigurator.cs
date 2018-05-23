using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Sillycore.Web.Abstractions;

namespace Sillycore.Web.Middlewares
{
    public class SillycoreMiddlewareConfigurator : IApplicationConfigurator
    {
        public int Order => 10503;
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            app.UseMiddleware<SillycoreMiddleware>();

            string dockerImageName = Environment.GetEnvironmentVariable("Sillycore.DockerImageName");

            if (!String.IsNullOrWhiteSpace(dockerImageName))
            {
                app.UseMiddleware<DockerImageVersionMiddleware>();
            }
        }
    }
}