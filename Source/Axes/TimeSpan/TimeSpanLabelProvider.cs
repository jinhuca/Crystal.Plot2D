using System;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Charts
{
  public class TimeSpanLabelProvider : LabelProviderBase<TimeSpan>
  {
    public override UIElement[] CreateLabels(ITicksInfo<TimeSpan> ticksInfo)
    {
      object info = ticksInfo.Info;
      var ticks = ticksInfo.Ticks;

      LabelTickInfo<TimeSpan> tickInfo = new();

      UIElement[] res = new UIElement[ticks.Length];
      for (int i = 0; i < ticks.Length; i++)
      {
        tickInfo.Tick = ticks[i];
        tickInfo.Info = info;

        string tickText = GetString(tickInfo);
        UIElement label = new TextBlock { Text = tickText, ToolTip = ticks[i] };
        res[i] = label;
      }
      return res;
    }
  }
}
