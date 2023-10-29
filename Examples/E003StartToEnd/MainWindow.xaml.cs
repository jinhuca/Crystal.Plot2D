using Crystal.Plot2D;
using Crystal.Plot2D.DataSources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.DataSources.OneDimensional;
using Crystal.Plot2D.Descriptions;

namespace E003StartToEnd
{
  public partial class MainWindow
  {
    private ObservableCollection<Point> _points;
    private List<Point> _dataStore;
    private int _startPoint = 0;

    private readonly DispatcherTimer _dispatcherTimer = new() { Interval = TimeSpan.FromSeconds(0.5) };

    public MainWindow()
    {
      InitializeComponent();
      InitializePoints();
      Loaded += MainWindow_Loaded;
    }

    private void InitializePoints()
    {
      _points = new ObservableCollection<Point>();
      _dataStore = new List<Point>
      {
        new Point(0.0, 37.0),
        new Point(0.1, 36.5),
        new Point(0.2, 36.0),
        new Point(0.3, 36.0),

        new Point(0.4, 35.9),
        new Point(0.5, 35.8),
        new Point(0.6, 35.5),
        new Point(0.7, 35.3),

        new Point(0.8, 35.1),
        new Point(0.9, 34.6),
        new Point(1.0, 34.0),
        new Point(1.1, 23.5),

        new Point(1.2, 22.8),
        new Point(1.3, 21.7),
        new Point(1.4, 20.6),
        new Point(1.5, 20.1),

        new Point(1.6, 19.8),
        new Point(1.7, 18.7),
        new Point(1.8, 17.6),
        new Point(1.9, 16.8),

        new Point(2.0, 15.8),
        new Point(2.1, 15.8),
        new Point(2.2, 15.3),
        new Point(2.3, 15.3),

        new Point(2.4, 15.9),
        new Point(2.5, 12.9),
        new Point(2.6, 12.0),
        new Point(2.7, 12.1),

        new Point(2.8, 11.2),
        new Point(2.9, 11.3),
        new Point(3.0, 10.4),
        new Point(3.1, 9.5),


        new Point(3.2, 9.2),
        new Point(3.3, 8.3),
        new Point(3.4, 7.4),
        new Point(3.5, 6.5),

        new Point(3.6, 6.0),
        new Point(3.7, 5.6),
        new Point(3.8, 5.0),
        new Point(3.9, 4.5),

        new Point(4.0, 4.1),
        new Point(4.1, 3.5),
        new Point(4.2, 3.0),
        new Point(4.3, 2.5),

        new Point(4.4, 2.0),
        new Point(4.5, 1.6),
        new Point(4.6, 0.8),
        new Point(4.7, 0.2),

        new Point(4.8, -1.2),
        new Point(4.9, -2.0),
        new Point(5.0, -2.8),
        new Point(5.1, -3.5),

        new Point(5.2, -4.2),
        new Point(5.3, -5.1),
        new Point(5.4, -5.8),
        new Point(5.5, -6.3),

      };
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
      _plotter.Viewport.UseApproximateContentBoundsComparison = false;
      _plotter.MainHorizontalAxis.Foreground = Brushes.YellowGreen;
      _plotter.MainHorizontalAxis.FontSize = 20;
      _plotter.MainHorizontalAxis.FontFamily = new FontFamily("Impact");
      _plotter.MainHorizontalAxis.Background = Brushes.Transparent;

      _plotter.MainVerticalAxis.Foreground = Brushes.LightGray;
      _plotter.MainVerticalAxis.FontSize = 20;
      _plotter.MainVerticalAxis.FontFamily = new FontFamily("Impact");
      
      var pen_ = new Pen { Brush = new SolidColorBrush(Colors.OrangeRed), Thickness = 5 };
      var lineGraph_ = _plotter.AddLineGraph(_points.AsDataSource(), pen_, new PenDescription("Ablation"));

      var rec_ = new Rect(new Point(0, -75), new Size(245, 125));
      _plotter.Visible = new DataRect(rec_); // new DataRect(xMin: 0, yMin: 0, width: 400, height: 15);

      //_plotter.FitToView();
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
      if (_startPoint < _dataStore.Count)
      {
        _points.Add(_dataStore[_startPoint++]);
      }
    }
  }
}
