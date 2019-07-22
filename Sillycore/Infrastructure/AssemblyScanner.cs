using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sillycore.Extensions;

namespace Sillycore.Infrastructure
{
    public class AssemblyScanner
    {
        private static readonly Dictionary<string, Assembly> Assemblies;

        static AssemblyScanner()
        {
            Assemblies = new Dictionary<string, Assembly>();

            Assembly entryAssembly = Assembly.GetEntryAssembly();
            LoadAssembly(entryAssembly);
        }

        public static List<Assembly> GetAssemblies()
        {
            return Assemblies.Select(p => p.Value).ToList();
        }

        public static List<Type> GetTypes()
        {
            return GetAssemblies().SelectMany(a => a.GetTypes().ToList()).ToList();
        }

        public static List<Type> GetAllTypesOfInterface<T>()
        {
            return GetTypes().Where(t => t.Implements(typeof(T)) && t != typeof(T)).ToList();
        }

        private static void LoadAssembly(Assembly assembly)
        {
            Assemblies.Add(assembly.FullName, assembly);

            AssemblyName[] assemblyNames = assembly.GetReferencedAssemblies();

            if (assemblyNames.HasElements())
            {
                foreach (AssemblyName assemblyName in assemblyNames)
                {
                    try
                    {
                        Assembly referencedAssembly = Assembly.Load(assemblyName);

                        if (!Assemblies.ContainsKey(referencedAssembly.FullName))
                        {
                            LoadAssembly(referencedAssembly);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                        Console.WriteLine($"There was a problem while loading {assemblyName}");
                    }
                }
            }
        }
    }
}