using System;
using System.ComponentModel;
using System.Reflection;

namespace Logg.Data
{
    public static class EnumUtility
    {
        //Enum stuff, allows for the use of the descriptions attribute for descriptive string value conversion
        public static string ToDescription<TEnum>(this TEnum EnumValue) where TEnum : struct
        {
            return GetEnumDescription((Enum)(object)(EnumValue));
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

    }
}
