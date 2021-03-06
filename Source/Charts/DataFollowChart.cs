using Crystal.Plot2D.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Crystal.Plot2D.Charts
{
  /// <summary>
  ///   Represents a marker with position.X bound to mouse cursor's position and position.
  ///   Y is determined by interpolation of <see cref="MarkerPointsGraph"/>'s points.
  /// </summary>
  [ContentProperty("MarkerTemplate")]
  public class DataFollowChart : ViewportHostPanel, INotifyPropertyChanged
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="DataFollowChart"/> class.
    /// </summary>
    public DataFollowChart()
    {
      Marker = CreateDefaultMarker();
      SetX(marker, 0);
      SetY(marker, 0);
      Children.Add(marker);
    }

    private static Ellipse CreateDefaultMarker()
    {
      return new Ellipse
      {
        Width = 10,
        Height = 10,
        Stroke = Brushes.Green,
        StrokeThickness = 1,
        Fill = Brushes.LightGreen,
        Visibility = Visibility.Hidden
      };
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="DataFollowChart"/> class, bound to specified <see cref="PointsGraphBase"/>.
    /// </summary>
    /// <param name="pointSource">
    ///   The point source.
    /// </param>
    public DataFollowChart(PointsGraphBase pointSource) : this()
    {
      PointSource = pointSource;
    }

    #region MarkerTemplate property

    /// <summary>
    ///   Gets or sets the template, used to create a marker. This is a dependency property.
    /// </summary>
    /// <value>
    ///   The marker template.
    /// </value>
    public DataTemplate MarkerTemplate
    {
      get { return (DataTemplate)GetValue(MarkerTemplateProperty); }
      set { SetValue(MarkerTemplateProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="MarkerTemplate"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MarkerTemplateProperty = DependencyProperty.Register(
      "MarkerTemplate",
      typeof(DataTemplate),
      typeof(DataFollowChart),
      new FrameworkPropertyMetadata(null, OnMarkerTemplateChanged));

    private static void OnMarkerTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      DataFollowChart chart = (DataFollowChart)d;
      DataTemplate template = (DataTemplate)e.NewValue;

      FrameworkElement marker;
      if (template != null)
      {
        marker = (FrameworkElement)template.LoadContent();
      }
      else
      {
        marker = CreateDefaultMarker();
      }

      chart.Children.Remove(chart.marker);
      chart.Marker = marker;
      chart.Children.Add(marker);
    }

    #endregion

    #region Point sources

    /// <summary>
    ///   Gets or sets the source of points. Can be null.
    /// </summary>
    /// <value>
    ///   The point source.
    /// </value>
    public PointsGraphBase PointSource
    {
      get { return (PointsGraphBase)GetValue(PointSourceProperty); }
      set { SetValue(PointSourceProperty, value); }
    }

    /// <summary>
    ///   Identifies the <see cref="PointSource"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty PointSourceProperty = DependencyProperty.Register(
      "PointSource",
      typeof(PointsGraphBase),
      typeof(DataFollowChart),
      new FrameworkPropertyMetadata(null, OnPointSourceChanged));

    private static void OnPointSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      DataFollowChart chart = (DataFollowChart)d;
      if (e.OldValue is PointsGraphBase previous)
      {
        previous.VisiblePointsChanged -= chart.Source_VisiblePointsChanged;
      }

      if (e.NewValue is PointsGraphBase current)
      {
        current.ProvideVisiblePoints = true;
        current.VisiblePointsChanged += chart.Source_VisiblePointsChanged;
        if (current.VisiblePoints != null)
        {
          chart.searcher = new SortedXSearcher1d(current.VisiblePoints);
        }
      }

      chart.UpdateUIRepresentation();
    }

    private SearchResult1d searchResult = SearchResult1d.Empty;
    private SortedXSearcher1d searcher;
    private FrameworkElement marker;

    [NotNull]
    public FrameworkElement Marker
    {
      get { return marker; }
      protected set
      {
        marker = value;
        marker.DataContext = followDataContext;
        PropertyChanged.Raise(this, "Marker");
      }
    }

    private readonly FollowDataContext followDataContext = new();
    public FollowDataContext FollowDataContext => followDataContext;

    private void UpdateUIRepresentation()
    {
      if (Plotter == null)
      {
        return;
      }

      PointsGraphBase source = PointSource;
      if (source == null || (source != null && source.VisiblePoints == null))
      {
        SetValue(MarkerPositionPropertyKey, new Point(double.NaN, double.NaN));
        marker.Visibility = Visibility.Hidden;
        return;
      }
      else
      {
        Point mousePos = Mouse.GetPosition(Plotter.CentralGrid);
        var transform = Plotter.Transform;
        Point viewportPos = mousePos.ScreenToViewport(transform);
        double x = viewportPos.X;
        searchResult = searcher.SearchXBetween(x, searchResult);
        SetValue(ClosestPointIndexPropertyKey, searchResult.Index);
        if (!searchResult.IsEmpty)
        {
          marker.Visibility = Visibility.Visible;
          IList<Point> points = source.VisiblePoints;
          Point ptBefore = points[searchResult.Index];
          Point ptAfter = points[searchResult.Index + 1];

          double ratio = (x - ptBefore.X) / (ptAfter.X - ptBefore.X);
          double y = ptBefore.Y + (ptAfter.Y - ptBefore.Y) * ratio;
          Point temp = new(x, y);
          SetX(marker, temp.X);
          SetY(marker, temp.Y);

          Point markerPosition = temp;
          followDataContext.Position = markerPosition;
          SetValue(MarkerPositionPropertyKey, markerPosition);
        }
        else
        {
          SetValue(MarkerPositionPropertyKey, new Point(double.NaN, double.NaN));
          marker.Visibility = Visibility.Hidden;
        }
      }
    }

    #region ClosestPointIndex property

    private static readonly DependencyPropertyKey ClosestPointIndexPropertyKey = DependencyProperty.RegisterReadOnly(
      "ClosestPointIndex",
      typeof(int),
      typeof(DataFollowChart),
      new PropertyMetadata(-1));

    public static readonly DependencyProperty ClosestPointIndexProperty = ClosestPointIndexPropertyKey.DependencyProperty;

    public int ClosestPointIndex
    {
      get { return (int)GetValue(ClosestPointIndexProperty); }
    }

    #endregion

    #region MarkerPositionProperty

    private static readonly DependencyPropertyKey MarkerPositionPropertyKey = DependencyProperty.RegisterReadOnly(
      "MarkerPosition",
      typeof(Point),
      typeof(DataFollowChart),
      new PropertyMetadata(new Point(), OnMarkerPositionChanged));

    private static void OnMarkerPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      DataFollowChart chart = (DataFollowChart)d;
      chart.MarkerPositionChanged.Raise(chart);
    }

    /// <summary>
    ///   Identifies the <see cref="MarkerPosition"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MarkerPositionProperty = MarkerPositionPropertyKey.DependencyProperty;

    /// <summary>
    ///   Gets the marker position.
    /// </summary>
    /// <value>
    ///   The marker position.
    /// </value>
    public Point MarkerPosition
    {
      get { return (Point)GetValue(MarkerPositionProperty); }
    }

    /// <summary>
    ///   Occurs when marker position changes.
    /// </summary>
    public event EventHandler MarkerPositionChanged;

    #endregion

    public override void OnPlotterAttached(PlotterBase plotter)
    {
      base.OnPlotterAttached(plotter);
      plotter.MainGrid.MouseMove += MainGrid_MouseMove;
    }

    private void MainGrid_MouseMove(object sender, MouseEventArgs e)
    {
      UpdateUIRepresentation();
    }

    public override void OnPlotterDetaching(PlotterBase plotter)
    {
      plotter.MainGrid.MouseMove -= MainGrid_MouseMove;
      base.OnPlotterDetaching(plotter);
    }

    protected override void Viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
    {
      base.Viewport_PropertyChanged(sender, e);
      UpdateUIRepresentation();
    }

    private void Source_VisiblePointsChanged(object sender, EventArgs e)
    {
      PointsGraphBase source = (PointsGraphBase)sender;
      if (source.VisiblePoints != null)
      {
        searcher = new SortedXSearcher1d(source.VisiblePoints);
      }
      UpdateUIRepresentation();
    }

    #endregion

    #region INotifyPropertyChanged Members

    /// <summary>
    ///   Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }

  /// <summary>
  ///   Represents a special data context, which encapsulates marker's position and custom data.
  ///   Used in <see cref="DataFollowChart"/>.
  /// </summary>
  public class FollowDataContext : INotifyPropertyChanged
  {
    private Point position;

    /// <summary>
    ///   Gets or sets the position of marker.
    /// </summary>
    /// <value>
    ///   The position.
    /// </value>
    public Point Position
    {
      get { return position; }
      set
      {
        position = value;
        PropertyChanged.Raise(this, "Position");
      }
    }

    private object data;

    /// <summary>
    ///   Gets or sets the additional custom data.
    /// </summary>
    /// <value>
    ///   The data.
    /// </value>
    public object Data
    {
      get { return data; }
      set
      {
        data = value;
        PropertyChanged.Raise(this, "Data");
      }
    }

    #region INotifyPropertyChanged Members

    /// <summary>
    ///   Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}