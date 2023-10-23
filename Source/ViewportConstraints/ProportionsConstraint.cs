using System;
using System.Windows;

namespace Crystal.Plot2D;

public sealed class ProportionsConstraint : ViewportConstraint
{
  private double widthToHeightRatio = 1;
  public double WidthToHeightRatio
  {
    get => widthToHeightRatio;
    set
    {
      if (widthToHeightRatio != value)
      {
        widthToHeightRatio = value;
        RaiseChanged();
      }
    }
  }

  public override DataRect Apply(DataRect oldDataRect, DataRect newDataRect, Viewport2D viewport)
  {
    double ratio = newDataRect.Width / newDataRect.Height;
    double coeff = Math.Sqrt(d: ratio);

    double newWidth = newDataRect.Width / coeff;
    double newHeight = newDataRect.Height * coeff;

    Point center = newDataRect.GetCenter();
    DataRect res = DataRect.FromCenterSize(center: center, width: newWidth, height: newHeight);
    return res;
  }
}
