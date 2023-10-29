using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D.Common.Auxiliary;

public static class IPointCollectionExtensions
{
  public static DataRect GetBounds(this IEnumerable<Point> points)
  {
    return BoundsHelper.GetViewportBounds(viewportPoints: points);
  }

  public static IEnumerable<Point> Skip(this IList<Point> points, int skipCount)
  {
    for (var i = skipCount; i < points.Count; i++)
    {
      yield return points[index: i];
    }
  }
}
