using Crystal.Plot2D.Charts;
using System;
using System.ComponentModel;
using System.Globalization;

namespace Crystal.Plot2D.Common;

public sealed class RangeConverter : TypeConverter
{
  public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
  {
    return (sourceType == typeof(string)) || base.CanConvertFrom(context: context, sourceType: sourceType);
  }

  public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
  {
    return (destinationType == typeof(string)) || base.CanConvertTo(context: context, destinationType: destinationType);
  }

  public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
  {
    if (value == null)
    {
      throw GetConvertFromException(value: value);
    }

    if (value is string source)
    {
      var parts = source.Split(separator: '-');
      var minStr = parts[0];
      var maxStr = parts[1];

      int minInt32 = 0;
      double minDouble = 0;
      DateTime minDateTime = DateTime.Now;
      if (int.TryParse(s: minStr, style: NumberStyles.Integer, provider: culture, result: out minInt32))
      {
        int maxInt32 = int.Parse(s: maxStr, style: NumberStyles.Integer, provider: culture);

        return new Range<int>(min: minInt32, max: maxInt32);
      }
      else if (double.TryParse(s: minStr, style: NumberStyles.Float, provider: culture, result: out minDouble))
      {
        double maxDouble = double.Parse(s: maxStr, style: NumberStyles.Float, provider: culture);
        return new Range<double>(min: minDouble, max: maxDouble);
      }
      else if (DateTime.TryParse(s: minStr, provider: culture, styles: DateTimeStyles.None, result: out minDateTime))
      {
        DateTime maxDateTime = DateTime.Parse(s: maxStr, provider: culture);
        return new Range<DateTime>(min: minDateTime, max: maxDateTime);
      }
    }

    return base.ConvertFrom(context: context, culture: culture, value: value);
  }

  public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
  {
    if (destinationType != null && value is DataRect)
    {
      DataRect rect = (DataRect)value;
      if (destinationType == typeof(string))
      {
        return rect.ConvertToString(format: null, provider: culture);
      }
    }
    return base.ConvertTo(context: context, culture: culture, value: value, destinationType: destinationType);
  }
}
