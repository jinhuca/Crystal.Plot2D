using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D.Charts;

public sealed class EmptyFilter : PointsFilterBase
{
  public override List<Point> Filter(List<Point> points)
  {
    return points;
  }
}
