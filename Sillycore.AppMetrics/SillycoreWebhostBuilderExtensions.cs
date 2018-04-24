using App.Metrics.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Sillycore.Web;

namespace Sillycore.AppMetrics
{
    public static class SillycoreWebhostBuilderExtensions
    {
        public static SillycoreWebhostBuilder WithAppMetrics(this SillycoreWebhostBuilder builder)
        {
            builder.SillycoreAppBuilder.Services.AddMetrics();
            builder.SillycoreAppBuilder.BeforeBuild(() =>
            {
                IWebHostBuilder webhostBuilder = builder.SillycoreAppBuilder.DataStore
                    .Get<IWebHostBuilder>(Web.Constants.WebHostBuilder)
                    .UseMetrics();

                builder.SillycoreAppBuilder.DataStore.Set(Web.Constants.WebHostBuilder, webhostBuilder);
            });

            return builder;
        }
    }
}
