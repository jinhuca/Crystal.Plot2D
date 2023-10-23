using Crystal.Plot2D.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Crystal.Plot2D.Charts;

[ContentProperty(name: "Content")]
public class ViewportUIContainer : DependencyObject, IPlotterElement
{
  public ViewportUIContainer() { }

  public ViewportUIContainer(FrameworkElement content)
  {
    Content = content;
  }

  private FrameworkElement content;
  [NotNull]
  public FrameworkElement Content
  {
    get => content;
    set => content = value;
  }

  #region IPlotterElement Members

  void IPlotterElement.OnPlotterAttached(PlotterBase _plotter)
  {
    plotter = _plotter;
    if (Content == null)
    {
      return;
    }

    var plotterPanel = GetPlotterPanel(obj: Content);
    //Plotter.SetPlotter(Content, _plotter);

    if (plotterPanel == PlotterPanel.MainCanvas)
    {
      // if all four Canvas.{Left|Right|Top|Bottom} properties are not set,
      // and as we are adding by default content to MainCanvas, 
      // and since I like more when buttons are by default in right down corner - 
      // set bottom and right to 10.
      var left = Canvas.GetLeft(element: content);
      var top = Canvas.GetTop(element: content);
      var bottom = Canvas.GetBottom(element: content);
      var right = Canvas.GetRight(element: content);

      if (left.IsNaN() && right.IsNaN() && bottom.IsNaN() && top.IsNaN())
      {
        Canvas.SetBottom(element: content, length: 10.0);
        Canvas.SetRight(element: content, length: 10.0);
      }
      _plotter.MainCanvas.Children.Add(element: Content);
    }
  }

  void IPlotterElement.OnPlotterDetaching(PlotterBase _plotter)
  {
    if (Content != null)
    {
      var plotterPanel = GetPlotterPanel(obj: Content);
      //Plotter.SetPlotter(Content, null);
      if (plotterPanel == PlotterPanel.MainCanvas)
      {
        _plotter.MainCanvas.Children.Remove(element: Content);
      }
    }

    plotter = null;
  }

  private PlotterBase plotter;
  PlotterBase IPlotterElement.Plotter => plotter;

  #endregion

  [AttachedPropertyBrowsableForChildren]
  public static PlotterPanel GetPlotterPanel(DependencyObject obj)
    => (PlotterPanel)obj.GetValue(dp: PlotterPanelProperty);

  public static void SetPlotterPanel(DependencyObject obj, PlotterPanel value) 
    => obj.SetValue(dp: PlotterPanelProperty, value: value);

  public static readonly DependencyProperty PlotterPanelProperty = DependencyProperty.RegisterAttached(
    name: "PlotterPanel",
    propertyType: typeof(PlotterPanel),
    ownerType: typeof(ViewportUIContainer),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: PlotterPanel.MainCanvas));
}