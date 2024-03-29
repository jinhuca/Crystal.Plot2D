﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Crystal.Plot2D.Common;

namespace Crystal.Plot2D.LegendItems;

public delegate IEnumerable<FrameworkElement> LegendItemsBuilder(IPlotterElement element);

public sealed class Legend : ItemsControl, IPlotterElement
{
  static Legend()
  {
    var thisType = typeof(Legend);
    DefaultStyleKeyProperty.OverrideMetadata(forType: thisType, typeMetadata: new FrameworkPropertyMetadata(defaultValue: thisType));
    PlotterBase.PlotterProperty.OverrideMetadata(forType: thisType, typeMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: OnPlotterChanged));
  }

  private readonly ObservableCollection<FrameworkElement> legendItems = new();

  public Legend() => ItemsSource = legendItems;

  #region IPlotterElement Members

  private static void OnPlotterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    var legend = (Legend)d;
    if (e.OldValue != null)
    {
      legend.DetachFromPlotter(plotter: (PlotterBase)e.OldValue);
    }
    if (e.NewValue != null)
    {
      legend.AttachToPlotter(plotter: (PlotterBase)e.NewValue);
    }
  }

  private PlotterBase plotter;
  public void OnPlotterAttached(PlotterBase plotter)
  {
    plotter.CentralGrid.Children.Add(element: this);
    AttachToPlotter(plotter: plotter);
  }

  private void AttachToPlotter(PlotterBase plotter)
  {
    if (plotter != this.plotter)
    {
      this.plotter = plotter;
      plotter.Children.CollectionChanged += PlotterChildren_CollectionChanged;
      PopulateLegend();
    }
  }

  public void OnPlotterDetaching(PlotterBase plotter)
  {
    plotter.CentralGrid.Children.Remove(element: this);
    DetachFromPlotter(plotter: plotter);
  }

  private void DetachFromPlotter(PlotterBase plotter)
  {
    if (plotter != null)
    {
      plotter.Children.CollectionChanged -= PlotterChildren_CollectionChanged;
      this.plotter = null;
      CleanLegend();
    }
  }

  public PlotterBase Plotter => plotter;

  #endregion

  private void PlotterChildren_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => PopulateLegend();

  public void PopulateLegend()
  {
    legendItems.Clear();

    if (!LegendVisible)
    {
      return;
    }

    foreach (var chart in plotter.Children.OfType<DependencyObject>())
    {
      var legendItemsBuilder = GetLegendItemsBuilder(obj: chart);
      if (legendItemsBuilder != null)
      {
        foreach (var legendItem in legendItemsBuilder(element: (IPlotterElement)chart))
        {
          legendItems.Add(item: legendItem);
        }
      }

      //var controller = LegendItemControllersStore.Current.GetController(chart.GetType());
      //if (controller != null)
      //{
      //    controller.ProcessChart(chart, this);
      //}
    }

    UpdateVisibility();
  }

  private void UpdateVisibility()
  {
    Visibility = legendItems.Count > 0 ? Visibility.Visible : Visibility.Hidden;
  }

  private void CleanLegend()
  {
    foreach (var legendItem in legendItems)
    {
      BindingOperations.ClearAllBindings(target: legendItem);
    }
    legendItems.Clear();
    UpdateVisibility();
  }

  #region Attached Properties

  #region Description

  public static object GetDescription(DependencyObject obj)
  {
    return obj.GetValue(dp: DescriptionProperty);
  }

  public static void SetDescription(DependencyObject obj, object value)
  {
    obj.SetValue(dp: DescriptionProperty, value: value);
  }

  public static readonly DependencyProperty DescriptionProperty = DependencyProperty.RegisterAttached(
    name: "Description",
    propertyType: typeof(object),
    ownerType: typeof(Legend),
    defaultMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: null));

  #endregion // end of Description

  #region Detailed description

  public static object GetDetailedDescription(DependencyObject obj)
  {
    return (object)obj.GetValue(dp: DetailedDescriptionProperty);
  }

  public static void SetDetailedDescription(DependencyObject obj, object value)
  {
    obj.SetValue(dp: DetailedDescriptionProperty, value: value);
  }

  public static readonly DependencyProperty DetailedDescriptionProperty = DependencyProperty.RegisterAttached(
    name: "DetailedDescription",
    propertyType: typeof(object),
    ownerType: typeof(Legend),
    defaultMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: null));

  #endregion // end of Detailed description

  #region VisualContent

  public static object GetVisualContent(DependencyObject obj)
  {
    return (object)obj.GetValue(dp: VisualContentProperty);
  }

  public static void SetVisualContent(DependencyObject obj, object value)
  {
    obj.SetValue(dp: VisualContentProperty, value: value);
  }

  public static readonly DependencyProperty VisualContentProperty = DependencyProperty.RegisterAttached(
    name: "VisualContent",
    propertyType: typeof(object),
    ownerType: typeof(Legend),
    defaultMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: null));

  #endregion // end of VisualContent

  #region SampleData

  public static object GetSampleData(DependencyObject obj)
  {
    return (object)obj.GetValue(dp: SampleDataProperty);
  }

  public static void SetSampleData(DependencyObject obj, object value)
  {
    obj.SetValue(dp: SampleDataProperty, value: value);
  }

  public static readonly DependencyProperty SampleDataProperty = DependencyProperty.RegisterAttached(
    name: "SampleData",
    propertyType: typeof(object),
    ownerType: typeof(Legend),
    defaultMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: null));

  #endregion // end of SampleData

  #region ShowInLegend

  public static bool GetShowInLegend(DependencyObject obj)
  {
    return (bool)obj.GetValue(dp: ShowInLegendProperty);
  }

  public static void SetShowInLegend(DependencyObject obj, bool value)
  {
    obj.SetValue(dp: ShowInLegendProperty, value: value);
  }

  public static readonly DependencyProperty ShowInLegendProperty = DependencyProperty.RegisterAttached(
    name: "ShowInLegend",
    propertyType: typeof(bool),
    ownerType: typeof(Legend),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: true, propertyChangedCallback: OnShowInLegendChanged));

  private static void OnShowInLegendChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    var legend = (Legend)d;
    legend.PopulateLegend();
  }

  #endregion // end of ShowInLegend

  #region LegendItemsBuilder

  public static LegendItemsBuilder GetLegendItemsBuilder(DependencyObject obj)
  {
    return (LegendItemsBuilder)obj.GetValue(dp: LegendItemsBuilderProperty);
  }

  public static void SetLegendItemsBuilder(DependencyObject obj, LegendItemsBuilder value)
  {
    obj.SetValue(dp: LegendItemsBuilderProperty, value: value);
  }

  public static readonly DependencyProperty LegendItemsBuilderProperty = DependencyProperty.RegisterAttached(
    name: "LegendItemsBuilder",
    propertyType: typeof(LegendItemsBuilder),
    ownerType: typeof(Legend),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: null, propertyChangedCallback: OnLegendItemsBuilderChanged));

  private static void OnLegendItemsBuilderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is IPlotterElement plotterElement && plotterElement.Plotter != null)
    {
      if (plotterElement.Plotter is Plotter plotter)
      {
        plotter.Legend.PopulateLegend();
      }
    }
  }

  #endregion // end of LegendItemsBuilder

  #endregion // end of Attached Properties

  #region Properties

  public bool LegendVisible
  {
    get => (bool)GetValue(dp: LegendVisibleProperty);
    set => SetValue(dp: LegendVisibleProperty, value: value);
  }

  public static readonly DependencyProperty LegendVisibleProperty = DependencyProperty.Register(
    name: nameof(LegendVisible),
    propertyType: typeof(bool),
    ownerType: typeof(Legend),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: true, propertyChangedCallback: OnLegendVisibleChanged));

  private static void OnLegendVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    var owner = (Legend)d;
    var visible = (bool)e.NewValue;
    owner.OnLegendVisibleChanged(visible: visible);
  }

  private void OnLegendVisibleChanged(bool visible)
  {
    Visibility = visible && legendItems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
  }

  #endregion // end of Properties

  #region Overrides

  public override void OnApplyTemplate()
  {
    base.OnApplyTemplate();

#if !RELEASEXBAP
    var rect = (Rectangle)Template.FindName(name: "backRect", templatedParent: this);
    if (rect != null)
    {
      rect.Effect = new DropShadowEffect { Direction = 300, ShadowDepth = 3, Opacity = 0.4 };
    }
#endif
  }

  #endregion // end of Overrides
}
