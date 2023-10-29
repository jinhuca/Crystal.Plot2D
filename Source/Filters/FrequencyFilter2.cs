using System;
using System.Collections.Generic;
using System.Windows;
using Crystal.Plot2D.Charts;

namespace Crystal.Plot2D.Filters;

public class FrequencyFilter2 : PointsFilterBase
{
  private Rect screenRect;
  public override void SetScreenRect(Rect screenRect)
  {
    this.screenRect = screenRect;
  }

  public override List<Point> Filter(List<Point> points)
  {
    List<Point> result = new();

    using (var enumerator = points.GetEnumerator())
    {
      var currentX = double.NegativeInfinity;

      double minX = 0, maxX = 0, minY = 0, maxY = 0;

      Point left = new(), right = new(), top = new(), bottom = new();

      var isFirstPoint = true;
      while (enumerator.MoveNext())
      {
        var currPoint = enumerator.Current;
        var x = currPoint.X;
        var y = currPoint.Y;
        var xInt = Math.Floor(d: x);
        if (xInt == currentX)
        {
          if (x > maxX)
          {
            maxX = x;
            right = currPoint;
          }

          if (y > maxY)
          {
            maxY = y;
            top = currPoint;
          }
          else if (y < minY)
          {
            minY = y;
            bottom = currPoint;
          }
        }
        else
        {
          if (!isFirstPoint)
          {
            result.Add(item: left);

            var leftY = top.X < bottom.X ? top : bottom;
            var rightY = top.X > bottom.X ? top : bottom;

            if (top != bottom)
            {
              result.Add(item: leftY);
              result.Add(item: rightY);
            }
            else if (top != left)
            {
              result.Add(item: top);
            }

            if (right != rightY)
            {
              result.Add(item: right);
            }
          }

          currentX = xInt;
          left = right = top = bottom = currPoint;
          minX = maxX = x;
          minY = maxY = y;
        }

        isFirstPoint = false;
      }
    }

    return result;
  }
}
