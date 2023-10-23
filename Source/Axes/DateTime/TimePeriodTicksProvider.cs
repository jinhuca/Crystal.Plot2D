using Crystal.Plot2D.Common;
using System;
using System.Collections.Generic;

namespace Crystal.Plot2D.Charts;

internal abstract class TimePeriodTicksProvider<T> : ITicksProvider<T>
{
  public event EventHandler Changed;
  protected void RaiseChanged()
  {
    if (Changed != null)
    {
      Changed(sender: this, e: EventArgs.Empty);
    }
  }

  protected abstract T RoundUp(T time, DifferenceIn diff);
  protected abstract T RoundDown(T time, DifferenceIn diff);

  private bool differenceInited;
  private DifferenceIn difference;
  protected DifferenceIn Difference
  {
    get
    {
      if (!differenceInited)
      {
        difference = GetDifferenceCore();
        differenceInited = true;
      }
      return difference;
    }
  }
  protected abstract DifferenceIn GetDifferenceCore();

  private int[] tickCounts;
  protected int[] TickCounts
  {
    get
    {
      if (tickCounts == null)
      {
        tickCounts = GetTickCountsCore();
      }

      return tickCounts;
    }
  }
  protected abstract int[] GetTickCountsCore();

  public int DecreaseTickCount(int ticksCount)
  {
    if (ticksCount > TickCounts[0])
    {
      return TickCounts[0];
    }

    for (int i = 0; i < TickCounts.Length; i++)
    {
      if (ticksCount > TickCounts[i])
      {
        return TickCounts[i];
      }
    }

    return TickCounts.Last();
  }

  public int IncreaseTickCount(int ticksCount)
  {
    if (ticksCount >= TickCounts[0])
    {
      return TickCounts[0];
    }

    for (int i = TickCounts.Length - 1; i >= 0; i--)
    {
      if (ticksCount < TickCounts[i])
      {
        return TickCounts[i];
      }
    }

    return TickCounts.Last();
  }

  protected abstract int GetSpecificValue(T start, T dt);
  protected abstract T GetStart(T start, int value, int step);
  protected abstract bool IsMinDate(T dt);
  protected abstract T AddStep(T dt, int step);

  public ITicksInfo<T> GetTicks(Range<T> range, int ticksCount)
  {
    T start = range.Min;
    T end = range.Max;
    DifferenceIn diff = Difference;
    start = RoundDown(start: start, end: end);
    end = RoundUp(start: start, end: end);

    RoundingInfo bounds = RoundingHelper.CreateRoundedRange(
      min: GetSpecificValue(start: start, dt: start),
      max: GetSpecificValue(start: start, dt: end));

    int delta = (int)(bounds.Max - bounds.Min);
    if (delta == 0)
    {
      return new TicksInfo<T> { Ticks = new[] { start } };
    }

    int step = delta / ticksCount;

    if (step == 0)
    {
      step = 1;
    }

    T tick = GetStart(start: start, value: (int)bounds.Min, step: step);
    bool isMinDateTime = IsMinDate(dt: tick) && step != 1;
    if (isMinDateTime)
    {
      step--;
    }

    List<T> ticks = new();
    T finishTick = AddStep(dt: range.Max, step: step);
    while (Continue(current: tick, end: finishTick))
    {
      ticks.Add(item: tick);
      tick = AddStep(dt: tick, step: step);
      if (isMinDateTime)
      {
        isMinDateTime = false;
        step++;
      }
    }

    ticks = Trim(ticks: ticks, range: range);

    TicksInfo<T> res = new() { Ticks = ticks.ToArray(), Info = diff };
    return res;
  }

  protected abstract bool Continue(T current, T end);

  protected abstract T RoundUp(T start, T end);

  protected abstract T RoundDown(T start, T end);

  protected abstract List<T> Trim(List<T> ticks, Range<T> range);

  public ITicksProvider<T> MinorProvider => throw new NotSupportedException();

  public ITicksProvider<T> MajorProvider => throw new NotSupportedException();
}

internal abstract class DatePeriodTicksProvider : TimePeriodTicksProvider<DateTime>
{
  protected sealed override bool Continue(DateTime current, DateTime end)
  {
    return current < end;
  }

  protected sealed override List<DateTime> Trim(List<DateTime> ticks, Range<DateTime> range)
  {
    int startIndex = 0;
    for (int i = 0; i < ticks.Count - 1; i++)
    {
      if (ticks[index: i] <= range.Min && range.Min <= ticks[index: i + 1])
      {
        startIndex = i;
        break;
      }
    }

    int endIndex = ticks.Count - 1;
    for (int i = ticks.Count - 1; i >= 1; i--)
    {
      if (ticks[index: i] >= range.Max && range.Max > ticks[index: i - 1])
      {
        endIndex = i;
        break;
      }
    }

    List<DateTime> res = new(capacity: endIndex - startIndex + 1);
    for (int i = startIndex; i <= endIndex; i++)
    {
      res.Add(item: ticks[index: i]);
    }

    return res;
  }

  protected sealed override DateTime RoundUp(DateTime start, DateTime end)
  {
    bool isPositive = (end - start).Ticks > 0;
    return isPositive ? SafelyRoundUp(dt: end) : RoundDown(time: end, diff: Difference);
  }

  private DateTime SafelyRoundUp(DateTime dt)
  {
    if (AddStep(dt: dt, step: 1) == DateTime.MaxValue)
    {
      return DateTime.MaxValue;
    }

    return RoundUp(dateTime: dt, diff: Difference);
  }

  protected sealed override DateTime RoundDown(DateTime start, DateTime end)
  {
    bool isPositive = (end - start).Ticks > 0;
    return isPositive ? RoundDown(time: start, diff: Difference) : SafelyRoundUp(dt: start);
  }

  protected sealed override DateTime RoundDown(DateTime time, DifferenceIn diff)
  {
    DateTime res = time;

    switch (diff)
    {
      case DifferenceIn.Year:
        res = new DateTime(year: time.Year, month: 1, day: 1);
        break;
      case DifferenceIn.Month:
        res = new DateTime(year: time.Year, month: time.Month, day: 1);
        break;
      case DifferenceIn.Day:
        res = time.Date;
        break;
      case DifferenceIn.Hour:
        res = time.Date.AddHours(value: time.Hour);
        break;
      case DifferenceIn.Minute:
        res = time.Date.AddHours(value: time.Hour).AddMinutes(value: time.Minute);
        break;
      case DifferenceIn.Second:
        res = time.Date.AddHours(value: time.Hour).AddMinutes(value: time.Minute).AddSeconds(value: time.Second);
        break;
      case DifferenceIn.Millisecond:
        res = time.Date.AddHours(value: time.Hour).AddMinutes(value: time.Minute).AddSeconds(value: time.Second).AddMilliseconds(value: time.Millisecond);
        break;
    }

    DebugVerify.Is(condition: res <= time);

    return res;
  }

  protected override DateTime RoundUp(DateTime dateTime, DifferenceIn diff)
  {
    DateTime res = RoundDown(time: dateTime, diff: diff);

    switch (diff)
    {
      case DifferenceIn.Year:
        res = res.AddYears(value: 1);
        break;
      case DifferenceIn.Month:
        res = res.AddMonths(months: 1);
        break;
      case DifferenceIn.Day:
        res = res.AddDays(value: 1);
        break;
      case DifferenceIn.Hour:
        res = res.AddHours(value: 1);
        break;
      case DifferenceIn.Minute:
        res = res.AddMinutes(value: 1);
        break;
      case DifferenceIn.Second:
        res = res.AddSeconds(value: 1);
        break;
      case DifferenceIn.Millisecond:
        res = res.AddMilliseconds(value: 1);
        break;
    }

    return res;
  }
}
