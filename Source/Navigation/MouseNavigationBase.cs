using System.Windows.Input;

namespace Crystal.Plot2D
{
  // todo проверить, как происходит работа когда мышь не над плоттером, а над его ребенком
  // todo если все ОК, то перевести все маус навигейшн контролы на этот класс как базовый
  public abstract class MouseNavigationBase : NavigationBase
  {
    protected override void OnPlotterAttached(PlotterBase plotter)
    {
      base.OnPlotterAttached(plotter);

      Mouse.AddMouseDownHandler(Parent, OnMouseDown);
      Mouse.AddMouseMoveHandler(Parent, OnMouseMove);
      Mouse.AddMouseUpHandler(Parent, OnMouseUp);
      Mouse.AddMouseWheelHandler(Parent, OnMouseWheel);
    }

    protected override void OnPlotterDetaching(PlotterBase plotter)
    {
      Mouse.RemoveMouseDownHandler(Parent, OnMouseDown);
      Mouse.RemoveMouseMoveHandler(Parent, OnMouseMove);
      Mouse.RemoveMouseUpHandler(Parent, OnMouseUp);
      Mouse.RemoveMouseWheelHandler(Parent, OnMouseWheel);

      base.OnPlotterDetaching(plotter);
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
      OnPlotterMouseWheel(e);
    }

    protected virtual void OnPlotterMouseWheel(MouseWheelEventArgs e) { }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
      OnPlotterMouseUp(e);
    }

    protected virtual void OnPlotterMouseUp(MouseButtonEventArgs e) { }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
      OnPlotterMouseDown(e);
    }

    protected virtual void OnPlotterMouseDown(MouseButtonEventArgs e) { }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      OnPlotterMouseMove(e);
    }

    protected virtual void OnPlotterMouseMove(MouseEventArgs e) { }

  }
}
