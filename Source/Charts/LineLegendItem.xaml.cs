using Crystal.Plot2D.Descriptions;

namespace Crystal.Plot2D.Charts;

/// <summary>
/// Interaction logic for LineLegendItem.xaml
/// </summary>
public sealed partial class LineLegendItem : LegendItem
{
  public LineLegendItem()
  {
    InitializeComponent();
  }

  public LineLegendItem(Description description) : base(description: description)
  {
    InitializeComponent();
  }
}
