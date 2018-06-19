using System;

namespace Sillycore.Configuration
{
    public class ConfigurationAttribute : Attribute
    {
        public string Section { get; }

        public ConfigurationAttribute(string section)
        {
            Section = section;
        }
    }
}