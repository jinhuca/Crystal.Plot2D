using System;
using System.Windows;

namespace Crystal.Plot2D.Common
{
  internal static class RandomExtensions
  {
    public static Point NextPoint(this Random rnd) => new(rnd.NextDouble(), rnd.NextDouble());

    public static Point NextPoint(this Random rnd, double xMin, double xMax, double yMin, double yMax)
    {
      double x = rnd.NextDouble() * (xMax - xMin) + xMin;
      double y = rnd.NextDouble() * (yMax - yMin) + yMin;
      return new Point(x, y);
    }

    public static Vector NextVector(this Random rnd) => new(rnd.NextDouble(), rnd.NextDouble());
  }
}
