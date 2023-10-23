using System;

namespace Crystal.Plot2D.Charts;

/// <summary>
/// AxisControl for DateTime axes.
/// </summary>
public class DateTimeAxisControl : AxisControl<DateTime>
{
  /// <summary>
  /// Initializes a new instance of the <see cref="DateTimeAxisControl"/> class.
  /// </summary>
  public DateTimeAxisControl()
  {
    LabelProvider = new DateTimeLabelProvider();
    TicksProvider = new DateTimeTicksProvider();
    MajorLabelProvider = new MajorDateTimeLabelProvider();

    ConvertToDouble = dt => dt.Ticks;

    Range = new Range<DateTime>(min: DateTime.Now, max: DateTime.Now.AddYears(value: 1));
  }
}
