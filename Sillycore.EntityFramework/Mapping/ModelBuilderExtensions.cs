using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;



namespace Sillycore.EntityFramework.Mapping
{
    /// <summary>
    /// Entity configuration extension from 
    /// </summary>
    /// <seealso cref="https://github.com/aspnet/EntityFrameworkCore/issues/2805"/>
    public static class ModelBuilderExtenions
    {
        private static IEnumerable<Type> GetMappingTypes(this Assembly assembly, Type mappingInterface)
        {
            return assembly
                .GetTypes()
                .Where(x =>
                    !x.GetTypeInfo().IsAbstract &&
                    x.GetInterfaces().Any(y => y.GetTypeInfo().IsGenericType && y.GetGenericTypeDefinition() == mappingInterface));
        }

        /// <seealso cref="https://github.com/aspnet/EntityFrameworkCore/issues/2805"/>
        public static void AddEntityConfigurationsFromAssembly(this ModelBuilder modelBuilder, Assembly assembly, Type dbContextType)
        {
            IEnumerable<Type> mappingTypes = assembly.GetMappingTypes(typeof(IEntityMappingConfiguration<>));

            foreach (IEntityMappingConfiguration config in mappingTypes.Select(Activator.CreateInstance).Cast<IEntityMappingConfiguration>())
            {
                DataContextAttribute dataContextAttribute = config.GetType().GetTypeInfo().GetCustomAttribute<DataContextAttribute>();
                if (dataContextAttribute == null || dataContextAttribute.Type == dbContextType)
                {
                    dynamic configurationInstance = Activator.CreateInstance(config.GetType());
                    config.Map(configurationInstance);
                }
            }
        }
    }
}
