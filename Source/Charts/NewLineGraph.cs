using Crystal.Plot2D.Charts;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.DataSources;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Crystal.Plot2D;

/// <summary>
///   Represents a series of points connected by one polyline.
/// </summary>
public class NewLineGraph : Canvas, IPlotterElement
{
  static NewLineGraph()
  {
    Type thisType = typeof(NewLineGraph);
    Legend.DescriptionProperty.OverrideMetadata(forType: thisType, typeMetadata: new FrameworkPropertyMetadata(defaultValue: "LineGraph"));
    Legend.LegendItemsBuilderProperty.OverrideMetadata(forType: thisType, typeMetadata: new FrameworkPropertyMetadata(defaultValue: new LegendItemsBuilder(DefaultLegendItemsBuilder)));
  }

  private static IEnumerable<FrameworkElement> DefaultLegendItemsBuilder(IPlotterElement plotterElement)
  {
    NewLineGraph lineGraph = (NewLineGraph)plotterElement;
    Line line = new() { X1 = 0, Y1 = 10, X2 = 20, Y2 = 0, Stretch = Stretch.Fill, DataContext = lineGraph };
    line.SetBinding(dp: Shape.StrokeProperty, path: "Stroke");
    line.SetBinding(dp: Shape.StrokeThicknessProperty, path: "StrokeThickness");
    Legend.SetVisualContent(obj: lineGraph, value: line);
    var legendItem = LegendItemsHelper.BuildDefaultLegendItem(chart: lineGraph);
    yield return legendItem;
  }

  private readonly FilterCollection filters = new();

  /// <summary>
  ///   Initializes a new instance of the <see cref="LineGraph"/> class.
  /// </summary>
  public NewLineGraph()
  {
    filters.CollectionChanged += filters_CollectionChanged;
    RenderTransform = layoutTransform;
    Background = Brushes.Green.MakeTransparent(opacity: 0.3);
  }

  private void filters_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
  {
    // todo
  }

  /// <summary>
  ///   Initializes a new instance of the <see cref="LineGraph"/> class.
  /// </summary>
  /// <param name="pointSource">
  ///   The point source.
  /// </param>
  public NewLineGraph(IPointDataSource pointSource) : this()
  {
    DataSource = pointSource;
  }

  /// <summary>
  ///   Provides access to filters collection.
  /// </summary>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Content)]
  public FilterCollection Filters => filters;

  #region Properties

  #region DataSource property

  public IPointDataSource DataSource
  {
    get => (IPointDataSource)GetValue(dp: DataSourceProperty);
    set => SetValue(dp: DataSourceProperty, value: value);
  }

  public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register(
    name: nameof(DataSource),
    propertyType: typeof(IPointDataSource),
    ownerType: typeof(NewLineGraph),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: null, propertyChangedCallback: OnDataSourceReplaced));

  private static void OnDataSourceReplaced(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    NewLineGraph owner = (NewLineGraph)d;
    owner.OnDataSourceReplaced(prevDataSource: (IPointDataSource)e.OldValue, currDataSource: (IPointDataSource)e.NewValue);
  }

  private void OnDataSourceReplaced(IPointDataSource prevDataSource, IPointDataSource currDataSource)
  {
    if (prevDataSource != null)
    {
      prevDataSource.DataChanged -= OnDataChanged;
    }

    if (currDataSource != null)
    {
      currDataSource.DataChanged += OnDataChanged;
    }
    Update();
  }

  void OnDataChanged(object sender, EventArgs e)
  {
    Update();
  }

  #endregion // end of DataSource property

  #region Stroke property

  public Brush Stroke
  {
    get => (Brush)GetValue(dp: StrokeProperty);
    set => SetValue(dp: StrokeProperty, value: value);
  }

  public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
    name: nameof(Stroke),
    propertyType: typeof(Brush),
    ownerType: typeof(NewLineGraph),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: Brushes.Blue));

  #endregion // end of Stroke property

  #region StrokeThickness property

  public double StrokeThickness
  {
    get => (double)GetValue(dp: StrokeThicknessProperty);
    set => SetValue(dp: StrokeThicknessProperty, value: value);
  }

  public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
    name: nameof(StrokeThickness),
    propertyType: typeof(double),
    ownerType: typeof(NewLineGraph),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: 1.0));

  #endregion // end of StrokeThickness property

  #endregion // end of Properties

  private bool smoothLinesJoin = true;
  public bool SmoothLinesJoin
  {
    get => smoothLinesJoin;
    set
    {
      smoothLinesJoin = value;
      Update();
    }
  }

  private List<Point> FilterPoints(List<Point> points)
  {
    var filteredPoints = filters.Filter(points: points, screenRect: plotter.Viewport.Output);

    return filteredPoints;
  }

  void Viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
  {
    if (e.PropertyName == "Visible")
    {
      DataRect prevVisible = (DataRect)e.OldValue;
      DataRect currVisible = (DataRect)e.NewValue;

      if (currVisible.Size != prevVisible.Size)
      {
        Update();
      }
      else
      {
        UpdateTransform();
      }
    }
    else
    {
      Update();
    }
  }

  readonly TranslateTransform layoutTransform = new();
  private void UpdateTransform()
  {
    var currentTransform = plotter.Transform;

    var shift = transformWhenCreated.ViewportRect.Location.ViewportToScreen(transform: currentTransform)
      - currentTransform.ViewportRect.Location.ViewportToScreen(transform: currentTransform);

    layoutTransform.X = shift.X;
    layoutTransform.Y = shift.Y;

    Debug.WriteLine(message: "X=" + shift.X);
    Debug.WriteLine(message: "Y=" + shift.Y);
  }

  CoordinateTransform transformWhenCreated;
  readonly ResourcePool<Polyline> polylinePool = new();
  private const int pointCount = 500;

  private void Update()
  {
    if (Plotter == null)
    {
      return;
    }

    if (DataSource == null)
    {
      return;
    }

    layoutTransform.X = 0;
    layoutTransform.Y = 0;

    var dataSource = DataSource;
    var dataPoints = dataSource.GetPoints();

    transformWhenCreated = plotter.Transform;

    var contentBounds = dataPoints.GetBounds();
    Viewport2D.SetContentBounds(obj: this, value: contentBounds);

    foreach (Polyline polyline in Children)
    {
      polylinePool.Put(item: polyline);
    }

    Children.Clear();

    PointCollection pointCollection = new();
    foreach (var screenPoint in dataPoints.DataToScreen(transform: plotter.Transform))
    {
      if (pointCollection.Count < pointCount)
      {
        pointCollection.Add(value: screenPoint);
      }
      else
      {
        var polyline = polylinePool.GetOrCreate();
        polyline.Points = pointCollection;

        SetPolylineBindings(polyline: polyline);

        Children.Add(element: polyline);
        Dispatcher.Invoke(callback: () => { }, priority: DispatcherPriority.ApplicationIdle);
        pointCollection = new PointCollection();
      }
    }
  }

  private void SetPolylineBindings(Polyline polyline)
  {
    polyline.SetBinding(dp: Shape.StrokeProperty, binding: new Binding { Source = this, Path = new PropertyPath(path: "Stroke") });
    polyline.SetBinding(dp: Shape.StrokeThicknessProperty, binding: new Binding { Source = this, Path = new PropertyPath(path: "StrokeThickness") });
  }

  #region IPlotterElement Members

  private PlotterBase plotter;
  public void OnPlotterAttached(PlotterBase plotter)
  {
    this.plotter = (PlotterBase)plotter;
    this.plotter.Viewport.PropertyChanged += Viewport_PropertyChanged;

    plotter.CentralGrid.Children.Add(element: this);

    Update();
  }

  public void OnPlotterDetaching(PlotterBase plotter)
  {
    plotter.CentralGrid.Children.Remove(element: this);
    this.plotter.Viewport.PropertyChanged -= Viewport_PropertyChanged;
    this.plotter = null;
  }

  public PlotterBase Plotter => plotter;

  #endregion
}
