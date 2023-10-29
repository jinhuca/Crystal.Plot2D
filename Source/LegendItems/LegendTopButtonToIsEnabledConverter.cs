using System;
using Crystal.Plot2D.Converters;

namespace Crystal.Plot2D.LegendItems;

internal sealed class LegendTopButtonToIsEnabledConverter : GenericValueConverter<double>
{
  public override object ConvertCore(double value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
  {
    var verticalOffset = value;
    return verticalOffset > 0;
  }
}
