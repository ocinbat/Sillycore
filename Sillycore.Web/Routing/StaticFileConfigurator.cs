using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sillycore.Web.Abstractions;

namespace Sillycore.Web.Routing
{
    public class StaticFileConfigurator : IApplicationConfigurator
    {
        public int Order => 10502;
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            StaticFilesConfiguration staticFilesConfiguration = app.ApplicationServices.GetService<StaticFilesConfiguration>();
            app.UseStaticFiles(new StaticFileOptions()
            {
                ServeUnknownFileTypes = staticFilesConfiguration.ServeUnknownFileTypes
            });
        }
    }
}