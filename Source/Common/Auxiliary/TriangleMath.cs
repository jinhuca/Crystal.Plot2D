using System;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Crystal.Plot2D.Common;

public static class TriangleMath
{
  public static bool TriangleContains(Point a, Point b, Point c, Point m)
  {
    double a0 = a.X - c.X;
    double a1 = b.X - c.X;
    double a2 = a.Y - c.Y;
    double a3 = b.Y - c.Y;

    if (AreClose(x: a0 * a3, y: a1 * a2))
    {
      // determinant is too close to zero => apexes are on one line
      Vector ab = a - b;
      Vector ac = a - c;
      Vector bc = b - c;
      Vector ax = a - m;
      Vector bx = b - m;
      bool res = AreClose(x: ab.X * ax.Y, y: ab.Y * ax.X) && !AreClose(x: ab.LengthSquared, y: 0) ||
        AreClose(x: ac.X * ax.Y, y: ac.Y * ax.X) && !AreClose(x: ac.LengthSquared, y: 0) ||
        AreClose(x: bc.X * bx.Y, y: bc.Y * bx.X) && !AreClose(x: bc.LengthSquared, y: 0);
      return res;
    }
    else
    {
      double b1 = m.X - c.X;
      double b2 = m.Y - c.Y;

      // alpha, beta and gamma - are baricentric coordinates of v 
      // in triangle with apexes a, b and c
      double beta = (b2 / a2 * a0 - b1) / (a3 / a2 * a0 - a1);
      double alpha = (b1 - a1 * beta) / a0;
      double gamma = 1 - beta - alpha;
      return alpha >= 0 && beta >= 0 && gamma >= 0;
    }
  }

  private const double eps = 0.00001;
  private static bool AreClose(double x, double y)
  {
    return Math.Abs(value: x - y) < eps;
  }

  public static Vector3D GetBaricentricCoordinates(Point a, Point b, Point c, Point m)
  {
    double Sac = GetSquare(a: a, b: c, c: m);
    double Sbc = GetSquare(a: b, b: c, c: m);
    double Sab = GetSquare(a: a, b: b, c: m);

    double sum = (Sab + Sac + Sbc) / 3;

    return new Vector3D(x: Sbc / sum, y: Sac / sum, z: Sab / sum);
  }

  public static double GetSquare(Point a, Point b, Point c)
  {
    double ab = (a - b).Length;
    double ac = (a - c).Length;
    double bc = (b - c).Length;

    double p = 0.5 * (ab + ac + bc); // half of perimeter
    return Math.Sqrt(d: p * (p - ab) * (p - ac) * (p - bc));
  }
}
