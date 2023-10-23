using System;
using System.Windows;
using System.Windows.Data;

namespace Crystal.Plot2D;

public abstract class FourValuesMultiConverter<T1, T2, T3, T4> : IMultiValueConverter
{
  #region IMultiValueConverter Members

  public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
  {
    if (values != null && values.Length == 4)
    {
      if (values[0] is T1 && values[1] is T2 && values[2] is T3 && values[3] is T4)
      {
        T1 param1 = (T1)values[0];
        T2 param2 = (T2)values[1];
        T3 param3 = (T3)values[2];
        T4 param4 = (T4)values[3];

        var result = ConvertCore(value1: param1, value2: param2, value3: param3, value4: param4, targetType: targetType, parameter: parameter, culture: culture);
        return result;
      }
    }
    return DependencyProperty.UnsetValue;
  }

  protected abstract object ConvertCore(T1 value1, T2 value2, T3 value3, T4 value4, Type targetType, object parameter, System.Globalization.CultureInfo culture);

  public virtual object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) => throw new NotSupportedException();

  #endregion
}
