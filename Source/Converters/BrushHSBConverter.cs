using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Crystal.Plot2D.Common.Auxiliary;

namespace Crystal.Plot2D.Converters;

public sealed class BrushHSBConverter : IValueConverter
{
  public double LightnessDelta { get; } = 1.0;
  public double SaturationDelta { get; } = 1.0;

  #region IValueConverter Members

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is SolidColorBrush brush)
    {
      var result = brush.ChangeLightness(lightnessFactor: LightnessDelta).ChangeSaturation(saturationFactor: SaturationDelta);
      return result;
    }

    return value;
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    => throw new NotImplementedException();

  #endregion
}
