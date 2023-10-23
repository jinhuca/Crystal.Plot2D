using System;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Charts;

/// <summary>
/// Represents a label provider for <see cref="System.DateTime"/> ticks.
/// </summary>
public class DateTimeLabelProvider : DateTimeLabelProviderBase
{
  /// <summary>
  /// Initializes a new instance of the <see cref="DateTimeLabelProvider"/> class.
  /// </summary>
  public DateTimeLabelProvider() { }

  public override UIElement[] CreateLabels(ITicksInfo<DateTime> ticksInfo)
  {
    object info = ticksInfo.Info;
    var ticks = ticksInfo.Ticks;

    if (info is DifferenceIn diff)
    {
      DateFormat = GetDateFormat(diff: diff);
    }

    LabelTickInfo<DateTime> tickInfo = new() { Info = info };

    UIElement[] res = new UIElement[ticks.Length];
    for (int i = 0; i < ticks.Length; i++)
    {
      tickInfo.Tick = ticks[i];

      string tickText = GetString(tickInfo: tickInfo);
      UIElement label = new TextBlock { Text = tickText, ToolTip = ticks[i] };
      ApplyCustomView(info: tickInfo, label: label);
      res[i] = label;
    }

    return res;
  }
}
