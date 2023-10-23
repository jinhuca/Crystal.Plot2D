using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Crystal.Plot2D.Charts;

/// <summary>
/// Adds to Plotter two lines upon axes, showing current cursor position.
/// </summary>
public class AxisCursorGraph : DependencyObject, IPlotterElement
{
  static AxisCursorGraph()
  {
    Shape.StrokeProperty.AddOwner(ownerType: typeof(AxisCursorGraph), typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Red));
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="AxisCursorGraph"/> class.
  /// </summary>
  public AxisCursorGraph() { }

  #region ShowVerticalLine property

  /// <summary>
  /// Gets or sets a value indicating whether to show line upon vertical axis.
  /// </summary>
  /// <value><c>true</c> if line upon vertical axis is shown; otherwise, <c>false</c>.</value>
  public bool ShowVerticalLine
  {
    get => (bool)GetValue(dp: ShowVerticalLineProperty);
    set => SetValue(dp: ShowVerticalLineProperty, value: value);
  }

  /// <summary>
  /// Identifies ShowVerticalLine dependency property.
  /// </summary>
  public static readonly DependencyProperty ShowVerticalLineProperty = DependencyProperty.Register(
    name: nameof(ShowVerticalLine),
    propertyType: typeof(bool),
    ownerType: typeof(AxisCursorGraph),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: true, propertyChangedCallback: OnShowLinePropertyChanged));

  private static void OnShowLinePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    AxisCursorGraph graph = (AxisCursorGraph)d;
    graph.UpdateUIRepresentation();
  }

  #endregion

  #region ShowHorizontalLine property

  /// <summary>
  /// Gets or sets a value indicating whether to show line upon horizontal axis.
  /// </summary>
  /// <value><c>true</c> if lien upon horizontal axis is shown; otherwise, <c>false</c>.</value>
  public bool ShowHorizontalLine
  {
    get => (bool)GetValue(dp: ShowHorizontalLineProperty);
    set => SetValue(dp: ShowHorizontalLineProperty, value: value);
  }

  public static readonly DependencyProperty ShowHorizontalLineProperty = DependencyProperty.Register(
    name: nameof(ShowHorizontalLine),
    propertyType: typeof(bool),
    ownerType: typeof(AxisCursorGraph),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: true, propertyChangedCallback: OnShowLinePropertyChanged));

  #endregion

  #region IPlotterElement Members

  private Line leftLine;
  private Line bottomLine;
  private Canvas leftCanvas;
  private Canvas bottomCanvas;

  private PlotterBase plotter;
  void IPlotterElement.OnPlotterAttached(PlotterBase plotter)
  {
    this.plotter = (PlotterBase)plotter;

    this.plotter.Viewport.PropertyChanged += Viewport_PropertyChanged;

    var parent = plotter.MainGrid;
    parent.MouseMove += parent_MouseMove;
    parent.MouseEnter += parent_MouseEnter;
    parent.MouseLeave += parent_MouseLeave;

    Style lineStyle = new(targetType: typeof(Line));
    AddBindingSetter(style: lineStyle, property: Shape.StrokeProperty);
    AddBindingSetter(style: lineStyle, property: Shape.StrokeThicknessProperty);

    leftCanvas = new Canvas();
    Grid.SetRow(element: leftCanvas, value: 1);
    Grid.SetColumn(element: leftCanvas, value: 0);
    leftLine = new Line { Style = lineStyle, IsHitTestVisible = false };
    leftCanvas.Children.Add(element: leftLine);
    parent.Children.Add(element: leftCanvas);

    bottomCanvas = new Canvas();
    Grid.SetRow(element: bottomCanvas, value: 2);
    Grid.SetColumn(element: bottomCanvas, value: 1);
    bottomLine = new Line { Style = lineStyle, IsHitTestVisible = false };
    bottomCanvas.Children.Add(element: bottomLine);
    parent.Children.Add(element: bottomCanvas);
  }

  private void AddBindingSetter(Style style, DependencyProperty property)
  {
    style.Setters.Add(item: new Setter(property: property,
      value: new Binding
      {
        Path = new PropertyPath(path: property.Name),
        Source = this
      }));
  }

  void Viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
  {
    UpdateUIRepresentation();
  }

  private void UpdateUIRepresentation()
  {
    if (plotter == null)
    {
      return;
    }

    var transform = plotter.Viewport.Transform;
    DataRect visible = plotter.Viewport.Visible;
    Rect output = plotter.Viewport.Output;

    Point mousePos = Mouse.GetPosition(relativeTo: plotter.CentralGrid);

    if (ShowVerticalLine)
    {
      double y = mousePos.Y;
      if (output.Top <= y && y <= output.Bottom)
      {
        leftLine.Visibility = Visibility.Visible;
        leftLine.X1 = 0;
        leftLine.X2 = plotter.LeftPanel.ActualWidth;

        leftLine.Y1 = leftLine.Y2 = y;
      }
      else
      {
        leftLine.Visibility = Visibility.Collapsed;
      }
    }
    else
    {
      leftLine.Visibility = Visibility.Collapsed;
    }

    if (ShowHorizontalLine)
    {
      double x = mousePos.X;
      if (output.Left <= x && x <= output.Right)
      {
        bottomLine.Visibility = Visibility.Visible;
        bottomLine.Y1 = 0;
        bottomLine.Y2 = plotter.BottomPanel.ActualHeight;

        bottomLine.X1 = bottomLine.X2 = x;
      }
      else
      {
        bottomLine.Visibility = Visibility.Collapsed;
      }
    }
    else
    {
      bottomLine.Visibility = Visibility.Collapsed;
    }
  }

  void parent_MouseLeave(object sender, MouseEventArgs e)
  {
    UpdateUIRepresentation();
  }

  void parent_MouseEnter(object sender, MouseEventArgs e)
  {
    UpdateUIRepresentation();
  }

  void parent_MouseMove(object sender, MouseEventArgs e)
  {
    UpdateUIRepresentation();
  }

  void IPlotterElement.OnPlotterDetaching(PlotterBase plotter)
  {
    this.plotter.Viewport.PropertyChanged -= Viewport_PropertyChanged;

    var parent = plotter.MainGrid;
    parent.MouseMove -= parent_MouseMove;
    parent.MouseEnter -= parent_MouseEnter;
    parent.MouseLeave -= parent_MouseLeave;

    parent.Children.Remove(element: leftCanvas);
    parent.Children.Remove(element: bottomCanvas);

    this.plotter = null;
  }

  PlotterBase IPlotterElement.Plotter => plotter;

  #endregion
}
