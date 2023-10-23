using Crystal.Plot2D.Common;
using System.Windows;

namespace Crystal.Plot2D;

public static class Viewport2DExtensions
{
  public static void Zoom(this Viewport2D viewport, double factor)
  {
    DataRect visible = viewport.Visible;
    DataRect oldVisible = visible;
    Point center = visible.GetCenter();
    Vector halfSize = new(x: visible.Width * factor / 2, y: visible.Height * factor / 2);
    viewport.Visible = new DataRect(point1: center - halfSize, point2: center + halfSize);
    viewport.Plotter.UndoProvider.AddAction(action: new DependencyPropertyChangedUndoAction(target: viewport, property: Viewport2D.VisibleProperty, oldValue: oldVisible, newValue: visible));
  }
}
