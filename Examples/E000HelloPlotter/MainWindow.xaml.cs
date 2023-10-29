using Crystal.Plot2D;
using Crystal.Plot2D.DataSources;
using System;
using System.Windows;
using System.Windows.Media;
using Crystal.Plot2D.DataSources.OneDimensional;
using Crystal.Plot2D.Descriptions;

namespace E00HelloPlotter
{
  public partial class MainWindow
  {
    public MainWindow()
    {
      InitializeComponent();
      Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
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
        var compositeDataSource_ = new CompositeDataSource(xDataSource_, yDataSource_);
        var pen_ = new Pen { Brush = new SolidColorBrush(Colors.OrangeRed), Thickness = 1.5 };
        var penDescription_ = new PenDescription("Sine");

        _plotter.AddLineGraph(
          compositeDataSource_,
          pen_,
          penDescription_
        );

        _plotter.FitToView();
      });
    }
  }
}
