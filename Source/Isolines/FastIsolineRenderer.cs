using Crystal.Plot2D.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Crystal.Plot2D.Charts;

public class FastIsolineRenderer : IsolineRenderer
{
  private List<IsolineCollection> additionalLines = new();
  private const int subDivisionNum = 10;

  protected override void CreateUIRepresentation()
  {
    InvalidateVisual();
  }

  protected override void OnPlotterAttached()
  {
    base.OnPlotterAttached();

    FrameworkElement parent = (FrameworkElement)Parent;
    Binding collectionBinding = new(path: "IsolineCollection") { Source = this };
    parent.SetBinding(dp: IsolineCollectionProperty, binding: collectionBinding);
  }

  protected override void OnRender(DrawingContext drawingContext)
  {
    if (Plotter2D == null)
    {
      return;
    }

    if (Collection == null)
    {
      return;
    }

    if (DataSource == null)
    {
      return;
    }

    if (Collection.Lines.Count == 0)
    {
      IsolineBuilder.DataSource = DataSource;
      IsolineBuilder.MissingValue = MissingValue;
      Collection = IsolineBuilder.BuildIsoline();
    }

    IsolineCollection = Collection;

    var dc = drawingContext;
    var strokeThickness = StrokeThickness;
    var collection = Collection;

    var bounds = DataRect.Empty;
    // determining content bounds
    foreach (LevelLine line in collection)
    {
      foreach (Point point in line.AllPoints)
      {
        bounds.Union(point: point);
      }
    }

    Viewport2D.SetContentBounds(obj: this, value: bounds);
    ViewportPanel.SetViewportBounds(obj: this, value: bounds);

    if (bounds.IsEmpty)
    {
      return;
    }

    // custom transform with output set to renderSize of this control
    var transform = Plotter2D.Transform.WithRects(visibleRect: bounds, screenRect: new Rect(size: RenderSize));

    // actual drawing of isolines
    RenderIsolineCollection(dc: dc, strokeThickness: strokeThickness, collection: collection, transform: transform);

    //var additionalLevels = GetAdditionalIsolines(collection);

    //var additionalIsolineCollections = additionalLevels.Select(level => IsolineBuilder.BuildIsoline(level));

    //foreach (var additionalCollection in additionalIsolineCollections)
    //{
    //    RenderIsolineCollection(dc, strokeThickness, additionalCollection, transform);
    //}

    RenderLabels(dc: dc, collection: collection);

    //    foreach (var additionalCollection in additionalIsolineCollections)
    //    {
    //        RenderLabels(dc, additionalCollection);
    //    }
  }

  private IEnumerable<double> GetAdditionalIsolines(IsolineCollection collection)
  {
    var dataSource = DataSource;
    var visibleMinMax = dataSource.GetMinMax(area: Plotter2D.Visible);
    var visibleMinMaxRatio = (collection.Max - collection.Min) / visibleMinMax.GetLength();

    var log = Math.Log10(d: visibleMinMaxRatio);
    if (log > 0.9)
    {
      var upperLog = Math.Ceiling(a: log);
      var divisionsNum = Math.Pow(x: 10, y: upperLog);
      var delta = (collection.Max - collection.Min) / divisionsNum;

      var start = Math.Ceiling(a: visibleMinMax.Min / delta) * delta;

      var x = start;
      while (x < visibleMinMax.Max)
      {
        yield return x;
        x += delta;
      }
    }
  }

  private void RenderLabels(DrawingContext dc, IsolineCollection collection)
  {
    if (Plotter2D == null)
    {
      return;
    }

    if (collection == null)
    {
      return;
    }

    if (!DrawLabels)
    {
      return;
    }

    var viewportBounds = ViewportPanel.GetViewportBounds(obj: this);
    if (viewportBounds.IsEmpty)
    {
      return;
    }

    var strokeThickness = StrokeThickness;
    var visible = Plotter2D.Visible;
    var output = Plotter2D.Viewport.Output;

    var transform = Plotter2D.Transform.WithRects(visibleRect: viewportBounds, screenRect: new Rect(size: RenderSize));
    var labelStringFormat = LabelStringFormat;

    // drawing constants
    var labelRectangleFill = Brushes.White;

    var biggerViewport = viewportBounds.ZoomOutFromCenter(ratio: 1.1);

    // getting and filtering annotations to draw only visible ones
    Annotater.WayBeforeText = Math.Sqrt(d: visible.Width * visible.Width + visible.Height * visible.Height) / 8 * WayBeforeTextMultiplier;
    var annotations = Annotater.Annotate(collection: collection, visible: visible)
    .Where(predicate: annotation =>
    {
      Point viewportPosition = annotation.Position.DataToViewport(transform: transform);
      return biggerViewport.Contains(point: viewportPosition);
    });

    var labelsScale = LabelsScaling;

    // drawing annotations
    foreach (var annotation in annotations)
    {
      FormattedText text = CreateFormattedText(text: annotation.Value.ToString(format: LabelStringFormat));
      Point position = annotation.Position.DataToScreen(transform: transform);

      var labelTransform = CreateTransform(isolineLabel: annotation, text: text, position: position);

      // creating rectange stroke
      double colorRatio = (annotation.Value - collection.Min) / (collection.Max - collection.Min);
      colorRatio = MathHelper.Clamp(value: colorRatio);
      Color rectangleStrokeColor = Palette.GetColor(t: colorRatio);
      SolidColorBrush rectangleStroke = new(color: rectangleStrokeColor);
      Pen labelRectangleStrokePen = new(brush: rectangleStroke, thickness: 2);

      dc.PushTransform(transform: new ScaleTransform(scaleX: 1, scaleY: labelsScale));
      dc.PushTransform(transform: labelTransform);
      {
        var bounds = RectExtensions.FromCenterSize(center: position, size: new Size(width: text.Width, height: text.Height));
        bounds = bounds.ZoomOutFromCenter(ratio: 1.3);
        dc.DrawRoundedRectangle(brush: labelRectangleFill, pen: labelRectangleStrokePen, rectangle: bounds, radiusX: 8, radiusY: 8);

        DrawTextInPosition(dc: dc, text: text, position: position);
      }
      dc.Pop();
      dc.Pop();
    }
  }

  private static void DrawTextInPosition(DrawingContext dc, FormattedText text, Point position)
  {
    var textPosition = position;
    textPosition.Offset(offsetX: -text.Width / 2, offsetY: -text.Height / 2);
    dc.DrawText(formattedText: text, origin: textPosition);
  }

  private static Transform CreateTransform(IsolineTextLabel isolineLabel, FormattedText text, Point position)
  {
    double angle = isolineLabel.Rotation;
    if (angle < 0)
    {
      angle += 360;
    }

    if (90 < angle && angle < 270)
    {
      angle -= 180;
    }

    RotateTransform transform = new(angle: angle, centerX: position.X, centerY: position.Y);
    return transform;
  }

  private static FormattedText CreateFormattedText(string text)
  {
#pragma warning disable CS0618 // 'FormattedText.FormattedText(string, CultureInfo, FlowDirection, Typeface, double, StrokeBrush)' is obsolete: 'Use the PixelsPerDip override'
    FormattedText result = new(textToFormat: text,
      culture: CultureInfo.CurrentCulture, flowDirection: FlowDirection.LeftToRight, typeface: new Typeface(typefaceName: "Arial"), emSize: 12, foreground: Brushes.Black);
#pragma warning restore CS0618 // 'FormattedText.FormattedText(string, CultureInfo, FlowDirection, Typeface, double, StrokeBrush)' is obsolete: 'Use the PixelsPerDip override'
    return result;
  }
}
