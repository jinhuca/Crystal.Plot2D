using Crystal.Plot2D.Common;
using System;
using System.Collections.Generic;

namespace Crystal.Plot2D.Charts;

public abstract class TimeTicksProviderBase<T> : ITicksProvider<T>
{
  public event EventHandler Changed;
  protected void RaiseChanged()
  {
    if (Changed != null)
    {
      Changed(sender: this, e: EventArgs.Empty);
    }
  }

  private static readonly Dictionary<DifferenceIn, ITicksProvider<T>> providers =
      new();

  protected static Dictionary<DifferenceIn, ITicksProvider<T>> Providers => providers;

  private static readonly Dictionary<DifferenceIn, ITicksProvider<T>> minorProviders =
      new();

  protected static Dictionary<DifferenceIn, ITicksProvider<T>> MinorProviders => minorProviders;

  protected abstract TimeSpan GetDifference(T start, T end);

  #region ITicksProvider<T> Members

  private IDateTimeTicksStrategy strategy = new DefaultDateTimeTicksStrategy();
  public IDateTimeTicksStrategy Strategy
  {
    get => strategy;
    set
    {
      if (strategy != value)
      {
        strategy = value;
        RaiseChanged();
      }
    }
  }

  private ITicksInfo<T> result;
  private DifferenceIn diff;

  public ITicksInfo<T> GetTicks(Range<T> range, int ticksCount)
  {
    Verify.IsTrue(condition: ticksCount > 0);

    T start = range.Min;
    T end = range.Max;
    TimeSpan length = GetDifference(start: start, end: end);

    diff = strategy.GetDifference(span: length);

    TicksInfo<T> result = new() { Info = diff };
    if (providers.TryGetValue(key: diff, value: out var provider))
    {
      ITicksInfo<T> innerResult = provider.GetTicks(range: range, ticksCount: ticksCount);
      T[] ticks = ModifyTicksGuard(ticks: innerResult.Ticks, info: diff);

      result.Ticks = ticks;
      this.result = result;
      return result;
    }

    throw new InvalidOperationException(message: Strings.Exceptions.UnsupportedRangeInAxis);
  }

  private T[] ModifyTicksGuard(T[] ticks, object info)
  {
    var result = ModifyTicks(ticks: ticks, info: info);
    if (result == null)
    {
      throw new ArgumentNullException(paramName: "ticks");
    }

    return result;
  }

  protected virtual T[] ModifyTicks(T[] ticks, object info)
  {
    return ticks;
  }

  /// <summary>
  /// Decreases the tick count.
  /// </summary>
  /// <param name="tickCount">The tick count.</param>
  /// <returns></returns>
  public int DecreaseTickCount(int ticksCount)
  {
    if (providers.TryGetValue(diff, out var provider))
    {
      return provider.DecreaseTickCount(ticksCount: ticksCount);
    }

    int res = ticksCount / 2;
    if (res < 2)
    {
      res = 2;
    }

    return res;
  }

  /// <summary>
  /// Increases the tick count.
  /// </summary>
  /// <param name="ticksCount">The tick count.</param>
  /// <returns></returns>
  public int IncreaseTickCount(int ticksCount)
  {
    DebugVerify.Is(condition: ticksCount < 2000);

    if (providers.TryGetValue(diff, out var provider))
    {
      return provider.IncreaseTickCount(ticksCount: ticksCount);
    }

    return ticksCount * 2;
  }

  public ITicksProvider<T> MinorProvider
  {
    get
    {
      DifferenceIn smallerDiff = DifferenceIn.Smallest;
      if (strategy.TryGetLowerDiff(diff: diff, lowerDiff: out smallerDiff) && minorProviders.TryGetValue(smallerDiff, out var provider))
      {
        var minorProvider = (MinorTimeProviderBase<T>)provider;
        minorProvider.SetTicks(ticks: result.Ticks);
        return minorProvider;
      }

      return null;
      // todo What to do if this already is the smallest provider?
    }
  }

  public ITicksProvider<T> MajorProvider
  {
    get
    {
      DifferenceIn biggerDiff = DifferenceIn.Smallest;
      if (strategy.TryGetBiggerDiff(diff: diff, biggerDiff: out biggerDiff))
      {
        return providers[key: biggerDiff];
      }

      return null;
      // todo What to do if this already is the biggest provider?
    }
  }

  #endregion
}
