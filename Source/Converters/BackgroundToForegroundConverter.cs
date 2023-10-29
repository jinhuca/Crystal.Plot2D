using System;
using System.Globalization;
using System.Windows.Media;

namespace Crystal.Plot2D.Converters;

public class BackgroundToForegroundConverter : GenericValueConverter<SolidColorBrush>
{
  public override object ConvertCore(SolidColorBrush value, Type targetType, object parameter, CultureInfo culture)
  {
    var back = value;
    var diff = back.Color - Colors.Black;
    var summ = diff.R + diff.G + diff.B;
    var border = 3 * 255 / 2;
    return summ > border ? Brushes.Black : Brushes.White;
  }
}
