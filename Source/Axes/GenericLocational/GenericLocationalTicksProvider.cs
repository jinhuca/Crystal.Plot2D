using Crystal.Plot2D.Common;
using System;
using System.Collections.Generic;

namespace Crystal.Plot2D.Charts;

public class GenericLocationalTicksProvider<TCollection, TAxis> : ITicksProvider<TAxis> where TAxis : IComparable<TAxis>
{
  private IList<TCollection> collection;
  public IList<TCollection> Collection
  {
    get => collection;
    set
    {
      if (value == null)
      {
        throw new ArgumentNullException(paramName: "value");
      }

      Changed.Raise(sender: this);
      collection = value;
    }
  }

  private Func<TCollection, TAxis> axisMapping;
  public Func<TCollection, TAxis> AxisMapping
  {
    get => axisMapping;
    set
    {
      if (value == null)
      {
        throw new ArgumentNullException(paramName: "value");
      }

      Changed.Raise(sender: this);
      axisMapping = value;
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="GenericLocationalTicksProvider&lt;T&gt;"/> class.
  /// </summary>
  public GenericLocationalTicksProvider() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="GenericLocationalTicksProvider&lt;T&gt;"/> class.
  /// </summary>
  /// <param name="collection">The collection of axis ticks and labels.</param>
  public GenericLocationalTicksProvider(IList<TCollection> collection)
  {
    Collection = collection;
  }

  public GenericLocationalTicksProvider(IList<TCollection> collection, Func<TCollection, TAxis> coordinateMapping)
  {
    Collection = collection;
    AxisMapping = coordinateMapping;
  }

  #region ITicksProvider<T> Members

  SearchResult1d minResult = SearchResult1d.Empty;
  SearchResult1d maxResult = SearchResult1d.Empty;
  GenericSearcher1d<TCollection, TAxis> searcher;
  /// <summary>
  /// Generates ticks for given range and preferred ticks count.
  /// </summary>
  /// <param name="range">The range.</param>
  /// <param name="ticksCount">The ticks count.</param>
  /// <returns></returns>
  public ITicksInfo<TAxis> GetTicks(Range<TAxis> range, int ticksCount)
  {
    EnsureSearcher();

    //minResult = searcher.SearchBetween(range.Min, minResult);
    //maxResult = searcher.SearchBetween(range.Max, maxResult);

    minResult = searcher.SearchFirstLess(x: range.Min);
    maxResult = searcher.SearchGreater(x: range.Max);

    if (!(minResult.IsEmpty && maxResult.IsEmpty))
    {
      int startIndex = !minResult.IsEmpty ? minResult.Index : 0;
      int endIndex = !maxResult.IsEmpty ? maxResult.Index : collection.Count - 1;

      int count = endIndex - startIndex + 1;

      TAxis[] ticks = new TAxis[count];
      for (int i = startIndex; i <= endIndex; i++)
      {
        ticks[i - startIndex] = axisMapping(arg: collection[index: i]);
      }

      TicksInfo<TAxis> result = new()
      {
        Info = startIndex,
        TickSizes = ArrayExtensions.CreateArray(length: count, defaultValue: 1.0),
        Ticks = ticks
      };

      return result;
    }
    else
    {
      return TicksInfo<TAxis>.Empty;
    }
  }

  private void EnsureSearcher()
  {
    if (searcher == null)
    {
      if (collection == null || axisMapping == null)
      {
        throw new InvalidOperationException(message: Strings.Exceptions.GenericLocationalProviderInvalidState);
      }

      searcher = new GenericSearcher1d<TCollection, TAxis>(_collection: collection, _selector: axisMapping);
    }
  }

  public int DecreaseTickCount(int ticksCount)
  {
    return collection.Count;
  }

  public int IncreaseTickCount(int ticksCount)
  {
    return collection.Count;
  }

  public ITicksProvider<TAxis> MinorProvider => null;

  public ITicksProvider<TAxis> MajorProvider => null;

  public event EventHandler Changed;

  #endregion
}
