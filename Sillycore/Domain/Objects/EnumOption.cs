using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Sillycore.Domain.Objects
{
    public class EnumOption
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }

        public static List<EnumOption> GetOptions<T>()
        {
            List<T> enumValues = Enum.GetValues(typeof(T)).Cast<T>().ToList();

            List<EnumOption> options = new List<EnumOption>();

            Type enumType = typeof(T);

            foreach (T enumValue in enumValues)
            {
                MemberInfo[] member = enumType.GetMember(enumValue.ToString());

                DisplayAttribute displayAttribute = (DisplayAttribute)member[0].GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();
                DescriptionAttribute descriptionAttribute = (DescriptionAttribute)member[0].GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault();

                EnumOption option = new EnumOption()
                {
                    Id = Convert.ToInt32(enumValue),
                    Name = Char.ToLowerInvariant(enumValue.ToString()[0]) + enumValue.ToString().Substring(1)
                };

                if (displayAttribute != null)
                {
                    option.DisplayName = displayAttribute.Name;
                }

                if (descriptionAttribute != null)
                {
                    option.Description = descriptionAttribute.Description;
                }

                options.Add(option);
            }

            return options;
        }
    }
}