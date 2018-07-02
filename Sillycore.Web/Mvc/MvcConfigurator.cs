using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Sillycore.Web.Abstractions;

namespace Sillycore.Web.Mvc
{
    public class MvcConfigurator : IApplicationConfigurator
    {
        public int Order => 10600;
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseMvc(r =>
            {
                if (SillycoreAppBuilder.Instance.DataStore.Get<bool>(Constants.RedirectRootToSwagger))
                {
                    r.MapRoute(name: "Default",
                        template: "",
                        defaults: new { controller = "Help", action = "Index" });
                }
                else
                {
                    r.MapRoute(name: "Default",
                        template: "",
                        defaults: new { controller = "Home", action = "Index" });
                }
            });
        }
    }
}