using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Charts;

public partial class FastIsolineDisplay : IsolineGraphBase
{
  public FastIsolineDisplay()
  {
    InitializeComponent();
  }

  protected override Panel HostPanel => Plotter2D.CentralGrid;

  public override void OnApplyTemplate()
  {
    base.OnApplyTemplate();

    var isolineRenderer = (FastIsolineRenderer)Template.FindName(name: "PART_IsolineRenderer", templatedParent: this);
    //Binding contentBoundsBinding = new Binding { Path = new PropertyPath("(0)", Viewport2D.ContentBoundsProperty), Source = isolineRenderer };
    //SetBinding(Viewport2D.ContentBoundsProperty, contentBoundsBinding);

    if (isolineRenderer != null)
    {
      isolineRenderer.AddHandler(routedEvent: Viewport2D.ContentBoundsChangedEvent, handler: new RoutedEventHandler(OnRendererContentBoundsChanged));
      UpdateContentBounds(source: isolineRenderer);
    }
  }

  private void OnRendererContentBoundsChanged(object sender, RoutedEventArgs e)
  {
    UpdateContentBounds(source: (DependencyObject)sender);
  }

  private void UpdateContentBounds(DependencyObject source)
  {
    var contentBounds = Viewport2D.GetContentBounds(obj: source);
    Viewport2D.SetContentBounds(obj: this, value: contentBounds);
  }
}
