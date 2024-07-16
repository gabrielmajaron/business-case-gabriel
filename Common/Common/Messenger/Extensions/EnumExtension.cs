using System;
using System.ComponentModel;

namespace Messenger.Extensions
{
    public static class EnumExtension
    {
        public static string GetDescription(this Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            
            if (field != null && Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                return attribute.Description;

            throw new ArgumentException("Could not get enum description.", nameof(enumValue));
        }
    }
}