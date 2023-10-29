using System.Collections.Generic;
using System.Windows;
using Crystal.Plot2D.Charts;

namespace Crystal.Plot2D.Filters;

public sealed class EmptyFilter : PointsFilterBase
{
  public override List<Point> Filter(List<Point> points)
  {
    return points;
  }
}
