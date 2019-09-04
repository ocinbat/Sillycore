using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Anetta.ServiceConfiguration;
using Sillycore.Infrastructure;

namespace Sillycore.Configuration
{
    public class ConfigurationBinder : IServiceConfigurator
    {
        public void Configure(IServiceCollection services, IConfiguration configuration)
        {
            foreach (Assembly assembly in AssemblyScanner.GetAssemblies())
            {
                foreach (TypeInfo typeInfo in assembly.DefinedTypes)
                {
                    ConfigurationAttribute configurationAttribute = typeInfo.GetCustomAttribute<ConfigurationAttribute>();

                    if (configurationAttribute != null)
                    {
                        var instance = Activator.CreateInstance(typeInfo);
                        configuration.Bind(configurationAttribute.Section, instance);

                        services.AddSingleton(typeInfo, sp => instance);
                    }
                }
            }
        }
    }
}