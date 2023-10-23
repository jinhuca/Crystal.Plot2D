using System;

namespace Crystal.Plot2D.Charts;

public class TimeSpanTicksProvider : TimeTicksProviderBase<TimeSpan>
{
  static TimeSpanTicksProvider()
  {
    Providers.Add(key: DifferenceIn.Year, value: new DayTimeSpanProvider());
    Providers.Add(key: DifferenceIn.Month, value: new DayTimeSpanProvider());
    Providers.Add(key: DifferenceIn.Day, value: new DayTimeSpanProvider());
    Providers.Add(key: DifferenceIn.Hour, value: new HourTimeSpanProvider());
    Providers.Add(key: DifferenceIn.Minute, value: new MinuteTimeSpanProvider());
    Providers.Add(key: DifferenceIn.Second, value: new SecondTimeSpanProvider());
    Providers.Add(key: DifferenceIn.Millisecond, value: new MillisecondTimeSpanProvider());

    MinorProviders.Add(key: DifferenceIn.Year, value: new MinorTimeSpanTicksProvider(owner: new DayTimeSpanProvider()));
    MinorProviders.Add(key: DifferenceIn.Month, value: new MinorTimeSpanTicksProvider(owner: new DayTimeSpanProvider()));
    MinorProviders.Add(key: DifferenceIn.Day, value: new MinorTimeSpanTicksProvider(owner: new DayTimeSpanProvider()));
    MinorProviders.Add(key: DifferenceIn.Hour, value: new MinorTimeSpanTicksProvider(owner: new HourTimeSpanProvider()));
    MinorProviders.Add(key: DifferenceIn.Minute, value: new MinorTimeSpanTicksProvider(owner: new MinuteTimeSpanProvider()));
    MinorProviders.Add(key: DifferenceIn.Second, value: new MinorTimeSpanTicksProvider(owner: new SecondTimeSpanProvider()));
    MinorProviders.Add(key: DifferenceIn.Millisecond, value: new MinorTimeSpanTicksProvider(owner: new MillisecondTimeSpanProvider()));
  }

  protected override TimeSpan GetDifference(TimeSpan start, TimeSpan end)
  {
    return end - start;
  }
}
