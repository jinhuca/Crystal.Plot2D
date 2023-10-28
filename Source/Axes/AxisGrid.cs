﻿using Crystal.Plot2D.Charts;
using Crystal.Plot2D.Common;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace Crystal.Plot2D;

/// <summary>
/// Draws grid over viewport. Number of 
/// grid lines depends on Plotter's MainHorizontalAxis and MainVerticalAxis ticks.
/// </summary>
public class AxisGrid : ContentControl, IPlotterElement
{
  static AxisGrid()
  {
    Type thisType = typeof(AxisGrid);
    Panel.ZIndexProperty.OverrideMetadata(forType: thisType, typeMetadata: new FrameworkPropertyMetadata(defaultValue: -1));
  }

  [SuppressMessage(category: "Microsoft.Performance", checkId: "CA1822:MarkMembersAsStatic")]
  internal void BeginTicksUpdate()
  {
  }
  internal void EndTicksUpdate()
  {
    UpdateUIRepresentation();
  }

  protected internal MinorTickInfo<double>[] MinorHorizontalTicks { get; set; }

  protected internal MinorTickInfo<double>[] MinorVerticalTicks { get; set; }

  protected internal double[] HorizontalTicks { get; set; }

  protected internal double[] VerticalTicks { get; set; }

  private bool drawVerticalTicks = true;

  /// <summary>
  /// Gets or sets a value indicating whether to draw vertical tick lines.
  /// </summary>
  /// <value><c>true</c> if draw vertical ticks; otherwise, <c>false</c>.</value>
  public bool DrawVerticalTicks
  {
    get => drawVerticalTicks;
    set
    {
      if (drawVerticalTicks != value)
      {
        drawVerticalTicks = value;
        UpdateUIRepresentation();
      }
    }
  }

  private bool drawHorizontalTicks = true;
  /// <summary>
  /// Gets or sets a value indicating whether to draw horizontal tick lines.
  /// </summary>
  /// <value><c>true</c> if draw horizontal ticks; otherwise, <c>false</c>.</value>
  public bool DrawHorizontalTicks
  {
    get => drawHorizontalTicks;
    set
    {
      if (drawHorizontalTicks != value)
      {
        drawHorizontalTicks = value;
        UpdateUIRepresentation();
      }
    }
  }

  private bool drawHorizontalMinorTicks;
  /// <summary>
  /// Gets or sets a value indicating whether to draw horizontal minor ticks.
  /// </summary>
  /// <value>
  /// 	<c>true</c> if draw horizontal minor ticks; otherwise, <c>false</c>.
  /// </value>
  public bool DrawHorizontalMinorTicks
  {
    get => drawHorizontalMinorTicks;
    set
    {
      if (drawHorizontalMinorTicks != value)
      {
        drawHorizontalMinorTicks = value;
        UpdateUIRepresentation();
      }
    }
  }

  private bool drawVerticalMinorTicks;
  /// <summary>
  /// Gets or sets a value indicating whether to draw vertical minor ticks.
  /// </summary>
  /// <value>
  /// 	<c>true</c> if draw vertical minor ticks; otherwise, <c>false</c>.
  /// </value>
  public bool DrawVerticalMinorTicks
  {
    get => drawVerticalMinorTicks;
    set
    {
      if (drawVerticalMinorTicks != value)
      {
        drawVerticalMinorTicks = value;
        UpdateUIRepresentation();
      }
    }
  }

  private readonly double gridBrushThickness = 1;

  public Path GridPath { get; set; } = new();

  private readonly Canvas canvas = new();

  /// <summary>
  /// Initializes a new instance of the <see cref="AxisGrid"/> class.
  /// </summary>
  public AxisGrid()
  {
    IsHitTestVisible = false;

    canvas.ClipToBounds = true;

    GridPath.Stroke = Brushes.LightGray;
    GridPath.StrokeThickness = gridBrushThickness;
    GridPath.StrokeDashArray = new DoubleCollection { 2 };

    Content = canvas;
  }

  private readonly ResourcePool<LineGeometry> lineGeometryPool = new();
  private readonly ResourcePool<Line> linePool = new();

  private void UpdateUIRepresentation()
  {
    foreach (UIElement item in canvas.Children)
    {
      if (item is Line line)
      {
        linePool.Put(item: line);
      }
    }

    canvas.Children.Clear();
    Size size = RenderSize;

    DrawMinorHorizontalTicks();
    DrawMinorVerticalTicks();

    if (GridPath.Data is GeometryGroup prevGroup)
    {
      foreach (LineGeometry geometry in prevGroup.Children)
      {
        lineGeometryPool.Put(item: geometry);
      }
    }

    GeometryGroup group = new();
    if (HorizontalTicks != null && drawHorizontalTicks)
    {
      double minY = 0;
      double maxY = size.Height;

      for (int i = 0; i < HorizontalTicks.Length; i++)
      {
        double screenX = HorizontalTicks[i];
        LineGeometry line = lineGeometryPool.GetOrCreate();
        line.StartPoint = new Point(x: screenX, y: minY);
        line.EndPoint = new Point(x: screenX, y: maxY);
        group.Children.Add(value: line);
      }
    }

    if (VerticalTicks != null && drawVerticalTicks)
    {
      double minX = 0;
      double maxX = size.Width;

      for (int i = 0; i < VerticalTicks.Length; i++)
      {
        double screenY = VerticalTicks[i];
        LineGeometry line = lineGeometryPool.GetOrCreate();
        line.StartPoint = new Point(x: minX, y: screenY);
        line.EndPoint = new Point(x: maxX, y: screenY);
        group.Children.Add(value: line);
      }
    }

    canvas.Children.Add(element: GridPath);
    GridPath.Data = group;
  }

  private void DrawMinorVerticalTicks()
  {
    Size size = RenderSize;
    if (MinorVerticalTicks != null && drawVerticalMinorTicks)
    {
      double minX = 0;
      double maxX = size.Width;

      for (int i = 0; i < MinorVerticalTicks.Length; i++)
      {
        double screenY = MinorVerticalTicks[i].Tick;
        if (screenY < 0)
        {
          continue;
        }

        if (screenY > size.Height)
        {
          continue;
        }

        Line line = linePool.GetOrCreate();

        line.Y1 = screenY;
        line.Y2 = screenY;
        line.X1 = minX;
        line.X2 = maxX;
        line.Stroke = Brushes.LightGray;
        line.StrokeThickness = MinorVerticalTicks[i].Value * gridBrushThickness;

        canvas.Children.Add(element: line);
      }
    }
  }

  private void DrawMinorHorizontalTicks()
  {
    Size size = RenderSize;
    if (MinorHorizontalTicks != null && drawHorizontalMinorTicks)
    {
      double minY = 0;
      double maxY = size.Height;

      for (int i = 0; i < MinorHorizontalTicks.Length; i++)
      {
        double screenX = MinorHorizontalTicks[i].Tick;
        if (screenX < 0)
        {
          continue;
        }

        if (screenX > size.Width)
        {
          continue;
        }

        Line line = linePool.GetOrCreate();
        line.X1 = screenX;
        line.X2 = screenX;
        line.Y1 = minY;
        line.Y2 = maxY;
        line.Stroke = Brushes.LightGray;
        line.StrokeThickness = MinorHorizontalTicks[i].Value * gridBrushThickness;

        canvas.Children.Add(element: line);
      }
    }
  }

  #region IPlotterElement Members

  void IPlotterElement.OnPlotterAttached(PlotterBase plotter)
  {
    this.plotter = plotter;
    plotter.CentralGrid.Children.Add(element: this);
  }

  void IPlotterElement.OnPlotterDetaching(PlotterBase plotter)
  {
    plotter.CentralGrid.Children.Remove(element: this);
    this.plotter = null;
  }

  private PlotterBase plotter;
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public PlotterBase Plotter => plotter;

  #endregion
}