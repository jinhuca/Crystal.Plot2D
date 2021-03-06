#define old

using Crystal.Plot2D.Common;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Crystal.Plot2D.Charts
{
  public sealed class PositionChangedEventArgs : EventArgs
  {
    public Point Position { get; internal set; }
    public Point PreviousPosition { get; internal set; }
  }

  public delegate Point PositionCoerceCallback(PositionalViewportUIContainer container, Point position);

  public class PositionalViewportUIContainer : ContentControl, IPlotterElement
  {
    static PositionalViewportUIContainer()
    {
      Type type = typeof(PositionalViewportUIContainer);

      // todo subscribe for properties changes
      HorizontalContentAlignmentProperty.AddOwner(type, new FrameworkPropertyMetadata(HorizontalAlignment.Center));
      VerticalContentAlignmentProperty.AddOwner(type, new FrameworkPropertyMetadata(VerticalAlignment.Center));
    }

    public PositionalViewportUIContainer()
    {
      PlotterEvents.PlotterChangedEvent.Subscribe(this, OnPlotterChanged);

      //SetBinding(ViewportPanel.XProperty, new Binding("Position.X") { Source = this, Mode = BindingMode.TwoWay });
      //SetBinding(ViewportPanel.YProperty, new Binding("Position.Y") { Source = this, Mode = BindingMode.TwoWay });
    }

    protected virtual void OnPlotterChanged(object sender, PlotterChangedEventArgs e)
    {
      if (e.CurrentPlotter != null)
      {
        OnPlotterAttached(e.CurrentPlotter);
      }
      else if (e.PreviousPlotter != null)
      {
        OnPlotterDetaching(e.PreviousPlotter);
      }
    }

    public Point Position
    {
      get { return (Point)GetValue(PositionProperty); }
      set { SetValue(PositionProperty, value); }
    }

    public static readonly DependencyProperty PositionProperty =
      DependencyProperty.Register(
        "Position",
        typeof(Point),
        typeof(PositionalViewportUIContainer),
        new FrameworkPropertyMetadata(new Point(0, 0), OnPositionChanged, CoercePosition));

    private static object CoercePosition(DependencyObject d, object value)
    {
      PositionalViewportUIContainer owner = (PositionalViewportUIContainer)d;
      if (owner.positionCoerceCallbacks.Count > 0)
      {
        Point position = (Point)value;
        foreach (var callback in owner.positionCoerceCallbacks)
        {
          position = callback(owner, position);
        }
        value = position;
      }
      return value;
    }

    private readonly ObservableCollection<PositionCoerceCallback> positionCoerceCallbacks = new();
    /// <summary>
    /// Gets the list of callbacks which are called every time Position changes to coerce it.
    /// </summary>
    /// <value>The position coerce callbacks.</value>
    public ObservableCollection<PositionCoerceCallback> PositionCoerceCallbacks
    {
      get { return positionCoerceCallbacks; }
    }

    private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      PositionalViewportUIContainer container = (PositionalViewportUIContainer)d;
      container.OnPositionChanged(e);
    }

    public event EventHandler<PositionChangedEventArgs> PositionChanged;

    private void OnPositionChanged(DependencyPropertyChangedEventArgs e)
    {
      PositionChanged.Raise(this, new PositionChangedEventArgs { Position = (Point)e.NewValue, PreviousPosition = (Point)e.OldValue });

      ViewportPanel.SetX(this, Position.X);
      ViewportPanel.SetY(this, Position.Y);
    }

    #region IPlotterElement Members

    private const string canvasName = "ViewportUIContainer_Canvas";
    private ViewportHostPanel hostPanel;
    private PlotterBase plotter;
    public void OnPlotterAttached(PlotterBase plotter)
    {
      if (Parent == null)
      {
        hostPanel = new ViewportHostPanel();
        Viewport2D.SetIsContentBoundsHost(hostPanel, false);
        hostPanel.Children.Add(this);

        plotter.Dispatcher.BeginInvoke(() =>
        {
          plotter.Children.Add(hostPanel);
        }, DispatcherPriority.Send);
      }
#if !old
			Canvas hostCanvas = (Canvas)hostPanel.FindName(canvasName);
			if (hostCanvas == null)
			{
				hostCanvas = new Canvas { ClipToBounds = true };
				Panel.SetZIndex(hostCanvas, 1);

				INameScope nameScope = NameScope.GetNameScope(hostPanel);
				if (nameScope == null)
				{
					nameScope = new NameScope();
					NameScope.SetNameScope(hostPanel, nameScope);
				}

				hostPanel.RegisterName(canvasName, hostCanvas);
				hostPanel.Children.Add(hostCanvas);
			}

			hostCanvas.Children.Add(this);
#else
#endif

      PlotterBase plotter2d = (PlotterBase)plotter;
      this.plotter = plotter2d;
    }

    public void OnPlotterDetaching(PlotterBase plotter)
    {
      PlotterBase plotter2d = (PlotterBase)plotter;

#if !old
			Canvas hostCanvas = (Canvas)hostPanel.FindName(canvasName);

			if (hostCanvas.Children.Count == 1)
			{
				// only this ViewportUIContainer left
				hostPanel.Children.Remove(hostCanvas);
			}
			hostCanvas.Children.Remove(this);
#else
      if (hostPanel != null)
      {
        hostPanel.Children.Remove(this);
      }
      plotter.Dispatcher.BeginInvoke(() =>
      {
        plotter.Children.Remove(hostPanel);
      }, DispatcherPriority.Send);
#endif

      this.plotter = null;
    }

    public PlotterBase Plotter
    {
      get { return plotter; }
    }

    PlotterBase IPlotterElement.Plotter
    {
      get { return plotter; }
    }

    #endregion
  }
}
