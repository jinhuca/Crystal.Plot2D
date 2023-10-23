using Crystal.Plot2D.Charts;
using Crystal.Plot2D.Common;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Crystal.Plot2D;

/// <summary>
/// Represents a title of vertical axis. Can be placed from left or right of Plotter.
/// </summary>
public class VerticalAxisTitle : ContentControl, IPlotterElement
{
  /// <summary>
  /// Initializes a new instance of the <see cref="VerticalAxisTitle"/> class.
  /// </summary>
  public VerticalAxisTitle()
  {
    ChangeLayoutTransform();
    VerticalAlignment = VerticalAlignment.Center;
    FontSize = 16;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="VerticalAxisTitle"/> class.
  /// </summary>
  /// <param name="content">The content.</param>
  public VerticalAxisTitle(object content) : this() => Content = content;

  private void ChangeLayoutTransform()
  {
    LayoutTransform = placement == AxisPlacement.Left ? new RotateTransform(angle: -90) : new RotateTransform(angle: 90);
  }
  public PlotterBase Plotter { get; private set; }

  public void OnPlotterAttached(PlotterBase plotter)
  {
    Plotter = plotter;
    var hostPanel = GetHostPanel(plotter: plotter);
    var index = GetInsertPosition(panel: hostPanel);
    hostPanel.Children.Insert(index: index, element: this);
  }

  public void OnPlotterDetaching(PlotterBase plotter)
  {
    Plotter = null;
    var hostPanel = GetHostPanel(plotter: plotter);
    hostPanel.Children.Remove(element: this);
  }

  private Panel GetHostPanel(PlotterBase plotter) => placement == AxisPlacement.Left ? plotter.LeftPanel : plotter.RightPanel;

  private int GetInsertPosition(Panel panel) => placement == AxisPlacement.Left ? 0 : panel.Children.Count;

  private AxisPlacement placement = AxisPlacement.Left;
  /// <summary>
  /// Gets or sets the placement of axis title.
  /// </summary>
  /// <value>The placement.</value>
  public AxisPlacement Placement
  {
    get => placement;
    set
    {
      if (value.IsBottomOrTop())
      {
        throw new ArgumentException(
          message: $"VerticalAxisTitle only supports Left and Right values of AxisPlacement, you passed '{value}'", paramName: nameof(Placement));
      }

      if (placement != value)
      {
        if (Plotter != null)
        {
          var oldPanel = GetHostPanel(plotter: Plotter);
          oldPanel.Children.Remove(element: this);
        }

        placement = value;

        ChangeLayoutTransform();

        if (Plotter != null)
        {
          var hostPanel = GetHostPanel(plotter: Plotter);
          var index = GetInsertPosition(panel: hostPanel);
          hostPanel.Children.Insert(index: index, element: this);
        }
      }
    }
  }
}