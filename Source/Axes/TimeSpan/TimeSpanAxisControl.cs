using System;

namespace Crystal.Plot2D.Charts;

public class TimeSpanAxisControl : AxisControl<TimeSpan>
{
  public TimeSpanAxisControl()
  {
    LabelProvider = new TimeSpanLabelProvider();
    TicksProvider = new TimeSpanTicksProvider();

    ConvertToDouble = time => time.Ticks;

    Range = new Range<TimeSpan>(min: new TimeSpan(), max: new TimeSpan(hours: 1, minutes: 0, seconds: 0));
  }
}
