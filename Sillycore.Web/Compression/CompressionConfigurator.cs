using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sillycore.Abstractions;
using Sillycore.Web.Abstractions;

namespace Sillycore.Web.Compression
{
    public class CompressionConfigurator : IServiceConfigurator, IApplicationConfigurator
    {
        public void Configure(IServiceCollection services, IConfiguration configuration)
        {
            services.AddResponseCompression();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            app.UseResponseCompression();
        }

        public int Order => 10002;
    }
}