using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Threading;

namespace Crystal.Plot2D.Charts;

/// <summary>
///   Represents an editor of points' position of ViewportPolyline or ViewportPolygon.
/// </summary>
[ContentProperty(name: "Polyline")]
public class PolylineEditor : IPlotterElement
{
  /// <summary>
  ///   Initializes a new instance of the <see cref="PolylineEditor"/> class.
  /// </summary>
  public PolylineEditor() { }

  private ViewportPolylineBase polyline;
  /// <summary>
  /// Gets or sets the polyline, to edit points of which.
  /// </summary>
  /// <value>The polyline.</value>
  [NotNull]
  public ViewportPolylineBase Polyline
  {
    get => polyline;
    set
    {
      if (value == null)
      {
        throw new ArgumentNullException(paramName: nameof(Polyline));
      }

      if (polyline != value)
      {
        polyline = value;
        var descr = DependencyPropertyDescriptor.FromProperty(dependencyProperty: ViewportPolylineBase.PointsProperty, targetType: typeof(ViewportPolylineBase));
        descr.AddValueChanged(component: polyline, handler: OnPointsReplaced);

        if (plotter != null)
        {
          AddLineToPlotter(asyncVar: false);
        }
      }
    }
  }

  bool pointsAdded;
  private void OnPointsReplaced(object sender, EventArgs e)
  {
    if (plotter == null)
    {
      return;
    }

    if (pointsAdded)
    {
      return;
    }

    ViewportPolylineBase line = (ViewportPolylineBase)sender;

    pointsAdded = true;
    List<IPlotterElement> draggablePoints = new();
    GetDraggablePoints(collection: draggablePoints);

    foreach (var point in draggablePoints)
    {
      plotter.Children.Add(item: point);
    }
  }

  private void AddLineToPlotter(bool asyncVar)
  {
    if (!asyncVar)
    {
      foreach (var item in GetAllElementsToAdd())
      {
        plotter.Children.Add(item: item);
      }
    }
    else
    {
      plotter.Dispatcher.BeginInvoke(method: ((Action)(() => { AddLineToPlotter(asyncVar: false); })), priority: DispatcherPriority.Send);
    }
  }

  private List<IPlotterElement> GetAllElementsToAdd()
  {
    var result = new List<IPlotterElement>(capacity: 1 + polyline.Points.Count);
    result.Add(item: polyline);

    GetDraggablePoints(collection: result);

    return result;
  }

  private void GetDraggablePoints(List<IPlotterElement> collection)
  {
    for (int i = 0; i < polyline.Points.Count; i++)
    {
      DraggablePoint point = new();
      point.SetBinding(dp: PositionalViewportUIContainer.PositionProperty, binding: new Binding
      {
        Source = polyline,
        Path = new PropertyPath(path: "Points[" + i + "]"),
        Mode = BindingMode.TwoWay
      });
      collection.Add(item: point);
    }
  }

  #region IPlotterElement Members

  void IPlotterElement.OnPlotterAttached(PlotterBase plotter)
  {
    this.plotter = (PlotterBase)plotter;

    if (polyline != null)
    {
      AddLineToPlotter(asyncVar: true);
    }
  }

  void IPlotterElement.OnPlotterDetaching(PlotterBase plotter)
  {
    this.plotter = null;
  }

  private PlotterBase plotter;
  /// <summary>
  /// Gets the parent plotter of chart.
  /// Should be equal to null if item is not connected to any plotter.
  /// </summary>
  /// <value>The plotter.</value>
  public PlotterBase Plotter => plotter;

  #endregion
}
