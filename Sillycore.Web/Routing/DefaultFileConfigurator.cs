using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Sillycore.Web.Abstractions;

namespace Sillycore.Web.Routing
{
    public class DefaultFileConfigurator : IApplicationConfigurator
    {
        public int Order => 10502;
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            app.UseDefaultFiles();
        }
    }
}