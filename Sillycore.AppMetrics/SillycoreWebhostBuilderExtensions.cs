using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters;
using App.Metrics.Formatters.Ascii;
using App.Metrics.Formatters.Json;
using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Sillycore.Web;

namespace Sillycore.AppMetrics
{
    public static class SillycoreWebhostBuilderExtensions
    {
        public static SillycoreWebhostBuilder WithAppMetrics(this SillycoreWebhostBuilder builder)
        {
            IMetricsRoot metrics = App.Metrics.AppMetrics.CreateDefaultBuilder()
                .OutputMetrics.AsPrometheusPlainText()
                .OutputMetrics.AsPlainText()
                .Build();


            builder.SillycoreAppBuilder.Services.AddMetrics();
            builder.SillycoreAppBuilder.BeforeBuild(() =>
            {
                IWebHostBuilder webhostBuilder = builder.SillycoreAppBuilder.DataStore
                    .Get<IWebHostBuilder>(Web.Constants.WebHostBuilder)
                    .ConfigureMetrics(metrics)
                    .UseMetrics(
                        options =>
                        {
                            options.EndpointOptions = endpointsOptions =>
                            {
                                endpointsOptions.MetricsTextEndpointOutputFormatter = metrics.OutputMetricsFormatters.GetType<MetricsTextOutputFormatter>();
                                endpointsOptions.MetricsEndpointOutputFormatter = metrics.OutputMetricsFormatters.GetType<MetricsPrometheusTextOutputFormatter>(); 
                            };
                        });

                builder.SillycoreAppBuilder.DataStore.Set(Web.Constants.WebHostBuilder, webhostBuilder);
            });

            return builder;
        }
    }
}
