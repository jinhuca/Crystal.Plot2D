using Crystal.Plot2D;
using Crystal.Plot2D.DataSources;
using System;
using System.Windows;
using System.Windows.Media;
using Crystal.Plot2D.DataSources.OneDimensional;
using Crystal.Plot2D.Descriptions;

namespace S001LineGraph;

public partial class MainWindow : Window
{
  public MainWindow()
  {
    InitializeComponent();
  }

  private void Window_Loaded(object sender, RoutedEventArgs e)
  {
    const int n = 1000;
    var xA_ = new double[n];
    var yA_ = new double[n];

    for(var i_ = 0; i_ < n; i_++)
    {
      xA_[i_] = .01 * i_;
      yA_[i_] = Math.Cos(xA_[i_]);
    }
    var xDataSource_ = new EnumerableDataSource<double>(xA_) { XMapping = x => x };
    var yDataSource_ = new EnumerableDataSource<double>(yA_) { YMapping = y => y };

    CompositeDataSource compositeDataSource_ = new(xDataSource_, yDataSource_);
    plotter.AddLineGraph(compositeDataSource_, new Pen(Brushes.Black, 3), new PenDescription("Cos"));
  }
}