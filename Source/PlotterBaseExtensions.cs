﻿using Crystal.Plot2D.Charts;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.DataSources.OneDimensional;
using Crystal.Plot2D.Descriptions;
using Crystal.Plot2D.Filters;
using Crystal.Plot2D.Graphs;
using Crystal.Plot2D.LegendItems;
using Crystal.Plot2D.Navigation;
using Crystal.Plot2D.PointMarkers;

namespace Crystal.Plot2D;

/// <summary>
/// Extensions for <see cref="PlotterBase"/> - simplified methods to add line and marker charts.
/// </summary>
public static class PlotterBaseExtensions
{
  #region [-- Cursor Coordinate Graphs --]

  public static void AddCursor(this PlotterBase plotter, CursorCoordinateGraph cursorGraph)
  {
    plotter.Children.Add(content: cursorGraph);
  }

  #endregion [-- Cursor Coordinate Graphs --]

  #region [-- Line graphs --]

  public static LineGraph AddLineGraph(this PlotterBase plotter, IPointDataSource pointDataSource)
  {
    return AddLineGraph(
      plotter,
      pointDataSource,
      new Pen { Brush = new SolidColorBrush(Colors.Black), Thickness = 1 },
      new PenDescription(nameof(pointDataSource)));
  }

  /// <summary>
  /// Extended method to add a LineGraph with a PointDataSource, and optional pen parameters.
  /// </summary>
  /// <param name="plotter">Host Plotter</param>
  /// <param name="pointSource">Data Source</param>
  /// <param name="linePen">Optional OutlinePen</param>
  /// <param name="descriptionForPen">Optional descriptionForPen for OutlinePen</param>
  /// <returns>LineGraph</returns>
  public static LineGraph AddLineGraph(
    this PlotterBase plotter, 
    IPointDataSource pointSource, 
    Pen linePen, 
    PenDescription descriptionForPen)
  {
    ArgumentNullException.ThrowIfNull(pointSource);
    linePen ??= new Pen { Brush = new SolidColorBrush(color: Colors.Black), Thickness = 1 };
    descriptionForPen ??= new PenDescription(description: nameof(pointSource));
    var lineGraph_ = new LineGraph
    {
      DataSource = pointSource,
      LinePen = linePen,
      Description = descriptionForPen
    };
    Legend.SetDescription(obj: lineGraph_, value: descriptionForPen.Brief);
    lineGraph_.Filters.Add(item: new FrequencyFilter());
    plotter.Children.Add(content: lineGraph_);
    return lineGraph_;
  }

  #endregion [-- Line graphs --]

  #region [-- MarkerPointsGraph --]

  /// <summary>
  /// Extension method to add a MarkerPointPoint with a PointDataSource and parameters.
  /// </summary>
  /// <param name="plotter">Host Plotter</param>
  /// <param name="pointSource">Data Source</param>
  /// <param name="marker">Marker to add</param>
  /// <param name="description">Description</param>
  /// <returns></returns>
  public static MarkerPointsGraph AddMarkerPointsGraph(
    this PlotterBase plotter, 
    IPointDataSource pointSource,
    PointMarker marker = default, 
    Description description = default)
  {
    ArgumentNullException.ThrowIfNull(pointSource);
    marker ??= new CirclePointMarker();
    var markerPointGraph_ = new MarkerPointsGraph
    {
      DataSource = pointSource, 
      Marker = marker, 
      Description = description
    };
    plotter.Children.Add(content: markerPointGraph_);
    return markerPointGraph_;
  }

  public static MarkerPointsGraph AddMarkerGraph<TMarker>(
    this PlotterBase plotter, 
    IPointDataSource pointSource,
    TMarker marker = default, 
    Description description = default) 
    where TMarker : PointMarker
  {
    var res_ = new MarkerPointsGraph();
    switch (marker)
    {
      case CirclePointMarker s_:
        break;
      case TrianglePointMarker t_:
        break;
    }
    
    return res_;
  }

  #endregion [-- MarkerPointsGraph --]

  #region [-- LineAndMarker graphs --]

  /// <summary>
  /// Adds one dimensional graph to plotter. This method allows you to specify
  /// as much graph parameters as possible.
  /// </summary>
  /// <param name="plotter"></param>
  /// <param name="pointSource">Source of points to plot</param>
  /// <param name="penForDrawingLine">OutlinePen to draw the line. If pen is null no lines will be drawn</param>
  /// <param name="marker">Marker to draw on points. If marker is null no points will be drawn</param>
  /// <param name="description">Description of graph to put in legend</param>
  /// <returns></returns>
  [SuppressMessage(category: "Microsoft.Design", checkId: "CA1011:ConsiderPassingBaseTypesAsParameters")]
  public static LineAndMarker<MarkerPointsGraph> AddLineAndMarkerGraph(
    this PlotterBase plotter,
    IPointDataSource pointSource,
    Pen penForDrawingLine,
    PointMarker marker,
    Description description)
  {
    ArgumentNullException.ThrowIfNull(pointSource);

    var res_ = new LineAndMarker<MarkerPointsGraph>();

    if (penForDrawingLine != null) // We are requested to draw line graphs
    {
      LineGraph graph_ = new()
      {
        DataSource = pointSource,
        LinePen = penForDrawingLine
      };
      if (description != null)
      {
        Legend.SetDescription(obj: graph_, value: description.Brief);
        graph_.Description = description;
      }
      if (marker == null)
      {
        // Add inclination filter only to graphs without markers
        // graph.Filters.Add(new InclinationFilter());
      }

      res_.LineGraph = graph_;

      graph_.Filters.Add(item: new FrequencyFilter());
      plotter.Children.Add(content: graph_);
    }

    if (marker != null) // We are requested to draw marker graphs
    {
      MarkerPointsGraph markerGraph_ = new()
      {
        DataSource = pointSource,
        Marker = marker
      };

      res_.MarkerGraph = markerGraph_;

      plotter.Children.Add(content: markerGraph_);
    }

    return res_;
  }

  /// <summary>
  /// Adds one dimensional graph to plotter. This method allows you to specify
  /// as much graph parameters as possible.
  /// </summary>
  /// <param name="plotter"></param>
  /// <param name="pointSource">Source of points to plot</param>
  /// <param name="marker">Marker to draw on points. If marker is null no points will be drawn</param>
  /// <param name="description">Description of graph to put in legend</param>
  /// <returns></returns>
  [SuppressMessage(category: "Microsoft.Design", checkId: "CA1011:ConsiderPassingBaseTypesAsParameters")]
  public static ElementMarkerPointsGraph AddElementMarkerPointsGraph(
    this PlotterBase plotter,
    IPointDataSource pointSource,
    ElementPointMarker marker,
    Description description)
  {
    ArgumentNullException.ThrowIfNull(pointSource);
    ArgumentNullException.ThrowIfNull(marker);

    //if (penForDrawingLine != null) // We are requested to draw line graphs
    //{
    //  LineGraph graph = new LineGraph
    //  {
    //    DataSource = pointSource,
    //    LinePen = penForDrawingLine
    //  };
    //  if (description != null)
    //  {
    //    Legend.SetDescription(graph, description.Brief);
    //    graph.Description = description;
    //  }
    //  if (marker == null)
    //  {
    //    // Add inclination filter only to graphs without markers
    //    // graph.Filters.Add(new InclinationFilter());
    //  }

    //  graph.Filters.Add(new FrequencyFilter());

    //  res.LineGraph = graph;

    //  plotter.Children.Add(graph);
    //}

    var markerGraph_ = new ElementMarkerPointsGraph
    {
      DataSource = pointSource,
      Marker = marker,
      Description = description
    };
    plotter.Children.Add(content: markerGraph_);
    return markerGraph_;
  }

  #endregion [-- LineAndMarker graphs --]

  #region Attaching LineGraphs

  public static void AttachLineGraph(this PlotterBase plotter, IPointDataSource pointSource, Pen linePen, PointMarker marker, Description desc)
  {
  }

  #endregion Attaching LineGraphs w/o Markers
}
