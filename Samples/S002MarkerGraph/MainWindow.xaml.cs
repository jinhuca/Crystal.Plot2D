using Crystal.Plot2D;
using Crystal.Plot2D.Charts;
using Crystal.Plot2D.DataSources;
using System;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace S002MarkerGraph
{
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      // Prepare data in arrays
      const int N = 120;
      double[] x = new double[N];
      double[] cs = new double[N];
      double[] sn = new double[N];

      for (int i = 0; i < N; i++)
      {
        x[i] = i * .1;
        cs[i] = Math.Sin(x[i]);
        sn[i] = Math.Cos(x[i]);
      }

      // Add data sources:
      // 3 partial data sources, containing each of arrays
      var snDataSource = new EnumerableDataSource<double>(sn)
      {
        //snDataSource.SetYMapping(y => y);
        YMapping = y => y
      };

      var xDataSource = new EnumerableDataSource<double>(x)
      {
        XMapping = lx => lx
      };
      //xDataSource.SetXMapping(lx => lx);

      var csDataSource = new EnumerableDataSource<double>(cs)
      {
        YMapping = y => y
      };
      //csDataSource.SetYMapping(y => y);

      var csqDataSource = new EnumerableDataSource<double>(cs)
      {
        //csqDataSource.SetYMapping(y => y * y);
        YMapping = y => y * y
      };

      // 2 composite data sources and 2 charts respectively:
      //  creating composite data source
      CompositeDataSource compositeDataSource1 = new(xDataSource, csDataSource);
      // adding graph to plotter

      plotter.AddLineGraph(
        compositeDataSource1,
        new Pen(Brushes.DarkGoldenrod, 1),
        new PenDescription("Sin"));

      plotter.AddCursor(new CursorCoordinateGraph() { LineStroke = Brushes.Red, LineStrokeThickness = 0.5 });
      plotter.MainCanvas.Background = Brushes.Transparent;
      plotter.MainCanvas.Opacity = 1;

      // creating composite data source for cs values
      CompositeDataSource compositeDataSource2 = new(xDataSource, csDataSource);

      // Adding second graph to plotter
      // plotter.AddLineGraph(compositeDataSource2, new OutlinePen(Brushes.Blue, 3), new PenDescription("Cos"));

      // creating composite data source for cs^2 values
      CompositeDataSource compositeDataSource3 = new(xDataSource, csqDataSource);

      // Adding thirs graph to plotter
      Pen dashed = new(Brushes.Magenta, 6)
      {
        DashStyle = DashStyles.Dot
      };
      //plotter.AddLineGraph(compositeDataSource3, dashed, new PenDescription("Cos^2"));

      var marker = new CirclePointMarker()
      {
        FillBrush = new SolidColorBrush(Colors.Red),
        OutlinePen = new Pen { Brush = new SolidColorBrush(Colors.Blue) },
        Diameter = 10
      };
      plotter.AddMarkerPointsGraph(compositeDataSource1, marker, new PenDescription("Cos^2"));

      // Force everything plotted to be visible
      plotter.FitToView();
    }
  }
}
