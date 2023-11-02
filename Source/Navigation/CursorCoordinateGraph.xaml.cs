using Crystal.Plot2D.Axes;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Transforms;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Crystal.Plot2D.Navigation;

/// <summary>
///   Adds to Plotter two crossed lines, bound to mouse cursor position, 
///   and two labels near axes with mouse position in its text.
/// </summary>
public sealed partial class CursorCoordinateGraph
{
  /// <summary>
  ///   Initializes a new instance of the <see cref="CursorCoordinateGraph"/> class.
  /// </summary>
  public CursorCoordinateGraph()
  {
    InitializeComponent();
  }

  private Vector blockShift = new(x: 3, y: 3);

  #region Plotter

  protected override void OnPlotterAttached()
  {
    var parent_ = (UIElement)Parent;

    parent_.MouseMove += parent_MouseMove;
    parent_.MouseEnter += Parent_MouseEnter;
    parent_.MouseLeave += Parent_MouseLeave;

    UpdateVisibility();
    UpdateUIRepresentation();
  }

  protected override void OnPlotterDetaching()
  {
    var parent_ = (UIElement)Parent;

    parent_.MouseMove -= parent_MouseMove;
    parent_.MouseEnter -= Parent_MouseEnter;
    parent_.MouseLeave -= Parent_MouseLeave;
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
    var position_ = followMouse ? Mouse.GetPosition(relativeTo: this) : Position;
    UpdateUIRepresentation(mousePos: position_);
  }

  private void UpdateUIRepresentation(Point mousePos)
  {
    if (Plotter2D == null)
    {
      return;
    }

    var transform_ = Plotter2D.Viewport.Transform;
    var visible_ = Plotter2D.Viewport.Visible;
    var output_ = Plotter2D.Viewport.Output;

    if (!output_.Contains(point: mousePos))
    {
      if (AutoHide)
      {
        horizGrid.Visibility = horizLine.Visibility = vertGrid.Visibility = vertLine.Visibility = Visibility.Hidden;
      }
      return;
    }

    if (!followMouse)
    {
      mousePos = mousePos.DataToScreen(transform: transform_);
    }

    horizLine.X1 = output_.Left;
    horizLine.X2 = output_.Right;
    horizLine.Y1 = mousePos.Y;
    horizLine.Y2 = mousePos.Y;

    vertLine.X1 = mousePos.X;
    vertLine.X2 = mousePos.X;
    vertLine.Y1 = output_.Top;
    vertLine.Y2 = output_.Bottom;

    if (UseDashOffset)
    {
      horizLine.StrokeDashOffset = (output_.Right - mousePos.X) / 2;
      vertLine.StrokeDashOffset = (output_.Bottom - mousePos.Y) / 2;
    }

    var mousePosInData_ = mousePos.ScreenToData(transform: transform_);

    string text_ = null;

    if (showVerticalLine)
    {
      var xValue_ = mousePosInData_.X;
      if (xTextMapping != null)
      {
        text_ = xTextMapping(arg: xValue_);
      }

      // does not have xTextMapping or it returned null
      text_ ??= GetRoundedValue(min: visible_.XMin, max: visible_.XMax, value: xValue_);

      if (!string.IsNullOrEmpty(value: customXFormat))
      {
        text_ = string.Format(format: customXFormat, arg0: text_);
      }

      horizTextBlock.Text = text_;
    }

    var width_ = horizGrid.ActualWidth;
    var x_ = mousePos.X + blockShift.X;
    if (x_ + width_ > output_.Right)
    {
      x_ = mousePos.X - blockShift.X - width_;
    }

    Canvas.SetLeft(element: horizGrid, length: x_);

    if (showHorizontalLine)
    {
      var yValue_ = mousePosInData_.Y;
      text_ = null;
      if (yTextMapping != null)
      {
        text_ = yTextMapping(arg: yValue_);
      }

      text_ ??= GetRoundedValue(min: visible_.YMin, max: visible_.YMax, value: yValue_);

      if (!string.IsNullOrEmpty(value: customYFormat))
      {
        text_ = string.Format(format: customYFormat, arg0: text_);
      }

      vertTextBlock.Text = text_;
    }

    // by default verticalGrid is positioned on the top of line.
    var height_ = vertGrid.ActualHeight;
    var y_ = mousePos.Y - blockShift.Y - height_;
    if (y_ < output_.Top)
    {
      y_ = mousePos.Y + blockShift.Y;
    }

    Canvas.SetTop(element: vertGrid, length: y_);

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
    var graph_ = (CursorCoordinateGraph)d;
    graph_.UpdateUIRepresentation(mousePos: (Point)e.NewValue);
  }

  private string GetRoundedValue(double min, double max, double value)
  {
    var roundedValue_ = value;
    var log_ = RoundingHelper.GetDifferenceLog(min: min, max: max);
    var format_ = "G3";
    var diff_ = Math.Abs(value: max - min);
    
    if (diff_ is > 1E3 and < 1E6)
    {
      format_ = "F0";
    }
   
    if (log_ < 0)
    {
      format_ = "G" + (-log_ + 2);
    }

    return roundedValue_.ToString(format: format_);
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
    var graph_ = (CursorCoordinateGraph)d;
    if ((bool)e.NewValue)
    {
      graph_.UpdateUIRepresentation();
    }
    else
    {
      graph_.vertLine.ClearValue(dp: Shape.StrokeDashOffsetProperty);
      graph_.horizLine.ClearValue(dp: Shape.StrokeDashOffsetProperty);
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
