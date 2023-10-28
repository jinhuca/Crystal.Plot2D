using System.Windows;

namespace Crystal.Plot2D.Charts;

/// <summary>
/// Paints vertical filled and outlined range in viewport coordinates.
/// </summary>
public sealed class VerticalRange : RangeHighlight
{
  protected override void UpdateUIRepresentationCore()
  {
    var transform = Plotter.Viewport.Transform;
    var visible = Plotter.Viewport.Visible;

    var p1Top = new Point(x: StartValue, y: visible.YMin).DataToScreen(transform: transform);
    var p1Bottom = new Point(x: StartValue, y: visible.YMax).DataToScreen(transform: transform);
    var p2Top = new Point(x: EndValue, y: visible.YMin).DataToScreen(transform: transform);
    var p2Bottom = new Point(x: EndValue, y: visible.YMax).DataToScreen(transform: transform);

    LineGeometry1.StartPoint = p1Top;
    LineGeometry1.EndPoint = p1Bottom;

    LineGeometry2.StartPoint = p2Top;
    LineGeometry2.EndPoint = p2Bottom;

    RectGeometry.Rect = new Rect(point1: p1Top, point2: p2Bottom);
  }
}
