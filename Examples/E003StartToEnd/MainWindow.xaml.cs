using Crystal.Plot2D;
using Crystal.Plot2D.DataSources;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace E003StartToEnd
{
  public partial class MainWindow
  {
    private readonly ObservableCollection<Point> _points = new();
    private int _startPoint = 0;
    private readonly DispatcherTimer _dispatcherTimer = new() { Interval = TimeSpan.FromSeconds(0.1) };

    public MainWindow()
    {
      InitializeComponent();
      Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
      AddNextPoint();
      _plotter.Viewport.UseApproximateContentBoundsComparison = false;
      var lineGraph_ = _plotter.AddLineGraph(_points.AsDataSource());

      _plotter.FitToView();
      Viewport2D.SetUsesApproximateContentBoundsComparison(lineGraph_, false);
      _dispatcherTimer.Tick += _dispatcherTimer_Tick;
      _dispatcherTimer.Start();

    }

    private void _dispatcherTimer_Tick(object? sender, EventArgs e)
    {
      AddNextPoint();
      if(_points.Count > 100)
        _points.RemoveAt(0);
      //_plotter.FitToView();
    }

    private void AddNextPoint()
    {
      var p_ = new Point
      {
        X = 0.05 * _startPoint++,
        Y = Math.Sin(_startPoint) * 5
      };
      _points.Add(p_);
    }
  }
}
