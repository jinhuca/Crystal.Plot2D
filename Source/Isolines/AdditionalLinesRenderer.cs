using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Crystal.Plot2D.Charts;

public class AdditionalLinesRenderer : IsolineRenderer
{
  protected override void CreateUIRepresentation()
  {
    InvalidateVisual();
  }

  protected override void OnPlotterAttached()
  {
    base.OnPlotterAttached();

    FrameworkElement parent = (FrameworkElement)Parent;
    var renderer = (FrameworkElement)parent.FindName(name: "PART_IsolineRenderer");

    Binding contentBoundsBinding = new() { Path = new PropertyPath(path: "(0)", pathParameters: Viewport2D.ContentBoundsProperty), Source = renderer };
    SetBinding(dp: Viewport2D.ContentBoundsProperty, binding: contentBoundsBinding);
    SetBinding(dp: ViewportPanel.ViewportBoundsProperty, binding: contentBoundsBinding);

    Plotter2D.Viewport.EndPanning += Viewport_EndPanning;
    Plotter2D.Viewport.PropertyChanged += Viewport_PropertyChanged;
  }

  void Viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
  {
    if (e.PropertyName == "Visible")
    {
      if (Plotter2D.Viewport.PanningState == Viewport2DPanningState.NotPanning)
      {
        InvalidateVisual();
      }
    }
  }

  protected override void OnPlotterDetaching()
  {
    Plotter2D.Viewport.EndPanning -= Viewport_EndPanning;
    Plotter2D.Viewport.PropertyChanged -= Viewport_PropertyChanged;

    base.OnPlotterDetaching();
  }

  private void Viewport_EndPanning(object sender, EventArgs e)
  {
    InvalidateVisual();
  }

  protected override void OnRender(DrawingContext drawingContext)
  {
    if (Plotter2D == null)
    {
      return;
    }

    if (DataSource == null)
    {
      return;
    }

    var collection = (IsolineCollection)Parent.GetValue(dp: IsolineCollectionProperty);
    if (collection == null)
    {
      return;
    }

    var bounds = ViewportPanel.GetViewportBounds(obj: this);
    if (bounds.IsEmpty)
    {
      return;
    }

    var dc = drawingContext;
    var strokeThickness = StrokeThickness;

    var transform = Plotter2D.Transform.WithRects(visibleRect: bounds, screenRect: new Rect(size: RenderSize));

    //dc.DrawRectangle(null, new OutlinePen(Brushes.Green, 2), new Rect(RenderSize));

    var additionalLevels = GetAdditionalLevels(collection: collection);
    IsolineBuilder.DataSource = DataSource;
    var additionalIsolineCollections = additionalLevels.Select(selector: level =>
    {
      return IsolineBuilder.BuildIsoline(level: level);
    });

    foreach (var additionalCollection in additionalIsolineCollections)
    {
      RenderIsolineCollection(dc: dc, strokeThickness: strokeThickness, collection: additionalCollection, transform: transform);
    }
  }
}
