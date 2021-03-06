using System;
using System.ComponentModel;
using System.Globalization;

namespace Crystal.Plot2D.Common
{
  public sealed class DataRectConverter : TypeConverter
  {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
      => (sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType);

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
      => (destinationType == typeof(string)) || base.CanConvertTo(context, destinationType);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
      if (value == null)
      {
        throw GetConvertFromException(value);
      }

      if (value is string source)
      {
        return DataRect.Parse(source);
      }

      return base.ConvertFrom(context, culture, value);
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
      if (destinationType != null && value is DataRect)
      {
        DataRect rect = (DataRect)value;
        if (destinationType == typeof(string))
        {
          return rect.ConvertToString(null, culture);
        }
      }
      return base.ConvertTo(context, culture, value, destinationType);
    }
  }
}
