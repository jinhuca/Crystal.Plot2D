﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Crystal.Plot2D;

public class GenericValueConverter<T> : IValueConverter
{
  public GenericValueConverter() { }

  public Func<T, object> Conversion { get; set; }

  public GenericValueConverter(Func<T, object> conversion)
  {
    Conversion = conversion ?? throw new ArgumentNullException(paramName: "conversion");
  }

  #region IValueConverter Members

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is T)
    {
      T genericValue = (T)value;
      object result = ConvertCore(value: genericValue, targetType: targetType, parameter: parameter, culture: culture);
      return result;
    }
    return null;
  }

  public virtual object ConvertCore(T value, Type targetType, object parameter, CultureInfo culture)
  {
    if (Conversion != null)
    {
      return Conversion(arg: value);
    }

    throw new NotImplementedException();
  }

  public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    throw new NotSupportedException();
  }

  #endregion
}
