using Crystal.Plot2D;
using Crystal.Plot2D.DataSources;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace S000Animation
{
  public partial class MainWindow : Window
  {
    double phase = 0;
    private readonly double[] animatedX = new double[1000];
    private readonly double[] animatedY = new double[1000];
    private EnumerableDataSource<double> animatedDataSource = null;

    private readonly Header chartHeader = new Header();
    private TextBlock headerContent = new TextBlock();
    private readonly DispatcherTimer timer = new DispatcherTimer();

    public MainWindow()
    {
      InitializeComponent();
      InitializeChart();
    }

    private void InitializeChart()
    {
      headerContent = new TextBlock
      {
        FontSize = 24,
        Text = "Phase = 0.00",
        HorizontalAlignment = HorizontalAlignment.Center
      };
      chartHeader.Content = headerContent;
      plotter.Children.Add(chartHeader);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      for(int i = 0; i < animatedX.Length; i++)
      {
        animatedX[i] = 2 * Math.PI * i / animatedX.Length;
        animatedY[i] = Math.Sin(animatedX[i]);
      }
      var xSrc = new EnumerableDataSource<double>(animatedX);
      xSrc.XMapping = x => x;
      //xSrc.SetXMapping(x => x);
      animatedDataSource = new EnumerableDataSource<double>(animatedY);
      //animatedDataSource.SetYMapping(y => y);
      animatedDataSource.YMapping = y => y;

      var lineGraph_ = new LineGraph();

      plotter.AddLineGraph(
        new CompositeDataSource(xSrc, animatedDataSource),
        new Pen(Brushes.Magenta, 3),
        new PenDescription("Sin(x + phase)"));

      timer.Interval = TimeSpan.FromMilliseconds(10);
      timer.Tick += AnimatedPlot_Timer;
      timer.IsEnabled = true;

      plotter.FitToView();
    }

    private void AnimatedPlot_Timer(object? sender, EventArgs e)
    {
      phase += 0.01;
      if(phase > 2 * Math.PI)
      {
        phase -= 2 * Math.PI;
      }
      for(var i = 0; i < animatedX.Length; i++)
      {
        animatedY[i] = Math.Cos(animatedX[i] + phase);
      }

      animatedDataSource.RaiseDataChanged();
      headerContent.Text = string.Format(CultureInfo.InvariantCulture, "Phase = {0:N2}", phase);
    }
  }
}
