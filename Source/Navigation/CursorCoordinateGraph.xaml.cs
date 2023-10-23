using Crystal.Plot2D.Common;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Crystal.Plot2D.Charts;

/// <summary>
///   Adds to Plotter two crossed lines, bound to mouse cursor position, 
///   and two labels near axes with mouse position in its text.
/// </summary>
public partial class CursorCoordinateGraph : ContentGraph
{
  /// <summary>
  ///   Initializes a new instance of the <see cref="CursorCoordinateGraph"/> class.
  /// </summary>
  public CursorCoordinateGraph()
  {
    InitializeComponent();
  }

  Vector blockShift = new(x: 3, y: 3);

  #region Plotter

  protected override void OnPlotterAttached()
  {
    UIElement parent = (UIElement)Parent;

    parent.MouseMove += parent_MouseMove;
    parent.MouseEnter += Parent_MouseEnter;
    parent.MouseLeave += Parent_MouseLeave;

    UpdateVisibility();
    UpdateUIRepresentation();
  }

  protected override void OnPlotterDetaching()
  {
    UIElement parent = (UIElement)Parent;

    parent.MouseMove -= parent_MouseMove;
    parent.MouseEnter -= Parent_MouseEnter;
    parent.MouseLeave -= Parent_MouseLeave;
  }

  #endregion

  /// <summary>
  /// Gets or sets a value indicating whether to hide automatically cursor lines when mouse leaves plotter.
  /// </summary>
  /// <value><c>true</c> if auto hide; otherwise, <c>false</c>.</value>
  public bool AutoHide { get; set; } = true;

  private void Parent_MouseEnter(object sender, MouseEventArgs e)
  {
    if (AutoHide)
    {
      UpdateVisibility();
    }
  }

  private void UpdateVisibility()
  {
    horizLine.Visibility = vertGrid.Visibility = GetHorizontalVisibility();
    vertLine.Visibility = horizGrid.Visibility = GetVerticalVisibility();
  }

  private Visibility GetHorizontalVisibility()
  {
    return showHorizontalLine ? Visibility.Visible : Visibility.Hidden;
  }

  private Visibility GetVerticalVisibility()
  {
    return showVerticalLine ? Visibility.Visible : Visibility.Hidden;
  }

  private void Parent_MouseLeave(object sender, MouseEventArgs e)
  {
    if (AutoHide)
    {
      horizLine.Visibility = Visibility.Hidden;
      vertLine.Visibility = Visibility.Hidden;
      horizGrid.Visibility = Visibility.Hidden;
      vertGrid.Visibility = Visibility.Hidden;
    }
  }

  private bool followMouse = true;
  /// <summary>
  /// Gets or sets a value indicating whether lines are following mouse cursor position.
  /// </summary>
  /// <value><c>true</c> if lines are following mouse cursor position; otherwise, <c>false</c>.</value>
  public bool FollowMouse
  {
    get => followMouse;
    set
    {
      followMouse = value;

      if (!followMouse)
      {
        AutoHide = false;
      }

      UpdateUIRepresentation();
    }
  }

  private void parent_MouseMove(object sender, MouseEventArgs e)
  {
    if (followMouse)
    {
      UpdateUIRepresentation();
    }
  }

  protected override void OnViewportPropertyChanged(ExtendedPropertyChangedEventArgs e)
  {
    UpdateUIRepresentation();
  }

  private string customXFormat;
  /// <summary>
  /// Gets or sets the custom format string of x label.
  /// </summary>
  /// <value>The custom X format.</value>
  public string CustomXFormat
  {
    get => customXFormat;
    set
    {
      if (customXFormat != value)
      {
        customXFormat = value;
        UpdateUIRepresentation();
      }
    }
  }

  private string customYFormat;
  /// <summary>
  /// Gets or sets the custom format string of y label.
  /// </summary>
  /// <value>The custom Y format.</value>
  public string CustomYFormat
  {
    get => customYFormat;
    set
    {
      if (customYFormat != value)
      {
        customYFormat = value;
        UpdateUIRepresentation();
      }
    }
  }

  private Func<double, string> xTextMapping;
  /// <summary>
  /// Gets or sets the text mapping of x label - function that builds text from x-coordinate of mouse in data.
  /// </summary>
  /// <value>The X text mapping.</value>
  public Func<double, string> XTextMapping
  {
    get => xTextMapping;
    set
    {
      if (xTextMapping != value)
      {
        xTextMapping = value;
        UpdateUIRepresentation();
      }
    }
  }

  private Func<double, string> yTextMapping;
  /// <summary>
  /// Gets or sets the text mapping of y label - function that builds text from y-coordinate of mouse in data.
  /// </summary>
  /// <value>The Y text mapping.</value>
  public Func<double, string> YTextMapping
  {
    get => yTextMapping;
    set
    {
      if (yTextMapping != value)
      {
        yTextMapping = value;
        UpdateUIRepresentation();
      }
    }
  }

  private bool showHorizontalLine = true;
  /// <summary>
  /// Gets or sets a value indicating whether to show horizontal line.
  /// </summary>
  /// <value><c>true</c> if horizontal line is shown; otherwise, <c>false</c>.</value>
  public bool ShowHorizontalLine
  {
    get => showHorizontalLine;
    set
    {
      if (showHorizontalLine != value)
      {
        showHorizontalLine = value;
        UpdateVisibility();
      }
    }
  }

  private bool showVerticalLine = true;
  /// <summary>
  /// Gets or sets a value indicating whether to show vertical line.
  /// </summary>
  /// <value><c>true</c> if vertical line is shown; otherwise, <c>false</c>.</value>
  public bool ShowVerticalLine
  {
    get => showVerticalLine;
    set
    {
      if (showVerticalLine != value)
      {
        showVerticalLine = value;
        UpdateVisibility();
      }
    }
  }

  private void UpdateUIRepresentation()
  {
    Point position = followMouse ? Mouse.GetPosition(relativeTo: this) : Position;
    UpdateUIRepresentation(mousePos: position);
  }

  private void UpdateUIRepresentation(Point mousePos)
  {
    if (Plotter2D == null)
    {
      return;
    }

    var transform = Plotter2D.Viewport.Transform;
    DataRect visible = Plotter2D.Viewport.Visible;
    Rect output = Plotter2D.Viewport.Output;

    if (!output.Contains(point: mousePos))
    {
      if (AutoHide)
      {
        horizGrid.Visibility = horizLine.Visibility = vertGrid.Visibility = vertLine.Visibility = Visibility.Hidden;
      }
      return;
    }

    if (!followMouse)
    {
      mousePos = mousePos.DataToScreen(transform: transform);
    }

    horizLine.X1 = output.Left;
    horizLine.X2 = output.Right;
    horizLine.Y1 = mousePos.Y;
    horizLine.Y2 = mousePos.Y;

    vertLine.X1 = mousePos.X;
    vertLine.X2 = mousePos.X;
    vertLine.Y1 = output.Top;
    vertLine.Y2 = output.Bottom;

    if (UseDashOffset)
    {
      horizLine.StrokeDashOffset = (output.Right - mousePos.X) / 2;
      vertLine.StrokeDashOffset = (output.Bottom - mousePos.Y) / 2;
    }

    Point mousePosInData = mousePos.ScreenToData(transform: transform);

    string text = null;

    if (showVerticalLine)
    {
      double xValue = mousePosInData.X;
      if (xTextMapping != null)
      {
        text = xTextMapping(arg: xValue);
      }

      // doesnot have xTextMapping or it returned null
      if (text == null)
      {
        text = GetRoundedValue(min: visible.XMin, max: visible.XMax, value: xValue);
      }

      if (!string.IsNullOrEmpty(value: customXFormat))
      {
        text = string.Format(format: customXFormat, arg0: text);
      }

      horizTextBlock.Text = text;
    }

    double width = horizGrid.ActualWidth;
    double x = mousePos.X + blockShift.X;
    if (x + width > output.Right)
    {
      x = mousePos.X - blockShift.X - width;
    }
    Canvas.SetLeft(element: horizGrid, length: x);

    if (showHorizontalLine)
    {
      double yValue = mousePosInData.Y;
      text = null;
      if (yTextMapping != null)
      {
        text = yTextMapping(arg: yValue);
      }

      if (text == null)
      {
        text = GetRoundedValue(min: visible.YMin, max: visible.YMax, value: yValue);
      }

      if (!string.IsNullOrEmpty(value: customYFormat))
      {
        text = string.Format(format: customYFormat, arg0: text);
      }

      vertTextBlock.Text = text;
    }

    // by default vertGrid is positioned on the top of line.
    double height = vertGrid.ActualHeight;
    double y = mousePos.Y - blockShift.Y - height;
    if (y < output.Top)
    {
      y = mousePos.Y + blockShift.Y;
    }
    Canvas.SetTop(element: vertGrid, length: y);

    if (followMouse)
    {
      Position = mousePos;
    }
  }

  /// <summary>
  /// Gets or sets the mouse position in screen coordinates.
  /// </summary>
  /// <value>The position.</value>
  public Point Position
  {
    get => (Point)GetValue(dp: PositionProperty);
    set => SetValue(dp: PositionProperty, value: value);
  }

  /// <summary>
  /// Identifies Position dependency property.
  /// </summary>
  public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
    name: nameof(Position),
    propertyType: typeof(Point),
    ownerType: typeof(CursorCoordinateGraph),
    typeMetadata: new UIPropertyMetadata(defaultValue: new Point(), propertyChangedCallback: OnPositionChanged));

  private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    CursorCoordinateGraph graph = (CursorCoordinateGraph)d;
    graph.UpdateUIRepresentation(mousePos: (Point)e.NewValue);
  }

  private string GetRoundedValue(double min, double max, double value)
  {
    double roundedValue = value;
    var log = RoundingHelper.GetDifferenceLog(min: min, max: max);
    string format = "G3";
    double diff = Math.Abs(value: max - min);
    if (1E3 < diff && diff < 1E6)
    {
      format = "F0";
    }
    if (log < 0)
    {
      format = "G" + (-log + 2).ToString();
    }

    return roundedValue.ToString(format: format);
  }

  #region UseDashOffset property

  public bool UseDashOffset
  {
    get => (bool)GetValue(dp: UseDashOffsetProperty);
    set => SetValue(dp: UseDashOffsetProperty, value: value);
  }

  public static readonly DependencyProperty UseDashOffsetProperty = DependencyProperty.Register(
    name: nameof(UseDashOffset),
    propertyType: typeof(bool),
    ownerType: typeof(CursorCoordinateGraph),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: true, propertyChangedCallback: UpdateUIRepresentation));

  private static void UpdateUIRepresentation(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    CursorCoordinateGraph graph = (CursorCoordinateGraph)d;
    if ((bool)e.NewValue)
    {
      graph.UpdateUIRepresentation();
    }
    else
    {
      graph.vertLine.ClearValue(dp: Shape.StrokeDashOffsetProperty);
      graph.horizLine.ClearValue(dp: Shape.StrokeDashOffsetProperty);
    }
  }

  #endregion

  #region LineStroke property

  public Brush LineStroke
  {
    get => (Brush)GetValue(dp: LineStrokeProperty);
    set => SetValue(dp: LineStrokeProperty, value: value);
  }

  public static readonly DependencyProperty LineStrokeProperty = DependencyProperty.Register(
    name: nameof(LineStroke),
    propertyType: typeof(Brush),
    ownerType: typeof(CursorCoordinateGraph),
    typeMetadata: new PropertyMetadata(defaultValue: new SolidColorBrush(color: Color.FromArgb(a: 170, r: 86, g: 86, b: 86))));

  #endregion

  #region LineStrokeThickness property

  public double LineStrokeThickness
  {
    get => (double)GetValue(dp: LineStrokeThicknessProperty);
    set => SetValue(dp: LineStrokeThicknessProperty, value: value);
  }

  public static readonly DependencyProperty LineStrokeThicknessProperty = DependencyProperty.Register(
    name: nameof(LineStrokeThickness),
    propertyType: typeof(double),
    ownerType: typeof(CursorCoordinateGraph),
    typeMetadata: new PropertyMetadata(defaultValue: 2.0));

  #endregion

  #region LineStrokeDashArray property

  [SuppressMessage(category: "Microsoft.Usage", checkId: "CA2227:CollectionPropertiesShouldBeReadOnly")]
  public DoubleCollection LineStrokeDashArray
  {
    get => (DoubleCollection)GetValue(dp: LineStrokeDashArrayProperty);
    set => SetValue(dp: LineStrokeDashArrayProperty, value: value);
  }

  public static readonly DependencyProperty LineStrokeDashArrayProperty = DependencyProperty.Register(
    name: nameof(LineStrokeDashArray),
    propertyType: typeof(DoubleCollection),
    ownerType: typeof(CursorCoordinateGraph),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: DoubleCollectionHelper.Create(collection: new double[] { 3, 3 })));

  #endregion
}
