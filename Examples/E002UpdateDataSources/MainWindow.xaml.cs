using Crystal.Plot2D.DataSources;
using Crystal.Plot2D;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Crystal.Plot2D.DataSources.OneDimensional;
using Crystal.Plot2D.Descriptions;

namespace E002UpdateDataSources
{
  public partial class MainWindow
  {
    private readonly DispatcherTimer _timer;
    private ObservableCollection<Point> _data = new();
    private EnumerableDataSource<double> _xDataSource;
    private EnumerableDataSource<double> _yDataSource;
    private CompositeDataSource _compositeDataSource;

    public MainWindow()
    {
      InitializeComponent();
      _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1), IsEnabled = true };
      _timer.Tick += _timer_Tick;
    }

    private void _timer_Tick(object? sender, EventArgs e)
    {
    }

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

        _xDataSource = x_.AsXDataSource();
        _yDataSource = y_.AsYDataSource();
        _compositeDataSource = new CompositeDataSource(_xDataSource, _yDataSource);

        var pen_ = new Pen { Brush = new SolidColorBrush(Colors.OrangeRed), Thickness = 1.5 };
        var penDescription_ = new PenDescription("Sine");

        _plotter.AddLineGraph(
          _compositeDataSource,
          pen_,
          penDescription_
        );

        _plotter.FitToView();
      });
    }

  }
}
