using System;
using System.Reflection;
using App.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Sillycore.Abstractions;
using Sillycore.Web.Configuration;
using Sillycore.Web.Filters;
using Sillycore.Web.Swagger;

namespace Sillycore.Web.Mvc
{
    public class MvcServiceConfigurator : IServiceConfigurator
    {
        public void Configure(IServiceCollection services, IConfiguration configuration)
        {
            MetricsElasticSearchOptions metricsElasticSearchOptions = new MetricsElasticSearchOptions();
            configuration.Bind("MetricsElasticSearchOptions", metricsElasticSearchOptions);

            IMetricsBuilder metricsBuilder = AppMetrics.CreateDefaultBuilder();

            metricsBuilder.Configuration.Configure(o =>
            {
                o.Enabled = true;
                o.ReportingEnabled = true;
                o.AddAppTag(SillycoreAppBuilder.Instance.DataStore.Get<string>(Constants.ApplicationName));
            });

            if (metricsElasticSearchOptions.Enabled)
            {
                metricsBuilder.Report.ToElasticsearch(o =>
                {
                    o.Elasticsearch.BaseUri = new Uri(metricsElasticSearchOptions.BaseUri);
                    o.Elasticsearch.Index = metricsElasticSearchOptions.Index;
                    o.FlushInterval = TimeSpan.FromSeconds(metricsElasticSearchOptions.FlushIntervalInSeconds);
                });
            }

            services.AddMetrics(metricsBuilder);
            services.AddMetricsTrackingMiddleware();
            services.AddMetricsReportScheduler();

            services.AddMvc()
                .AddMetrics()
                .AddApplicationPart(Assembly.GetEntryAssembly())
                .AddApplicationPart(GetType().Assembly)
                .AddMvcOptions(o =>
                {
                    o.InputFormatters.RemoveType<XmlDataContractSerializerInputFormatter>();
                    o.InputFormatters.RemoveType<XmlSerializerInputFormatter>();

                    o.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
                    o.OutputFormatters.RemoveType<StreamOutputFormatter>();
                    o.OutputFormatters.RemoveType<StringOutputFormatter>();
                    o.OutputFormatters.RemoveType<XmlDataContractSerializerOutputFormatter>();
                    o.OutputFormatters.RemoveType<XmlSerializerOutputFormatter>();

                    o.Filters.Add<GlobalExceptionFilter>();
                })
                .AddJsonOptions(o =>
                {
                    o.SerializerSettings.ContractResolver = SillycoreApp.JsonSerializerSettings.ContractResolver;
                    o.SerializerSettings.Formatting = SillycoreApp.JsonSerializerSettings.Formatting;
                    o.SerializerSettings.NullValueHandling = SillycoreApp.JsonSerializerSettings.NullValueHandling;
                    o.SerializerSettings.DefaultValueHandling = SillycoreApp.JsonSerializerSettings.DefaultValueHandling;
                    o.SerializerSettings.ReferenceLoopHandling = SillycoreApp.JsonSerializerSettings.ReferenceLoopHandling;
                    o.SerializerSettings.DateTimeZoneHandling = SillycoreApp.JsonSerializerSettings.DateTimeZoneHandling;
                    o.SerializerSettings.Converters.Clear();

                    foreach (JsonConverter converter in SillycoreApp.JsonSerializerSettings.Converters)
                    {
                        o.SerializerSettings.Converters.Add(converter);
                    }
                });
        }
    }
}