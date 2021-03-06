using System.Windows.Automation.Peers;

namespace Crystal.Plot2D.Common
{
  public class PlotterAutomationPeer : FrameworkElementAutomationPeer
  {
    public PlotterAutomationPeer(PlotterBase owner)
      : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Custom;

    protected override string GetClassNameCore() => "Plotter";
  }
}
