using System.Windows;

namespace Crystal.Plot2D.Common;

internal static class SizeHelper
{
  public static Size CreateInfiniteSize()
  {
    return new Size(double.PositiveInfinity, double.PositiveInfinity);
  }
}
