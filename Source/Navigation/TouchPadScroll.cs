using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Crystal.Plot2D;

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
    WindowInteropHelper helper = new(window: Window.GetWindow(dependencyObject: this));
    HwndSource source = HwndSource.FromHwnd(hwnd: helper.Handle);
    source.AddHook(hook: OnMessageAppeared);
  }

  private IntPtr OnMessageAppeared(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
  {
    if (msg == WindowsMessages.WM_MOUSEWHEEL)
    {
      Point mousePos = MessagesHelper.GetMousePosFromLParam(lParam: lParam);
      mousePos = TranslatePoint(point: mousePos, relativeTo: this);

      if (Viewport.Output.Contains(point: mousePos))
      {
        MouseWheelZoom(mousePos: MessagesHelper.GetMousePosFromLParam(lParam: lParam), wheelRotationDelta: MessagesHelper.GetWheelDataFromWParam(wParam: wParam));
        handled = true;
      }
    }
    return IntPtr.Zero;
  }

  double wheelZoomSpeed = 1.2;
  public double WheelZoomSpeed
  {
    get => wheelZoomSpeed;
    set => wheelZoomSpeed = value;
  }

  private void MouseWheelZoom(Point mousePos, int wheelRotationDelta)
  {
    Point zoomTo = mousePos.ScreenToData(transform: Viewport.Transform);

    double zoomSpeed = Math.Abs(value: wheelRotationDelta / Mouse.MouseWheelDeltaForOneLine);
    zoomSpeed *= wheelZoomSpeed;
    if (wheelRotationDelta < 0)
    {
      zoomSpeed = 1 / zoomSpeed;
    }
    Viewport.Visible = Viewport.Visible.Zoom(to: zoomTo, ratio: zoomSpeed);
  }
}
