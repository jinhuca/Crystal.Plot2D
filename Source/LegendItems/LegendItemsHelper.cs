using System.Windows;
using System.Windows.Data;

namespace Crystal.Plot2D.Charts;

public static class LegendItemsHelper
{
  public static LegendItem BuildDefaultLegendItem(IPlotterElement chart)
  {
    DependencyObject dependencyChart = (DependencyObject)chart;
    LegendItem result = new();
    SetCommonBindings(legendItem: result, chart: chart);
    return result;
  }

  public static void SetCommonBindings(LegendItem legendItem, object chart)
  {
    legendItem.DataContext = chart;
    legendItem.SetBinding(dp: Legend.VisualContentProperty, binding: new Binding { Path = new PropertyPath(path: "(0)", pathParameters: Legend.VisualContentProperty) });
    legendItem.SetBinding(dp: Legend.DescriptionProperty, binding: new Binding { Path = new PropertyPath(path: "(0)", pathParameters: Legend.DescriptionProperty) });
  }
}