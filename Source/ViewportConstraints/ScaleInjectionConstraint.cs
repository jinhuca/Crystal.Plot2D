using System;
using Crystal.Plot2D.Common;

namespace Crystal.Plot2D.ViewportConstraints;

public class ScaleInjectionConstraint : ViewportConstraint
{
  private readonly Viewport2D parentViewport;
  public ScaleInjectionConstraint(Viewport2D parentViewport)
  {
    this.parentViewport = parentViewport ?? throw new ArgumentNullException(paramName: nameof(parentViewport));
    parentViewport.PropertyChanged += parentViewport_PropertyChanged;
  }

  private void parentViewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
  {
    if (e.PropertyName == "Visible")
    {
      RaiseChanged();
    }
  }

  public void SetHorizontalTransform(double parentMin, double childMin, double parentMax, double childMax)
  {
    xScale = (childMax - childMin) / (parentMax - parentMin);
    xShift = childMin - parentMin;
  }

  public void SetVerticalTransform(double parentMin, double childMin, double parentMax, double childMax)
  {
    yScale = (childMax - childMin) / (parentMax - parentMin);
    yShift = childMin - parentMin;
  }

  private double xShift;
  private double xScale = 1;
  private double yShift;
  private double yScale = 1;

  public override DataRect Apply(DataRect previousDataRect, DataRect proposedDataRect, Viewport2D viewport)
  {
    var parentVisible = parentViewport.Visible;

    var xmin = parentVisible.XMin * xScale + xShift;
    var xmax = parentVisible.XMax * xScale + xShift;
    var ymin = parentVisible.YMin * yScale + yShift;
    var ymax = parentVisible.YMax * yScale + yShift;

    return DataRect.Create(xMin: xmin, yMin: ymin, xMax: xmax, yMax: ymax);
  }
}
