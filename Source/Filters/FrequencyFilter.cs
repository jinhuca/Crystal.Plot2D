using Crystal.Plot2D.Charts;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D;

public sealed class FrequencyFilter : PointsFilterBase
{

  /// <summary>
  ///   Visible region in screen coordinates.
  /// </summary>
  private Rect screenRect;

  #region IPointFilter Members

  public override void SetScreenRect(Rect screenRect)
  {
    this.screenRect = screenRect;
  }

  // todo probably use LINQ here.
  public override List<Point> Filter(List<Point> points)
  {
    if (points.Count == 0)
    {
      return points;
    }

    List<Point> resultPoints = points;
    List<Point> currentChain = new();

    if (points.Count > 2 * screenRect.Width)
    {
      resultPoints = new List<Point>();

      double currentX = Math.Floor(d: points[index: 0].X);
      foreach (Point p in points)
      {
        if (Math.Floor(d: p.X) == currentX)
        {
          currentChain.Add(item: p);
        }
        else
        {
          // Analyse current chain
          if (currentChain.Count <= 2)
          {
            resultPoints.AddRange(collection: currentChain);
          }
          else
          {
            Point first = MinByX(points: currentChain);
            Point last = MaxByX(points: currentChain);
            Point min = MinByY(points: currentChain);
            Point max = MaxByY(points: currentChain);
            resultPoints.Add(item: first);

            Point smaller = min.X < max.X ? min : max;
            Point greater = min.X > max.X ? min : max;
            if (smaller != resultPoints.GetLast())
            {
              resultPoints.Add(item: smaller);
            }
            if (greater != resultPoints.GetLast())
            {
              resultPoints.Add(item: greater);
            }
            if (last != resultPoints.GetLast())
            {
              resultPoints.Add(item: last);
            }
          }
          currentChain.Clear();
          currentChain.Add(item: p);
          currentX = Math.Floor(d: p.X);
        }
      }
    }

    resultPoints.AddRange(collection: currentChain);

    return resultPoints;
  }

  #endregion

  private static Point MinByX(IList<Point> points)
  {
    Point minPoint = points[index: 0];
    foreach (Point p in points)
    {
      if (p.X < minPoint.X)
      {
        minPoint = p;
      }
    }
    return minPoint;
  }

  private static Point MaxByX(IList<Point> points)
  {
    Point maxPoint = points[index: 0];
    foreach (Point p in points)
    {
      if (p.X > maxPoint.X)
      {
        maxPoint = p;
      }
    }
    return maxPoint;
  }

  private static Point MinByY(IList<Point> points)
  {
    Point minPoint = points[index: 0];
    foreach (Point p in points)
    {
      if (p.Y < minPoint.Y)
      {
        minPoint = p;
      }
    }
    return minPoint;
  }

  private static Point MaxByY(IList<Point> points)
  {
    Point maxPoint = points[index: 0];
    foreach (Point p in points)
    {
      if (p.Y > maxPoint.Y)
      {
        maxPoint = p;
      }
    }
    return maxPoint;
  }
}
