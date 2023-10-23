using Crystal.Plot2D.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Crystal.Plot2D.Charts;

/// <summary>
/// Defines a base class for axis UI representation.
/// Contains a number of properties that can be used to adjust ticks set and their look.
/// </summary>
/// <typeparam name="T"></typeparam>
[TemplatePart(Name = "PART_AdditionalLabelsCanvas", Type = typeof(StackCanvas))]
[TemplatePart(Name = "PART_CommonLabelsCanvas", Type = typeof(StackCanvas))]
[TemplatePart(Name = "PART_TicksPath", Type = typeof(Path))]
[TemplatePart(Name = "PART_ContentsGrid", Type = typeof(Grid))]
public abstract class AxisControl<T> : AxisControlBase
{
  private const string TemplateKey = "axisControlTemplate";
  private const string AdditionalLabelTransformKey = "additionalLabelsTransform";
  private const string PartAdditionalLabelsCanvas = "PART_AdditionalLabelsCanvas";
  private const string PartCommonLabelsCanvas = "PART_CommonLabelsCanvas";
  private const string PartTicksPath = "PART_TicksPath";
  private const string PartContentsGrid = "PART_ContentsGrid";

  /// <summary>
  /// Initializes a new instance of the <see cref="AxisControl&lt;T&gt;"/> class.
  /// </summary>
  protected AxisControl()
  {
    HorizontalContentAlignment = HorizontalAlignment.Stretch;
    VerticalContentAlignment = VerticalAlignment.Stretch;

    Background = Brushes.Transparent;
    //ClipToBounds = true;
    Focusable = false;

    UpdateUIResources();
    UpdateSizeGetters();
  }

  internal void MakeDependent() => independent = false;

  /// <summary>
  /// This conversion is performed to make horizontal one-string and two-string labels
  /// stay at one height.
  /// </summary>
  /// <param name="placement"></param>
  /// <returns></returns>
  private static AxisPlacement GetBetterPlacement(AxisPlacement placement)
  {
    switch (placement)
    {
      case AxisPlacement.Left:
        return AxisPlacement.Left;
      case AxisPlacement.Right:
        return AxisPlacement.Right;
      case AxisPlacement.Top:
        return AxisPlacement.Top;
      case AxisPlacement.Bottom:
        return AxisPlacement.Bottom;
      default:
        throw new NotSupportedException();
    }
  }

  #region Properties

  private AxisPlacement placement = AxisPlacement.Bottom;
  /// <summary>
  /// Gets or sets the placement of axis control.
  /// Relative positioning of parts of axis depends on this value.
  /// </summary>
  /// <value>The placement.</value>
  public AxisPlacement Placement
  {
    get => placement;
    set
    {
      if (placement != value)
      {
        placement = value;
        UpdateUIResources();
        UpdateSizeGetters();
      }
    }
  }

  private void UpdateSizeGetters()
  {
    switch (placement)
    {
      case AxisPlacement.Left:
      case AxisPlacement.Right:
        getSize = size => size.Height;
        getCoordinate = p => p.Y;
        createScreenPoint1 = d => new Point(x: scrCoord1, y: d);
        createScreenPoint2 = (d, size) => new Point(x: scrCoord2 * size, y: d);
        break;
      case AxisPlacement.Top:
      case AxisPlacement.Bottom:
        getSize = size => size.Width;
        getCoordinate = p => p.X;
        createScreenPoint1 = d => new Point(x: d, y: scrCoord1);
        createScreenPoint2 = (d, size) => new Point(x: d, y: scrCoord2 * size);
        break;
    }

    switch (placement)
    {
      case AxisPlacement.Left:
        createDataPoint = d => new Point(x: 0, y: d);
        break;
      case AxisPlacement.Right:
        createDataPoint = d => new Point(x: 1, y: d);
        break;
      case AxisPlacement.Top:
        createDataPoint = d => new Point(x: d, y: 1);
        break;
      case AxisPlacement.Bottom:
        createDataPoint = d => new Point(x: d, y: 0);
        break;
    }
  }

  private void UpdateUIResources()
  {
    ResourceDictionary resources = new()
    {
      Source = new Uri(uriString: Constants.AxisResourceUri, uriKind: UriKind.Relative)
    };

    AxisPlacement placement = GetBetterPlacement(placement: this.placement);
    ControlTemplate template = (ControlTemplate)resources[key: TemplateKey + placement.ToString()];
    Verify.AssertNotNull(obj: template);
    var content = (FrameworkElement)template.LoadContent();

    if (ticksPath != null && ticksPath.Data != null)
    {
      GeometryGroup group = (GeometryGroup)ticksPath.Data;
      foreach (var child in group.Children)
      {
        LineGeometry geometry = (LineGeometry)child;
        lineGeomPool.Put(item: geometry);
      }
      group.Children.Clear();
    }

    ticksPath = (Path)content.FindName(name: PartTicksPath);
    ticksPath.SnapsToDevicePixels = true;
    Verify.AssertNotNull(obj: ticksPath);

    // as this method can be called not only on loading of axisControl, but when its placement changes, internal panels
    // can be not empty and their contents should be released
    if (commonLabelsCanvas != null && labelProvider != null)
    {
      foreach (UIElement child in commonLabelsCanvas.Children)
      {
        if (child != null)
        {
          labelProvider.ReleaseLabel(label: child);
        }
      }

      labels = null;
      commonLabelsCanvas.Children.Clear();
    }

    commonLabelsCanvas = (StackCanvas)content.FindName(name: PartCommonLabelsCanvas);
    Verify.AssertNotNull(obj: commonLabelsCanvas);
    commonLabelsCanvas.Placement = placement;

    if (additionalLabelsCanvas != null && majorLabelProvider != null)
    {
      foreach (UIElement child in additionalLabelsCanvas.Children)
      {
        if (child != null)
        {
          majorLabelProvider.ReleaseLabel(label: child);
        }
      }
    }

    additionalLabelsCanvas = (StackCanvas)content.FindName(name: PartAdditionalLabelsCanvas);
    Verify.AssertNotNull(obj: additionalLabelsCanvas);
    additionalLabelsCanvas.Placement = placement;

    mainGrid = (Grid)content.FindName(name: PartContentsGrid);
    Verify.AssertNotNull(obj: mainGrid);

    mainGrid.SetBinding(dp: BackgroundProperty, binding: new Binding { Path = new PropertyPath(path: "Background"), Source = this });
    mainGrid.SizeChanged += mainGrid_SizeChanged;

    Content = mainGrid;

    string transformKey = AdditionalLabelTransformKey + placement.ToString();
    if (resources.Contains(key: transformKey))
    {
      additionalLabelTransform = (Transform)resources[key: transformKey];
    }
  }

  void mainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
  {
    if (placement.IsBottomOrTop() && e.WidthChanged ||
       e.HeightChanged)
    {
      // this is performed because if not, whole axisControl's size was measured wrongly.
      InvalidateMeasure();
      UpdateUI();
    }
  }

  private bool updateOnCommonChange = true;

  internal IDisposable OpenUpdateRegion(bool forceUpdate)
  {
    return new UpdateRegionHolder<T>(owner: this, forceUpdate: forceUpdate);
  }

  private sealed class UpdateRegionHolder<TT> : IDisposable
  {
    private readonly Range<TT> prevRange;
    private readonly CoordinateTransform prevTransform;
    private AxisControl<TT> owner;
    private readonly bool forceUpdate;

    public UpdateRegionHolder(AxisControl<TT> owner) : this(owner: owner, forceUpdate: false) { }

    public UpdateRegionHolder(AxisControl<TT> owner, bool forceUpdate)
    {
      this.owner = owner;
      owner.updateOnCommonChange = false;

      prevTransform = owner.transform;
      prevRange = owner.range;
      this.forceUpdate = forceUpdate;
    }

    #region IDisposable Members

    public void Dispose()
    {
      owner.updateOnCommonChange = true;

      bool shouldUpdate = owner.range != prevRange;

      var screenRect = owner.Transform.ScreenRect;
      var prevScreenRect = prevTransform.ScreenRect;
      if (owner.placement.IsBottomOrTop())
      {
        shouldUpdate |= prevScreenRect.Width != screenRect.Width;
      }
      else
      {
        shouldUpdate |= prevScreenRect.Height != screenRect.Height;
      }

      shouldUpdate |= owner.transform.DataTransform != prevTransform.DataTransform;
      shouldUpdate |= forceUpdate;

      if (shouldUpdate)
      {
        owner.UpdateUI();
      }
      owner = null;
    }

    #endregion
  }

  private Range<T> range;
  /// <summary>
  /// Gets or sets the range, which ticks are generated for.
  /// </summary>
  /// <value>The range.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Range<T> Range
  {
    get => range;
    set
    {
      range = value;
      if (updateOnCommonChange)
      {
        UpdateUI();
      }
    }
  }

  private bool drawMinorTicks = true;
  /// <summary>
  /// Gets or sets a value indicating whether to show minor ticks.
  /// </summary>
  /// <value><c>true</c> if show minor ticks; otherwise, <c>false</c>.</value>
  public bool DrawMinorTicks
  {
    get => drawMinorTicks;
    set
    {
      if (drawMinorTicks != value)
      {
        drawMinorTicks = value;
        UpdateUI();
      }
    }
  }

  private bool drawMajorLabels = true;
  /// <summary>
  /// Gets or sets a value indicating whether to show major labels.
  /// </summary>
  /// <value><c>true</c> if show major labels; otherwise, <c>false</c>.</value>
  public bool DrawMajorLabels
  {
    get => drawMajorLabels;
    set
    {
      if (drawMajorLabels != value)
      {
        drawMajorLabels = value;
        UpdateUI();
      }
    }
  }

  private bool drawTicks = true;
  public bool DrawTicks
  {
    get => drawTicks;
    set
    {
      if (drawTicks != value)
      {
        drawTicks = value;
        UpdateUI();
      }
    }
  }

  #region TicksProvider

  private ITicksProvider<T> ticksProvider;
  /// <summary>
  /// Gets or sets the ticks provider - generator of ticks for given range.
  /// 
  /// Should not be null.
  /// </summary>
  /// <value>The ticks provider.</value>
  public ITicksProvider<T> TicksProvider
  {
    get => ticksProvider;
    set
    {
      if (value == null)
      {
        throw new ArgumentNullException(paramName: "value");
      }

      if (ticksProvider != value)
      {
        DetachTicksProvider();

        ticksProvider = value;

        AttachTicksProvider();

        UpdateUI();
      }
    }
  }

  private void AttachTicksProvider()
  {
    if (ticksProvider != null)
    {
      ticksProvider.Changed += ticksProvider_Changed;
    }
  }

  private void ticksProvider_Changed(object sender, EventArgs e)
  {
    UpdateUI();
  }

  private void DetachTicksProvider()
  {
    if (ticksProvider != null)
    {
      ticksProvider.Changed -= ticksProvider_Changed;
    }
  }

  #endregion

  [EditorBrowsable(state: EditorBrowsableState.Never)]
  public override bool ShouldSerializeContent()
  {
    return false;
  }

  protected override bool ShouldSerializeProperty(DependencyProperty dp)
  {
    // do not serialize template - for XAML serialization
    if (dp == TemplateProperty)
    {
      return false;
    }

    return base.ShouldSerializeProperty(dp: dp);
  }

  #region MajorLabelProvider

  private LabelProviderBase<T> majorLabelProvider;
  /// <summary>
  /// Gets or sets the major label provider, which creates labels for major ticks.
  /// If null, major labels will not be shown.
  /// </summary>
  /// <value>The major label provider.</value>
  public LabelProviderBase<T> MajorLabelProvider
  {
    get => majorLabelProvider;
    set
    {
      if (majorLabelProvider != value)
      {
        DetachMajorLabelProvider();

        majorLabelProvider = value;

        AttachMajorLabelProvider();

        UpdateUI();
      }
    }
  }

  private void AttachMajorLabelProvider()
  {
    if (majorLabelProvider != null)
    {
      majorLabelProvider.Changed += majorLabelProvider_Changed;
    }
  }

  private void majorLabelProvider_Changed(object sender, EventArgs e)
  {
    UpdateUI();
  }

  private void DetachMajorLabelProvider()
  {
    if (majorLabelProvider != null)
    {
      majorLabelProvider.Changed -= majorLabelProvider_Changed;
    }
  }

  #endregion

  #region LabelProvider

  private LabelProviderBase<T> labelProvider;
  /// <summary>
  /// Gets or sets the label provider, which generates labels for axis ticks.
  /// Should not be null.
  /// </summary>
  /// <value>The label provider.</value>
  [NotNull]
  public LabelProviderBase<T> LabelProvider
  {
    get => labelProvider;
    set
    {
      if (value == null)
      {
        throw new ArgumentNullException(paramName: "value");
      }

      if (labelProvider != value)
      {
        DetachLabelProvider();

        labelProvider = value;

        AttachLabelProvider();

        UpdateUI();
      }
    }
  }

  private void AttachLabelProvider()
  {
    if (labelProvider != null)
    {
      labelProvider.Changed += labelProvider_Changed;
    }
  }

  private void labelProvider_Changed(object sender, EventArgs e)
  {
    UpdateUI();
  }

  private void DetachLabelProvider()
  {
    if (labelProvider != null)
    {
      labelProvider.Changed -= labelProvider_Changed;
    }
  }

  #endregion

  private CoordinateTransform transform = CoordinateTransform.CreateDefault();
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  [EditorBrowsable(state: EditorBrowsableState.Never)]
  public CoordinateTransform Transform
  {
    get => transform;
    set
    {
      transform = value;
      if (updateOnCommonChange)
      {
        UpdateUI();
      }
    }
  }

  #endregion

  private const double defaultSmallerSize = 1;
  private const double defaultBiggerSize = 150;
  protected override Size MeasureOverride(Size constraint)
  {
    var baseSize = base.MeasureOverride(constraint: constraint);

    mainGrid.Measure(availableSize: constraint);
    Size gridSize = mainGrid.DesiredSize;
    Size result = gridSize;

    bool isHorizontal = placement == AxisPlacement.Bottom || placement == AxisPlacement.Top;
    if (double.IsInfinity(d: constraint.Width) && isHorizontal)
    {
      result = new Size(width: defaultBiggerSize, height: gridSize.Height != 0 ? gridSize.Height : defaultSmallerSize);
    }
    else if (double.IsInfinity(d: constraint.Height) && !isHorizontal)
    {
      result = new Size(width: gridSize.Width != 0 ? gridSize.Width : defaultSmallerSize, height: defaultBiggerSize);
    }

    return result;
  }

  //protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
  //{
  //    base.OnRenderSizeChanged(sizeInfo);

  //    bool isHorizontal = placement == AxisPlacement.Top || placement == AxisPlacement.Bottom;
  //    if (isHorizontal && sizeInfo.WidthChanged || !isHorizontal && sizeInfo.HeightChanged)
  //    {
  //        UpdateUIRepresentation();
  //    }
  //}

  private void InitTransform(Size newRenderSize)
  {
    Rect dataRect = CreateDataRect();

    transform = transform.WithRects(visibleRect: dataRect, screenRect: new Rect(size: newRenderSize));
  }

  private Rect CreateDataRect()
  {
    double min = convertToDouble(arg: range.Min);
    double max = convertToDouble(arg: range.Max);

    Rect dataRect;
    switch (placement)
    {
      case AxisPlacement.Left:
      case AxisPlacement.Right:
        dataRect = new Rect(point1: new Point(x: min, y: min), point2: new Point(x: max, y: max));
        break;
      case AxisPlacement.Top:
      case AxisPlacement.Bottom:
        dataRect = new Rect(point1: new Point(x: min, y: min), point2: new Point(x: max, y: max));
        break;
      default:
        throw new NotSupportedException();
    }
    return dataRect;
  }

  /// <summary>
  /// Gets the Path with ticks strokes.
  /// </summary>
  /// <value>The ticks path.</value>
  public override Path TicksPath => ticksPath;

  private Grid mainGrid;
  private StackCanvas additionalLabelsCanvas;
  private StackCanvas commonLabelsCanvas;
  private Path ticksPath;
  private bool rendered;

  protected override void OnRender(DrawingContext dc)
  {
    base.OnRender(drawingContext: dc);

    if (!rendered)
    {
      UpdateUI();
    }
    rendered = true;
  }

  private bool independent = true;

  private readonly double scrCoord1 = 0; // px
  private double scrCoord2 = 10; // px
  /// <summary>
  /// Gets or sets the size of main axis ticks.
  /// </summary>
  /// <value>The size of the tick.</value>
  public double TickSize
  {
    get => scrCoord2;
    set
    {
      if (scrCoord2 != value)
      {
        scrCoord2 = value;
        UpdateUI();
      }
    }
  }

  private GeometryGroup geomGroup = new();
  internal void UpdateUI()
  {
    if (range.IsEmpty)
    {
      return;
    }

    if (transform == null)
    {
      return;
    }

    if (independent)
    {
      InitTransform(newRenderSize: RenderSize);
    }

    bool isHorizontal = Placement == AxisPlacement.Bottom || Placement == AxisPlacement.Top;
    if (transform.ScreenRect.Width == 0 && isHorizontal
        || transform.ScreenRect.Height == 0 && !isHorizontal)
    {
      return;
    }

    if (!IsMeasureValid)
    {
      InvalidateMeasure();
    }

    CreateTicks();

    // removing unfinite screen ticks
    var tempTicks = new List<T>(collection: ticks);
    var tempScreenTicks = new List<double>(capacity: ticks.Length);
    var tempLabels = new List<UIElement>(collection: labels);

    int i = 0;
    while (i < tempTicks.Count)
    {
      T tick = tempTicks[index: i];
      double screenTick = getCoordinate(arg: createDataPoint(arg: convertToDouble(arg: tick)).DataToScreen(transform: transform));
      if (screenTick.IsFinite())
      {
        tempScreenTicks.Add(item: screenTick);
        i++;
      }
      else
      {
        tempTicks.RemoveAt(index: i);
        tempLabels.RemoveAt(index: i);
      }
    }

    ticks = tempTicks.ToArray();
    screenTicks = tempScreenTicks.ToArray();
    labels = tempLabels.ToArray();

    // saving generated lines into pool
    for (i = 0; i < geomGroup.Children.Count; i++)
    {
      var geometry = (LineGeometry)geomGroup.Children[index: i];
      lineGeomPool.Put(item: geometry);
    }

    geomGroup = new GeometryGroup
    {
      Children = new GeometryCollection(capacity: lineGeomPool.Count)
    };

    if (drawTicks)
    {
      DoDrawTicks(screenTicksX: screenTicks, lines: geomGroup.Children);
    }

    if (drawMinorTicks)
    {
      DoDrawMinorTicks(lines: geomGroup.Children);
    }

    ticksPath.Data = geomGroup;

    DoDrawCommonLabels(screenTicksX: screenTicks);

    if (drawMajorLabels)
    {
      DoDrawMajorLabels();
    }

    ScreenTicksChanged.Raise(sender: this);
  }

  bool drawTicksOnEmptyLabel;
  /// <summary>
  /// Gets or sets a value indicating whether to draw ticks on empty label.
  /// </summary>
  /// <value>
  /// 	<c>true</c> if draw ticks on empty label; otherwise, <c>false</c>.
  /// </value>
  public bool DrawTicksOnEmptyLabel
  {
    get => drawTicksOnEmptyLabel;
    set
    {
      if (drawTicksOnEmptyLabel != value)
      {
        drawTicksOnEmptyLabel = value;
        UpdateUI();
      }
    }
  }

  private readonly ResourcePool<LineGeometry> lineGeomPool = new();
  private void DoDrawTicks(double[] screenTicksX, ICollection<Geometry> lines)
  {
    for (int i = 0; i < screenTicksX.Length; i++)
    {
      if (labels[i] == null && !drawTicksOnEmptyLabel)
      {
        continue;
      }

      Point p1 = createScreenPoint1(arg: screenTicksX[i]);
      Point p2 = createScreenPoint2(arg1: screenTicksX[i], arg2: 1);

      LineGeometry line = lineGeomPool.GetOrCreate();

      line.StartPoint = p1;
      line.EndPoint = p2;
      lines.Add(item: line);
    }
  }

  private double GetRangesRatio(Range<T> nominator, Range<T> denominator)
  {
    double nomMin = ConvertToDouble(arg: nominator.Min);
    double nomMax = ConvertToDouble(arg: nominator.Max);
    double denMin = ConvertToDouble(arg: denominator.Min);
    double denMax = ConvertToDouble(arg: denominator.Max);

    return (nomMax - nomMin) / (denMax - denMin);
  }

  Transform additionalLabelTransform;
  private void DoDrawMajorLabels()
  {
    ITicksProvider<T> majorTicksProvider = ticksProvider.MajorProvider;
    additionalLabelsCanvas.Children.Clear();

    if (majorTicksProvider != null && majorLabelProvider != null)
    {
      additionalLabelsCanvas.Visibility = Visibility.Visible;

      Size renderSize = RenderSize;
      var majorTicks = majorTicksProvider.GetTicks(range: range, ticksCount: DefaultTicksProvider.DefaultTicksCount);

      double[] screenCoords = majorTicks.Ticks.Select(selector: tick => createDataPoint(arg: convertToDouble(arg: tick))).
          Select(selector: p => p.DataToScreen(transform: transform)).Select(selector: p => getCoordinate(arg: p)).ToArray();

      // todo this is not the best decision - when displaying, for example,
      // milliseconds, it causes to create hundreds and thousands of textBlocks.
      double rangesRatio = GetRangesRatio(nominator: majorTicks.Ticks.GetPairs().ToArray()[0], denominator: range);

      object info = majorTicks.Info;
      MajorLabelsInfo newInfo = new()
      {
        Info = info,
        MajorLabelsCount = (int)Math.Ceiling(a: rangesRatio)
      };

      var newMajorTicks = new TicksInfo<T>
      {
        Info = newInfo,
        Ticks = majorTicks.Ticks,
        TickSizes = majorTicks.TickSizes
      };

      UIElement[] additionalLabels = MajorLabelProvider.CreateLabels(ticksInfo: newMajorTicks);

      for (int i = 0; i < additionalLabels.Length; i++)
      {
        if (screenCoords[i].IsNaN())
        {
          continue;
        }

        UIElement tickLabel = additionalLabels[i];

        tickLabel.Measure(availableSize: renderSize);

        StackCanvas.SetCoordinate(obj: tickLabel, value: screenCoords[i]);
        StackCanvas.SetEndCoordinate(obj: tickLabel, value: screenCoords[i + 1]);

        if (tickLabel is FrameworkElement)
        {
          ((FrameworkElement)tickLabel).LayoutTransform = additionalLabelTransform;
        }

        additionalLabelsCanvas.Children.Add(element: tickLabel);
      }
    }
    else
    {
      additionalLabelsCanvas.Visibility = Visibility.Collapsed;
    }
  }

  private int prevMinorTicksCount = DefaultTicksProvider.DefaultTicksCount;
  private const int maxTickArrangeIterations = 12;
  private void DoDrawMinorTicks(ICollection<Geometry> lines)
  {
    ITicksProvider<T> minorTicksProvider = ticksProvider.MinorProvider;
    if (minorTicksProvider != null)
    {
      int minorTicksCount = prevMinorTicksCount;
      int prevActualTicksCount = -1;
      ITicksInfo<T> minorTicks;
      TickCountChange result = TickCountChange.OK;
      int iteration = 0;
      do
      {
        Verify.IsTrue(condition: ++iteration < maxTickArrangeIterations);

        minorTicks = minorTicksProvider.GetTicks(range: range, ticksCount: minorTicksCount);

        prevActualTicksCount = minorTicks.Ticks.Length;
        var prevResult = result;
        result = CheckMinorTicksArrangement(minorTicks: minorTicks);
        if (prevResult == TickCountChange.Decrease && result == TickCountChange.Increase)
        {
          // stop tick number oscillating
          result = TickCountChange.OK;
        }
        if (result == TickCountChange.Decrease)
        {
          int newMinorTicksCount = minorTicksProvider.DecreaseTickCount(ticksCount: minorTicksCount);
          if (newMinorTicksCount == minorTicksCount)
          {
            result = TickCountChange.OK;
          }
          minorTicksCount = newMinorTicksCount;
        }
        else if (result == TickCountChange.Increase)
        {
          int newCount = minorTicksProvider.IncreaseTickCount(ticksCount: minorTicksCount);
          if (newCount == minorTicksCount)
          {
            result = TickCountChange.OK;
          }
          minorTicksCount = newCount;
        }

      } while (result != TickCountChange.OK);
      prevMinorTicksCount = minorTicksCount;

      double[] sizes = minorTicks.TickSizes;

      double[] screenCoords = minorTicks.Ticks.Select(
          selector: coord => getCoordinate(arg: createDataPoint(arg: convertToDouble(arg: coord)).
              DataToScreen(transform: transform))).ToArray();

      minorScreenTicks = new MinorTickInfo<double>[screenCoords.Length];
      for (int i = 0; i < screenCoords.Length; i++)
      {
        minorScreenTicks[i] = new MinorTickInfo<double>(value: sizes[i], tick: screenCoords[i]);
      }

      for (int i = 0; i < screenCoords.Length; i++)
      {
        double screenCoord = screenCoords[i];

        Point p1 = createScreenPoint1(arg: screenCoord);
        Point p2 = createScreenPoint2(arg1: screenCoord, arg2: sizes[i]);

        LineGeometry line = lineGeomPool.GetOrCreate();
        line.StartPoint = p1;
        line.EndPoint = p2;

        lines.Add(item: line);
      }
    }
  }

  private TickCountChange CheckMinorTicksArrangement(ITicksInfo<T> minorTicks)
  {
    Size renderSize = RenderSize;
    TickCountChange result = TickCountChange.OK;
    if (minorTicks.Ticks.Length * 3 > getSize(arg: renderSize))
    {
      result = TickCountChange.Decrease;
    }
    else if (minorTicks.Ticks.Length * 6 < getSize(arg: renderSize))
    {
      result = TickCountChange.Increase;
    }

    return result;
  }

  private bool isStaticAxis;
  /// <summary>
  /// Gets or sets a value indicating whether this instance is a static axis.
  /// If axis is static, its labels from sides are shifted so that they are not clipped by axis bounds.
  /// </summary>
  /// <value>
  /// 	<c>true</c> if this instance is static axis; otherwise, <c>false</c>.
  /// </value>
  public bool IsStaticAxis
  {
    get => isStaticAxis;
    set
    {
      if (isStaticAxis != value)
      {
        isStaticAxis = value;
        UpdateUI();
      }
    }
  }

  private double ToScreen(T value)
  {
    return getCoordinate(arg: createDataPoint(arg: convertToDouble(arg: value)).DataToScreen(transform: transform));
  }

  private readonly double staticAxisMargin = 1; // px

  private void DoDrawCommonLabels(double[] screenTicksX)
  {
    Size renderSize = RenderSize;

    commonLabelsCanvas.Children.Clear();

#if DEBUG
    if (labels != null)
    {
      foreach (FrameworkElement item in labels)
      {
        if (item != null)
        {
          Debug.Assert(condition: item.Parent == null);
        }
      }
    }
#endif

    double minCoordUnsorted = ToScreen(value: range.Min);
    double maxCoordUnsorted = ToScreen(value: range.Max);

    double minCoord = Math.Min(val1: minCoordUnsorted, val2: maxCoordUnsorted);
    double maxCoord = Math.Max(val1: minCoordUnsorted, val2: maxCoordUnsorted);

    double maxCoordDiff = (maxCoord - minCoord) / labels.Length / 2.0;

    double minCoordToAdd = minCoord - maxCoordDiff;
    double maxCoordToAdd = maxCoord + maxCoordDiff;

    for (int i = 0; i < ticks.Length; i++)
    {
      FrameworkElement tickLabel = (FrameworkElement)labels[i];
      if (tickLabel == null)
      {
        continue;
      }

      Debug.Assert(condition: tickLabel.Parent == null);

      tickLabel.Measure(availableSize: new Size(width: double.PositiveInfinity, height: double.PositiveInfinity));

      double screenX = screenTicksX[i];
      double coord = screenX;

      tickLabel.HorizontalAlignment = HorizontalAlignment.Center;
      tickLabel.VerticalAlignment = VerticalAlignment.Center;

      if (isStaticAxis)
      {
        // getting real size of label
        tickLabel.Measure(availableSize: renderSize);
        Size tickLabelSize = tickLabel.DesiredSize;

        if (Math.Abs(value: screenX - minCoord) < maxCoordDiff)
        {
          coord = minCoord + staticAxisMargin;
          if (placement.IsBottomOrTop())
          {
            tickLabel.HorizontalAlignment = HorizontalAlignment.Left;
          }
          else
          {
            tickLabel.VerticalAlignment = VerticalAlignment.Top;
          }
        }
        else if (Math.Abs(value: screenX - maxCoord) < maxCoordDiff)
        {
          coord = maxCoord - getSize(arg: tickLabelSize) / 2 - staticAxisMargin;
          if (!placement.IsBottomOrTop())
          {
            tickLabel.VerticalAlignment = VerticalAlignment.Bottom;
            coord = maxCoord - staticAxisMargin;
          }
        }
      }

      // label is out of visible area
      if (coord < minCoord || coord > maxCoord)
      {
        continue;
      }

      if (coord.IsNaN())
      {
        continue;
      }

      StackCanvas.SetCoordinate(obj: tickLabel, value: coord);

      commonLabelsCanvas.Children.Add(element: tickLabel);
    }
  }

  private double GetCoordinateFromTick(T tick)
  {
    return getCoordinate(arg: createDataPoint(arg: convertToDouble(arg: tick)).DataToScreen(transform: transform));
  }

  private Func<T, double> convertToDouble;
  /// <summary>
  /// Gets or sets the convertion of tick to double.
  /// Should not be null.
  /// </summary>
  /// <value>The convert to double.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Func<T, double> ConvertToDouble
  {
    get => convertToDouble;
    set
    {
      if (value == null)
      {
        throw new ArgumentNullException(paramName: "value");
      }

      convertToDouble = value;
      UpdateUI();
    }
  }

  internal event EventHandler ScreenTicksChanged;
  private double[] screenTicks;
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  [EditorBrowsable(state: EditorBrowsableState.Never)]
  public double[] ScreenTicks => screenTicks;

  private MinorTickInfo<double>[] minorScreenTicks;
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  [EditorBrowsable(state: EditorBrowsableState.Never)]
  public MinorTickInfo<double>[] MinorScreenTicks => minorScreenTicks;

  ITicksInfo<T> ticksInfo;
  private T[] ticks;
  private UIElement[] labels;
  private const double increaseRatio = 3.0;
  private const double decreaseRatio = 1.6;

  private Func<Size, double> getSize = size => size.Width;
  private Func<Point, double> getCoordinate = p => p.X;
  private Func<double, Point> createDataPoint = d => new Point(x: d, y: 0);

  private Func<double, Point> createScreenPoint1 = d => new Point(x: d, y: 0);
  private Func<double, double, Point> createScreenPoint2 = (d, size) => new Point(x: d, y: size);

  private int previousTickCount = DefaultTicksProvider.DefaultTicksCount;
  private void CreateTicks()
  {
    TickCountChange result = TickCountChange.OK;

    int prevActualTickCount = -1;

    int tickCount = previousTickCount;
    int iteration = 0;

    do
    {
      Verify.IsTrue(condition: ++iteration < maxTickArrangeIterations);

      ticksInfo = ticksProvider.GetTicks(range: range, ticksCount: tickCount);
      ticks = ticksInfo.Ticks;

      if (ticks.Length == prevActualTickCount)
      {
        break;
      }

      prevActualTickCount = ticks.Length;

      if (labels != null)
      {
        for (int i = 0; i < labels.Length; i++)
        {
          labelProvider.ReleaseLabel(label: labels[i]);
        }
      }

      labels = labelProvider.CreateLabels(ticksInfo: ticksInfo);

      var prevResult = result;
      result = CheckLabelsArrangement(labels: labels, ticks: ticks);

      if (prevResult == TickCountChange.Decrease && result == TickCountChange.Increase)
      {
        // stop tick number oscillating
        result = TickCountChange.OK;
      }

      if (result != TickCountChange.OK)
      {
        int prevTickCount = tickCount;
        if (result == TickCountChange.Decrease)
        {
          tickCount = ticksProvider.DecreaseTickCount(ticksCount: tickCount);
        }
        else
        {
          tickCount = ticksProvider.IncreaseTickCount(ticksCount: tickCount);
          //DebugVerify.Is(tickCount >= prevTickCount);
        }

        // ticks provider could not create less ticks or tick number didn't change
        if (tickCount == 0 || prevTickCount == tickCount)
        {
          tickCount = prevTickCount;
          result = TickCountChange.OK;
        }
      }
    } while (result != TickCountChange.OK);

    previousTickCount = tickCount;
  }

  private TickCountChange CheckLabelsArrangement(UIElement[] labels, T[] ticks)
  {
    var actualLabels = labels.Select(selector: (label, i) => new { Label = label, Index = i })
        .Where(predicate: el => el.Label != null)
        .Select(selector: el => new { Label = el.Label, Tick = ticks[el.Index] })
        .ToList();

    actualLabels.ForEach(action: item => item.Label.Measure(availableSize: RenderSize));

    var sizeInfos = actualLabels.Select(selector: item =>
        new { X = GetCoordinateFromTick(tick: item.Tick), Size = getSize(arg: item.Label.DesiredSize) })
        .OrderBy(keySelector: item => item.X).ToArray();

    TickCountChange res = TickCountChange.OK;

    int increaseCount = 0;
    for (int i = 0; i < sizeInfos.Length - 1; i++)
    {
      if ((sizeInfos[i].X + sizeInfos[i].Size * decreaseRatio) > sizeInfos[i + 1].X)
      {
        res = TickCountChange.Decrease;
        break;
      }
      if ((sizeInfos[i].X + sizeInfos[i].Size * increaseRatio) < sizeInfos[i + 1].X)
      {
        increaseCount++;
      }
    }
    if (increaseCount > sizeInfos.Length / 2)
    {
      res = TickCountChange.Increase;
    }

    return res;
  }
}

[DebuggerDisplay(value: "{X} + {Size}")]
internal sealed class SizeInfo : IComparable<SizeInfo>
{
  public double Size { get; set; }
  public double X { get; set; }


  public int CompareTo(SizeInfo other)
  {
    return X.CompareTo(value: other.X);
  }
}

internal enum TickCountChange
{
  Increase = -1,
  OK = 0,
  Decrease = 1
}

/// <summary>
/// Represents an auxiliary structure for storing additional info during major DateTime labels generation.
/// </summary>
public struct MajorLabelsInfo
{
  public object Info { get; set; }
  public int MajorLabelsCount { get; set; }

  public override string ToString()
  {
    return string.Format(format: "{0}, Count={1}", arg0: Info, arg1: MajorLabelsCount);
  }
}
