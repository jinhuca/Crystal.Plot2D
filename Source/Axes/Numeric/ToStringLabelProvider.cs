using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Charts;

/// <summary>
/// Represents a simple label provider for double ticks, which simply returns result of .ToString() method, called for rounded ticks.
/// </summary>
public class ToStringLabelProvider : NumericLabelProviderBase
{
  /// <summary>
  /// Initializes a new instance of the <see cref="ToStringLabelProvider"/> class.
  /// </summary>
  public ToStringLabelProvider() { }

  public override UIElement[] CreateLabels(ITicksInfo<double> ticksInfo)
  {
    var ticks = ticksInfo.Ticks;

    Init(ticks: ticks);

    UIElement[] res = new UIElement[ticks.Length];
    LabelTickInfo<double> tickInfo = new() { Info = ticksInfo.Info };
    for (int i = 0; i < res.Length; i++)
    {
      tickInfo.Tick = ticks[i];
      tickInfo.Index = i;

      string labelText = GetString(tickInfo: tickInfo);

      TextBlock label = (TextBlock)GetResourceFromPool();
      if (label == null)
      {
        label = new TextBlock();
      }

      label.Text = labelText;
      label.ToolTip = ticks[i].ToString(provider: CultureInfo.InvariantCulture);

      res[i] = label;

      ApplyCustomView(info: tickInfo, label: label);
    }
    return res;
  }
}
