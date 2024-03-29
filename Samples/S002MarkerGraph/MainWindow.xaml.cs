﻿using Crystal.Plot2D;
using Crystal.Plot2D.Charts;
using Crystal.Plot2D.DataSources;
using System;
using System.Data;
using System.Windows;
using System.Windows.Media;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.DataSources.OneDimensional;
using Crystal.Plot2D.Navigation;
using Crystal.Plot2D.PointMarkers;

namespace S002MarkerGraph
{
  public partial class MainWindow
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      var table_ = new DataTable();
      table_.Columns.Add("Sine", typeof(double));
      table_.Columns.Add("Time", typeof(DateTime));
      table_.Columns.Add("Index", typeof(int));
      table_.Columns.Add("Sqrt", typeof(double));
      table_.Columns.Add("Cosine", typeof(double));

      for (int i = 0; i < 1000; i++)
      {
        table_.Rows.Add(
          Math.Sin(i / 100.0), 
          DateTime.Now + new TimeSpan(0, 0, i), 
          i,
          Math.Sqrt(i / 100.0),
          Math.Cos(i / 100.0));
      }

      var data1 = new TableDataSource(table_)
      {
        XMapping = row => ((DateTime)row["Time"] - (DateTime)table_.Rows[0][1]).TotalSeconds,
        YMapping = row => 10 * (double)row["Sine"]
      };

      // Map HSB color computes from "Index" column to dependency property Brush of marker
      data1.AddMapping(ShapePointMarker.FillBrushProperty, row => new SolidColorBrush(new HsbColor(15 * (int)row["Index"], 1, 1).ToArgbColor()));

      // Map "Sqrt" based values to marker size
      data1.AddMapping(ShapePointMarker.DiameterProperty, row => 3 * (double)row["Sqrt"]);

      // Plot first graph
      plotter.AddMarkerPointsGraph(data1);

      // Plot second graph
      var data2 = new TableDataSource(table_)
      {
        XMapping = row => ((DateTime)row["Time"] - (DateTime)table_.Rows[0][1]).TotalSeconds,
        YMapping = row => 10 * (double)row["Cosine"]
      };

      data2.AddMapping(ShapePointMarker.FillBrushProperty, row => new SolidColorBrush(new HsbColor(15 * (int)row["Index"], 1, 1).ToArgbColor()));
      data2.AddMapping(ShapePointMarker.DiameterProperty, row => 3 * (double)row["Sqrt"]);

      var circleMarker = new CirclePointMarker()
      {
        OutlinePen = new Pen { Brush = new SolidColorBrush(Colors.Black) }
      };

      var triangleMarker = new TrianglePointMarker()
      {
        OutlinePen = new Pen { Brush = new SolidColorBrush(Colors.Black) }
      };
      triangleMarker.OutlinePen.Freeze();

      var rectangleMarker = new RectanglePointMarker()
      {
        OutlinePen = new Pen { Brush = new SolidColorBrush(Colors.Black) }
      };

      plotter.AddMarkerPointsGraph(pointSource: data2, triangleMarker);

      plotter.AddCursor(new CursorCoordinateGraph());
    }
  }
}
