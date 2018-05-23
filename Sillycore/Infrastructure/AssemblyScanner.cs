using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sillycore.Extensions;

namespace Sillycore.Infrastructure
{
    public class AssemblyScanner
    {
        private static readonly List<Assembly> Assemblies;

        static AssemblyScanner()
        {
            Assemblies = new List<Assembly>();

            Assembly entryAssembly = Assembly.GetEntryAssembly();

            Assemblies.Add(entryAssembly);
            Assemblies.AddRange(entryAssembly.GetReferencedAssemblies().Select(Assembly.Load));
        }

        public static List<Assembly> GetAssemblies()
        {
            return Assemblies;
        }

        public static List<Type> GetTypes()
        {
            return Assemblies.SelectMany(a => a.GetTypes().ToList()).ToList();
        }

        public static List<Type> GetAllTypesOfInterface<T>()
        {
            return GetTypes().Where(t => t.Implements(typeof(T)) && t != typeof(T)).ToList();
        }
    }
}