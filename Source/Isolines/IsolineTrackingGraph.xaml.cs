using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Transforms;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Crystal.Plot2D.Isolines;

/// <summary>
/// Draws one isoline line through mouse position.
/// </summary>
public partial class IsolineTrackingGraph
{
  /// <summary>
  /// Initializes a new instance of the <see cref="IsolineTrackingGraph"/> class.
  /// </summary>
  public IsolineTrackingGraph()
  {
    InitializeComponent();
  }

  private Style pathStyle;
  
  /// <summary>
  /// Gets or sets style, applied to line path.
  /// </summary>
  /// <value>The path style.</value>
  public Style PathStyle
  {
    get => pathStyle;
    set
    {
      pathStyle = value;
      foreach (var path in addedPaths)
      {
        path.Style = pathStyle;
      }
    }
  }

  private Point prevMousePos;

  protected override void OnPlotterAttached()
  {
    var parent = (UIElement)Parent;
    parent.MouseMove += parent_MouseMove;

    UpdateUIRepresentation();
  }

  protected override void OnPlotterDetaching()
  {
    var parent = (UIElement)Parent;
    parent.MouseMove -= parent_MouseMove;
  }

  private void parent_MouseMove(object sender, MouseEventArgs e)
  {
    var mousePos = e.GetPosition(relativeTo: this);
    if (mousePos != prevMousePos)
    {
      prevMousePos = mousePos;

      UpdateUIRepresentation();
    }
  }

  protected override void UpdateDataSource()
  {
    IsolineBuilder.DataSource = DataSource;

    UpdateUIRepresentation();
  }

  protected override void OnViewportPropertyChanged(ExtendedPropertyChangedEventArgs e)
  {
    UpdateUIRepresentation();
  }

  private readonly List<Path> addedPaths = new();
  private Vector labelShift = new(x: 3, y: 3);
  private void UpdateUIRepresentation()
  {
    if (Plotter2D == null)
    {
      return;
    }

    foreach (var path in addedPaths)
    {
      content.Children.Remove(element: path);
    }
    addedPaths.Clear();

    if (DataSource == null)
    {
      labelGrid.Visibility = Visibility.Hidden;
      return;
    }

    var output = Plotter2D.Viewport.Output;

    var mousePos = Mouse.GetPosition(relativeTo: this);
    if (!output.Contains(point: mousePos))
    {
      return;
    }

    var transform = Plotter2D.Viewport.Transform;
    var visiblePoint = mousePos.ScreenToData(transform: transform);
    var visible = Plotter2D.Viewport.Visible;

    double isolineLevel;
    if (Search(pt: visiblePoint, foundVal: out isolineLevel))
    {
      var collection = IsolineBuilder.BuildIsoline(level: isolineLevel);

      var format = "G3";
      if (Math.Abs(value: isolineLevel) < 1000)
      {
        format = "F";
      }

      textBlock.Text = isolineLevel.ToString(format: format);

      var x = mousePos.X + labelShift.X;
      if (x + labelGrid.ActualWidth > output.Right)
      {
        x = mousePos.X - labelShift.X - labelGrid.ActualWidth;
      }

      var y = mousePos.Y - labelShift.Y - labelGrid.ActualHeight;
      if (y < output.Top)
      {
        y = mousePos.Y + labelShift.Y;
      }

      Canvas.SetLeft(element: labelGrid, length: x);
      Canvas.SetTop(element: labelGrid, length: y);

      foreach (var segment in collection.Lines)
      {
        StreamGeometry streamGeom = new();
        using (var context = streamGeom.Open())
        {
          var startPoint = segment.StartPoint.DataToScreen(transform: transform);
          var otherPoints = segment.OtherPoints.DataToScreenAsList(transform: transform);
          context.BeginFigure(startPoint: startPoint, isFilled: false, isClosed: false);
          context.PolyLineTo(points: otherPoints, isStroked: true, isSmoothJoin: true);
        }

        Path path = new()
        {
          Stroke = new SolidColorBrush(color: Palette.GetColor(t: segment.Value01)),
          Data = streamGeom,
          Style = pathStyle
        };
        content.Children.Add(element: path);
        addedPaths.Add(item: path);

        labelGrid.Visibility = Visibility.Visible;

        Binding pathBinding = new() { Path = new PropertyPath(path: "StrokeThickness"), Source = this };
        path.SetBinding(dp: Shape.StrokeThicknessProperty, binding: pathBinding);
      }
    }
    else
    {
      labelGrid.Visibility = Visibility.Hidden;
    }
  }

  private int foundI;
  private int foundJ;
  private Quad foundQuad;
  private bool Search(Point pt, out double foundVal)
  {
    var grid = DataSource.Grid;

    foundVal = 0;

    var width = DataSource.Width;
    var height = DataSource.Height;
    var found = false;
    int i_, j_ = 0;
    for (i_ = 0; i_ < width - 1; i_++)
    {
      for (j_ = 0; j_ < height - 1; j_++)
      {
        Quad quad = new(
        v00: grid[i_, j_],
        v01: grid[i_, j_ + 1],
        v11: grid[i_ + 1, j_ + 1],
        v10: grid[i_ + 1, j_]);
        if (quad.Contains(pt: pt))
        {
          found = true;
          foundQuad = quad;
          foundI = i_;
          foundJ = j_;

          break;
        }
      }
      if (found)
      {
        break;
      }
    }
    if (!found)
    {
      foundQuad = null;
      return false;
    }

    var data = DataSource.Data;

    var x = pt.X;
    var y = pt.Y;
    var A = grid[i_, j_ + 1].ToVector();         // @TODO: in common case add a sorting of points:
    var B = grid[i_ + 1, j_ + 1].ToVector();       //   maxA ___K___ B
    var C = grid[i_ + 1, j_].ToVector();         //      |         |
    var D = grid[i_, j_].ToVector();           //      M    P    N
    var a = data[i_, j_ + 1];            //		|         |
    var b = data[i_ + 1, j_ + 1];          //		В ___L____С min
    var c = data[i_ + 1, j_];
    var d = data[i_, j_];

    Vector K, L;
    double k, l;
    k = x >= A.X 
      ? Interpolate(v0: A, v1: B, u0: a, u1: b, a: K = new Vector(x: x, y: GetY(v0: A, v1: B, x: x)))
      : Interpolate(v0: D, v1: A, u0: d, u1: a, a: K = new Vector(x: x, y: GetY(v0: D, v1: A, x: x)));

    l = x >= C.X 
      ? Interpolate(v0: C, v1: B, u0: c, u1: b, a: L = new Vector(x: x, y: GetY(v0: C, v1: B, x: x))) 
      : Interpolate(v0: D, v1: C, u0: d, u1: c, a: L = new Vector(x: x, y: GetY(v0: D, v1: C, x: x)));

    foundVal = Interpolate(v0: L, v1: K, u0: l, u1: k, a: new Vector(x: x, y: y));
    return !double.IsNaN(d: foundVal);
  }

  private double Interpolate(Vector v0, Vector v1, double u0, double u1, Vector a)
  {
    var l1 = a - v0;
    var l = v1 - v0;

    var res = (u1 - u0) / l.Length * l1.Length + u0;
    return res;
  }

  private double GetY(Vector v0, Vector v1, double x)
  {
    var res = v0.Y + (v1.Y - v0.Y) / (v1.X - v0.X) * (x - v0.X);
    return res;
  }
}
