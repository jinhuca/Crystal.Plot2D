using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Axes.TimeSpan;

public sealed class TimeSpanLabelProvider : LabelProviderBase<System.TimeSpan>
{
  public override UIElement[] CreateLabels(ITicksInfo<System.TimeSpan> ticksInfo)
  {
    var info = ticksInfo.Info;
    var ticks = ticksInfo.Ticks;

    LabelTickInfo<System.TimeSpan> tickInfo = new();

    var res = new UIElement[ticks.Length];
    for (var i = 0; i < ticks.Length; i++)
    {
      tickInfo.Tick = ticks[i];
      tickInfo.Info = info;

      var tickText = GetString(tickInfo: tickInfo);
      UIElement label = new TextBlock { Text = tickText, ToolTip = ticks[i] };
      res[i] = label;
    }
    return res;
  }
}
