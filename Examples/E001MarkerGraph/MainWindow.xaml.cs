using Crystal.Plot2D;
using Crystal.Plot2D.Charts;
using Crystal.Plot2D.DataSources;
using System;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace E001MarkerGraph
{
  public partial class MainWindow
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private const int Count = 500;
    private const string TimeColumn = "Time";
    private const string CosineColumn = "Cosine";
    private const string IndexColumn = "Index";
    private const string SqrtColumn = "Sqrt";
    private const string SineColumn = "Sine";

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
      var table_ = new DataTable();
      table_.Columns.Add(IndexColumn, typeof(int));
      table_.Columns.Add(TimeColumn, typeof(DateTime));
      table_.Columns.Add(CosineColumn, typeof(double));
      table_.Columns.Add(SqrtColumn, typeof(double));
      table_.Columns.Add(SineColumn, typeof(double));

      for(int i_ = 0; i_ < Count; i_++)
      {
        table_.Rows.Add(
          i_,
          DateTime.Now + new TimeSpan(0, 0, i_),
          Math.Cos(i_ / 50.0),
          Math.Sqrt(i_ / 50.0),
          Math.Sin(i_ / 30.0)
        );
      }

      var dataSource_ = new TableDataSource(table_)
      {
        XMapping = row => ((DateTime)row[TimeColumn] - (DateTime)table_.Rows[0][1]).TotalSeconds,
        YMapping = row => 10 * (double)row[CosineColumn]
      };

      dataSource_.AddMapping(
        ShapePointMarker.FillBrushProperty, 
        row => new SolidColorBrush(new HsbColor((int)row[IndexColumn], 1, 1).ToArgbColor()));

      dataSource_.AddMapping(
        ShapePointMarker.DiameterProperty, 
        row => 2 * (double)row[SqrtColumn]);

      _plotter.AddMarkerPointsGraph(dataSource_);

      var triangleMarker_ = new TrianglePointMarker
      {
        //OutlinePen = new Pen { Brush = new SolidColorBrush(Colors.Red) },
      };

      //_plotter.AddMarkerPointsGraph(pointSource: dataSource_, triangleMarker_);
      _plotter.AddCursor(new CursorCoordinateGraph());
    }
  }
}
