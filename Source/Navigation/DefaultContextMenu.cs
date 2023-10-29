using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;

namespace Crystal.Plot2D.Navigation;

/// <summary>
///   Is responsible for displaying and populating default context menu of Plotter
/// </summary>
public sealed class DefaultContextMenu : IPlotterElement
{
  private static readonly BitmapImage helpIcon;
  private static readonly BitmapImage copyScreenshotIcon;
  private static readonly BitmapImage saveScreenshotIcon;
  private static readonly BitmapImage fitToViewIcon;

  private static readonly StreamGeometry helpIconGeometry;

  static DefaultContextMenu()
  {
    helpIcon = LoadIcon(name: "HelpIcon");
    helpIconGeometry = new StreamGeometry();
    saveScreenshotIcon = LoadIcon(name: "SaveIcon");
    copyScreenshotIcon = LoadIcon(name: "CopyScreenshotIcon");
    fitToViewIcon = LoadIcon(name: "FitToViewIcon");
  }

  private static BitmapImage LoadIcon(string name)
  {
    var currentAssembly = typeof(DefaultContextMenu).Assembly;

    BitmapImage icon = new();
    icon.BeginInit();
    icon.StreamSource = currentAssembly.GetManifestResourceStream(name: "Crystal.Plot2D.Resources." + name + ".png");
    icon.EndInit();
    icon.Freeze();

    return icon;
  }

  private static StreamGeometry LoadIconGeometry(string geometryKey)
  {
    var currentAssembly = typeof(DefaultContextMenu).Assembly;
    StreamGeometry iconGeometry = new();

    return iconGeometry;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="DefaultContextMenu"/> class.
  /// </summary>
  public DefaultContextMenu() { }

  private ContextMenu PopulateContextMenu(PlotterBase target)
  {
    ContextMenu menu = new();
    //menu.Background = Brushes.Beige;
    MenuItem fitToViewMenuItem = new()
    {
      Header = Strings.UIResources.ContextMenuFitToView,
      ToolTip = Strings.UIResources.ContextMenuFitToViewTooltip,
      Icon = new Image { Source = fitToViewIcon },
      Command = ChartCommands.FitToView,
      CommandTarget = target
    };

    MenuItem savePictureMenuItem = new()
    {
      Header = Strings.UIResources.ContextMenuSaveScreenshot,
      ToolTip = Strings.UIResources.ContextMenuSaveScreenshotTooltip,
      Icon = new Image { Source = saveScreenshotIcon },
      Command = ChartCommands.SaveScreenshot,
      CommandTarget = target
    };

    MenuItem copyPictureMenuItem = new()
    {
      Header = Strings.UIResources.ContextMenuCopyScreenshot,
      ToolTip = Strings.UIResources.ContextMenuCopyScreenshotTooltip,
      Icon = new Image { Source = copyScreenshotIcon },
      Command = ChartCommands.CopyScreenshot,
      CommandTarget = target
    };

    MenuItem quickHelpMenuItem = new()
    {
      Header = Strings.UIResources.ContextMenuQuickHelp,
      ToolTip = Strings.UIResources.ContextMenuQuickHelpTooltip,
      Command = ChartCommands.ShowHelp,
      Icon = new Image { Source = helpIcon },
      CommandTarget = target
    };

    MenuItem reportFeedback = new()
    {
      Header = Strings.UIResources.ContextMenuReportFeedback,
      ToolTip = Strings.UIResources.ContextMenuReportFeedbackTooltip,
      Icon = (Image)plotter.Resources[key: "SendFeedbackIcon"]
    };
    reportFeedback.Click += reportFeedback_Click;

    staticMenuItems.Add(item: fitToViewMenuItem);
    staticMenuItems.Add(item: copyPictureMenuItem);
    staticMenuItems.Add(item: savePictureMenuItem);
    staticMenuItems.Add(item: quickHelpMenuItem);
    staticMenuItems.Add(item: reportFeedback);

    menu.ItemsSource = staticMenuItems;

    return menu;
  }

  private void reportFeedback_Click(object sender, RoutedEventArgs e)
  {
    try
    {
      using (Process.Start(fileName: "mailto:" + Strings.UIResources.SendFeedbackEmail + "?Subject=[D3]%20" + typeof(DefaultContextMenu).Assembly.GetName().Version)) { }
    }
    catch (Exception)
    {
      MessageBox.Show(messageBoxText: Strings.UIResources.SendFeedbackError + Strings.UIResources.SendFeedbackEmail, caption: "Error while sending feedback", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
    }
  }

  private readonly ObservableCollection<object> staticMenuItems = new();

  // hidden because default menu items' command target is plotter, and serializing this will
  // cause circular reference

  /// <summary>
  /// Gets the static menu items.
  /// </summary>
  /// <value>The static menu items.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public ObservableCollection<object> StaticMenuItems => staticMenuItems;

  #region IPlotterElement Members

  private PlotterBase plotter;
  void IPlotterElement.OnPlotterAttached(PlotterBase plotter)
  {
    this.plotter = (PlotterBase)plotter;

    var menu = PopulateContextMenu(target: plotter);
    plotter.ContextMenu = menu;

    plotter.PreviewMouseRightButtonDown += plotter_PreviewMouseRightButtonDown;
    plotter.PreviewMouseRightButtonUp += plotter_PreviewMouseRightButtonUp;
    plotter.PreviewMouseLeftButtonDown += plotter_PreviewMouseLeftButtonDown;

    plotter.ContextMenu.Closed += ContextMenu_Closed;
  }

  private void plotter_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
  {
    // this will prevent other tools like PointSelector from wrong actuation
    if (contextMenuOpen)
    {
      plotter.Focus();
      e.Handled = true;
    }
  }

  void IPlotterElement.OnPlotterDetaching(PlotterBase plotter)
  {
    plotter.ContextMenu.Closed -= ContextMenu_Closed;

    plotter.ContextMenu = null;
    plotter.PreviewMouseRightButtonDown -= plotter_PreviewMouseRightButtonDown;
    plotter.PreviewMouseRightButtonUp -= plotter_PreviewMouseRightButtonUp;

    this.plotter = null;
  }

  private void ContextMenu_Closed(object sender, RoutedEventArgs e)
  {
    contextMenuOpen = false;
    foreach (var item in dynamicMenuItems)
    {
      staticMenuItems.Remove(item: item);
    }
  }

  private Point mousePos;
  /// <summary>
  /// Gets the mouse position when right mouse button was clicked.
  /// </summary>
  /// <value>The mouse position on click.</value>
  public Point MousePositionOnClick => mousePos;

  private void plotter_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
  {
    contextMenuOpen = false;
    mousePos = e.GetPosition(relativeTo: plotter);
  }

  private bool contextMenuOpen;
  private readonly ObservableCollection<object> dynamicMenuItems = new();

  private void plotter_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
  {
    var position = e.GetPosition(relativeTo: plotter);
    if (mousePos == position)
    {
      hitResults.Clear();
      VisualTreeHelper.HitTest(reference: plotter, filterCallback: null, resultCallback: CollectAllVisuals_Callback, hitTestParameters: new PointHitTestParameters(point: position));

      foreach (var item in dynamicMenuItems)
      {
        staticMenuItems.Remove(item: item);
      }
  
      dynamicMenuItems.Clear();
      var dynamicItems = hitResults.Where(predicate: r =>
      {
        if (r is IPlotterContextMenuSource menuSource)
        {
          menuSource.BuildMenu();
        }

        var items = GetPlotterContextMenu(obj: r);
        return items != null && items.Count > 0;
      }).SelectMany(selector: r =>
      {
        var menuItems = GetPlotterContextMenu(obj: r);

        if (r is FrameworkElement chart)
        {
          foreach (var menuItem in menuItems.OfType<MenuItem>())
          {
            menuItem.DataContext = chart.DataContext;
          }
        }
        return menuItems;
      }).ToList();

      foreach (var item in dynamicItems)
      {
        dynamicMenuItems.Add(item: item);
        //MenuItem menuItem = item as MenuItem;
        //if (menuItem != null)
        //{
        //    menuItem.CommandTarget = plotter;
        //}
      }

      staticMenuItems.AddMany(addingItems: dynamicMenuItems);

      plotter.Focus();
      contextMenuOpen = true;

      // in XBAP applications these lines throws a SecurityException, as (as I think)
      // we are opening "new window" here. So in XBAP we are not opening context menu manually, but
      // it will be opened by itself as this is right click.
#if !RELEASEXBAP
      plotter.ContextMenu.IsOpen = true;
      e.Handled = true;
#endif
    }
    else
    {
      // this is to prevent showing menu when RMB was pressed, then moved and now is releasing.
      e.Handled = true;
    }
  }

  #region PlotterContextMenu property

  /// <summary>
  /// Gets the plotter context menu.
  /// </summary>
  /// <param name="obj">The obj.</param>
  /// <returns></returns>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public static ObservableCollection<object> GetPlotterContextMenu(DependencyObject obj)
  {
    return (ObservableCollection<object>)obj.GetValue(dp: PlotterContextMenuProperty);
  }

  /// <summary>
  /// Sets the plotter context menu.
  /// </summary>
  /// <param name="obj">The obj.</param>
  /// <param name="value">The value.</param>
  public static void SetPlotterContextMenu(DependencyObject obj, ObservableCollection<object> value)
  {
    obj.SetValue(dp: PlotterContextMenuProperty, value: value);
  }

  /// <summary>
  /// Identifies the PlotterContextMenu attached property.
  /// </summary>
  public static readonly DependencyProperty PlotterContextMenuProperty = DependencyProperty.RegisterAttached(
    name: "PlotterContextMenu",
    propertyType: typeof(ObservableCollection<object>),
    ownerType: typeof(DefaultContextMenu),
    defaultMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: null));

  #endregion

  private readonly List<DependencyObject> hitResults = new();
  private HitTestResultBehavior CollectAllVisuals_Callback(HitTestResult result)
  {
    if (result == null || result.VisualHit == null)
    {
      return HitTestResultBehavior.Stop;
    }

    hitResults.Add(item: result.VisualHit);
    return HitTestResultBehavior.Continue;
  }

  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  PlotterBase IPlotterElement.Plotter => plotter;

  #endregion
}

/// <summary>
/// Represents a collection of additional menu items in Plotter's context menu.
/// </summary>
//public sealed class ObjectCollection : ObservableCollection<Object> { }
