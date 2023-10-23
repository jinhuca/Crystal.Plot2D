using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Crystal.Plot2D;

/// <summary>
/// ViewportElement2D is intended to be a child of Viewport2D. 
/// Specifics of ViewportElement2D is Viewport2D attached property.
/// </summary>
public abstract class ViewportElement2D : FrameworkElement, IPlotterElement, INotifyPropertyChanged
{
  protected virtual Panel GetHostPanel(PlotterBase plotter) => plotter.CentralGrid;

  #region IPlotterElement Members

  void IPlotterElement.OnPlotterAttached(PlotterBase plotter) => OnPlotterAttached(plotter: plotter);
  void IPlotterElement.OnPlotterDetaching(PlotterBase plotter) => OnPlotterDetaching(plotter: plotter);
  public PlotterBase Plotter { get; private set; }

  protected virtual void OnPlotterAttached(PlotterBase plotter)
  {
    Plotter = plotter;
    GetHostPanel(plotter: plotter).Children.Add(element: this);
    Viewport = Plotter.Viewport;
    Viewport.PropertyChanged += OnViewportPropertyChanged;
  }

  private void OnViewportPropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
  {
    switch (e.PropertyName)
    {
      case "Visible":
        OnVisibleChanged(newRect: (DataRect)e.NewValue, oldRect: (DataRect)e.OldValue);
        break;
      case "Output":
        OnOutputChanged(newRect: (Rect)e.NewValue, oldRect: (Rect)e.OldValue);
        break;
      case "Transform":
        Update();
        break;
    }
  }

  protected virtual void OnPlotterDetaching(PlotterBase plotter)
  {
    Viewport.PropertyChanged -= OnViewportPropertyChanged;
    Viewport = null;
    GetHostPanel(plotter: plotter).Children.Remove(element: this);
    Plotter = null;
  }

  #endregion IPlotterElement Members

  public int ZIndex
  {
    get => Panel.GetZIndex(element: this);
    set => Panel.SetZIndex(element: this, value: value);
  }

  #region Viewport

  protected Viewport2D Viewport { get; private set; }

  #endregion

  #region Description

  /// <summary>
  /// Creates the default description.
  /// </summary>
  /// <returns></returns>
  protected virtual Description CreateDefaultDescription()
  {
    return new StandardDescription();
  }

  private Description description;
  /// <summary>
  /// Gets or sets the description.
  /// </summary>
  /// <value>The description.</value>
  public Description Description
  {
    get
    {
      if (description == null)
      {
        description = CreateDefaultDescription();
        description.Attach(element: this);
      }
      return description;
    }
    set
    {
      if (description != null)
      {
        description.Detach();
      }
      description = value;
      if (description != null)
      {
        description.Attach(element: this);
      }
      RaisePropertyChanged();
    }
  }

  public override string ToString()
  {
    return GetType().Name + ": " + Description.Brief;
  }

  #endregion

  protected internal Vector Offset { get; set; }

  //bool SizeEqual(Diameter s1, Diameter s2, double eps)
  //{
  //    double width = Math.Min(s1.Width, s2.Width);
  //    double height = Math.Min(s1.Height, s2.Height);
  //    return Math.Abs(s1.Width - s2.Width) < width * eps &&
  //           Math.Abs(s1.Height - s2.Height) < height * eps;
  //}

  protected virtual void OnVisibleChanged(DataRect newRect, DataRect oldRect)
  {
    if (newRect.Size == oldRect.Size)
    {
      var transform = Viewport.Transform;
      Offset += oldRect.Location.DataToScreen(transform: transform) - newRect.Location.DataToScreen(transform: transform);
      if (ManualTranslate)
      {
        Update();
      }
    }
    else
    {
      Offset = new Vector();
      Update();
    }
  }

  protected virtual void OnOutputChanged(Rect newRect, Rect oldRect)
  {
    Offset = new Vector();
    Update();
  }

  /// <summary>
  ///   Gets a value indicating whether this instance is translated.
  /// </summary>
  /// <value>
  /// 	<c>true</c> if this instance is translated; otherwise, <c>false</c>.
  /// </value>
  protected bool IsTranslated => Offset.X != 0 || Offset.Y != 0;

  #region IsLevel

  public bool IsLayer
  {
    get => (bool)GetValue(dp: IsLayerProperty);
    set => SetValue(dp: IsLayerProperty, value: value);
  }

  public static readonly DependencyProperty IsLayerProperty = DependencyProperty.Register(
    name: nameof(IsLayer),
    propertyType: typeof(bool),
    ownerType: typeof(ViewportElement2D),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: false));

  #endregion

  #region Rendering & caching options

  protected object GetValueSync(DependencyProperty property) =>
    Dispatcher.Invoke(priority: DispatcherPriority.Send, method: (DispatcherOperationCallback)delegate { return GetValue(dp: property); }, arg: property);

  protected void SetValueAsync(DependencyProperty property, object value)
    => Dispatcher.BeginInvoke(priority: DispatcherPriority.Send, method: (SendOrPostCallback)delegate { SetValue(dp: property, value: value); }, arg: value);

  private bool manualClip;

  /// <summary>
  ///   Gets or sets a value indicating whether descendant graph class relies on autotic clipping by Viewport.Output or
  ///   does its own clipping.
  /// </summary>
  public bool ManualClip
  {
    get => manualClip;
    set => manualClip = value;
  }

  private bool manualTranslate;
  /// <summary>
  ///   Gets or sets a value indicating whether descendant graph class relies on automatic translation of it, or does its own.
  /// </summary>
  public bool ManualTranslate
  {
    get => manualTranslate;
    set => manualTranslate = value;
  }

  private RenderTo renderTarget = RenderTo.Screen;
  /// <summary>
  ///   Gets or sets a value indicating whether descendant graph class uses cached rendering of its content to image, or not.
  /// </summary>
  public RenderTo RenderTarget
  {
    get => renderTarget;
    set => renderTarget = value;
  }

  private enum ImageKind
  {
    Real,
    BeingRendered,
    Empty
  }

  #endregion

  private RenderState CreateRenderState(DataRect renderVisible, RenderTo renderingType)
    => new(renderVisible: renderVisible, visible: Viewport.Visible, output: Viewport.Output, renderingType: renderingType);

  protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
  {
    base.OnPropertyChanged(e: e);
    if (e.Property == VisibilityProperty)
    {
      Update();
    }
  }

  private bool updateCalled;
  private bool beforeFirstUpdate = true;
  protected void Update()
  {
    if (Viewport == null)
    {
      return;
    }

    UpdateCore();
    if (!beforeFirstUpdate)
    {
      updateCalled = true;
      InvalidateVisual();
    }
    beforeFirstUpdate = false;
  }

  protected virtual void UpdateCore() { }

  protected void TranslateVisual()
  {
    if (!ManualTranslate)
    {
      shouldReRender = false;
    }
    InvalidateVisual();
  }

  #region Thumbnail

  private ImageSource thumbnail;
  public ImageSource Thumbnail
  {
    get
    {
      if (!CreateThumbnail)
      {
        CreateThumbnail = true;
      }
      return thumbnail;
    }
  }

  private bool createThumbnail;
  public bool CreateThumbnail
  {
    get => createThumbnail;
    set
    {
      if (createThumbnail != value)
      {
        createThumbnail = value;
        if (value)
        {
          RenderThumbnail();
        }
        else
        {
          thumbnail = null;
          RaisePropertyChanged(propertyNamme: "Thumbnail");
        }
      }
    }
  }

  private bool ShouldCreateThumbnail => IsLayer && createThumbnail;

  private void RenderThumbnail()
  {
    if (Viewport == null)
    {
      return;
    }

    Rect output = Viewport.Output;
    if (output.Width == 0 || output.Height == 0)
    {
      return;
    }

    DataRect visible = Viewport.Visible;
    var transform = Viewport.Transform;
    DrawingVisual visual = new();
    using (DrawingContext dc = visual.RenderOpen())
    {
      Point outputStart = visible.Location.DataToScreen(transform: transform);
      double x = -outputStart.X + Offset.X;
      double y = -outputStart.Y + output.Bottom - output.Top + Offset.Y;
      bool translate = !manualTranslate && IsTranslated;
      if (translate)
      {
        dc.PushTransform(transform: new TranslateTransform(offsetX: x, offsetY: y));
      }

      const byte c = 240;
      Brush brush = new SolidColorBrush(color: Color.FromArgb(a: 120, r: c, g: c, b: c));
      Pen pen = new(brush: Brushes.Black, thickness: 1);
      dc.DrawRectangle(brush: brush, pen: pen, rectangle: output);
      dc.DrawDrawing(drawing: graphContents);

      if (translate)
      {
        dc.Pop();
      }
    }

    RenderTargetBitmap bmp = new(pixelWidth: (int)output.Width, pixelHeight: (int)output.Height, dpiX: 96, dpiY: 96, pixelFormat: PixelFormats.Pbgra32);
    bmp.Render(visual: visual);
    thumbnail = bmp;
    RaisePropertyChanged(propertyNamme: "Thumbnail");
  }

  #endregion

  private bool shouldReRender = true;
  private DrawingGroup graphContents;
  protected sealed override void OnRender(DrawingContext drawingContext)
  {
    if (Viewport == null)
    {
      return;
    }

    Rect output = Viewport.Output;
    if (output.Width == 0 || output.Height == 0)
    {
      return;
    }
    if (output.IsEmpty)
    {
      return;
    }
    if (Visibility != Visibility.Visible)
    {
      return;
    }
    if (shouldReRender || manualTranslate || renderTarget == RenderTo.Image || beforeFirstUpdate || updateCalled)
    {
      graphContents ??= new DrawingGroup();
      if (beforeFirstUpdate)
      {
        Update();
      }

      using (DrawingContext context = graphContents.Open())
      {
        if (renderTarget == RenderTo.Screen)
        {
          RenderState state = CreateRenderState(renderVisible: Viewport.Visible, renderingType: RenderTo.Screen);
          OnRenderCore(dc: context, state: state);
        }
      }
      updateCalled = false;
    }

    // thumbnail is not created, if
    // 1) CreateThumbnail is false
    // 2) this graph has IsLayer property, set to false
    if (ShouldCreateThumbnail)
    {
      RenderThumbnail();
    }

    if (!manualClip)
    {
      drawingContext.PushClip(clipGeometry: new RectangleGeometry(rect: output));
    }
    bool translate = !manualTranslate && IsTranslated;
    if (translate)
    {
      drawingContext.PushTransform(transform: new TranslateTransform(offsetX: Offset.X, offsetY: Offset.Y));
    }

    drawingContext.DrawDrawing(drawing: graphContents);

    if (translate)
    {
      drawingContext.Pop();
    }
    if (!manualClip)
    {
      drawingContext.Pop();
    }
    shouldReRender = true;
  }

  protected abstract void OnRenderCore(DrawingContext dc, RenderState state);

  #region INotifyPropertyChanged Members

  public event PropertyChangedEventHandler PropertyChanged;

  protected void RaisePropertyChanged([CallerMemberName] string propertyNamme = "")
    => PropertyChanged?.Invoke(sender: this, e: new PropertyChangedEventArgs(propertyName: propertyNamme));

  #endregion
}