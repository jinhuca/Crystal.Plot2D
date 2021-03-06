using Crystal.Plot2D.Charts;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D
{
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
        double currentX = double.NegativeInfinity;

        double minX = 0, maxX = 0, minY = 0, maxY = 0;

        Point left = new(), right = new(), top = new(), bottom = new();

        bool isFirstPoint = true;
        while (enumerator.MoveNext())
        {
          Point currPoint = enumerator.Current;
          double x = currPoint.X;
          double y = currPoint.Y;
          double xInt = Math.Floor(x);
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
              result.Add(left);

              Point leftY = top.X < bottom.X ? top : bottom;
              Point rightY = top.X > bottom.X ? top : bottom;

              if (top != bottom)
              {
                result.Add(leftY);
                result.Add(rightY);
              }
              else if (top != left)
              {
                result.Add(top);
              }

              if (right != rightY)
              {
                result.Add(right);
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
}
