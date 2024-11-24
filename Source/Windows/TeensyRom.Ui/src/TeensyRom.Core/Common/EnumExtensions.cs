using System.ComponentModel;
using System.Reflection;

namespace TeensyRom.Core.Common
{
    public static class EnumExtensions
    {
        public static string ToDescription<TEnum>(this TEnum value) where TEnum : Enum
        {
            var field = value.GetType().GetField(value.ToString());

            if (field == null)
            {
                return value.ToString();
            }
            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute?.Description ?? value.ToString();
        }

        public static TEnum ToEnum<TEnum>(this string description) where TEnum : struct, Enum
        {
            foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null && attribute.Description == description)
                {
                    return (TEnum)field.GetValue(null)!;
                }
            }
            if (Enum.TryParse<TEnum>(description, out var result))
            {
                return result;
            }
            throw new ArgumentException($"No matching enum value found for description '{description}'.", nameof(description));
        }
    }
}
