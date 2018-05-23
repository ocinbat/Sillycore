using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Sillycore.Web.Filters;
using Sillycore.Web.Security;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using Anetta.Extensions;
using Sillycore.Abstractions;
using Sillycore.Infrastructure;
using Sillycore.Web.Abstractions;
using Sillycore.Web.Middlewares;

namespace Sillycore.Web
{
    public class SillycoreStartup
    {
        public IServiceProvider ServiceProvider { get; set; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            foreach (ServiceDescriptor descriptor in SillycoreAppBuilder.Instance.Services)
            {
                services.Add(descriptor);
            }

            SillycoreAppBuilder.Instance.Services = services;
            ServiceProvider = services.BuildAnettaServiceProvider();
            SillycoreAppBuilder.Instance.DataStore.Set(Sillycore.Constants.ServiceProvider, ServiceProvider);
            return ServiceProvider;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            List<IApplicationConfigurator> configurators = new List<IApplicationConfigurator>();

            foreach (Type type in AssemblyScanner.GetAllTypesOfInterface<IApplicationConfigurator>())
            {
                IApplicationConfigurator configurator = (IApplicationConfigurator)Activator.CreateInstance(type);
                configurators.Add(configurator);
            }

            foreach (IApplicationConfigurator configurator in configurators.OrderBy(c => c.Order))
            {
                configurator.Configure(app, env, SillycoreAppBuilder.Instance.Configuration, app.ApplicationServices);
            }
        }
    }
}