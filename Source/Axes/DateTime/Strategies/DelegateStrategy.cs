using System;

namespace Crystal.Plot2D.Charts;

public class DelegateDateTimeStrategy : DefaultDateTimeTicksStrategy
{
  private readonly Func<TimeSpan, DifferenceIn?> function;
  public DelegateDateTimeStrategy(Func<TimeSpan, DifferenceIn?> function)
  {
    if (function == null)
    {
      throw new ArgumentNullException(paramName: "function");
    }

    this.function = function;
  }

  public override DifferenceIn GetDifference(TimeSpan span)
  {
    DifferenceIn? customResult = function(arg: span);

    DifferenceIn result = customResult.HasValue ?
      customResult.Value :
      base.GetDifference(span: span);

    return result;
  }
}
