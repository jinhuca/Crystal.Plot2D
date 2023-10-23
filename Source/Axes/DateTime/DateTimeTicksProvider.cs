using System;

namespace Crystal.Plot2D.Charts;

/// <summary>
/// Represents a ticks provider for ticks of <see cref="T:System.DateTime"/> type.
/// </summary>
public class DateTimeTicksProvider : TimeTicksProviderBase<DateTime>
{
  /// <summary>
  /// Initializes a new instance of the <see cref="DateTimeTicksProvider"/> class.
  /// </summary>
  public DateTimeTicksProvider() { }

  static DateTimeTicksProvider()
  {
    Providers.Add(key: DifferenceIn.Year, value: new YearDateTimeProvider());
    Providers.Add(key: DifferenceIn.Month, value: new MonthDateTimeProvider());
    Providers.Add(key: DifferenceIn.Day, value: new DayDateTimeProvider());
    Providers.Add(key: DifferenceIn.Hour, value: new HourDateTimeProvider());
    Providers.Add(key: DifferenceIn.Minute, value: new MinuteDateTimeProvider());
    Providers.Add(key: DifferenceIn.Second, value: new SecondDateTimeProvider());
    Providers.Add(key: DifferenceIn.Millisecond, value: new MillisecondDateTimeProvider());

    MinorProviders.Add(key: DifferenceIn.Year, value: new MinorDateTimeProvider(owner: new YearDateTimeProvider()));
    MinorProviders.Add(key: DifferenceIn.Month, value: new MinorDateTimeProvider(owner: new MonthDateTimeProvider()));
    MinorProviders.Add(key: DifferenceIn.Day, value: new MinorDateTimeProvider(owner: new DayDateTimeProvider()));
    MinorProviders.Add(key: DifferenceIn.Hour, value: new MinorDateTimeProvider(owner: new HourDateTimeProvider()));
    MinorProviders.Add(key: DifferenceIn.Minute, value: new MinorDateTimeProvider(owner: new MinuteDateTimeProvider()));
    MinorProviders.Add(key: DifferenceIn.Second, value: new MinorDateTimeProvider(owner: new SecondDateTimeProvider()));
    MinorProviders.Add(key: DifferenceIn.Millisecond, value: new MinorDateTimeProvider(owner: new MillisecondDateTimeProvider()));
  }

  protected sealed override TimeSpan GetDifference(DateTime start, DateTime end)
  {
    return end - start;
  }
}

internal static class DateTimeArrayExtensions
{
  internal static int GetIndex(this DateTime[] array, DateTime value)
  {
    for (int i = 0; i < array.Length - 1; i++)
    {
      if (array[i] <= value && value < array[i + 1])
      {
        return i;
      }
    }

    return array.Length - 1;
  }
}

internal sealed class MinorDateTimeProvider : MinorTimeProviderBase<DateTime>
{
  public MinorDateTimeProvider(ITicksProvider<DateTime> owner) : base(provider: owner) { }

  protected override bool IsInside(DateTime value, Range<DateTime> range)
  {
    return range.Min < value && value < range.Max;
  }
}

internal sealed class YearDateTimeProvider : DatePeriodTicksProvider
{
  protected override DifferenceIn GetDifferenceCore()
  {
    return DifferenceIn.Year;
  }

  protected override int[] GetTickCountsCore()
  {
    return new[] { 20, 10, 5, 4, 2, 1 };
  }

  protected override int GetSpecificValue(DateTime start, DateTime dt)
  {
    return dt.Year;
  }

  protected override DateTime GetStart(DateTime start, int value, int step)
  {
    int year = start.Year;
    int newYear = (year / step) * step;
    if (newYear == 0)
    {
      newYear = 1;
    }

    return new DateTime(year: newYear, month: 1, day: 1);
  }

  protected override bool IsMinDate(DateTime dt)
  {
    return dt.Year == DateTime.MinValue.Year;
  }

  protected override DateTime AddStep(DateTime dt, int step)
  {
    if (dt.Year + step > DateTime.MaxValue.Year)
    {
      return DateTime.MaxValue;
    }

    return dt.AddYears(value: step);
  }
}

internal sealed class MonthDateTimeProvider : DatePeriodTicksProvider
{
  protected override DifferenceIn GetDifferenceCore()
  {
    return DifferenceIn.Month;
  }

  protected override int[] GetTickCountsCore()
  {
    return new[] { 12, 6, 4, 3, 2, 1 };
  }

  protected override int GetSpecificValue(DateTime start, DateTime dt)
  {
    return dt.Month + (dt.Year - start.Year) * 12;
  }

  protected override DateTime GetStart(DateTime start, int value, int step)
  {
    return new DateTime(year: start.Year, month: 1, day: 1);
  }

  protected override bool IsMinDate(DateTime dt)
  {
    return dt.Month == DateTime.MinValue.Month;
  }

  protected override DateTime AddStep(DateTime dt, int step)
  {
    return dt.AddMonths(months: step);
  }
}

internal sealed class DayDateTimeProvider : DatePeriodTicksProvider
{
  protected override DifferenceIn GetDifferenceCore()
  {
    return DifferenceIn.Day;
  }

  protected override int[] GetTickCountsCore()
  {
    return new[] { 30, 15, 10, 5, 2, 1 };
  }

  protected override int GetSpecificValue(DateTime start, DateTime dt)
  {
    return (dt - start).Days;
  }

  protected override DateTime GetStart(DateTime start, int value, int step)
  {
    return start.Date;
  }

  protected override bool IsMinDate(DateTime dt)
  {
    return dt.Day == 1;
  }

  protected override DateTime AddStep(DateTime dt, int step)
  {
    return dt.AddDays(value: step);
  }
}

internal sealed class HourDateTimeProvider : DatePeriodTicksProvider
{
  protected override DifferenceIn GetDifferenceCore()
  {
    return DifferenceIn.Hour;
  }

  protected override int[] GetTickCountsCore()
  {
    return new[] { 24, 12, 6, 4, 3, 2, 1 };
  }

  protected override int GetSpecificValue(DateTime start, DateTime dt)
  {
    return (int)(dt - start).TotalHours;
  }

  protected override DateTime GetStart(DateTime start, int value, int step)
  {
    return start.Date;
  }

  protected override bool IsMinDate(DateTime dt)
  {
    return false;
  }

  protected override DateTime AddStep(DateTime dt, int step)
  {
    return dt.AddHours(value: step);
  }
}

internal sealed class MinuteDateTimeProvider : DatePeriodTicksProvider
{
  protected override DifferenceIn GetDifferenceCore()
  {
    return DifferenceIn.Minute;
  }

  protected override int[] GetTickCountsCore()
  {
    return new[] { 60, 30, 20, 15, 10, 5, 4, 3, 2 };
  }

  protected override int GetSpecificValue(DateTime start, DateTime dt)
  {
    return (int)(dt - start).TotalMinutes;
  }

  protected override DateTime GetStart(DateTime start, int value, int step)
  {
    return start.Date.AddHours(value: start.Hour);
  }

  protected override bool IsMinDate(DateTime dt)
  {
    return false;
  }

  protected override DateTime AddStep(DateTime dt, int step)
  {
    return dt.AddMinutes(value: step);
  }
}

internal sealed class SecondDateTimeProvider : DatePeriodTicksProvider
{
  protected override DifferenceIn GetDifferenceCore()
  {
    return DifferenceIn.Second;
  }

  protected override int[] GetTickCountsCore()
  {
    return new[] { 60, 30, 20, 15, 10, 5, 4, 3, 2 };
  }

  protected override int GetSpecificValue(DateTime start, DateTime dt)
  {
    return (int)(dt - start).TotalSeconds;
  }

  protected override DateTime GetStart(DateTime start, int value, int step)
  {
    return start.Date.AddHours(value: start.Hour).AddMinutes(value: start.Minute);
  }

  protected override bool IsMinDate(DateTime dt)
  {
    return false;
  }

  protected override DateTime AddStep(DateTime dt, int step)
  {
    return dt.AddSeconds(value: step);
  }
}

internal sealed class MillisecondDateTimeProvider : DatePeriodTicksProvider
{
  protected override DifferenceIn GetDifferenceCore()
  {
    return DifferenceIn.Millisecond;
  }

  protected override int[] GetTickCountsCore()
  {
    return new[] { 100, 50, 40, 25, 20, 10, 5, 4, 2 };
  }

  protected override int GetSpecificValue(DateTime start, DateTime dt)
  {
    return (int)(dt - start).TotalMilliseconds;
  }

  protected override DateTime GetStart(DateTime start, int value, int step)
  {
    return start.Date.AddHours(value: start.Hour).AddMinutes(value: start.Minute).AddSeconds(value: start.Second);
  }

  protected override bool IsMinDate(DateTime dt)
  {
    return false;
  }

  protected override DateTime AddStep(DateTime dt, int step)
  {
    return dt.AddMilliseconds(value: step);
  }
}
