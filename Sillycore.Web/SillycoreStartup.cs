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
using Sillycore.Extensions;
using Sillycore.Web.Middlewares;

namespace Sillycore.Web
{
    public class SillycoreStartup
    {
        public SillycoreStartup()
        {
        }

        public SillycoreStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public InMemoryDataStore DataStore => SillycoreAppBuilder.Instance.DataStore;

        public void ConfigureServices(IServiceCollection services)
        {
            foreach (ServiceDescriptor descriptor in SillycoreAppBuilder.Instance.Services)
            {
                services.Add(descriptor);
            }

            services.AddMvc()
                .AddApplicationPart(Assembly.GetEntryAssembly())
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
                    o.Filters.Add<ValidateModelStateFilter>();
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

            if (DataStore.Get<bool>(Constants.UseSwagger))
            {
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new Info { Title = DataStore.Get<string>(Constants.ApplicationName), Version = "v1" });
                    c.DescribeAllEnumsAsStrings();
                    c.DescribeStringEnumsInCamelCase();
                    c.DescribeAllParametersInCamelCase();
                    c.IgnoreObsoleteActions();
                    c.IgnoreObsoleteProperties();
                });
            }

            if (DataStore.Get<bool>(Constants.RequiresAuthentication))
            {
                ConfigureAuthentication(services);
                ConfigureAuthorization(services);
            }

            ConfigureServicesInner(services);
            SillycoreAppBuilder.Instance.Services = services;
        }

        private void ConfigureAuthorization(IServiceCollection services)
        {
            var authorizationOptions = DataStore.Get<SillycoreAuthorizationOptions>(Constants.AuthorizationOptions);
            if (authorizationOptions != null)
            {
                services.AddAuthorization(options =>
                {
                    ConfigureAuthorizationPolicies(authorizationOptions, options);
                });
            }
        }

        private static void ConfigureAuthorizationPolicies(SillycoreAuthorizationOptions authorizationOptions,
            AuthorizationOptions options)
        {
            foreach (var authorizationPolicy in authorizationOptions.Policies)
            {
                options.AddPolicy(authorizationPolicy.Name, builder =>
                {
                    builder.RequireClaim("scope", authorizationPolicy.RequiredScopes);
                });
            }
        }

        private void ConfigureAuthentication(IServiceCollection services)
        {
            var authenticationOptions = DataStore.Get<SillycoreAuthenticationOptions>(Constants.AuthenticationOptions);

            if (authenticationOptions != null)
            {
                var authority = DataStore.Get<IConfiguration>(Sillycore.Constants.Configuration).GetValue<string>(authenticationOptions.AuthorityConfigKey);

                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.Authority = authority;
                        options.RequireHttpsMetadata = authenticationOptions.RequiresHttpsMetadata;
                        if (authenticationOptions.LegacyAudienceValidation)
                        {
                            options.TokenValidationParameters.ValidateAudience = false;
                        }
                    });
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            SillycoreAppBuilder.Instance.DataStore.Set(Sillycore.Constants.ServiceProvider, app.ApplicationServices);

            app.UseMiddleware<SillycoreMiddleware>();

            string dockerImageName = Environment.GetEnvironmentVariable("Sillycore.DockerImageName");

            if (!String.IsNullOrWhiteSpace(dockerImageName))
            {
                app.UseMiddleware<DockerImageVersionMiddleware>();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (DataStore.Get<bool>(Constants.RequiresAuthentication))
            {
                app.UseAuthentication();
            }

            app.UseMvc(r =>
            {
                if (DataStore.Get<bool>(Constants.UseSwagger))
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

            if (DataStore.Get<bool>(Constants.UseSwagger))
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    c.InjectOnCompleteJavaScript("");
                });
            }

            ConfigureInner(app, env);

            app.UseDefaultFiles();
            app.UseStaticFiles();
            
            RegisterStartAndStopActions(app);
        }

        private void RegisterStartAndStopActions(IApplicationBuilder app)
        {
            IApplicationLifetime applicationLifetime = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();

            List<Action> onStartActions = DataStore.Get<List<Action>>(Constants.OnStartActions);
            List<Action> onStopActions = DataStore.Get<List<Action>>(Constants.OnStopActions);

            foreach (var onStartAction in onStartActions)
            {
                applicationLifetime.ApplicationStarted.Register(onStartAction);
            }

            if (onStopActions.IsEmpty())
            {
                onStopActions.Add(OnShutdown);
            }

            foreach (var onStopAction in onStopActions)
            {
                applicationLifetime.ApplicationStopping.Register(onStopAction);
            }
        }

        private void OnShutdown()
        {
            SillycoreApp.Instance.DataStore.Set(Constants.IsShuttingDown, true);
            Thread.Sleep(10000);
        }

        public virtual void ConfigureServicesInner(IServiceCollection services) { }
        public virtual void ConfigureInner(IApplicationBuilder app, IHostingEnvironment env) { }
    }
}