using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Crystal.Plot2D.Common;

namespace Crystal.Plot2D.Navigation;

public class PhysicalNavigation : IPlotterElement
{
  private readonly DispatcherTimer timer;

  public PhysicalNavigation()
  {
    timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(value: 30) };
    timer.Tick += timer_Tick;
  }

  private DateTime startTime;
  private void timer_Tick(object sender, EventArgs e)
  {
    var time = DateTime.Now - startTime;
    animation.UseMouse = isMouseDown;
    if (!isMouseDown)
    {
      animation.LiquidFrictionQuadraticCoeff = 1;
    }

    plotter.Viewport.Visible = animation.GetValue(timeSpan: time);
    if (animation.IsFinished && !isMouseDown)
    {
      timer.Stop();
    }
  }

  private bool isMouseDown;
  private PhysicalRectAnimation animation;
  private Point clickPos;
  private void OnMouseDown(object sender, MouseButtonEventArgs e)
  {
    if (e.ChangedButton == MouseButton.Left)
    {
      clickPos = e.GetPosition(relativeTo: plotter.CentralGrid);

      isMouseDown = true;
      startTime = DateTime.Now;

      animation = new PhysicalRectAnimation(
        viewport: plotter.Viewport,
        initialMousePos: e.GetPosition(relativeTo: plotter.ViewportPanel));

      timer.Start();
    }
  }

  private void OnMouseUp(object sender, MouseButtonEventArgs e)
  {
    if (e.ChangedButton == MouseButton.Left)
    {
      isMouseDown = false;
      if (clickPos == e.GetPosition(relativeTo: plotter.CentralGrid))
      {
        timer.Stop();
        animation = null;
      }
      else
      {
        if (animation.IsFinished)
        {
          timer.Stop();
          animation = null;
        }
        else
        {
          animation.UseMouse = false;
          animation.LiquidFrictionQuadraticCoeff = 1;
        }
      }
    }
  }

  #region IPlotterElement Members

  private PlotterBase plotter;
  void IPlotterElement.OnPlotterAttached(PlotterBase plotter)
  {
    this.plotter = (PlotterBase)plotter;

    Mouse.AddPreviewMouseDownHandler(element: plotter.CentralGrid, handler: OnMouseDown);
    Mouse.AddPreviewMouseUpHandler(element: plotter.CentralGrid, handler: OnMouseUp);
    plotter.CentralGrid.MouseLeave += CentralGrid_MouseLeave;
  }

  private void CentralGrid_MouseLeave(object sender, MouseEventArgs e)
  {
    isMouseDown = false;
  }

  void IPlotterElement.OnPlotterDetaching(PlotterBase plotter)
  {
    plotter.CentralGrid.MouseLeave -= CentralGrid_MouseLeave;
    Mouse.RemovePreviewMouseDownHandler(element: plotter.CentralGrid, handler: OnMouseDown);
    Mouse.RemovePreviewMouseUpHandler(element: plotter.CentralGrid, handler: OnMouseUp);

    this.plotter = null;
  }

  PlotterBase IPlotterElement.Plotter => plotter;

  #endregion
}
