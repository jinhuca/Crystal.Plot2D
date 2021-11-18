using Crystal.Plot2D;
using Crystal.Plot2D.DataSources;
using System;
using System.Windows;

namespace S001LineGraph
{
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      const int N = 100;
      double[] xA = new double[N];
      double[] yA = new double[N];

      for (int i = 0; i < N; i++)
      {
        xA[i] = (double)i;
        yA[i] = (double).1* Math.Sin(xA[i]);
      }

      var xDataSource = new EnumerableDataSource<double>(xA)
      {
        XMapping = x => x
      };

      var yDataSource = new EnumerableDataSource<double>(yA)
      {
        YMapping = y => 2 * y
      };

      CompositeDataSource compositeDataSource = new(xDataSource, yDataSource);

      plotter.AddLineGraph(compositeDataSource);

    }
  }
}
