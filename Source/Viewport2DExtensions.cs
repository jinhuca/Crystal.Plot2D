using Crystal.Plot2D.Common;
using System.Windows;

namespace Crystal.Plot2D
{
  public static class Viewport2DExtensions
  {
    public static void Zoom(this Viewport2D viewport, double factor)
    {
      DataRect visible = viewport.Visible;
      DataRect oldVisible = visible;
      Point center = visible.GetCenter();
      Vector halfSize = new(visible.Width * factor / 2, visible.Height * factor / 2);
      viewport.Visible = new DataRect(center - halfSize, center + halfSize);
      viewport.Plotter.UndoProvider.AddAction(new DependencyPropertyChangedUndoAction(viewport, Viewport2D.VisibleProperty, oldVisible, visible));
    }
  }
}
