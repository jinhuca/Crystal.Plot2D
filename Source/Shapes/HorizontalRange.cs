using System.Windows;

namespace Crystal.Plot2D.Charts;

/// <summary>
/// Paints horizontal filled and outlined range in viewport coordinates.
/// </summary>
public sealed class HorizontalRange : RangeHighlight
{
  protected override void UpdateUIRepresentationCore()
  {
    var transform = Plotter.Viewport.Transform;
    DataRect visible = Plotter.Viewport.Visible;

    Point p1_left = new Point(x: visible.XMin, y: Value1).DataToScreen(transform: transform);
    Point p1_right = new Point(x: visible.XMax, y: Value1).DataToScreen(transform: transform);
    Point p2_left = new Point(x: visible.XMin, y: Value2).DataToScreen(transform: transform);
    Point p2_right = new Point(x: visible.XMax, y: Value2).DataToScreen(transform: transform);

    LineGeometry1.StartPoint = p1_left;
    LineGeometry1.EndPoint = p1_right;

    LineGeometry2.StartPoint = p2_left;
    LineGeometry2.EndPoint = p2_right;

    RectGeometry.Rect = new Rect(point1: p1_left, point2: p2_right);
  }
}
