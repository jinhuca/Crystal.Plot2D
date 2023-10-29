using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Transforms;

namespace Crystal.Plot2D.Navigation;

///<summary>
/// This class allows convenient navigation around viewport using touchpad on some notebooks.
///</summary>
public sealed class TouchpadScroll : NavigationBase
{
  public TouchpadScroll()
  {
    Loaded += OnLoaded;
  }

  private void OnLoaded(object sender, RoutedEventArgs e)
  {
    WindowInteropHelper helper_ = new(window: Window.GetWindow(dependencyObject: this) ?? throw new InvalidOperationException());
    var source_ = HwndSource.FromHwnd(hwnd: helper_.Handle);
    source_.AddHook(hook: OnMessageAppeared);
  }

  private IntPtr OnMessageAppeared(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
  {
    if (msg == WindowsMessages.WM_MOUSEWHEEL)
    {
      var mousePos_ = MessagesHelper.GetMousePosFromLParam(lParam: lParam);
      mousePos_ = TranslatePoint(point: mousePos_, relativeTo: this);

      if (Viewport.Output.Contains(point: mousePos_))
      {
        MouseWheelZoom(mousePos: MessagesHelper.GetMousePosFromLParam(lParam: lParam), wheelRotationDelta: MessagesHelper.GetWheelDataFromWParam(wParam: wParam));
        handled = true;
      }
    }

    return IntPtr.Zero;
  }

  private double wheelZoomSpeed = 1.2;

  public double WheelZoomSpeed
  {
    get => wheelZoomSpeed;
    set => wheelZoomSpeed = value;
  }

  private void MouseWheelZoom(Point mousePos, int wheelRotationDelta)
  {
    var zoomTo_ = mousePos.ScreenToData(transform: Viewport.Transform);

    double zoomSpeed_ = Math.Abs(value: wheelRotationDelta / Mouse.MouseWheelDeltaForOneLine);
    zoomSpeed_ *= wheelZoomSpeed;
    if (wheelRotationDelta < 0)
    {
      zoomSpeed_ = 1 / zoomSpeed_;
    }

    Viewport.Visible = Viewport.Visible.Zoom(to: zoomTo_, ratio: zoomSpeed_);
  }
}
