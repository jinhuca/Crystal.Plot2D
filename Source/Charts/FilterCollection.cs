using Crystal.Plot2D.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;

namespace Crystal.Plot2D.Charts;

/// <summary>
///   Represents a collection of point filters of <see cref="LineGraph"/>.
/// </summary>
public sealed class FilterCollection : NotifiableCollection<IPointsFilter>
{
  protected override void OnItemAdding(IPointsFilter item)
  {
    if (item == null)
    {
      throw new ArgumentNullException(paramName: "item");
    }
  }

  protected override void OnItemAdded(IPointsFilter item)
  {
    item.Changed += OnItemChanged;
  }

  private void OnItemChanged(object sender, EventArgs e)
  {
    OnCollectionChanged(e: new NotifyCollectionChangedEventArgs(action: NotifyCollectionChangedAction.Reset));
  }

  protected override void OnItemRemoving(IPointsFilter item)
  {
    item.Changed -= OnItemChanged;
  }

  internal List<Point> Filter(List<Point> points, Rect screenRect)
  {
    foreach (var filter in Items)
    {
      filter.SetScreenRect(screenRect: screenRect);
      points = filter.Filter(points: points);
    }

    return points;
  }
}
