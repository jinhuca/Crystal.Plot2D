using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D;

/// <summary>
/// Class that renders triangular marker at every point of graph.
/// </summary>
public class TrianglePointMarker : ShapePointMarker
{
  public override void Render(DrawingContext dc, Point screenPoint)
  {
    Point pt0 = Point.Add(point: screenPoint, vector: new Vector(x: -Diameter / 2, y: -Diameter / 2));
    Point pt1 = Point.Add(point: screenPoint, vector: new Vector(x: 0, y: Diameter / 2));
    Point pt2 = Point.Add(point: screenPoint, vector: new Vector(x: Diameter / 2, y: -Diameter / 2));

    var streamGeom = new StreamGeometry();
    using var context = streamGeom.Open();
    context.BeginFigure(startPoint: pt0, isFilled: true, isClosed: true);
    context.LineTo(point: pt1, isStroked: true, isSmoothJoin: true);
    context.LineTo(point: pt2, isStroked: true, isSmoothJoin: true);
    dc.DrawGeometry(brush: FillBrush, pen: OutlinePen, geometry: streamGeom);
  }
}
