using Crystal.Plot2D.Common;
using System;
using System.Collections.Generic;

namespace Crystal.Plot2D.Charts
{
  public abstract class TimeTicksProviderBase<T> : ITicksProvider<T>
  {
    public event EventHandler Changed;
    protected void RaiseChanged()
    {
      if (Changed != null)
      {
        Changed(this, EventArgs.Empty);
      }
    }

    private static readonly Dictionary<DifferenceIn, ITicksProvider<T>> providers =
        new();

    protected static Dictionary<DifferenceIn, ITicksProvider<T>> Providers
    {
      get { return providers; }
    }

    private static readonly Dictionary<DifferenceIn, ITicksProvider<T>> minorProviders =
        new();

    protected static Dictionary<DifferenceIn, ITicksProvider<T>> MinorProviders
    {
      get { return minorProviders; }
    }

    protected abstract TimeSpan GetDifference(T start, T end);

    #region ITicksProvider<T> Members

    private IDateTimeTicksStrategy strategy = new DefaultDateTimeTicksStrategy();
    public IDateTimeTicksStrategy Strategy
    {
      get { return strategy; }
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
      Verify.IsTrue(ticksCount > 0);

      T start = range.Min;
      T end = range.Max;
      TimeSpan length = GetDifference(start, end);

      diff = strategy.GetDifference(length);

      TicksInfo<T> result = new() { Info = diff };
      if (providers.ContainsKey(diff))
      {
        ITicksInfo<T> innerResult = providers[diff].GetTicks(range, ticksCount);
        T[] ticks = ModifyTicksGuard(innerResult.Ticks, diff);

        result.Ticks = ticks;
        this.result = result;
        return result;
      }

      throw new InvalidOperationException(Strings.Exceptions.UnsupportedRangeInAxis);
    }

    private T[] ModifyTicksGuard(T[] ticks, object info)
    {
      var result = ModifyTicks(ticks, info);
      if (result == null)
      {
        throw new ArgumentNullException("ticks");
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
      if (providers.ContainsKey(diff))
      {
        return providers[diff].DecreaseTickCount(ticksCount);
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
      DebugVerify.Is(ticksCount < 2000);

      if (providers.ContainsKey(diff))
      {
        return providers[diff].IncreaseTickCount(ticksCount);
      }

      return ticksCount * 2;
    }

    public ITicksProvider<T> MinorProvider
    {
      get
      {
        DifferenceIn smallerDiff = DifferenceIn.Smallest;
        if (strategy.TryGetLowerDiff(diff, out smallerDiff) && minorProviders.ContainsKey(smallerDiff))
        {
          var minorProvider = (MinorTimeProviderBase<T>)minorProviders[smallerDiff];
          minorProvider.SetTicks(result.Ticks);
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
        if (strategy.TryGetBiggerDiff(diff, out biggerDiff))
        {
          return providers[biggerDiff];
        }

        return null;
        // todo What to do if this already is the biggest provider?
      }
    }

    #endregion
  }
}
