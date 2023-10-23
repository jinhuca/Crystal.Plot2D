﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Crystal.Plot2D.Charts;

/// <summary>
/// Draws one isoline line through mouse position.
/// </summary>
public partial class IsolineTrackingGraph : IsolineGraphBase
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

  Point prevMousePos;

  protected override void OnPlotterAttached()
  {
    UIElement parent = (UIElement)Parent;
    parent.MouseMove += parent_MouseMove;

    UpdateUIRepresentation();
  }

  protected override void OnPlotterDetaching()
  {
    UIElement parent = (UIElement)Parent;
    parent.MouseMove -= parent_MouseMove;
  }

  private void parent_MouseMove(object sender, MouseEventArgs e)
  {
    Point mousePos = e.GetPosition(relativeTo: this);
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

    Rect output = Plotter2D.Viewport.Output;

    Point mousePos = Mouse.GetPosition(relativeTo: this);
    if (!output.Contains(point: mousePos))
    {
      return;
    }

    var transform = Plotter2D.Viewport.Transform;
    Point visiblePoint = mousePos.ScreenToData(transform: transform);
    DataRect visible = Plotter2D.Viewport.Visible;

    double isolineLevel;
    if (Search(pt: visiblePoint, foundVal: out isolineLevel))
    {
      var collection = IsolineBuilder.BuildIsoline(level: isolineLevel);

      string format = "G3";
      if (Math.Abs(value: isolineLevel) < 1000)
      {
        format = "F";
      }

      textBlock.Text = isolineLevel.ToString(format: format);

      double x = mousePos.X + labelShift.X;
      if (x + labelGrid.ActualWidth > output.Right)
      {
        x = mousePos.X - labelShift.X - labelGrid.ActualWidth;
      }

      double y = mousePos.Y - labelShift.Y - labelGrid.ActualHeight;
      if (y < output.Top)
      {
        y = mousePos.Y + labelShift.Y;
      }

      Canvas.SetLeft(element: labelGrid, length: x);
      Canvas.SetTop(element: labelGrid, length: y);

      foreach (LevelLine segment in collection.Lines)
      {
        StreamGeometry streamGeom = new();
        using (StreamGeometryContext context = streamGeom.Open())
        {
          Point startPoint = segment.StartPoint.DataToScreen(transform: transform);
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

  int foundI;
  int foundJ;
  Quad foundQuad;
  private bool Search(Point pt, out double foundVal)
  {
    var grid = DataSource.Grid;

    foundVal = 0;

    int width = DataSource.Width;
    int height = DataSource.Height;
    bool found = false;
    int i = 0, j = 0;
    for (i = 0; i < width - 1; i++)
    {
      for (j = 0; j < height - 1; j++)
      {
        Quad quad = new(
        v00: grid[i, j],
        v01: grid[i, j + 1],
        v11: grid[i + 1, j + 1],
        v10: grid[i + 1, j]);
        if (quad.Contains(pt: pt))
        {
          found = true;
          foundQuad = quad;
          foundI = i;
          foundJ = j;

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

    double x = pt.X;
    double y = pt.Y;
    Vector A = grid[i, j + 1].ToVector();         // @TODO: in common case add a sorting of points:
    Vector B = grid[i + 1, j + 1].ToVector();       //   maxA ___K___ B
    Vector C = grid[i + 1, j].ToVector();         //      |         |
    Vector D = grid[i, j].ToVector();           //      M    P    N
    double a = data[i, j + 1];            //		|         |
    double b = data[i + 1, j + 1];          //		В ___L____Сmin
    double c = data[i + 1, j];
    double d = data[i, j];

    Vector K, L;
    double k, l;
    if (x >= A.X)
    {
      k = Interpolate(v0: A, v1: B, u0: a, u1: b, a: K = new Vector(x: x, y: GetY(v0: A, v1: B, x: x)));
    }
    else
    {
      k = Interpolate(v0: D, v1: A, u0: d, u1: a, a: K = new Vector(x: x, y: GetY(v0: D, v1: A, x: x)));
    }

    if (x >= C.X)
    {
      l = Interpolate(v0: C, v1: B, u0: c, u1: b, a: L = new Vector(x: x, y: GetY(v0: C, v1: B, x: x)));
    }
    else
    {
      l = Interpolate(v0: D, v1: C, u0: d, u1: c, a: L = new Vector(x: x, y: GetY(v0: D, v1: C, x: x)));
    }

    foundVal = Interpolate(v0: L, v1: K, u0: l, u1: k, a: new Vector(x: x, y: y));
    return !double.IsNaN(d: foundVal);
  }

  private double Interpolate(Vector v0, Vector v1, double u0, double u1, Vector a)
  {
    Vector l1 = a - v0;
    Vector l = v1 - v0;

    double res = (u1 - u0) / l.Length * l1.Length + u0;
    return res;
  }

  private double GetY(Vector v0, Vector v1, double x)
  {
    double res = v0.Y + (v1.Y - v0.Y) / (v1.X - v0.X) * (x - v0.X);
    return res;
  }
}
