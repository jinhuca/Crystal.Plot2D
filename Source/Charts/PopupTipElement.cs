using Crystal.Plot2D.Common;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Crystal.Plot2D.Charts;

public abstract class PopupTipElement : IPlotterElement
{
  /// <summary>
  ///   Shows tooltips.
  /// </summary>
  private PopupTip popup;

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
      PlacementTarget = plotter.CentralGrid
    };
    Plotter.Children.Add(content: popup);
    return popup;
  }

  private void OnMouseLeave(object sender, MouseEventArgs e)
  {
    GetPopupTipWindow().Hide();
  }

  private void OnMouseMove(object sender, MouseEventArgs e)
  {
    var popup = GetPopupTipWindow();
    if (popup.IsOpen)
    {
      popup.Hide();
    }

    Point screenPoint = e.GetPosition(relativeTo: plotter.CentralGrid);
    Point viewportPoint = screenPoint.ScreenToData(transform: plotter.Transform);

    var tooltip = GetTooltipForPoint(viewportPosition: viewportPoint);
    if (tooltip == null)
    {
      return;
    }

    popup.VerticalOffset = screenPoint.Y + 20;
    popup.HorizontalOffset = screenPoint.X;
    popup.ShowDelayed(delay: TimeSpan.FromSeconds(value: 0));

    Grid grid = new();
    Rectangle rect = new()
    {
      Stroke = Brushes.Black,
      Fill = SystemColors.InfoBrush
    };

    StackPanel panel = new();
    panel.Orientation = Orientation.Vertical;
    panel.Children.Add(element: tooltip);
    panel.Margin = new Thickness(left: 4, top: 2, right: 4, bottom: 2);

    var textBlock = new TextBlock
    {
      Text = $"Location: {viewportPoint.X:F2}, {viewportPoint.Y:F2}",
      Foreground = SystemColors.GrayTextBrush
    };
    panel.Children.Add(element: textBlock);

    grid.Children.Add(element: rect);
    grid.Children.Add(element: panel);
    grid.Measure(availableSize: SizeHelper.CreateInfiniteSize());
    popup.Child = grid;
  }

  protected virtual UIElement GetTooltipForPoint(Point viewportPosition)
  {
    return null;
  }

  #region IPlotterElement Members

  private PlotterBase plotter;
  public void OnPlotterAttached(PlotterBase plotter)
  {
    this.plotter = (PlotterBase)plotter;
    plotter.CentralGrid.MouseMove += OnMouseMove;
    plotter.CentralGrid.MouseLeave += OnMouseLeave;
  }

  public void OnPlotterDetaching(PlotterBase plotter)
  {
    plotter.CentralGrid.MouseMove -= OnMouseMove;
    plotter.CentralGrid.MouseLeave -= OnMouseLeave;
    this.plotter = null;
  }

  public PlotterBase Plotter => plotter;

  #endregion
}