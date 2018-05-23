using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Sillycore.Web.Abstractions;

namespace Sillycore.Web.Authentication
{
    public class OldAuthenticationConfigurator : IApplicationConfigurator
    {
        public int Order => 10505;

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            if (SillycoreAppBuilder.Instance.DataStore.Get<bool>(Constants.RequiresAuthentication))
            {
                app.UseAuthentication();
            }
        }
    }
}