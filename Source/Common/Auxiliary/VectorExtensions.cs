using System.Windows;
using System.Windows.Media.Media3D;

namespace Crystal.Plot2D.Common
{
  internal static class VectorExtensions
  {
    public static Point ToPoint(this Vector vector)
    {
      return new Point(vector.X, vector.Y);
    }

    public static Vector DecreaseLength(this Vector vector, double width, double heigth)
    {
      vector.X /= width;
      vector.Y /= heigth;

      return vector;
    }

    public static Vector Perpendicular(this Vector v)
    {
      var result = Vector3D.CrossProduct(new Vector3D(v.X, v.Y, 0), new Vector3D(0, 0, 1));
      return new Vector(result.X, result.Y);
    }
  }
}
