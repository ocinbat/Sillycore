using System;
using Newtonsoft.Json;

namespace Sillycore.Extensions
{
    public static class GenericExtensions
    {
        public static bool IsDefault<T>(this T value) where T : struct
        {
            bool isDefault = value.Equals(default(T));

            return isDefault;
        }

        public static T Clone<T>(this T source)
        {
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(source.ToJson(), SillycoreApp.JsonSerializerSettings);
        }
    }
}