namespace Crystal.Plot2D.Charts;

public class IntegerAxis : AxisBase<int>
{
  public IntegerAxis()
    : base(axisControl: new IntegerAxisControl(),
      convertFromDouble: d => (int)d,
      convertToDouble: i => (double)i)
  {

  }
}
