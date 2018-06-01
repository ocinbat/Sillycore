using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Sillycore.Extensions
{
    public static class EnumExtensions
    {
        public static int ToInt(this Enum e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            int value = Convert.ToInt32(e);
            return value;
        }

        public static string GetDisplayName(this Enum item)
        {
            Type type = item.GetType();
            MemberInfo[] member = type.GetMember(item.ToString());
            DisplayAttribute displayAttribute = (DisplayAttribute)member[0].GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();

            if (displayAttribute != null)
            {
                return displayAttribute.Name;
            }

            return item.ToString();
        }
    }
}