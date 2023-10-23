using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Charts;

public class GenericLocationalLabelProvider<TItem, TAxis> : LabelProviderBase<TAxis>
{
  private readonly IList<TItem> collection;
  private readonly Func<TItem, string> displayMemberMapping;

  public GenericLocationalLabelProvider(IList<TItem> collection, Func<TItem, string> displayMemberMapping)
  {
    if (collection == null)
    {
      throw new ArgumentNullException(paramName: "collection");
    }

    if (displayMemberMapping == null)
    {
      throw new ArgumentNullException(paramName: "displayMemberMapping");
    }

    this.collection = collection;
    this.displayMemberMapping = displayMemberMapping;
  }

  int startIndex;
  public override UIElement[] CreateLabels(ITicksInfo<TAxis> ticksInfo)
  {
    var ticks = ticksInfo.Ticks;

    if (ticks.Length == 0)
    {
      return EmptyLabelsArray;
    }

    startIndex = (int)ticksInfo.Info;

    UIElement[] result = new UIElement[ticks.Length];

    LabelTickInfo<TAxis> labelInfo = new() { Info = ticksInfo.Info };

    for (int i = 0; i < result.Length; i++)
    {
      var tick = ticks[i];
      labelInfo.Tick = tick;
      labelInfo.Index = i;

      string labelText = GetString(tickInfo: labelInfo);

      TextBlock label = new() { Text = labelText };

      ApplyCustomView(info: labelInfo, label: label);

      result[i] = label;
    }

    return result;
  }

  protected override string GetStringCore(LabelTickInfo<TAxis> tickInfo)
  {
    return displayMemberMapping(arg: collection[index: tickInfo.Index + startIndex]);
  }
}
