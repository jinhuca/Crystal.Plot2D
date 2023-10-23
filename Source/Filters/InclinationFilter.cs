using Crystal.Plot2D.Charts;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D;

[Obsolete(message: "Works incorrectly", error: true)]
public sealed class InclinationFilter : PointsFilterBase
{
  private double criticalAngle = 179;
  public double CriticalAngle
  {
    get => criticalAngle;
    set
    {
      if (criticalAngle != value)
      {
        criticalAngle = value;
        RaiseChanged();
      }
    }
  }

  #region IPointFilter Members

  public override List<Point> Filter(List<Point> points)
  {
    if (points.Count == 0)
    {
      return points;
    }

    List<Point> res = new() { points[index: 0] };

    int i = 1;
    while (i < points.Count)
    {
      bool added = false;
      int j = i;
      while (!added && (j < points.Count - 1))
      {
        Point x1 = res[index: res.Count - 1];
        Point x2 = points[index: j];
        Point x3 = points[index: j + 1];

        double a = (x1 - x2).Length;
        double b = (x2 - x3).Length;
        double c = (x1 - x3).Length;

        double angle13 = Math.Acos(d: (a * a + b * b - c * c) / (2 * a * b));
        double degrees = 180 / Math.PI * angle13;
        if (degrees < criticalAngle)
        {
          res.Add(item: x2);
          added = true;
          i = j + 1;
        }
        else
        {
          j++;
        }
      }
      // reached the end of resultPoints
      if (!added)
      {
        res.Add(item: points.GetLast());
        break;
      }
    }
    return res;
  }

  #endregion
}
