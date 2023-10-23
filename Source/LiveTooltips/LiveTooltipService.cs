using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace Crystal.Plot2D.Charts;

public static class LiveToolTipService
{

  #region Properties

  public static object GetToolTip(DependencyObject obj)
  {
    return (object)obj.GetValue(dp: ToolTipProperty);
  }

  public static void SetToolTip(DependencyObject obj, object value)
  {
    obj.SetValue(dp: ToolTipProperty, value: value);
  }

  public static readonly DependencyProperty ToolTipProperty = DependencyProperty.RegisterAttached(
    name: "ToolTip",
    propertyType: typeof(object),
    ownerType: typeof(LiveToolTipService),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: null, propertyChangedCallback: OnToolTipChanged));

  private static LiveToolTip GetLiveToolTip(DependencyObject obj)
  {
    return (LiveToolTip)obj.GetValue(dp: LiveToolTipProperty);
  }

  private static void SetLiveToolTip(DependencyObject obj, LiveToolTip value)
  {
    obj.SetValue(dp: LiveToolTipProperty, value: value);
  }

  private static readonly DependencyProperty LiveToolTipProperty = DependencyProperty.RegisterAttached(
    name: "LiveToolTip",
    propertyType: typeof(LiveToolTip),
    ownerType: typeof(LiveToolTipService),
    defaultMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: null));

  #region Opacity

  public static double GetTooltipOpacity(DependencyObject obj)
  {
    return (double)obj.GetValue(dp: TooltipOpacityProperty);
  }

  public static void SetTooltipOpacity(DependencyObject obj, double value)
  {
    obj.SetValue(dp: TooltipOpacityProperty, value: value);
  }

  public static readonly DependencyProperty TooltipOpacityProperty = DependencyProperty.RegisterAttached(
    name: "TooltipOpacity",
    propertyType: typeof(double),
    ownerType: typeof(LiveToolTipService),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: 1.0, propertyChangedCallback: OnTooltipOpacityChanged));

  private static void OnTooltipOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    LiveToolTip liveTooltip = GetLiveToolTip(obj: d);
    if (liveTooltip != null)
    {
      liveTooltip.Opacity = (double)e.NewValue;
    }
  }

  #endregion // end of Opacity

  #region IsPropertyProxy property

  public static bool GetIsPropertyProxy(DependencyObject obj)
  {
    return (bool)obj.GetValue(dp: IsPropertyProxyProperty);
  }

  public static void SetIsPropertyProxy(DependencyObject obj, bool value)
  {
    obj.SetValue(dp: IsPropertyProxyProperty, value: value);
  }

  public static readonly DependencyProperty IsPropertyProxyProperty = DependencyProperty.RegisterAttached(
    name: "IsPropertyProxy",
    propertyType: typeof(bool),
    ownerType: typeof(LiveToolTipService),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: false));

  #endregion // end of IsPropertyProxy property

  #endregion

  private static void OnToolTipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    FrameworkElement source = (FrameworkElement)d;

    if (e.NewValue == null)
    {
      source.Loaded -= source_Loaded;
      source.ClearValue(dp: LiveToolTipProperty);
    }

    if (GetIsPropertyProxy(obj: source))
    {
      return;
    }

    var content = e.NewValue;

    if (content is DataTemplate template)
    {
      content = template.LoadContent();
    }

    LiveToolTip tooltip = null;
    if (e.NewValue is LiveToolTip)
    {
      tooltip = e.NewValue as LiveToolTip;
    }
    else
    {
      tooltip = new LiveToolTip { Content = content };
    }

    if (tooltip == null && e.OldValue == null)
    {
      tooltip = new LiveToolTip { Content = content };
    }

    if (tooltip != null)
    {
      SetLiveToolTip(obj: source, value: tooltip);
      if (!source.IsLoaded)
      {
        source.Loaded += source_Loaded;
      }
      else
      {
        AddTooltip(source: source);
      }
    }
  }

  private static void AddTooltipForElement(FrameworkElement source, LiveToolTip tooltip)
  {
    AdornerLayer layer = AdornerLayer.GetAdornerLayer(visual: source);

    LiveToolTipAdorner adorner = new(adornedElement: source, tooltip: tooltip);
    layer.Add(adorner: adorner);
  }

  private static void source_Loaded(object sender, RoutedEventArgs e)
  {
    FrameworkElement source = (FrameworkElement)sender;

    if (source.IsLoaded)
    {
      AddTooltip(source: source);
    }
  }

  private static void AddTooltip(FrameworkElement source)
  {
    if (DesignerProperties.GetIsInDesignMode(element: source))
    {
      return;
    }

    LiveToolTip tooltip = GetLiveToolTip(obj: source);

    Window window = Window.GetWindow(dependencyObject: source);
    FrameworkElement child = source;
    FrameworkElement parent = null;
    if (window != null)
    {
      while (parent != window)
      {
        parent = (FrameworkElement)VisualTreeHelper.GetParent(reference: child);
        child = parent;
        var nameScope = NameScope.GetNameScope(dependencyObject: parent);
        if (nameScope != null)
        {
          string nameScopeName = nameScope.ToString();
          if (nameScopeName != "System.Windows.TemplateNameScope")
          {
            NameScope.SetNameScope(dependencyObject: tooltip, value: nameScope);
            break;
          }
        }
      }
    }

    var binding = BindingOperations.GetBinding(target: tooltip, dp: ContentControl.ContentProperty);
    if (binding != null)
    {
      BindingOperations.ClearBinding(target: tooltip, dp: ContentControl.ContentProperty);
      BindingOperations.SetBinding(target: tooltip, dp: ContentControl.ContentProperty, binding: binding);
    }

    Binding dataContextBinding = new() { Path = new PropertyPath(path: "DataContext"), Source = source };
    tooltip.SetBinding(dp: FrameworkElement.DataContextProperty, binding: dataContextBinding);

    tooltip.Owner = source;
    if (GetTooltipOpacity(obj: source) != (double)TooltipOpacityProperty.DefaultMetadata.DefaultValue)
    {
      tooltip.Opacity = GetTooltipOpacity(obj: source);
    }

    AddTooltipForElement(source: source, tooltip: tooltip);
  }
}
