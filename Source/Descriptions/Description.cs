using System;
using System.Windows;

namespace Crystal.Plot2D;

public class ResolveLegendItemEventArgs : EventArgs
{
  public ResolveLegendItemEventArgs(LegendItem legendItem)
  {
    LegendItem = legendItem;
  }

  public LegendItem LegendItem { get; set; }
}

public abstract class Description : FrameworkElement
{
  private LegendItem legendItem;
  public LegendItem LegendItem
  {
    get
    {
      if (legendItem == null)
      {
        legendItem = CreateLegendItem();
      }
      return legendItem;
    }
  }

  private LegendItem CreateLegendItem()
  {
    LegendItem item = CreateLegendItemCore();
    return RaiseResolveLegendItem(uncustomizedLegendItem: item);
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
    else
    {
      return uncustomizedLegendItem;
    }
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
