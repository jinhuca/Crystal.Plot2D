using Crystal.Plot2D;
using Crystal.Plot2D.DataSources;
using System;
using System.Windows;
using System.Windows.Media;
using Crystal.Plot2D.Charts;

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
      const int N = 900;
      double[] xA = new double[N];
      double[] yA = new double[N];

      for (int i = 0; i < N; i++)
      {
        xA[i] = .01 * i;
        yA[i] = Math.Cos(xA[i]);
      }

      var xDataSource = new EnumerableDataSource<double>(xA) { XMapping = x => x };

      var yDataSource = new EnumerableDataSource<double>(yA) { YMapping = y => y };

      CompositeDataSource compositeDataSource = new(xDataSource, yDataSource);
      plotter.AddLineGraph(compositeDataSource, new Pen(Brushes.Blue, 3), new PenDescription("Cos"));
      
    }
  }
}
