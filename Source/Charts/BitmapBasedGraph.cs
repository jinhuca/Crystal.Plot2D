using Crystal.Plot2D.Charts;
using Crystal.Plot2D.Common;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Crystal.Plot2D
{
  public abstract class BitmapBasedGraph : ViewportElement2D, IDisposable
  {
    static BitmapBasedGraph()
    {
      Type thisType = typeof(BitmapBasedGraph);
      BackgroundRenderer.UsesBackgroundRenderingProperty.OverrideMetadata(thisType, new FrameworkPropertyMetadata(true));
    }

    private int disposed = 0;

    private int nextRequestId = 0;

    /// <summary>
    /// Latest complete request
    /// </summary>
    private RenderRequest completedRequest = null;

    /// <summary>
    /// Currently running request
    /// </summary>
    private RenderRequest activeRequest = null;

    /// <summary>Result of latest complete request</summary>
    private BitmapSource completedBitmap = null;

    /// <summary>Pending render request</summary>
    private RenderRequest pendingRequest = null;

    /// <summary>Single apartment thread used for background rendering</summary>
    /// <remarks>STA is required for creating WPF components in this thread</remarks>
    private Thread renderThread = null;

    private readonly AutoResetEvent renderRequested = new(false);

    private readonly ManualResetEvent shutdownRequested = new(false);

    /// <summary>True means that current bitmap is invalidated and is to be re-rendered.</summary>
    private bool bitmapInvalidated = true;

    /// <summary>Shows tooltips.</summary>
    private PopupTip popup;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkerPointsGraph"/> class.
    /// </summary>
    protected BitmapBasedGraph()
    {
      ManualTranslate = true;
    }

    protected virtual UIElement GetTooltipForPoint(Point point, DataRect visible, Rect output)
    {
      return null;
    }

    protected PopupTip GetPopupTipWindow()
    {
      if (popup != null)
      {
        return popup;
      }

      foreach (var item in Plotter.Children)
      {
        if (item is ViewportUIContainer)
        {
          ViewportUIContainer container = (ViewportUIContainer)item;
          if (container.Content is PopupTip)
          {
            return popup = (PopupTip)container.Content;
          }
        }
      }

      popup = new PopupTip
      {
        Placement = PlacementMode.Relative,
        PlacementTarget = this
      };
      Plotter.Children.Add(popup);
      return popup;
    }

    protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
    {
      base.OnMouseMove(e);
      var popup = GetPopupTipWindow();
      if (popup.IsOpen)
      {
        popup.Hide();
      }

      if (bitmapInvalidated)
      {
        return;
      }

      Point p = e.GetPosition(this);
      Point dp = p.ScreenToData(Plotter.Transform);

      var tooltip = GetTooltipForPoint(p, completedRequest.Visible, completedRequest.Output);
      if (tooltip == null)
      {
        return;
      }

      popup.VerticalOffset = p.Y + 20;
      popup.HorizontalOffset = p.X;
      popup.ShowDelayed(new TimeSpan(0, 0, 1));

      Grid grid = new();
      Rectangle rect = new()
      {
        Stroke = Brushes.Black,
        Fill = SystemColors.InfoBrush
      };

      StackPanel sp = new();
      sp.Orientation = Orientation.Vertical;
      sp.Children.Add(tooltip);
      sp.Margin = new Thickness(4, 2, 4, 2);

      var tb = new TextBlock
      {
        Text = $"Location: {dp.X:F2}, {dp.Y:F2}", //String.Format("Location: {0:F2}, {1:F2}", dp.X, dp.Y);
        Foreground = SystemColors.GrayTextBrush
      };
      sp.Children.Add(tb);

      grid.Children.Add(rect);
      grid.Children.Add(sp);
      grid.Measure(SizeHelper.CreateInfiniteSize());
      popup.Child = grid;
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
      base.OnMouseLeave(e);
      GetPopupTipWindow().Hide();
    }

    /// <summary>
    ///   Adds a render task and invalidates visual.
    /// </summary>
    public void UpdateVisualization()
    {
      if (Viewport == null)
      {
        return;
      }

      Rect output = new(RenderSize);
      CreateRenderTask(Viewport.Visible, output);
      InvalidateVisual();
    }

    protected override void OnVisibleChanged(DataRect newRect, DataRect oldRect)
    {
      base.OnVisibleChanged(newRect, oldRect);
      CreateRenderTask(newRect, Viewport.Output);
      InvalidateVisual();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      base.OnRenderSizeChanged(sizeInfo);
      CreateRenderTask(Viewport.Visible, new Rect(sizeInfo.NewSize));
      InvalidateVisual();
    }

    protected abstract BitmapSource RenderFrame(DataRect visible, Rect output);

    private void RenderThreadFunc()
    {
      WaitHandle[] events = new WaitHandle[] { renderRequested, shutdownRequested };
      while (true)
      {
        lock (this)
        {
          activeRequest = null;
          if (pendingRequest != null)
          {
            activeRequest = pendingRequest;
            pendingRequest = null;
          }
        }
        if (activeRequest == null)
        {
          WaitHandle.WaitAny(events);
          if (shutdownRequested.WaitOne(0))
          {
            break;
          }
        }
        else
        {
          try
          {
            BitmapSource result = (BitmapSource)RenderFrame(activeRequest.Visible, activeRequest.Output);
            if (result != null && !IsDisposed)
            {
              Dispatcher.BeginInvoke(new RenderCompletionHandler(OnRenderCompleted), new RenderResult(activeRequest, result));
            }
          }
          catch (Exception exc)
          {
            Trace.WriteLine(string.Format("RenderRequest {0} failed: {1}", activeRequest.RequestID, exc.Message));
          }
        }
      }
    }

    private void CreateRenderTask(DataRect visible, Rect output)
    {
      lock (this)
      {
        bitmapInvalidated = true;
        if (activeRequest != null)
        {
          activeRequest.Cancel();
        }
        pendingRequest = new RenderRequest(nextRequestId++, visible, output);
        renderRequested.Set();
      }
      if (renderThread == null)
      {
        renderThread = new Thread(RenderThreadFunc)
        {
          IsBackground = true
        };
        renderThread.SetApartmentState(ApartmentState.STA);
        renderThread.Start();
      }
    }

    private delegate void RenderCompletionHandler(RenderResult result);

    protected virtual void OnRenderCompleted(RenderResult result)
    {
      if (IsDisposed)
      {
        return;
      }

      completedRequest = result.Request;
      completedBitmap = result.Bitmap;
      bitmapInvalidated = false;
      InvalidateVisual();
      BackgroundRenderer.RaiseRenderingFinished(this);
    }

    protected override void OnRenderCore(DrawingContext dc, RenderState state)
    {
      if (completedRequest != null && completedBitmap != null)
      {
        dc.DrawImage(completedBitmap, completedRequest.Visible.ViewportToScreen(Viewport.Transform));
      }
    }

    public bool IsDisposed => disposed > 0;

    #region IDisposable Members

    public void Dispose()
    {
      Interlocked.Increment(ref disposed);
      shutdownRequested.Set();
    }

    #endregion
  }

  public class RenderRequest
  {
    private readonly int requestId;
    private readonly DataRect visible;
    private readonly Rect output;
    private int cancelling;

    public RenderRequest(int requestId, DataRect visible, Rect output)
    {
      this.requestId = requestId;
      this.visible = visible;
      this.output = output;
    }

    public int RequestID => requestId;

    public DataRect Visible => visible;

    public Rect Output => output;

    public bool IsCancellingRequested => cancelling > 0;

    public void Cancel()
    {
      Interlocked.Increment(ref cancelling);
    }
  }

  public class RenderResult
  {
    private readonly RenderRequest request;
    private readonly BitmapSource bitmap;

    /// <summary>
    ///   Constructs successul rendering result.
    /// </summary>
    /// <param name="request">
    ///   Source request.
    /// </param>
    /// <param name="result">
    ///   Rendered bitmap.
    /// </param>
    public RenderResult(RenderRequest request, BitmapSource result)
    {
      bitmap = result;
      this.request = request;
      result.Freeze();
    }

    public RenderRequest Request => request;

    public BitmapSource Bitmap => bitmap;
  }
}
