using System;
using Microsoft.Extensions.Configuration;

namespace Sillycore.Extensions
{
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Attempts to bind the given object type to the configuration section specified by the key by matching property names against configuration keys recursively.
        /// </summary>
        /// <param name="configuration">The configuration instance to bind.</param>
        /// <param name="key">The key of the configuration section to bind.</param>
        public static T Bind<T>(this IConfiguration configuration, string key)
        {
            T type = Activator.CreateInstance<T>();
            configuration.Bind(key, type);
            return type;
        }
    }
}