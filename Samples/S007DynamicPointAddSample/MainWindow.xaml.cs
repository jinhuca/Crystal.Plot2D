using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using Crystal.Plot2D;
using Crystal.Plot2D.DataSources;

namespace S007DynamicPointAddSample;

public partial class MainWindow
{
  public MainWindow()
  {
    InitializeComponent();
    Loaded += MainWindow_Loaded;
  }

  private DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
  private ObservableCollection<Point> _data = new ObservableCollection<Point>();
  private int x = 0;
  private Random rnd = new Random();

  private void MainWindow_Loaded(object sender, RoutedEventArgs e)
  {
    for (int i = 0; i < 170; i++)
    {
      AddNextPoint();
    }

    // switching off approximate content bound's comparison, as this can cause improper behavior.
    plotter.Viewport.UseApproximateContentBoundsComparison = false;

    // adding line chart to plotter
    var line_ = plotter.AddLineGraph(_data.AsDataSource());

    Viewport2D.SetUsesApproximateContentBoundsComparison(line_, false);

    _timer.Tick += timer_tick;
    _timer.Start();
  }
    
  void timer_tick(object sender, EventArgs e)
  {
    AddNextPoint();
    _data.RemoveAt(0);
    plotter.FitToView();
  }

  private void AddNextPoint()
  {
    var p = new Point
    {
      X = x++, 
      Y = 0.1 * Math.Sqrt(x) * Math.Cos(x)
    };
    _data.Add(p);
  }
}