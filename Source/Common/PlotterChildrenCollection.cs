using Crystal.Plot2D.Charts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;

namespace Crystal.Plot2D.Common;

/// <summary>
/// Contains all charts added to Plotter.
/// </summary>
[ContentWrapper(typeof(ViewportUIContainer))]
public sealed class PlotterChildrenCollection : NotifiableCollection<IPlotterElement>, IList
{
  /// <summary>
  /// Initializes a new instance of the <see cref="PlotterChildrenCollection"/> class.
  /// </summary>
  internal PlotterChildrenCollection(PlotterBase plotter)
  {
    Plotter = plotter ?? throw new ArgumentNullException(nameof(plotter));
  }

  public PlotterBase Plotter { get; }

  /// <summary>
  /// Called before item added to collection. Enables to perform validation.
  /// </summary>
  /// <param name="item">The adding item.</param>
  protected override void OnItemAdding(IPlotterElement item)
  {
    if (item == null)
    {
      throw new ArgumentNullException(nameof(item));
    }
  }

  /// <summary>
  /// This override enables notifying about removing each element, instead of
  /// notifying about collection reset.
  /// </summary>
  protected override void ClearItems()
  {
    var items = new List<IPlotterElement>(Items);
    foreach (var item in items)
    {
      Remove(item);
    }
  }

  #region Foreign content

  public void Add(FrameworkElement content)
  {
    if (content == null)
    {
      throw new ArgumentNullException(nameof(content));
    }

    if (content is IPlotterElement plotterElement)
    {
      Add(plotterElement);
    }
    else
    {
      ViewportUIContainer container = new(content);
      Add(container);
    }
  }

  #endregion // end of Foreign content

  #region IList Members

  int IList.Add(object value)
  {
    switch (value)
    {
      case null:
        throw new ArgumentNullException(nameof(value));
      case FrameworkElement content:
        Add(content);
        return 0;
      case IPlotterElement element:
        Add(element);
        return 0;
      default:
        break;
    }

    throw new ArgumentException($"Children of type '{value.GetType()}' are not supported.");
  }

  void IList.Clear() => Clear();

  bool IList.Contains(object value)
  {
    return value is IPlotterElement element && Contains(element);
  }

  int IList.IndexOf(object value)
  {
    if (value is IPlotterElement element)
    {
      return IndexOf(element);
    }

    return -1;
  }

  void IList.Insert(int index, object value)
  {
    if (value is IPlotterElement element)
    {
      Insert(index, element);
    }
  }

  bool IList.IsFixedSize => false;

  bool IList.IsReadOnly => false;

  void IList.Remove(object value)
  {
    if (value is IPlotterElement element)
    {
      Remove(element);
    }
  }

  void IList.RemoveAt(int index) => RemoveAt(index);

  object IList.this[int index]
  {
    get
    {
      return this[index];
    }
    set
    {
      if (value is IPlotterElement element)
      {
        this[index] = element;
      }
    }
  }

  #endregion

  #region ICollection Members

  void ICollection.CopyTo(Array array, int index)
  {
    if (array is IPlotterElement[] elements)
    {
      CopyTo(elements, index);
    }
  }

  int ICollection.Count => Count;
  bool ICollection.IsSynchronized => false;
  object ICollection.SyncRoot => null;

  #endregion

  #region IEnumerable Members

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

  #endregion
}
