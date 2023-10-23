using System.Windows.Input;

namespace Crystal.Plot2D;

// todo проверить, как происходит работа когда мышь не над плоттером, а над его ребенком
// todo если все ОК, то перевести все маус навигейшн контролы на этот класс как базовый
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
    OnPlotterMouseWheel(e: e);
  }

  protected virtual void OnPlotterMouseWheel(MouseWheelEventArgs e) { }

  private void OnMouseUp(object sender, MouseButtonEventArgs e)
  {
    OnPlotterMouseUp(e: e);
  }

  protected virtual void OnPlotterMouseUp(MouseButtonEventArgs e) { }

  private void OnMouseDown(object sender, MouseButtonEventArgs e)
  {
    OnPlotterMouseDown(e: e);
  }

  protected virtual void OnPlotterMouseDown(MouseButtonEventArgs e) { }

  private void OnMouseMove(object sender, MouseEventArgs e)
  {
    OnPlotterMouseMove(e: e);
  }

  protected virtual void OnPlotterMouseMove(MouseEventArgs e) { }

}
