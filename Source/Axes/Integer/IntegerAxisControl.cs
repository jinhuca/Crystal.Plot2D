namespace Crystal.Plot2D.Charts;

public class IntegerAxisControl : AxisControl<int>
{
  public IntegerAxisControl()
  {
    LabelProvider = new GenericLabelProvider<int>();
    TicksProvider = new IntegerTicksProvider();
    ConvertToDouble = i => (double)i;
    Range = new Range<int>(min: 0, max: 1);
  }
}
