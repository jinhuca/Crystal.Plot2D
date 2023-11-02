using Crystal.Plot2D.Converters;

namespace Crystal.Plot2D.LegendItems;

internal sealed class LegendBottomButtonIsEnabledConverter : ThreeValuesMultiConverter<double, double, double>
{
  protected override object ConvertCore(double value1, double value2, double value3)
  {
    var extentHeight = value1;
    var viewportHeight = value2;
    var offset = value3;

    return viewportHeight < extentHeight - offset;
  }
}
