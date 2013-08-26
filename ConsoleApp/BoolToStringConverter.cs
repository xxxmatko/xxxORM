using System;
using System.ComponentModel;
using System.Globalization;


namespace ConsoleApp
{
    /// <summary>
    /// Class for converting bool to string and back.
    /// </summary>
    public class BoolToStringConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(bool))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }


        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if(destinationType == typeof(string))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }


        // Overrides the ConvertFrom method of TypeConverter.
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is bool)
            {
                return (bool)value ? "YES" : "NO";
            }
            return base.ConvertFrom(context, culture, value);
        }


        // Overrides the ConvertTo method of TypeConverter.
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(bool))
            {
                return ((string)value).Equals("YES", StringComparison.InvariantCultureIgnoreCase);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
