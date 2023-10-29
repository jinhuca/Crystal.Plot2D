using Crystal.Plot2D.Common;
using System.Windows;
using System.Windows.Data;

namespace Crystal.Plot2D.LegendItems;

public static class LegendItemsHelper
{
  public static LegendItem BuildDefaultLegendItem(IPlotterElement chart)
  {
    LegendItem result_ = new();
    SetCommonBindings(legendItem: result_, chart: chart);
    return result_;
  }

  public static void SetCommonBindings(LegendItem legendItem, object chart)
  {
    legendItem.DataContext = chart;
    legendItem.SetBinding(
      dp: Legend.VisualContentProperty,
      binding: new Binding { Path = new PropertyPath(path: "(0)", pathParameters: Legend.VisualContentProperty) });
    legendItem.SetBinding(
      dp: Legend.DescriptionProperty, 
      binding: new Binding { Path = new PropertyPath(path: "(0)", pathParameters: Legend.DescriptionProperty) });
  }
}