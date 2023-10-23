using Crystal.Plot2D.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace Crystal.Plot2D;

/// <summary>
/// Plotter is a base control for displaying various graphs. 
/// It provides means to draw chart itself and side space for axes, annotations, etc.
/// </summary>
[ContentProperty(name: "Children")]
[TemplatePart(Name = "PART_HeaderPanel", Type = typeof(StackPanel))]
[TemplatePart(Name = "PART_FooterPanel", Type = typeof(StackPanel))]
[TemplatePart(Name = "PART_BottomPanel", Type = typeof(StackPanel))]
[TemplatePart(Name = "PART_LeftPanel", Type = typeof(StackPanel))]
[TemplatePart(Name = "PART_RightPanel", Type = typeof(StackPanel))]
[TemplatePart(Name = "PART_TopPanel", Type = typeof(StackPanel))]
[TemplatePart(Name = "PART_MainCanvas", Type = typeof(Canvas))]
[TemplatePart(Name = "PART_CentralGrid", Type = typeof(Grid))]
[TemplatePart(Name = "PART_MainGrid", Type = typeof(Grid))]
[TemplatePart(Name = "PART_ContentsGrid", Type = typeof(Grid))]
[TemplatePart(Name = "PART_ParallelCanvas", Type = typeof(Canvas))]
public abstract class PlotterBase : ContentControl
{
  protected PlotterLoadMode LoadMode { get; }

  #region Constructors

  protected PlotterBase() : this(loadMode: PlotterLoadMode.Normal)
  {
    Children.CollectionChanged += (s, e) => viewportInstance.UpdateIterationCount = 0;
    InitViewport();
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="PlotterBase"/> class with a <see cref="PlotterLoadMode"/>.
  /// </summary>
  /// <param name="loadMode"></param>
  protected PlotterBase(PlotterLoadMode loadMode)
  {
    LoadMode = loadMode;
    SetPlotter(obj: this, value: this);
    if (loadMode == PlotterLoadMode.Normal)
    {
      UpdateUIParts();
    }
    Children = new PlotterChildrenCollection(plotter: this);
    Children.CollectionChanged += OnChildrenCollectionChanged;
    Loaded += Plotter_Loaded;
    Unloaded += Plotter_Unloaded;
    if (LoadMode != PlotterLoadMode.Empty)
    {
      InitViewport();
    }
    var uri = new Uri(uriString: Constants.ThemeUri, uriKind: UriKind.Relative);
    GenericResources = (ResourceDictionary)Application.LoadComponent(resourceLocator: uri);
    ContextMenu = null;
  }

  #endregion Constructors

  #region Loading

  private void Plotter_Loaded(object sender, RoutedEventArgs e)
  {
    ExecuteWaitingChildrenAdditions();
    OnLoaded();
  }

  protected virtual void OnLoaded() => Focus();

  #endregion Loading

  #region Unloading

  void Plotter_Unloaded(object sender, RoutedEventArgs e) => OnUnloaded();

  protected virtual void OnUnloaded() { }

  #endregion Unloading

  protected override AutomationPeer OnCreateAutomationPeer() => new PlotterAutomationPeer(owner: this);

  [EditorBrowsable(state: EditorBrowsableState.Never)]
  public override bool ShouldSerializeContent() => false;

  /// <summary>
  /// Do not serialize context menu if it was created by DefaultContextMenu, 
  /// because that context menu items contains references of plotter.
  /// </summary>
  /// <param name="dp"></param>
  /// <returns></returns>
  protected override bool ShouldSerializeProperty(DependencyProperty dp)
  {
    if (dp == ContextMenuProperty && Children.Any(predicate: element => element is DefaultContextMenu))
    {
      return false;
    }
    if (dp == TemplateProperty || dp == ContentProperty)
    {
      return false;
    }
    return base.ShouldSerializeProperty(dp: dp);
  }

  private const string TemplateKey = "defaultPlotterTemplate";
  private const string StyleKey = "defaultPlotterStyle";
  private void UpdateUIParts()
  {
    var dict = new ResourceDictionary { Source = new Uri(uriString: "/Crystal.Plot2D;component/Common/PlotterStyle.xaml", uriKind: UriKind.Relative) };
    Resources.MergedDictionaries.Add(item: dict);
    Style = (Style)dict[key: StyleKey];
    var template = (ControlTemplate)dict[key: TemplateKey];
    Template = template;
    ApplyTemplate();
  }

  protected ResourceDictionary GenericResources { get; }

  /// <summary>
  /// Forces plotter to load.
  /// </summary>
  public void PerformLoad()
  {
    _isLoadedIntensionally = true;
    ApplyTemplate();
    Plotter_Loaded(sender: null, e: null);
  }

  private bool _isLoadedIntensionally;
  protected virtual bool IsLoadedInternal => _isLoadedIntensionally || IsLoaded;

  protected internal void ExecuteWaitingChildrenAdditions()
  {
    foreach (var action in _waitingForExecute)
    {
      action();
    }
    _waitingForExecute.Clear();
  }

  private Grid _contentsGrid;
  public override void OnApplyTemplate()
  {
    base.OnApplyTemplate();
    _addedVisualElements.Clear();
    foreach (var item in GetAllPanels())
    {
      if (item is INotifyingPanel panel)
      {
        panel.ChildrenCreated -= NotifyingItem_ChildrenCreated;
        if (panel.NotifyingChildren != null)
        {
          panel.NotifyingChildren.CollectionChanged -= OnVisualCollectionChanged;
        }
      }
    }

    var headerPanel = GetPart<StackPanel>(name: "PART_HeaderPanel");
    MigrateChildren(previousParent: HeaderPanel, currentParent: headerPanel);
    HeaderPanel = headerPanel;

    var footerPanel = GetPart<StackPanel>(name: "PART_FooterPanel");
    MigrateChildren(previousParent: FooterPanel, currentParent: footerPanel);
    FooterPanel = footerPanel;

    var leftPanel = GetPart<StackPanel>(name: "PART_LeftPanel");
    MigrateChildren(previousParent: LeftPanel, currentParent: leftPanel);
    LeftPanel = leftPanel;

    var bottomPanel = GetPart<StackPanel>(name: "PART_BottomPanel");
    MigrateChildren(previousParent: BottomPanel, currentParent: bottomPanel);
    BottomPanel = bottomPanel;

    var rightPanel = GetPart<StackPanel>(name: "PART_RightPanel");
    MigrateChildren(previousParent: RightPanel, currentParent: rightPanel);
    RightPanel = rightPanel;

    var topPanel = GetPart<StackPanel>(name: "PART_TopPanel");
    MigrateChildren(previousParent: TopPanel, currentParent: topPanel);
    TopPanel = topPanel;

    var mainCanvas = GetPart<Canvas>(name: "PART_MainCanvas");
    MigrateChildren(previousParent: MainCanvas, currentParent: mainCanvas);
    MainCanvas = mainCanvas;

    var centralGrid = GetPart<Grid>(name: "PART_CentralGrid");
    MigrateChildren(previousParent: CentralGrid, currentParent: centralGrid);
    CentralGrid = centralGrid;

    var mainGrid = GetPart<Grid>(name: "PART_MainGrid");
    MigrateChildren(previousParent: MainGrid, currentParent: mainGrid);
    MainGrid = mainGrid;

    var parallelCanvas = GetPart<Canvas>(name: "PART_ParallelCanvas");
    MigrateChildren(previousParent: ParallelCanvas, currentParent: parallelCanvas);
    ParallelCanvas = parallelCanvas;

    var _contentsGrid = GetPart<Grid>(name: "PART_ContentsGrid");
    MigrateChildren(previousParent: _contentsGrid, currentParent: _contentsGrid);
    this._contentsGrid = _contentsGrid;

    Content = _contentsGrid;
    AddLogicalChild(child: _contentsGrid);

    foreach (var notifyingItem in GetAllPanels())
    {
      if (notifyingItem is INotifyingPanel panel)
      {
        if (panel.NotifyingChildren == null)
        {
          panel.ChildrenCreated += NotifyingItem_ChildrenCreated;
        }
        else
        {
          panel.NotifyingChildren.CollectionChanged += OnVisualCollectionChanged;
        }
      }
    }
  }

  private static void MigrateChildren(Panel previousParent, Panel currentParent)
  {
    if (previousParent != null && currentParent != null)
    {
      UIElement[] children = new UIElement[previousParent.Children.Count];
      previousParent.Children.CopyTo(array: children, index: 0);
      previousParent.Children.Clear();

      foreach (var child in children)
      {
        if (!currentParent.Children.Contains(element: child))
        {
          currentParent.Children.Add(element: child);
        }
      }
    }
    else if (previousParent != null)
    {
      previousParent.Children.Clear();
    }
  }

  private void NotifyingItem_ChildrenCreated(object sender, EventArgs e)
  {
    INotifyingPanel panel = (INotifyingPanel)sender;
    SubscribePanelEvents(panel: panel);
  }

  private void SubscribePanelEvents(INotifyingPanel panel)
  {
    panel.ChildrenCreated -= NotifyingItem_ChildrenCreated;
    panel.NotifyingChildren.CollectionChanged -= OnVisualCollectionChanged;
    panel.NotifyingChildren.CollectionChanged += OnVisualCollectionChanged;
  }

  private void OnVisualCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
  {
    if (e.NewItems != null)
    {
      foreach (var item in e.NewItems)
      {
        if (item is INotifyingPanel notifyingPanel)
        {
          if (notifyingPanel.NotifyingChildren != null)
          {
            notifyingPanel.NotifyingChildren.CollectionChanged -= OnVisualCollectionChanged;
            notifyingPanel.NotifyingChildren.CollectionChanged += OnVisualCollectionChanged;
          }
          else
          {
            notifyingPanel.ChildrenCreated += NotifyingItem_ChildrenCreated;
          }
        }
        OnVisualChildAdded(target: (UIElement)item, uIElementCollection: (UIElementCollection)sender);
      }
    }
    if (e.OldItems != null)
    {
      foreach (var item in e.OldItems)
      {
        if (item is INotifyingPanel notifyingPanel)
        {
          notifyingPanel.ChildrenCreated -= NotifyingItem_ChildrenCreated;
          if (notifyingPanel.NotifyingChildren != null)
          {
            notifyingPanel.NotifyingChildren.CollectionChanged -= OnVisualCollectionChanged;
          }
        }
        OnVisualChildRemoved(target: (UIElement)item, uiElementCollection: (UIElementCollection)sender);
      }
    }
  }
  public VisualBindingCollection VisualBindings { get; } = new();

  protected virtual void OnVisualChildAdded(UIElement target, UIElementCollection uIElementCollection)
  {
    if (_addingElements.Count > 0)
    {
      var element = _addingElements.Peek();

      var dict = VisualBindings.Cache;
      var proxy = dict[key: element];

      List<UIElement> visualElements;
      if (!_addedVisualElements.ContainsKey(key: element))
      {
        visualElements = new List<UIElement>();
        _addedVisualElements.Add(key: element, value: visualElements);
      }
      else
      {
        visualElements = _addedVisualElements[key: element];
      }

      visualElements.Add(item: target);

      SetBindings(proxy: proxy, target: target);
    }
  }

  private static void SetBindings(UIElement proxy, UIElement target)
  {
    if (proxy != target)
    {
      foreach (var property in GetPropertiesToSetBindingOn())
      {
        BindingOperations.SetBinding(target: target, dp: property, binding: new Binding { Path = new PropertyPath(path: property.Name), Source = proxy, Mode = BindingMode.TwoWay });
      }
    }
  }

  private static void RemoveBindings(UIElement proxy, UIElement target)
  {
    if (proxy != target)
    {
      foreach (var property in GetPropertiesToSetBindingOn())
      {
        BindingOperations.ClearBinding(target: target, dp: property);
      }
    }
  }

  private static IEnumerable<DependencyProperty> GetPropertiesToSetBindingOn()
  {
    yield return OpacityProperty;
    yield return VisibilityProperty;
    yield return IsHitTestVisibleProperty;
    //yield return FrameworkElement.DataContextProperty;
  }

  protected virtual void OnVisualChildRemoved(UIElement target, UIElementCollection uiElementCollection)
  {
	    if (_removingElements.Count > 0)
    {
      var element = _removingElements.Peek();

      var dict = VisualBindings.Cache;
      var proxy = dict[key: element];

      if (_addedVisualElements.ContainsKey(key: element))
      {
        var list = _addedVisualElements[key: element];
        list.Remove(item: target);

        if (list.Count == 0)
        {
          dict.Remove(key: element);
        }

        _addedVisualElements.Remove(key: element);
      }

      RemoveBindings(proxy: proxy, target: target);
    }
  }

  internal virtual IEnumerable<Panel> GetAllPanels()
  {
    yield return HeaderPanel;
    yield return FooterPanel;

    yield return LeftPanel;
    yield return BottomPanel;
    yield return RightPanel;
    yield return TopPanel;

    yield return MainCanvas;
    yield return CentralGrid;
    yield return MainGrid;
    yield return ParallelCanvas;
    yield return _contentsGrid;
  }

  private T GetPart<T>(string name)
  {
    return (T)Template.FindName(name: name, templatedParent: this);
  }

  /// <summary>
  /// Provides access to Plotter's children charts.
  /// </summary>
  /// <value>The children.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Content)]
  public PlotterChildrenCollection Children { [DebuggerStepThrough] get; }

  private readonly List<Action> _waitingForExecute = new();

  bool _executedWaitingChildrenAdding;
  private void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
  {
    if (IsLoadedInternal && !_executedWaitingChildrenAdding)
    {
      _executedWaitingChildrenAdding = true;
      ExecuteWaitingChildrenAdditions();
    }

    if (e.NewItems != null)
    {
      foreach (IPlotterElement item in e.NewItems)
      {
        if (IsLoadedInternal)
        {
          OnChildAdded(child: item);
        }
        else
        {
          _waitingForExecute.Add(item: () => OnChildAdded(child: item));
        }
      }
    }
    if (e.OldItems != null)
    {
      foreach (IPlotterElement item in e.OldItems)
      {
        if (IsLoadedInternal)
        {
          OnChildRemoving(child: item);
        }
        else
        {
          _waitingForExecute.Add(item: () => OnChildRemoving(child: item));
        }
      }
    }
  }

  private readonly Stack<IPlotterElement> _addingElements = new();
  internal bool PerformChildChecks { get; set; } = true;
  protected IPlotterElement CurrentChild { get; private set; }

  protected virtual void OnChildAdded(IPlotterElement child)
  {
    if (child != null)
    {
      _addingElements.Push(item: child);
      CurrentChild = child;
      try
      {
        UIElement visualProxy = CreateVisualProxy(child: child);
        VisualBindings.Cache.Add(key: child, value: visualProxy);

        if (PerformChildChecks && child.Plotter != null)
        {
          throw new InvalidOperationException(message: Strings.Exceptions.PlotterElementAddedToAnotherPlotter);
        }

        if (child is FrameworkElement styleableElement)
        {
          Type key = styleableElement.GetType();
          if (GenericResources.Contains(key: key))
          {
            Style elementStyle = (Style)GenericResources[key: key];
            styleableElement.Style = elementStyle;
          }
        }

        if (PerformChildChecks)
        {
          child.OnPlotterAttached(plotter: this);
          if (child.Plotter != this)
          {
            throw new InvalidOperationException(message: Strings.Exceptions.InvalidParentPlotterValue);
          }
        }

        if (child is DependencyObject dependencyObject)
        {
          SetPlotter(obj: dependencyObject, value: this);
        }
      }
      finally
      {
        _addingElements.Pop();
        CurrentChild = null;
      }
    }
  }

  private UIElement CreateVisualProxy(IPlotterElement child)
  {
    if (VisualBindings.Cache.ContainsKey(key: child))
    {
      throw new InvalidOperationException(message: Strings.Exceptions.VisualBindingsWrongState);
    }


    if (child is not UIElement result)
    {
      result = new UIElement();
    }

    return result;
  }

  private readonly Stack<IPlotterElement> _removingElements = new();
  protected virtual void OnChildRemoving(IPlotterElement child)
  {
    if (child != null)
    {
      CurrentChild = child;
      _removingElements.Push(item: child);
      try
      {
        // todo probably here child.Plotter can be null.
        if (PerformChildChecks && child.Plotter != this && child.Plotter != null)
        {
          throw new InvalidOperationException(message: Strings.Exceptions.InvalidParentPlotterValueRemoving);
        }

        if (PerformChildChecks)
        {
          if (child.Plotter != null)
          {
            child.OnPlotterDetaching(plotter: this);
          }

          if (child.Plotter != null)
          {
            throw new InvalidOperationException(message: Strings.Exceptions.ParentPlotterNotNull);
          }
        }

        if (child is DependencyObject dependencyObject)
        {
          SetPlotter(obj: dependencyObject, value: null);
        }

        VisualBindings.Cache.Remove(key: child);

        if (_addedVisualElements.ContainsKey(key: child) && _addedVisualElements[key: child].Count > 0)
        {
          throw new InvalidOperationException(message: string.Format(format: Strings.Exceptions.PlotterElementDidnotCleanedAfterItself, arg0: child.ToString()));
        }
      }
      finally
      {
        CurrentChild = null;
        _removingElements.Pop();
      }
    }
  }

  private readonly Dictionary<IPlotterElement, List<UIElement>> _addedVisualElements = new();

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel ParallelCanvas { get; protected set; }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel HeaderPanel { get; protected set; }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel FooterPanel { get; protected set; }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel LeftPanel { get; protected set; }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel RightPanel { get; protected set; }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel TopPanel { get; protected set; }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel BottomPanel { get; protected set; }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel MainCanvas { get; protected set; }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel CentralGrid { get; protected set; }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel MainGrid { get; protected set; }

  #region Screenshots & copy to clipboard

  public BitmapSource CreateScreenshot()
  {
    UIElement parent = (UIElement)Parent;

    Rect renderBounds = new(size: RenderSize);

    Point p1 = renderBounds.TopLeft;
    Point p2 = renderBounds.BottomRight;

    if (parent != null)
    {
      //p1 = TranslatePoint(p1, parent);
      //p2 = TranslatePoint(p2, parent);
    }

    Int32Rect rect = new Rect(point1: p1, point2: p2).ToInt32Rect();

    return ScreenshotHelper.CreateScreenshot(uiElement: this, screenshotSource: rect);
  }


  /// <summary>Saves screenshot to file.</summary>
  /// <param name="filePath">File path.</param>
  public void SaveScreenshot(string filePath)
  {
    ScreenshotHelper.SaveBitmapToFile(bitmap: CreateScreenshot(), filePath: filePath);
  }

  /// <summary>
  /// Saves screenshot to stream.
  /// </summary>
  /// <param name="stream">The stream.</param>
  /// <param name="fileExtension">The file type extension.</param>
  public void SaveScreenshotToStream(Stream stream, string fileExtension)
  {
    ScreenshotHelper.SaveBitmapToStream(bitmap: CreateScreenshot(), stream: stream, fileExtension: fileExtension);
  }

  /// <summary>Copies the screenshot to clipboard.</summary>
  public void CopyScreenshotToClipboard()
  {
    Clipboard.Clear();
    Clipboard.SetImage(image: CreateScreenshot());
  }

  #endregion

  #region IsDefaultElement attached property

  protected void SetAllChildrenAsDefault()
  {
    foreach (var child in Children.OfType<DependencyObject>())
    {
      child.SetValue(dp: IsDefaultElementProperty, value: true);
    }
  }

  /// <summary>Gets a value whether specified graphics object is default to this plotter or not</summary>
  /// <param name="obj">Graphics object to check</param>
  /// <returns>True if it is default or false otherwise</returns>
  public static bool GetIsDefaultElement(DependencyObject obj)
  {
    return (bool)obj.GetValue(dp: IsDefaultElementProperty);
  }

  public static void SetIsDefaultElement(DependencyObject obj, bool value)
  {
    obj.SetValue(dp: IsDefaultElementProperty, value: value);
  }

  public static readonly DependencyProperty IsDefaultElementProperty = DependencyProperty.RegisterAttached(
    name: "IsDefaultElement",
    propertyType: typeof(bool),
    ownerType: typeof(PlotterBase),
    defaultMetadata: new UIPropertyMetadata(defaultValue: false));

  /// <summary>Removes all user graphs from given UIElementCollection, 
  /// leaving only default graphs</summary>
  protected static void RemoveUserElements(IList<IPlotterElement> elements)
  {
    int index = 0;

    while (index < elements.Count)
    {
      if (elements[index: index] is DependencyObject d && !GetIsDefaultElement(obj: d))
      {
        elements.RemoveAt(index: index);
      }
      else
      {
        index++;
      }
    }
  }

  public void RemoveUserElements()
  {
    RemoveUserElements(elements: Children);
  }

  #endregion

  #region IsDefaultAxis

  public static bool GetIsDefaultAxis(DependencyObject obj)
  {
    return (bool)obj.GetValue(dp: IsDefaultAxisProperty);
  }

  public static void SetIsDefaultAxis(DependencyObject obj, bool value)
  {
    obj.SetValue(dp: IsDefaultAxisProperty, value: value);
  }

  public static readonly DependencyProperty IsDefaultAxisProperty = DependencyProperty.RegisterAttached(
    name: "IsDefaultAxis",
    propertyType: typeof(bool),
    ownerType: typeof(PlotterBase),
    defaultMetadata: new UIPropertyMetadata(defaultValue: false, propertyChangedCallback: OnIsDefaultAxisChanged));

  private static void OnIsDefaultAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is IPlotterElement plotterElement)
    {
      var parentPlotter = plotterElement.Plotter;
      parentPlotter?.OnIsDefaultAxisChangedCore(d: d, e: e);
    }
  }

  protected virtual void OnIsDefaultAxisChangedCore(DependencyObject d, DependencyPropertyChangedEventArgs e) { }

  #endregion

  #region Undo

  public UndoProvider UndoProvider { get; } = new();

  /// <summary>
  /// Gets or sets the panel, which contains viewport.
  /// </summary>
  /// <value>
  /// The viewport panel.
  /// </value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public Panel ViewportPanel { get; protected set; }

  /// <summary>
  ///   Gets the viewport.
  /// </summary>
  /// <value>
  ///   The viewport.
  /// </value>
  [NotNull]
  public Viewport2D Viewport
  {
    get
    {
      bool useDeferredPanning = false;
      if (CurrentChild is DependencyObject dependencyChild)
      {
        useDeferredPanning = Viewport2D.GetUseDeferredPanning(obj: dependencyChild);
      }

      if (useDeferredPanning)
      {
        return deferredPanningProxy ??= new Viewport2dDeferredPanningProxy(viewport: viewportInstance);
      }

      return viewportInstance;
    }
    protected set => viewportInstance = value;
  }

  /// <summary>
  /// Gets or sets the CoordinateTransform.
  /// </summary>
  /// <value>
  /// The Transform.
  /// </value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public CoordinateTransform Transform
  {
    get => viewportInstance.Transform;
    set => viewportInstance.Transform = value;
  }

  /// <summary>
  ///   Gets or sets the visible area rectangle.
  /// </summary>
  /// <value>
  ///   The visible.
  /// </value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public DataRect Visible
  {
    get => viewportInstance.Visible;
    set => viewportInstance.Visible = value;
  }

  #endregion

  #region Plotter attached property

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public static PlotterBase GetPlotter(DependencyObject obj)
  {
    return (PlotterBase)obj.GetValue(dp: PlotterProperty);
  }

  public static void SetPlotter(DependencyObject obj, PlotterBase value)
  {
    obj.SetValue(dp: PlotterProperty, value: value);
  }

  public static readonly DependencyProperty PlotterProperty = DependencyProperty.RegisterAttached(
    name: "Plotter",
    propertyType: typeof(PlotterBase),
    ownerType: typeof(PlotterBase),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: null, flags: FrameworkPropertyMetadataOptions.Inherits, propertyChangedCallback: OnPlotterChanged));

  private static void OnPlotterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    PlotterBase prevPlotter = (PlotterBase)e.OldValue;
    PlotterBase currPlotter = (PlotterBase)e.NewValue;

    // raise Plotter[*] events, where * is Attached, Detaching, Changed.
    if (d is FrameworkElement element)
    {
      PlotterChangedEventArgs args = new(prevPlotter: prevPlotter, currPlotter: currPlotter, routedEvent: PlotterDetachingEvent);

      if (currPlotter == null && prevPlotter != null)
      {
        RaisePlotterEvent(element: element, args: args);
      }
      else if (currPlotter != null)
      {
        args.RoutedEvent = PlotterAttachedEvent;
        RaisePlotterEvent(element: element, args: args);
      }

      args.RoutedEvent = PlotterChangedEvent;
      RaisePlotterEvent(element: element, args: args);
    }
  }

  private static void RaisePlotterEvent(FrameworkElement element, PlotterChangedEventArgs args)
  {
    element.RaiseEvent(e: args);
    PlotterEvents.Notify(target: element, args: args);
  }

  #endregion

  #region Plotter routed events

  public static readonly RoutedEvent PlotterAttachedEvent = EventManager.RegisterRoutedEvent(
    name: "PlotterAttached",
    routingStrategy: RoutingStrategy.Direct,
    handlerType: typeof(PlotterChangedEventHandler),
    ownerType: typeof(PlotterBase));

  public static readonly RoutedEvent PlotterDetachingEvent = EventManager.RegisterRoutedEvent(
    name: "PlotterDetaching",
    routingStrategy: RoutingStrategy.Direct,
    handlerType: typeof(PlotterChangedEventHandler),
    ownerType: typeof(PlotterBase));

  public static readonly RoutedEvent PlotterChangedEvent = EventManager.RegisterRoutedEvent(
    name: "PlotterChanged",
    routingStrategy: RoutingStrategy.Direct,
    handlerType: typeof(PlotterChangedEventHandler),
    ownerType: typeof(PlotterBase));

  protected Viewport2D viewportInstance;
  private Viewport2dDeferredPanningProxy deferredPanningProxy;

  #endregion

  protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
  {
    // This is part of endless axis resize loop workaround
    if (viewportInstance != null)
    {
      viewportInstance.UpdateIterationCount = 0;
      if (!viewportInstance.EnforceRestrictions)
      {
        Debug.WriteLine(message: "Plotter: enabling viewport constraints");
        viewportInstance.EnforceRestrictions = true;
      }
    }
    base.OnRenderSizeChanged(sizeInfo: sizeInfo);
  }

  /// <summary>
  ///   Fits to view.
  /// </summary>
  public void FitToView() => viewportInstance.FitToView();

  protected void InitViewport()
  {
    ViewportPanel = new Canvas();
    Grid.SetColumn(element: ViewportPanel, value: 1);
    Grid.SetRow(element: ViewportPanel, value: 1);

    viewportInstance = new Viewport2D(host: ViewportPanel, plotter: this);
    if (LoadMode != PlotterLoadMode.Empty)
    {
      MainGrid.Children.Add(element: ViewportPanel);
    }
  }
}

public delegate void PlotterChangedEventHandler(object sender, PlotterChangedEventArgs e);
