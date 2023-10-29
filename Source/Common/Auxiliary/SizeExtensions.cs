using System;
using System.Windows;

namespace Crystal.Plot2D.Common.Auxiliary;

internal static class SizeExtensions
{
  private const double sizeRatio = 1e-7;
  public static bool EqualsApproximately(this Size size1, Size size2)
  {
    var widthEquals = Math.Abs(value: size1.Width - size2.Width) / size1.Width < sizeRatio;
    var heightEquals = Math.Abs(value: size1.Height - size2.Height) / size1.Height < sizeRatio;

    return widthEquals && heightEquals;
  }
}
