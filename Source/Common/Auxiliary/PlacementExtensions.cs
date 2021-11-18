using Crystal.Plot2D.Charts;

namespace Crystal.Plot2D.Common
{
  internal static class PlacementExtensions
  {
    public static bool IsBottomOrTop(this AxisPlacement placement)
    {
      return placement == AxisPlacement.Bottom || placement == AxisPlacement.Top;
    }
  }
}
