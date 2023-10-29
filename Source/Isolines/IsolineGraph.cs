using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Transforms;

namespace Crystal.Plot2D.Isolines;

/// <summary>
/// Draws isolines on given two-dimensional scalar data.
/// </summary>
public sealed class IsolineGraph : IsolineRenderer
{
  private static readonly Brush labelBackground = new SolidColorBrush(color: Color.FromArgb(a: 130, r: 255, g: 255, b: 255));

  /// <summary>
  /// Initializes a new instance of the <see cref="IsolineGraph"/> class.
  /// </summary>
  public IsolineGraph()
  {
    Content = content;
    Viewport2D.SetIsContentBoundsHost(obj: this, value: true);
  }

  protected override void OnPlotterAttached()
  {
    CreateUIRepresentation();
    UpdateUIRepresentation();
  }

  private readonly Canvas content = new();

  protected override void UpdateDataSource()
  {
    base.UpdateDataSource();

    CreateUIRepresentation();
    rebuildText = true;
    UpdateUIRepresentation();
  }

  protected override void OnLineThicknessChanged()
  {
    foreach (var path in linePaths)
    {
      path.StrokeThickness = StrokeThickness;
    }
  }

  private readonly List<FrameworkElement> textBlocks = new();
  private readonly List<Path> linePaths = new();
  protected override void CreateUIRepresentation()
  {
    if (Plotter2D == null)
    {
      return;
    }

    content.Children.Clear();
    linePaths.Clear();

    if (Collection != null)
    {
      var bounds = DataRect.Empty;

      foreach (var line in Collection.Lines)
      {
        foreach (var point in line.AllPoints)
        {
          bounds.Union(point: point);
        }

        Path path = new()
        {
          Stroke = new SolidColorBrush(color: Palette.GetColor(t: line.Value01)),
          StrokeThickness = StrokeThickness,
          Data = CreateGeometry(lineData: line),
          Tag = line
        };
        content.Children.Add(element: path);
        linePaths.Add(item: path);
      }

      Viewport2D.SetContentBounds(obj: this, value: bounds);

      if (DrawLabels)
      {
        var transform = Plotter2D.Viewport.Transform;
        var wayBeforeText = new Rect(size: new Size(width: 2000, height: 2000)).ScreenToData(transform: transform).Width;
        Annotator.WayBeforeText = wayBeforeText;
        var textLabels = Annotator.Annotate(collection: Collection, visible: Plotter2D.Viewport.Visible);

        foreach (var textLabel in textLabels)
        {
          var text = CreateTextLabel(textLabel: textLabel);
          content.Children.Add(element: text);
          textBlocks.Add(item: text);
        }
      }
    }
  }

  private FrameworkElement CreateTextLabel(IsolineTextLabel textLabel)
  {
    var transform = Plotter2D.Viewport.Transform;
    var screenPos = textLabel.Position.DataToScreen(transform: transform);

    var angle = textLabel.Rotation;
    if (angle < 0)
    {
      angle += 360;
    }

    if (135 < angle && angle < 225)
    {
      angle -= 180;
    }

    var tooltip = textLabel.Value.ToString(format: "F"); //String.Format("{0} at ({1}, {2})", textLabel.Text, textLabel.Position.X, textLabel.Position.Y);
    Grid grid = new()
    {
      RenderTransform = new RotateTransform(angle: angle),
      Tag = textLabel,
      RenderTransformOrigin = new Point(x: 0.5, y: 0.5),
      ToolTip = tooltip
    };

    TextBlock res = new()
    {
      Text = textLabel.Value.ToString(format: "F"),
      Margin = new Thickness(left: 3, top: 1, right: 3, bottom: 1)
    };

    //res.Measure(SizeHelper.CreateInfiniteSize());

    Rectangle rect = new()
    {
      Stroke = Brushes.Gray,
      Fill = labelBackground,
      RadiusX = 8,
      RadiusY = 8
    };

    grid.Children.Add(element: rect);
    grid.Children.Add(element: res);

    grid.Measure(availableSize: SizeHelper.CreateInfiniteSize());

    var textSize = grid.DesiredSize;
    Point position = new(x: screenPos.X - textSize.Width / 2, y: screenPos.Y - textSize.Height / 2);

    Canvas.SetLeft(element: grid, length: position.X);
    Canvas.SetTop(element: grid, length: position.Y);
    return grid;
  }

  private Geometry CreateGeometry(LevelLine lineData)
  {
    var transform = Plotter2D.Viewport.Transform;

    StreamGeometry geometry = new();
    using (var context = geometry.Open())
    {
      context.BeginFigure(startPoint: lineData.StartPoint.DataToScreen(transform: transform), isFilled: false, isClosed: false);
      context.PolyLineTo(points: lineData.OtherPoints.DataToScreenAsList(transform: transform), isStroked: true, isSmoothJoin: true);
    }
    geometry.Freeze();
    return geometry;
  }

  private bool rebuildText = true;
  protected override void OnViewportPropertyChanged(ExtendedPropertyChangedEventArgs e)
  {
    if (e.PropertyName == "Visible" || e.PropertyName == "Output")
    {
      var isVisibleChanged = e.PropertyName == "Visible";
      var prevRect = isVisibleChanged ? (DataRect)e.OldValue : new DataRect(rect: (Rect)e.OldValue);
      var currRect = isVisibleChanged ? (DataRect)e.NewValue : new DataRect(rect: (Rect)e.NewValue);

      // completely rebuild text only if width and height have changed many
      const double smallChangePercent = 0.05;
      var widthChangedLittle = Math.Abs(value: currRect.Width - prevRect.Width) / currRect.Width < smallChangePercent;
      var heightChangedLittle = Math.Abs(value: currRect.Height - prevRect.Height) / currRect.Height < smallChangePercent;

      rebuildText = !(widthChangedLittle && heightChangedLittle);
    }
    UpdateUIRepresentation();
  }

  private void UpdateUIRepresentation()
  {
    if (Plotter2D == null)
    {
      return;
    }

    foreach (var path in linePaths)
    {
      var line = (LevelLine)path.Tag;
      path.Data = CreateGeometry(lineData: line);
    }

    var transform = Plotter2D.Viewport.Transform;
    var output = Plotter2D.Viewport.Output;
    var visible = Plotter2D.Viewport.Visible;

    if (rebuildText && DrawLabels)
    {
      rebuildText = false;
      foreach (var text in textBlocks)
      {
        if (text.Visibility == Visibility.Visible)
        {
          content.Children.Remove(element: text);
        }
      }
      textBlocks.Clear();

      var wayBeforeText = new Rect(size: new Size(width: 100, height: 100)).ScreenToData(transform: transform).Width;
      Annotator.WayBeforeText = wayBeforeText;
      var textLabels = Annotator.Annotate(collection: Collection, visible: Plotter2D.Viewport.Visible);
      foreach (var textLabel in textLabels)
      {
        var text = CreateTextLabel(textLabel: textLabel);
        textBlocks.Add(item: text);
        if (visible.Contains(point: textLabel.Position))
        {
          content.Children.Add(element: text);
        }
        else
        {
          text.Visibility = Visibility.Hidden;
        }
      }
    }
    else
    {
      foreach (var text in textBlocks)
      {
        var label = (IsolineTextLabel)text.Tag;
        var screenPos = label.Position.DataToScreen(transform: transform);
        var textSize = text.DesiredSize;

        Point position = new(x: screenPos.X - textSize.Width / 2, y: screenPos.Y - textSize.Height / 2);

        if (output.Contains(point: position))
        {
          Canvas.SetLeft(element: text, length: position.X);
          Canvas.SetTop(element: text, length: position.Y);
          if (text.Visibility == Visibility.Hidden)
          {
            text.Visibility = Visibility.Visible;
            content.Children.Add(element: text);
          }
        }
        else if (text.Visibility == Visibility.Visible)
        {
          text.Visibility = Visibility.Hidden;
          content.Children.Remove(element: text);
        }
      }
    }
  }
}
