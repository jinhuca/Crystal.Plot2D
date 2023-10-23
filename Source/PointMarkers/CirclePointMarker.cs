﻿using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D;

/// <summary>
/// Class that Renders circle around each point of graph.
/// </summary>
public class CirclePointMarker : ShapePointMarker
{
  public override void Render(DrawingContext dc, Point screenPoint)
  {
    dc.DrawEllipse(brush: FillBrush, pen: OutlinePen, center: screenPoint, radiusX: Diameter / 2, radiusY: Diameter / 2);
  }
}

