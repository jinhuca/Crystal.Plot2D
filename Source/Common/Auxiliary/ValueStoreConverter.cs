using System;
using System.Globalization;
using System.Windows.Data;

namespace Crystal.Plot2D
{
  public sealed class ValueStoreConverter : IValueConverter
  {

    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      ValueStore store = (ValueStore)value;
      string key = (string)parameter;

      return store[key];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }

    #endregion
  }
}
