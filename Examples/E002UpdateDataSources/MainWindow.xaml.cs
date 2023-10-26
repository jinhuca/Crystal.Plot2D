using Crystal.Plot2D.DataSources;
using Crystal.Plot2D;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace E002UpdateDataSources
{
  public partial class MainWindow
  {
    private readonly DispatcherTimer _timer;

    public MainWindow()
    {
      InitializeComponent();
      _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1), IsEnabled = true };
      _timer.Tick += _timer_Tick;
    }

    private void _timer_Tick(object? sender, EventArgs e)
    {
    }

    private CompositeDataSource compositeDataSource;

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
      InitializePlotter();
    }

    private void InitializePlotter()
    {
      Application.Current.Dispatcher.BeginInvoke(() =>
      {
        const int n = 500;
        var x_ = new double[n];
        var y_ = new double[n];

        for(var i_ = 0; i_ < n; i_++)
        {
          x_[i_] = i_ * 0.05;
          y_[i_] = Math.Sin(x_[i_]) * 5;
        }

        var xDataSource_ = x_.AsXDataSource();
        var yDataSource_ = y_.AsYDataSource();
        compositeDataSource = new CompositeDataSource(xDataSource_, yDataSource_);
        var pen_ = new Pen { Brush = new SolidColorBrush(Colors.OrangeRed), Thickness = 1.5 };
        var penDescription_ = new PenDescription("Sine");

        _plotter.AddLineGraph(
          compositeDataSource,
          pen_,
          penDescription_
        );

        _plotter.FitToView();
      });
    }
  }
}
