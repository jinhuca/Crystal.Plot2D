using System.Collections.Generic;
using Crystal.Plot2D.Common;

namespace Crystal.Plot2D.Axes.TimeSpan;

internal abstract class TimeSpanTicksProviderBase : TimePeriodTicksProvider<System.TimeSpan>
{
  protected sealed override bool Continue(System.TimeSpan current, System.TimeSpan end)
  {
    return current < end;
  }

  protected sealed override System.TimeSpan RoundDown(System.TimeSpan start, System.TimeSpan end)
  {
    return RoundDown(timeSpan: start, diff: Difference);
  }

  protected sealed override System.TimeSpan RoundUp(System.TimeSpan start, System.TimeSpan end)
  {
    return RoundUp(dateTime: end, diff: Difference);
  }

  protected static System.TimeSpan Shift(System.TimeSpan span, DifferenceIn diff)
  {
    var res = span;

    System.TimeSpan shift = new();
    switch (diff)
    {
      case DifferenceIn.Year:
      case DifferenceIn.Month:
      case DifferenceIn.Day:
        shift = System.TimeSpan.FromDays(value: 1);
        break;
      case DifferenceIn.Hour:
        shift = System.TimeSpan.FromHours(value: 1);
        break;
      case DifferenceIn.Minute:
        shift = System.TimeSpan.FromMinutes(value: 1);
        break;
      case DifferenceIn.Second:
        shift = System.TimeSpan.FromSeconds(value: 1);
        break;
      case DifferenceIn.Millisecond:
        shift = System.TimeSpan.FromMilliseconds(value: 1);
        break;
    }

    res = res.Add(ts: shift);
    return res;
  }

  protected sealed override System.TimeSpan RoundDown(System.TimeSpan timeSpan, DifferenceIn diff)
  {
    var res = timeSpan;

    if (timeSpan.Ticks < 0)
    {
      res = RoundUp(dateTime: timeSpan.Duration(), diff: diff).Negate();
    }
    else
    {
      switch (diff)
      {
        case DifferenceIn.Year:
        case DifferenceIn.Month:
        case DifferenceIn.Day:
          res = System.TimeSpan.FromDays(value: timeSpan.Days);
          break;
        case DifferenceIn.Hour:
          res = System.TimeSpan.FromDays(value: timeSpan.Days).
            Add(ts: System.TimeSpan.FromHours(value: timeSpan.Hours));
          break;
        case DifferenceIn.Minute:
          res = System.TimeSpan.FromDays(value: timeSpan.Days).
            Add(ts: System.TimeSpan.FromHours(value: timeSpan.Hours)).
            Add(ts: System.TimeSpan.FromMinutes(value: timeSpan.Minutes));
          break;
        case DifferenceIn.Second:
          res = System.TimeSpan.FromDays(value: timeSpan.Days).
            Add(ts: System.TimeSpan.FromHours(value: timeSpan.Hours)).
            Add(ts: System.TimeSpan.FromMinutes(value: timeSpan.Minutes)).
            Add(ts: System.TimeSpan.FromSeconds(value: timeSpan.Seconds));
          break;
        case DifferenceIn.Millisecond:
          res = timeSpan;
          break;
      }
    }

    return res;
  }

  protected sealed override System.TimeSpan RoundUp(System.TimeSpan dateTime, DifferenceIn diff)
  {
    var res = RoundDown(timeSpan: dateTime, diff: diff);
    res = Shift(span: res, diff: diff);

    return res;
  }

  protected override List<System.TimeSpan> Trim(List<System.TimeSpan> ticks, Range<System.TimeSpan> range)
  {
    var startIndex = 0;
    for (var i = 0; i < ticks.Count - 1; i++)
    {
      if (ticks[index: i] <= range.Min && range.Min <= ticks[index: i + 1])
      {
        startIndex = i;
        break;
      }
    }

    var endIndex = ticks.Count - 1;
    for (var i = ticks.Count - 1; i >= 1; i--)
    {
      if (ticks[index: i] >= range.Max && range.Max > ticks[index: i - 1])
      {
        endIndex = i;
        break;
      }
    }

    List<System.TimeSpan> res = new(capacity: endIndex - startIndex + 1);
    for (var i = startIndex; i <= endIndex; i++)
    {
      res.Add(item: ticks[index: i]);
    }

    return res;
  }

  protected sealed override bool IsMinDate(System.TimeSpan dt)
  {
    return false;
  }
}

internal sealed class DayTimeSpanProvider : TimeSpanTicksProviderBase
{
  protected override DifferenceIn GetDifferenceCore()
  {
    return DifferenceIn.Day;
  }

  protected override int[] GetTickCountsCore()
  {
    return new int[] { 20, 10, 5, 2, 1 };
  }

  protected override int GetSpecificValue(System.TimeSpan start, System.TimeSpan dt)
  {
    return (dt - start).Days;
  }

  protected override System.TimeSpan GetStart(System.TimeSpan start, int value, int step)
  {
    var days = start.TotalDays;
    double newDays = (int)(days / step) * step;
    if (newDays > days)
    {
      newDays -= step;
    }
    return System.TimeSpan.FromDays(value: newDays);
    //return TimeSpan.FromDays(start.Days);
  }

  protected override System.TimeSpan AddStep(System.TimeSpan dt, int step)
  {
    return dt.Add(ts: System.TimeSpan.FromDays(value: step));
  }
}

internal sealed class HourTimeSpanProvider : TimeSpanTicksProviderBase
{
  protected override DifferenceIn GetDifferenceCore()
  {
    return DifferenceIn.Hour;
  }

  protected override int[] GetTickCountsCore()
  {
    return new int[] { 24, 12, 6, 4, 3, 2, 1 };
  }

  protected override int GetSpecificValue(System.TimeSpan start, System.TimeSpan dt)
  {
    return (int)(dt - start).TotalHours;
  }

  protected override System.TimeSpan GetStart(System.TimeSpan start, int value, int step)
  {
    var hours = start.TotalHours;
    double newHours = (int)(hours / step) * step;
    if (newHours > hours)
    {
      newHours -= step;
    }
    return System.TimeSpan.FromHours(value: newHours);
    //return TimeSpan.FromDays(start.Days);
  }

  protected override System.TimeSpan AddStep(System.TimeSpan dt, int step)
  {
    return dt.Add(ts: System.TimeSpan.FromHours(value: step));
  }
}

internal sealed class MinuteTimeSpanProvider : TimeSpanTicksProviderBase
{
  protected override DifferenceIn GetDifferenceCore()
  {
    return DifferenceIn.Minute;
  }

  protected override int[] GetTickCountsCore()
  {
    return new int[] { 60, 30, 20, 15, 10, 5, 4, 3, 2 };
  }

  protected override int GetSpecificValue(System.TimeSpan start, System.TimeSpan dt)
  {
    return (int)(dt - start).TotalMinutes;
  }

  protected override System.TimeSpan GetStart(System.TimeSpan start, int value, int step)
  {
    var minutes = start.TotalMinutes;
    double newMinutes = (int)(minutes / step) * step;
    if (newMinutes > minutes)
    {
      newMinutes -= step;
    }

    return System.TimeSpan.FromMinutes(value: newMinutes);
  }

  protected override System.TimeSpan AddStep(System.TimeSpan dt, int step)
  {
    return dt.Add(ts: System.TimeSpan.FromMinutes(value: step));
  }
}

internal sealed class SecondTimeSpanProvider : TimeSpanTicksProviderBase
{
  protected override DifferenceIn GetDifferenceCore()
  {
    return DifferenceIn.Second;
  }

  protected override int[] GetTickCountsCore()
  {
    return new int[] { 60, 30, 20, 15, 10, 5, 4, 3, 2 };
  }

  protected override int GetSpecificValue(System.TimeSpan start, System.TimeSpan dt)
  {
    return (int)(dt - start).TotalSeconds;
  }

  protected override System.TimeSpan GetStart(System.TimeSpan start, int value, int step)
  {
    var seconds = start.TotalSeconds;
    double newSeconds = (int)(seconds / step) * step;
    if (newSeconds > seconds)
    {
      newSeconds -= step;
    }

    return System.TimeSpan.FromSeconds(value: newSeconds);
    //return new TimeSpan(start.Days, start.Hours, start.Minutes, 0);
  }

  protected override System.TimeSpan AddStep(System.TimeSpan dt, int step)
  {
    return dt.Add(ts: System.TimeSpan.FromSeconds(value: step));
  }
}

internal sealed class MillisecondTimeSpanProvider : TimeSpanTicksProviderBase
{
  protected override DifferenceIn GetDifferenceCore()
  {
    return DifferenceIn.Millisecond;
  }

  protected override int[] GetTickCountsCore()
  {
    return new int[] { 100, 50, 40, 25, 20, 10, 5, 4, 2 };
  }

  protected override int GetSpecificValue(System.TimeSpan start, System.TimeSpan dt)
  {
    return (int)(dt - start).TotalMilliseconds;
  }

  protected override System.TimeSpan GetStart(System.TimeSpan start, int value, int step)
  {
    var millis = start.TotalMilliseconds;
    double newMillis = (int)(millis / step) * step;
    if (newMillis > millis)
    {
      newMillis -= step;
    }

    return System.TimeSpan.FromMilliseconds(value: newMillis);
    //return start;
  }

  protected override System.TimeSpan AddStep(System.TimeSpan dt, int step)
  {
    return dt.Add(ts: System.TimeSpan.FromMilliseconds(value: step));
  }
}
