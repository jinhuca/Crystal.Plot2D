using System;
using System.Windows;
using Crystal.Plot2D.Charts;

namespace Crystal.Plot2D.Descriptions;

public sealed class ResolveLegendItemEventArgs : EventArgs
{
  public ResolveLegendItemEventArgs(LegendItem legendItem)
  {
    LegendItem = legendItem;
  }

  public LegendItem LegendItem { get; }
}

public abstract class Description : FrameworkElement
{
  private LegendItem legendItem;
  public LegendItem LegendItem => legendItem ??= CreateLegendItem();

  private LegendItem CreateLegendItem()
  {
    var item_ = CreateLegendItemCore();
    return RaiseResolveLegendItem(uncustomizedLegendItem: item_);
  }

  protected virtual LegendItem CreateLegendItemCore()
  {
    return null;
  }

  public event EventHandler<ResolveLegendItemEventArgs> ResolveLegendItem;
  private LegendItem RaiseResolveLegendItem(LegendItem uncustomizedLegendItem)
  {
    if (ResolveLegendItem != null)
    {
      ResolveLegendItemEventArgs e = new(legendItem: uncustomizedLegendItem);
      ResolveLegendItem(sender: this, e: e);
      return e.LegendItem;
    }

    return uncustomizedLegendItem;
  }

  public UIElement ViewportElement { get; private set; }

  internal void Attach(UIElement element)
  {
    ViewportElement = element;
    AttachCore(element: element);
  }

  protected virtual void AttachCore(UIElement element) { }

  internal void Detach() => ViewportElement = null;

  public abstract string Brief { get; }

  public abstract string Full { get; }

  public override string ToString() => Brief;
}
