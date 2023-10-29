using System;
using System.Windows;
using System.Windows.Controls;
using Crystal.Plot2D.Common.Auxiliary;

namespace Crystal.Plot2D.Common.NotifyingPanels;

internal sealed class NotifyingGrid : Grid, INotifyingPanel
{
  public NotifyingUIElementCollection NotifyingChildren { get; private set; }

  protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
  {
    NotifyingChildren = new NotifyingUIElementCollection(visualParent: this, logicalParent: logicalParent);
    ChildrenCreated.Raise(sender: this);
    return NotifyingChildren;
  }

  public event EventHandler ChildrenCreated;
}
