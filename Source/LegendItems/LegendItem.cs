using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Charts;

public class LegendItem : Control
{
  static LegendItem()
  {
    var thisType = typeof(LegendItem);
    DefaultStyleKeyProperty.OverrideMetadata(forType: thisType, typeMetadata: new FrameworkPropertyMetadata(defaultValue: thisType));
  }

  //public object VisualContent
  //{
  //    get { return Legend.GetVisualContent(this); }
  //    set { Legend.SetVisualContent(this, value); }
  //}

  //[Bindable(true)]
  //public object Description
  //{
  //    get { return Legend.GetDescription(this); }
  //    set { Legend.SetDescription(this, value); }
  //}
}
