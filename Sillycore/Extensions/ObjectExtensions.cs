using Newtonsoft.Json;

namespace Sillycore.Extensions
{
    public static class ObjectExtensions
    {
        public static bool HasProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }

        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, SillycoreApp.JsonSerializerSettings);
        }
    }
}