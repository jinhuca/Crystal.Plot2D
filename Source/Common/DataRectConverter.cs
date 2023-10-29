using System;
using System.ComponentModel;
using System.Globalization;

namespace Crystal.Plot2D.Common;

public sealed class DataRectConverter : TypeConverter
{
  public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    => sourceType == typeof(string) || base.CanConvertFrom(context: context, sourceType: sourceType);

  public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    => destinationType == typeof(string) || base.CanConvertTo(context: context, destinationType: destinationType);

  public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
  {
    if (value == null)
    {
      throw GetConvertFromException(value: value);
    }

    if (value is string source)
    {
      return DataRect.Parse(source: source);
    }

    return base.ConvertFrom(context: context, culture: culture, value: value);
  }

  public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
  {
    if (destinationType != null && value is DataRect)
    {
      var rect = (DataRect)value;
      if (destinationType == typeof(string))
      {
        return rect.ConvertToString(format: null, provider: culture);
      }
    }
    return base.ConvertTo(context: context, culture: culture, value: value, destinationType: destinationType);
  }
}
