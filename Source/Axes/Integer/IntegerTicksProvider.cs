﻿using Crystal.Plot2D.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crystal.Plot2D.Charts;

/// <summary>
/// Represents a ticks provider for intefer values.
/// </summary>
public class IntegerTicksProvider : ITicksProvider<int>
{
  /// <summary>
  /// Initializes a new instance of the <see cref="IntegerTicksProvider"/> class.
  /// </summary>
  public IntegerTicksProvider() { }

  private int minStep;
  /// <summary>
  /// Gets or sets the minimal step between ticks.
  /// </summary>
  /// <value>The min step.</value>
  public int MinStep
  {
    get => minStep;
    set
    {
      Verify.IsTrue(condition: value >= 0, paramName: "value");
      if (minStep != value)
      {
        minStep = value;
        RaiseChangedEvent();
      }
    }
  }

  private int maxStep = int.MaxValue;
  /// <summary>
  /// Gets or sets the maximal step between ticks.
  /// </summary>
  /// <value>The max step.</value>
  public int MaxStep
  {
    get => maxStep;
    set
    {
      if (maxStep != value)
      {
        if (value < 0)
        {
          throw new ArgumentOutOfRangeException(paramName: "value", message: Strings.Exceptions.ParameterShouldBePositive);
        }

        maxStep = value;
        RaiseChangedEvent();
      }
    }
  }

  #region ITicksProvider<int> Members

  /// <summary>
  /// Generates ticks for given range and preferred ticks count.
  /// </summary>
  /// <param name="range">The range.</param>
  /// <param name="ticksCount">The ticks count.</param>
  /// <returns></returns>
  public ITicksInfo<int> GetTicks(Range<int> range, int ticksCount)
  {
    double start = range.Min;
    double finish = range.Max;

    double delta = finish - start;

    int log = (int)Math.Round(a: Math.Log10(d: delta));

    double newStart = RoundingHelper.Round(number: start, rem: log);
    double newFinish = RoundingHelper.Round(number: finish, rem: log);
    if (newStart == newFinish)
    {
      log--;
      newStart = RoundingHelper.Round(number: start, rem: log);
      newFinish = RoundingHelper.Round(number: finish, rem: log);
    }

    // calculating step between ticks
    double unroundedStep = (newFinish - newStart) / ticksCount;
    int stepLog = log;
    // trying to round step
    int step = (int)RoundingHelper.Round(number: unroundedStep, rem: stepLog);
    if (step == 0)
    {
      stepLog--;
      step = (int)RoundingHelper.Round(number: unroundedStep, rem: stepLog);
      if (step == 0)
      {
        // step will not be rounded if attempts to be rounded to zero.
        step = (int)unroundedStep;
      }
    }

    if (step < minStep)
    {
      step = minStep;
    }

    if (step > maxStep)
    {
      step = maxStep;
    }

    if (step <= 0)
    {
      step = 1;
    }

    int[] ticks = CreateTicks(start: start, finish: finish, step: step);

    TicksInfo<int> res = new() { Info = log, Ticks = ticks };

    return res;
  }

  private static int[] CreateTicks(double start, double finish, int step)
  {
    DebugVerify.Is(condition: step != 0);

    int x = (int)(step * Math.Floor(d: start / (double)step));
    List<int> res = new();

    checked
    {
      double increasedFinish = finish + step * 1.05;
      while (x <= increasedFinish)
      {
        res.Add(item: x);
        x += step;
      }
    }
    return res.ToArray();
  }

  private static readonly int[] tickCounts = new int[] { 20, 10, 5, 4, 2, 1 };

  /// <summary>
  /// Decreases the tick count.
  /// Returned value should be later passed as ticksCount parameter to GetTicks method.
  /// </summary>
  /// <param name="ticksCount">The ticks count.</param>
  /// <returns>Decreased ticks count.</returns>
  public int DecreaseTickCount(int ticksCount)
  {
    return tickCounts.FirstOrDefault(predicate: tick => tick < ticksCount);
  }

  /// <summary>
  /// Increases the tick count.
  /// Returned value should be later passed as ticksCount parameter to GetTicks method.
  /// </summary>
  /// <param name="ticksCount">The ticks count.</param>
  /// <returns>Increased ticks count.</returns>
  public int IncreaseTickCount(int ticksCount)
  {
    int newTickCount = tickCounts.Reverse().FirstOrDefault(predicate: tick => tick > ticksCount);
    if (newTickCount == 0)
    {
      newTickCount = tickCounts[0];
    }

    return newTickCount;
  }

  /// <summary>
  /// Gets the minor ticks provider, used to generate ticks between each two adjacent ticks.
  /// </summary>
  /// <value>The minor provider.</value>
  public ITicksProvider<int> MinorProvider => null;

  /// <summary>
  /// Gets the major provider, used to generate major ticks - for example, years for common ticks as months.
  /// </summary>
  /// <value>The major provider.</value>
  public ITicksProvider<int> MajorProvider => null;

  protected void RaiseChangedEvent()
  {
    Changed.Raise(sender: this);
  }
  public event EventHandler Changed;

  #endregion
}
