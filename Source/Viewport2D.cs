using Crystal.Plot2D.Common;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Data;

namespace Crystal.Plot2D
{
  /// <summary>
  ///   Viewport2D provides virtual coordinates.
  /// </summary>
  public class Viewport2D : DependencyObject
  {
    /// <summary>
    ///   This counter is a part of workaround for endless axis resize loop. 
    ///   It is reset on Viewport property assignment and on entire plotter resize.
    /// </summary>
    internal int UpdateIterationCount { get; set; }

    internal PlotterBase PlotterBase { get; }

    internal FrameworkElement HostElement { get; }

    protected internal Viewport2D(FrameworkElement _host, PlotterBase _plotter)
    {
      HostElement = _host;
      _host.ClipToBounds = true;
      _host.SizeChanged += OnHostElementSizeChanged;

      PlotterBase = _plotter;
      _plotter.Children.CollectionChanged += OnPlotterChildrenChanged;
      constraints = new ConstraintCollection(this);
      constraints.Add(new MinimalSizeConstraint());
      constraints.CollectionChanged += constraints_CollectionChanged;

      fitToViewConstraints = new ConstraintCollection(this);
      fitToViewConstraints.CollectionChanged += fitToViewConstraints_CollectionChanged;
      readonlyContentBoundsHosts = new ReadOnlyObservableCollection<DependencyObject>(contentBoundsHosts);
      UpdateVisible();
      UpdateTransform();
    }

    private void OnHostElementSizeChanged(object sender, SizeChangedEventArgs e)
    {
      SetValue(OutputPropertyKey, new Rect(e.NewSize));
      CoerceValue(VisibleProperty);
    }

    private void fitToViewConstraints_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (IsFittedToView)
      {
        CoerceValue(VisibleProperty);
      }
    }

    private void constraints_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      CoerceValue(VisibleProperty);
    }

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      Viewport2D viewport = (Viewport2D)d;
      // This counter is a part of workaround for endless axis resize loop
      // If internal update count exceeds threshold stop enoforcing restrictions
      if (e.Property == VisibleProperty)
      {
        if (viewport.UpdateIterationCount++ > 8)
        {
          viewport.EnforceRestrictions = false;
          Debug.WriteLine("Plotter: update cycle detected. Viewport constraints disabled.");
        }
      }
      viewport.UpdateTransform();
      viewport.RaisePropertyChangedEvent(e);
    }

    public BindingExpressionBase SetBinding(DependencyProperty property, BindingBase binding)
    {
      return BindingOperations.SetBinding(this, property, binding);
    }

    /// <summary>
    ///   Forces viewport to go to fit to view mode - clears locally set value of <see cref="Visible"/> property
    ///   and sets it during the coercion process to a value of united content bounds of all charts inside of <see cref="Plotter"/>.
    /// </summary>
    public void FitToView()
    {
      if (!IsFittedToView)
      {
        ClearValue(VisibleProperty);
        CoerceValue(VisibleProperty);
      }
    }

    /// <summary>
    ///   Gets a value indicating whether Viewport is fitted to view.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if Viewport is fitted to view; otherwise, <c>false</c>.
    /// </value>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsFittedToView => ReadLocalValue(VisibleProperty) == DependencyProperty.UnsetValue;

    internal void UpdateVisible()
    {
      //if (updateVisibleOperation == null)
      //{
      //    updateVisibleOperation = Dispatcher.BeginInvoke(() => UpdateVisible(), DispatcherPriority.Normal);
      //    return;
      //}

      //updateVisibleOperation = Dispatcher.BeginInvoke(() =>
      //{
      //    updateVisibleOperation = null;

      if (IsFittedToView)
      {
        CoerceValue(VisibleProperty);
      }
      //}, DispatcherPriority.Normal);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public PlotterBase Plotter => PlotterBase;

    private readonly ConstraintCollection constraints;

    /// <summary>
    ///   Gets the collection of <see cref="ViewportConstraint"/>s that are applied each time <see cref="Visible"/> is updated.
    /// </summary>
    /// <value>
    ///   The constraints.
    /// </value>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ConstraintCollection Constraints => constraints;

    private readonly ConstraintCollection fitToViewConstraints;

    private bool enforceConstraints = true;

    public bool EnforceRestrictions
    {
      get { return enforceConstraints; }
      set { enforceConstraints = value; }
    }

    /// <summary>
    ///   Gets the collection of <see cref="ViewportConstraint"/>s that are applied only when Viewport is fitted to view.
    /// </summary>
    /// <value>
    ///   The fit to view constraints.
    /// </value>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public ConstraintCollection FitToViewConstraints => fitToViewConstraints;

    #region Output property

    /// <summary>
    ///   Gets the rectangle in screen coordinates that is output.
    /// </summary>
    /// <value>
    ///   The output.
    /// </value>
    public Rect Output
    {
      get { return (Rect)GetValue(OutputProperty); }
    }

    [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    private static readonly DependencyPropertyKey OutputPropertyKey = DependencyProperty.RegisterReadOnly(
      "Output",
      typeof(Rect),
      typeof(Viewport2D),
      new FrameworkPropertyMetadata(new Rect(0, 0, 1, 1), OnPropertyChanged));

    /// <summary>
    ///   Identifies the <see cref="Output"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty OutputProperty = OutputPropertyKey.DependencyProperty;

    #endregion

    #region UnitedContentBounds property

    /// <summary>
    ///   Gets the united content bounds of all the charts.
    /// </summary>
    /// <value>
    ///   The content bounds.
    /// </value>
    public DataRect UnitedContentBounds
    {
      get { return (DataRect)GetValue(UnitedContentBoundsProperty); }
      internal set { SetValue(UnitedContentBoundsProperty, value); }
    }

    /// <summary>
    ///   Identifies the <see cref="UnitedContentBounds"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty UnitedContentBoundsProperty = DependencyProperty.Register(
      "UnitedContentBounds",
      typeof(DataRect),
      typeof(Viewport2D),
      new FrameworkPropertyMetadata(DataRect.Empty, OnUnitedContentBoundsChanged));

    private static void OnUnitedContentBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      Viewport2D owner = (Viewport2D)d;
      owner.ContentBoundsChanged.Raise(owner);
    }

    public event EventHandler ContentBoundsChanged;

    #endregion

    #region Visible property

    /// <summary>
    ///   Gets or sets the visible rectangle.
    /// </summary>
    /// <value>
    ///   The visible.
    /// </value>
    public DataRect Visible
    {
      get { return (DataRect)GetValue(VisibleProperty); }
      set
      {
        // This code is a part of workaround for endless axis resize loop
        UpdateIterationCount = 0;
        if (!EnforceRestrictions)
        {
          Debug.WriteLine("Plotter: enabling viewport constraints");
          EnforceRestrictions = true;
        }
        SetValue(VisibleProperty, value);
      }
    }

    /// <summary>
    ///   Identifies the <see cref="Visible"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register(
      "Visible",
      typeof(DataRect),
      typeof(Viewport2D),
      new FrameworkPropertyMetadata(new DataRect(0, 0, 1, 1), OnPropertyChanged, OnCoerceVisible),
      ValidateVisibleCallback);

    private static bool ValidateVisibleCallback(object value)
    {
      DataRect rect = (DataRect)value;
      return !rect.IsNaN();
    }

    private void UpdateContentBoundsHosts()
    {
      contentBoundsHosts.Clear();
      foreach (var item in PlotterBase.Children)
      {
        if (item is DependencyObject dependencyObject)
        {
          bool hasNonEmptyBounds = !GetContentBounds(dependencyObject).IsEmpty;
          if (hasNonEmptyBounds && GetIsContentBoundsHost(dependencyObject))
          {
            contentBoundsHosts.Add(dependencyObject);
          }
        }
      }

      UpdateVisible();
    }

    private readonly ObservableCollection<DependencyObject> contentBoundsHosts = new();
    private readonly ReadOnlyObservableCollection<DependencyObject> readonlyContentBoundsHosts;

    /// <summary>
    ///   Gets the collection of all charts that can has its own content bounds.
    /// </summary>
    /// <value>
    ///   The content bounds hosts.
    /// </value>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ReadOnlyObservableCollection<DependencyObject> ContentBoundsHosts
    {
      get { return readonlyContentBoundsHosts; }
    }

    private bool useApproximateContentBoundsComparison = true;

    /// <summary>
    ///   Gets or sets a value indicating whether to use approximate content bounds comparison.
    ///   This this property to true can increase performance, as Visible will change less often.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if approximate content bounds comparison is used; otherwise, <c>false</c>.
    /// </value>
    public bool UseApproximateContentBoundsComparison
    {
      get { return useApproximateContentBoundsComparison; }
      set { useApproximateContentBoundsComparison = value; }
    }

    private double maxContentBoundsComparisonMistake = 0.02;
    public double MaxContentBoundsComparisonMistake
    {
      get { return maxContentBoundsComparisonMistake; }
      set { maxContentBoundsComparisonMistake = value; }
    }

    private DataRect prevContentBounds = DataRect.Empty;
    protected virtual DataRect CoerceVisible(DataRect newVisible)
    {
      if (Plotter == null)
      {
        return newVisible;
      }

      bool isDefaultValue = newVisible == (DataRect)VisibleProperty.DefaultMetadata.DefaultValue;
      if (isDefaultValue)
      {
        newVisible = DataRect.Empty;
      }

      if (isDefaultValue && IsFittedToView)
      {
        // determining content bounds
        DataRect bounds = DataRect.Empty;

        foreach (var item in contentBoundsHosts)
        {
          if (item is not IPlotterElement plotterElement)
          {
            continue;
          }

          if (plotterElement.Plotter == null)
          {
            continue;
          }

          var plotter = (PlotterBase)plotterElement.Plotter;
          var visual = plotter.VisualBindings[plotterElement];
          if (visual.Visibility == Visibility.Visible)
          {
            DataRect contentBounds = GetContentBounds(item);
            if (contentBounds.Width.IsNaN() || contentBounds.Height.IsNaN())
            {
              continue;
            }

            bounds.UnionFinite(contentBounds);
          }
        }

        if (useApproximateContentBoundsComparison)
        {
          var intersection = prevContentBounds;
          intersection.Intersect(bounds);

          double currSquare = bounds.GetSquare();
          double prevSquare = prevContentBounds.GetSquare();
          double intersectionSquare = intersection.GetSquare();
          double squareTopLimit = 1 + maxContentBoundsComparisonMistake;
          double squareBottomLimit = 1 - maxContentBoundsComparisonMistake;

          if (intersectionSquare != 0)
          {
            double currRatio = currSquare / intersectionSquare;
            double prevRatio = prevSquare / intersectionSquare;

            if (squareBottomLimit < currRatio && currRatio < squareTopLimit && squareBottomLimit < prevRatio && prevRatio < squareTopLimit)
            {
              bounds = prevContentBounds;
            }
          }
        }

        prevContentBounds = bounds;
        UnitedContentBounds = bounds;

        // applying fit-to-view constraints
        bounds = fitToViewConstraints.Apply(Visible, bounds, this);

        // enlarging
        if (!bounds.IsEmpty)
        {
          bounds = CoordinateUtilities.RectZoom(bounds, bounds.GetCenter(), clipToBoundsEnlargeFactor);
        }
        else
        {
          bounds = (DataRect)VisibleProperty.DefaultMetadata.DefaultValue;
        }
        newVisible.Union(bounds);
      }

      if (newVisible.IsEmpty)
      {
        newVisible = (DataRect)VisibleProperty.DefaultMetadata.DefaultValue;
      }
      else if (newVisible.Width == 0 || newVisible.Height == 0)
      {
        DataRect defRect = (DataRect)VisibleProperty.DefaultMetadata.DefaultValue;
        Size size = newVisible.Size;
        Point location = newVisible.Location;

        if (newVisible.Width == 0)
        {
          size.Width = defRect.Width;
          location.X -= size.Width / 2;
        }
        if (newVisible.Height == 0)
        {
          size.Height = defRect.Height;
          location.Y -= size.Height / 2;
        }

        newVisible = new DataRect(location, size);
      }

      // apply domain constraint
      newVisible = domainConstraint.Apply(Visible, newVisible, this);

      // apply other restrictions
      if (enforceConstraints)
      {
        newVisible = constraints.Apply(Visible, newVisible, this);
      }

      // applying transform's data domain constraint
      if (!transform.DataTransform.DataDomain.IsEmpty)
      {
        var newDataRect = newVisible.ViewportToData(transform);
        newDataRect = DataRect.Intersect(newDataRect, transform.DataTransform.DataDomain);
        newVisible = newDataRect.DataToViewport(transform);
      }

      if (newVisible.IsEmpty)
      {
        newVisible = new Rect(0, 0, 1, 1);
      }

      return newVisible;
    }

    private static object OnCoerceVisible(DependencyObject d, object newValue)
    {
      Viewport2D viewport = (Viewport2D)d;
      DataRect newRect = viewport.CoerceVisible((DataRect)newValue);
      if (newRect.Width == 0 || newRect.Height == 0)
      {
        // doesn't apply rects with zero square
        return DependencyProperty.UnsetValue;
      }
      else
      {
        return newRect;
      }
    }

    #endregion

    #region Domain

    private readonly DomainConstraint domainConstraint = new() { Domain = Rect.Empty };

    /// <summary>
    ///   Gets or sets the domain - rectangle in viewport coordinates that limits maximal size of <see cref="Visible"/> rectangle.
    /// </summary>
    /// <value>
    ///   The domain.
    /// </value>
    public DataRect Domain
    {
      get { return (DataRect)GetValue(DomainProperty); }
      set { SetValue(DomainProperty, value); }
    }

    public static readonly DependencyProperty DomainProperty = DependencyProperty.Register(
      "Domain",
      typeof(DataRect),
      typeof(Viewport2D),
      new FrameworkPropertyMetadata(DataRect.Empty, OnDomainReplaced));

    private static void OnDomainReplaced(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      Viewport2D owner = (Viewport2D)d;
      owner.OnDomainChanged();
    }

    private void OnDomainChanged()
    {
      domainConstraint.Domain = Domain;
      DomainChanged.Raise(this);
      CoerceValue(VisibleProperty);
    }

    /// <summary>
    ///   Occurs when <see cref="Domain"/> property changes.
    /// </summary>
    public event EventHandler DomainChanged;

    #endregion

    private double clipToBoundsEnlargeFactor = 1.10;

    /// <summary>
    ///   Gets or sets the viewport enlarge factor.
    /// </summary>
    /// <remarks>
    ///   Default value is 1.10.
    /// </remarks>
    /// <value>The clip to bounds factor.</value>
    public double ClipToBoundsEnlargeFactor
    {
      get { return clipToBoundsEnlargeFactor; }
      set
      {
        if (clipToBoundsEnlargeFactor != value)
        {
          clipToBoundsEnlargeFactor = value;
          UpdateVisible();
        }
      }
    }

    private void UpdateTransform()
    {
      transform = transform.WithRects(Visible, Output);
    }

    private CoordinateTransform transform = CoordinateTransform.CreateDefault();

    /// <summary>
    ///   Gets or sets the coordinate transform of Viewport.
    /// </summary>
    /// <value>
    ///   The transform.
    /// </value>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [NotNull]
    public virtual CoordinateTransform Transform
    {
      get { return transform; }
      set
      {
        value.VerifyNotNull();
        if (value != transform)
        {
          var oldTransform = transform;
          transform = value;
          RaisePropertyChangedEvent(nameof(Transform), oldTransform, transform);
        }
      }
    }

    /// <summary>
    ///   Occurs when viewport property changes.
    /// </summary>
    public event EventHandler<ExtendedPropertyChangedEventArgs> PropertyChanged;

    private void RaisePropertyChangedEvent(string propertyName, object oldValue, object newValue)
    {
      if (PropertyChanged != null)
      {
        RaisePropertyChanged(new ExtendedPropertyChangedEventArgs { PropertyName = propertyName, OldValue = oldValue, NewValue = newValue });
      }
    }

    private void RaisePropertyChangedEvent(string propertyName)
    {
      if (PropertyChanged != null)
      {
        RaisePropertyChanged(new ExtendedPropertyChangedEventArgs { PropertyName = propertyName });
      }
    }

    private void RaisePropertyChangedEvent(DependencyPropertyChangedEventArgs e)
    {
      if (PropertyChanged != null)
      {
        RaisePropertyChanged(ExtendedPropertyChangedEventArgs.FromDependencyPropertyChanged(e));
      }
    }

    //private DispatcherOperation pendingRaisePropertyChangedOperation;
    //private bool inRaisePropertyChanged = false;
    protected virtual void RaisePropertyChanged(ExtendedPropertyChangedEventArgs args)
    {
      //if (inRaisePropertyChanged)
      //{
      //    if (pendingRaisePropertyChangedOperation != null)
      //        pendingRaisePropertyChangedOperation.Abort();
      //    pendingRaisePropertyChangedOperation = Dispatcher.BeginInvoke(() => RaisePropertyChanged(args), DispatcherPriority.Normal);
      //    return;
      //}

      //pendingRaisePropertyChangedOperation = null;
      //inRaisePropertyChanged = true;

      PropertyChanged.Raise(this, args);

      //inRaisePropertyChanged = false;
    }

    private void OnPlotterChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      UpdateContentBoundsHosts();
    }

    #region Panning state

    private Viewport2DPanningState panningState = Viewport2DPanningState.NotPanning;
    public Viewport2DPanningState PanningState
    {
      get { return panningState; }
      set
      {
        var prevState = panningState;
        panningState = value;
        OnPanningStateChanged(prevState, panningState);
      }
    }

    private void OnPanningStateChanged(Viewport2DPanningState prevState, Viewport2DPanningState currState)
    {
      PanningStateChanged.Raise(this, prevState, currState);
      if (currState == Viewport2DPanningState.Panning)
      {
        BeginPanning.Raise(this);
      }
      else if (currState == Viewport2DPanningState.NotPanning)
      {
        EndPanning.Raise(this);
      }
    }

    internal event EventHandler<ValueChangedEventArgs<Viewport2DPanningState>> PanningStateChanged;

    public event EventHandler BeginPanning;
    public event EventHandler EndPanning;

    #endregion // end of Panning state


    #region Attached Properties

    #region IsContentBoundsHost attached property

    public static bool GetIsContentBoundsHost(DependencyObject obj) => (bool)obj.GetValue(IsContentBoundsHostProperty);

    public static void SetIsContentBoundsHost(DependencyObject obj, bool value) => obj.SetValue(IsContentBoundsHostProperty, value);

    public static readonly DependencyProperty IsContentBoundsHostProperty = DependencyProperty.RegisterAttached(
      "IsContentBoundsHost",
      typeof(bool),
      typeof(Viewport2D),
      new FrameworkPropertyMetadata(true, OnIsContentBoundsChanged));

    private static void OnIsContentBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is IPlotterElement plotterElement && plotterElement.Plotter != null)
      {
        PlotterBase plotter2d = plotterElement.Plotter;
        plotter2d.Viewport.UpdateContentBoundsHosts();
      }
    }

    #endregion

    #region ContentBounds attached property

    public static DataRect GetContentBounds(DependencyObject obj) => (DataRect)obj.GetValue(ContentBoundsProperty);

    public static void SetContentBounds(DependencyObject obj, DataRect value) => obj.SetValue(ContentBoundsProperty, value);

    public static readonly DependencyProperty ContentBoundsProperty = DependencyProperty.RegisterAttached(
      "ContentBounds",
      typeof(DataRect),
      typeof(Viewport2D),
      new FrameworkPropertyMetadata(DataRect.Empty, OnContentBoundsChanged, CoerceContentBounds));

    private static object CoerceContentBounds(DependencyObject d, object value)
    {
      DataRect prevBounds = GetContentBounds(d);
      DataRect currBounds = (DataRect)value;
      bool approximateComparanceAllowed = GetUsesApproximateContentBoundsComparison(d);
      bool areClose = approximateComparanceAllowed && currBounds.IsCloseTo(prevBounds, 0.005);
      return areClose ? DependencyProperty.UnsetValue : value;
    }

    private static void OnContentBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is IPlotterElement element)
      {
        if (element is FrameworkElement frElement)
        {
          frElement.RaiseEvent(new RoutedEventArgs(ContentBoundsChangedEvent));
        }

        if (element.Plotter is PlotterBase plotter2d)
        {
          plotter2d.Viewport.UpdateContentBoundsHosts();
        }
      }
    }

    public static readonly RoutedEvent ContentBoundsChangedEvent = EventManager.RegisterRoutedEvent(
      "ContentBoundsChanged",
      RoutingStrategy.Direct,
      typeof(RoutedEventHandler),
      typeof(Viewport2D));

    #endregion

    #region UsesApproximateContentBoundsComparison

    /// <summary>
    ///   Gets a value indicating whether approximate content bounds comparison will be used while deciding whether to updating Viewport2D.ContentBounds
    ///   attached dependency property or not.
    ///   Approximate content bounds comparison can make Viewport's Visible to changed less frequent, but it can lead to
    ///   some bugs if content bounds are large but visible area is not compared to them.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if approximate content bounds comparison is used while deciding whether to set new value of content bounds, or not; otherwise, <c>false</c>.
    /// </value>
    public static bool GetUsesApproximateContentBoundsComparison(DependencyObject obj)
    {
      return (bool)obj.GetValue(UsesApproximateContentBoundsComparisonProperty);
    }

    /// <summary>
    /// Sets a value indicating whether approximate content bounds comparison will be used while deciding whether to updating Viewport2D.ContentBounds
    /// attached dependency property or not.
    /// Approximate content bounds comparison can make Viewport's Visible to changed less frequent, but it can lead to
    /// some bugs if content bounds are large but visible area is not compared to them.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if approximate content bounds comparison is used while deciding whether to set new value of content bounds, or not; otherwise, <c>false</c>.
    /// </value>		
    public static void SetUsesApproximateContentBoundsComparison(DependencyObject obj, bool value)
    {
      obj.SetValue(UsesApproximateContentBoundsComparisonProperty, value);
    }

    public static readonly DependencyProperty UsesApproximateContentBoundsComparisonProperty = DependencyProperty.RegisterAttached(
      "UsesApproximateContentBoundsComparison",
      typeof(bool),
      typeof(Viewport2D),
      new FrameworkPropertyMetadata(true, OnUsesApproximateContentBoundsComparisonChanged));

    private static void OnUsesApproximateContentBoundsComparisonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is IPlotterElement element)
      {
        if (element.Plotter is PlotterBase plotter2d)
        {
          plotter2d.Viewport.UpdateVisible();
        }
      }
    }

    #endregion // end of UsesApproximateContentBoundsComparison

    #region UseDeferredPanning attached property

    public static bool GetUseDeferredPanning(DependencyObject obj)
    {
      return (bool)obj.GetValue(UseDeferredPanningProperty);
    }

    public static void SetUseDeferredPanning(DependencyObject obj, bool value)
    {
      obj.SetValue(UseDeferredPanningProperty, value);
    }

    public static readonly DependencyProperty UseDeferredPanningProperty = DependencyProperty.RegisterAttached(
      "UseDeferredPanning",
      typeof(bool),
      typeof(Viewport2D),
      new FrameworkPropertyMetadata(false));

    #endregion // end of UseDeferredPanning attached property

    #endregion Attached Properties
  }
}