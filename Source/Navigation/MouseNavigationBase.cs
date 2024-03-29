﻿using System.Windows.Input;
using Crystal.Plot2D.Common;

namespace Crystal.Plot2D.Navigation;

// todo: check how work happens when the mouse is not over the plotter, but over its child
// todo: if everything is OK, then transfer all mouse navigation controls to this class as a base one
public abstract class MouseNavigationBase : NavigationBase
{
  protected override void OnPlotterAttached(PlotterBase plotter)
  {
    base.OnPlotterAttached(plotter: plotter);

    Mouse.AddMouseDownHandler(element: Parent, handler: OnMouseDown);
    Mouse.AddMouseMoveHandler(element: Parent, handler: OnMouseMove);
    Mouse.AddMouseUpHandler(element: Parent, handler: OnMouseUp);
    Mouse.AddMouseWheelHandler(element: Parent, handler: OnMouseWheel);
  }

  protected override void OnPlotterDetaching(PlotterBase plotter)
  {
    Mouse.RemoveMouseDownHandler(element: Parent, handler: OnMouseDown);
    Mouse.RemoveMouseMoveHandler(element: Parent, handler: OnMouseMove);
    Mouse.RemoveMouseUpHandler(element: Parent, handler: OnMouseUp);
    Mouse.RemoveMouseWheelHandler(element: Parent, handler: OnMouseWheel);

    base.OnPlotterDetaching(plotter: plotter);
  }

  private void OnMouseWheel(object sender, MouseWheelEventArgs e)
  {
    OnPlotterMouseWheel();
  }

  protected virtual void OnPlotterMouseWheel()
  {
  }

  private void OnMouseUp(object sender, MouseButtonEventArgs e)
  {
    OnPlotterMouseUp();
  }

  protected virtual void OnPlotterMouseUp()
  {
  }

  private void OnMouseDown(object sender, MouseButtonEventArgs e)
  {
    OnPlotterMouseDown();
  }

  protected virtual void OnPlotterMouseDown()
  {
  }

  private void OnMouseMove(object sender, MouseEventArgs e)
  {
    OnPlotterMouseMove();
  }

  protected virtual void OnPlotterMouseMove()
  {
  }
}
