using Crystal.Plot2D.Converters;

namespace Crystal.Plot2D.LegendItems;

internal sealed class LegendTopButtonToIsEnabledConverter : GenericValueConverter<double>
{
  protected override object ConvertCore(double value)
  {
    var verticalOffset = value;
    return verticalOffset > 0;
  }
}
