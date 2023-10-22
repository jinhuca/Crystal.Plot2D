using System;

namespace Crystal.Plot2D.Common;

internal interface INotifyingPanel
{
  NotifyingUIElementCollection NotifyingChildren { get; }
  event EventHandler ChildrenCreated;
}
