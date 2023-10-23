﻿using Crystal.Plot2D.Charts;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D;

/// <summary>
/// Plotter that can render axis and grid
/// </summary>
public class Plotter : PlotterBase
{
  private GeneralAxis _horizontalAxis = new HorizontalAxis();       // Default to axis of numeric??
  private GeneralAxis _verticalAxis = new VerticalAxis();

  public Legend Legend { get; set; } = new();

  public ItemsPanelTemplate LegendPanelTemplate
  {
    get => Legend.ItemsPanel;
    set => Legend.ItemsPanel = value;
  }

  public Style LegendStyle
  {
    get => Legend.Style;
    set => Legend.Style = value;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="Plotter"/> class.
  /// </summary>
  public Plotter()
  {
    _horizontalAxis.TicksChanged += OnHorizontalAxisTicksChanged;
    _verticalAxis.TicksChanged += OnVerticalAxisTicksChanged;

    SetIsDefaultAxis(obj: _horizontalAxis, value: true);
    SetIsDefaultAxis(obj: _verticalAxis, value: true);

    _mouseNavigation = new MouseNavigation();
    _keyboardNavigation = new KeyboardNavigation();
    _defaultContextMenu = new DefaultContextMenu();
    horizontalAxisNavigation = new AxisNavigation { Placement = AxisPlacement.Bottom };
    verticalAxisNavigation = new AxisNavigation { Placement = AxisPlacement.Left };

    Children.AddMany(
      children: new IPlotterElement[]
      {
        _horizontalAxis,
        _verticalAxis,
        AxisGrid,
        _mouseNavigation,
        _keyboardNavigation,
        _defaultContextMenu,
        horizontalAxisNavigation,
        verticalAxisNavigation,
        new LongOperationsIndicator(),
        Legend
      });

#if DEBUG
    Children.Add(item: new DebugMenu());
#endif

    SetAllChildrenAsDefault();
  }

  protected Plotter(PlotterLoadMode loadMode) : base(loadMode: loadMode) { }

  #region Default charts

  private readonly MouseNavigation _mouseNavigation;
  /// <summary>
  /// Gets the default mouse navigation of Plotter.
  /// </summary>
  /// <value>The mouse navigation.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public MouseNavigation MouseNavigation => _mouseNavigation;

  private readonly KeyboardNavigation _keyboardNavigation;
  /// <summary>
  /// Gets the default keyboard navigation of Plotter.
  /// </summary>
  /// <value>The keyboard navigation.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public KeyboardNavigation KeyboardNavigation => _keyboardNavigation;

  private readonly DefaultContextMenu _defaultContextMenu;
  /// <summary>
  /// Gets the default context menu of Plotter.
  /// </summary>
  /// <value>The default context menu.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public DefaultContextMenu DefaultContextMenu => _defaultContextMenu;

  private readonly AxisNavigation horizontalAxisNavigation;
  /// <summary>
  /// Gets the default horizontal axis navigation of Plotter.
  /// </summary>
  /// <value>The horizontal axis navigation.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public AxisNavigation HorizontalAxisNavigation => horizontalAxisNavigation;

  private readonly AxisNavigation verticalAxisNavigation;
  /// <summary>
  /// Gets the default vertical axis navigation of Plotter.
  /// </summary>
  /// <value>The vertical axis navigation.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public AxisNavigation VerticalAxisNavigation => verticalAxisNavigation;

  /// <summary>
  /// Gets the default axis grid of Plotter.
  /// </summary>
  /// <value>The axis grid.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public AxisGrid AxisGrid { get; } = new();

  #endregion

  private void OnHorizontalAxisTicksChanged(object sender, EventArgs e)
  {
    GeneralAxis axis = (GeneralAxis)sender;
    UpdateHorizontalTicks(axis: axis);
  }

  private void UpdateHorizontalTicks(GeneralAxis axis)
  {
    AxisGrid.BeginTicksUpdate();

    if (axis != null)
    {
      AxisGrid.HorizontalTicks = axis.ScreenTicks;
      AxisGrid.MinorHorizontalTicks = axis.MinorScreenTicks;
    }
    else
    {
      AxisGrid.HorizontalTicks = null;
      AxisGrid.MinorHorizontalTicks = null;
    }

    AxisGrid.EndTicksUpdate();
  }

  private void OnVerticalAxisTicksChanged(object sender, EventArgs e)
  {
    GeneralAxis axis = (GeneralAxis)sender;
    UpdateVerticalTicks(axis: axis);
  }

  private void UpdateVerticalTicks(GeneralAxis axis)
  {
    AxisGrid.BeginTicksUpdate();

    if (axis != null)
    {
      AxisGrid.VerticalTicks = axis.ScreenTicks;
      AxisGrid.MinorVerticalTicks = axis.MinorScreenTicks;
    }
    else
    {
      AxisGrid.VerticalTicks = null;
      AxisGrid.MinorVerticalTicks = null;
    }

    AxisGrid.EndTicksUpdate();
  }

  bool _keepOldAxis;
  bool _updatingAxis;

  /// <summary>
  /// Gets or sets the main vertical axis of Plotter.
  /// Main vertical axis of Plotter is axis which ticks are used to draw horizontal lines on AxisGrid.
  /// Value can be set to null to completely remove main vertical axis.
  /// </summary>
  /// <value>The main vertical axis.</value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public GeneralAxis MainVerticalAxis
  {
    get => _verticalAxis;
    set
    {
      if (_updatingAxis)
      {
        return;
      }

      if (value == null && _verticalAxis != null)
      {
        if (!_keepOldAxis)
        {
          Children.Remove(item: _verticalAxis);
        }
        _verticalAxis.TicksChanged -= OnVerticalAxisTicksChanged;
        _verticalAxis = null;
        UpdateVerticalTicks(axis: _verticalAxis);
        return;
      }

      VerifyAxisType(axisPlacement: value.Placement, axisType: AxisType.Vertical);

      if (value != _verticalAxis)
      {
        ValidateVerticalAxis(axis: value);
        _updatingAxis = true;
        if (_verticalAxis != null)
        {
          _verticalAxis.TicksChanged -= OnVerticalAxisTicksChanged;
          SetIsDefaultAxis(obj: _verticalAxis, value: false);
          if (!_keepOldAxis)
          {
            Children.Remove(item: _verticalAxis);
          }
          value.Visibility = _verticalAxis.Visibility;
        }
        SetIsDefaultAxis(obj: value, value: true);
        _verticalAxis = value;
        _verticalAxis.TicksChanged += OnVerticalAxisTicksChanged;

        if (!Children.Contains(item: value))
        {
          Children.Add(content: value);
        }

        UpdateVerticalTicks(axis: value);
        OnVerticalAxisChanged();
        _updatingAxis = false;
      }
    }
  }

  protected virtual void OnVerticalAxisChanged() { }
  protected virtual void ValidateVerticalAxis(GeneralAxis axis) { }

  /// <summary>
  ///   Gets or sets the main horizontal axis visibility.
  /// </summary>
  /// <value>
  ///   The main horizontal axis visibility.
  /// </value>
  public Visibility MainHorizontalAxisVisibility
  {
    get => MainHorizontalAxis?.Visibility ?? Visibility.Hidden;
    set
    {
      if (MainHorizontalAxis != null)
      {
        MainHorizontalAxis.Visibility = value;
      }
    }
  }

  /// <summary>
  ///   Gets or sets the main vertical axis visibility.
  /// </summary>
  /// <value>
  ///   The main vertical axis visibility.
  /// </value>
  public Visibility MainVerticalAxisVisibility
  {
    get => MainVerticalAxis?.Visibility ?? Visibility.Hidden;
    set
    {
      if (MainVerticalAxis != null)
      {
        MainVerticalAxis.Visibility = value;
      }
    }
  }

  /// <summary>
  ///   Gets or sets the main horizontal axis of Plotter.
  ///   Main horizontal axis of Plotter is axis which ticks are used to draw vertical lines on AxisGrid.
  ///   Value can be set to null to completely remove main horizontal axis.
  /// </summary>
  /// <value>
  ///   The main horizontal axis.
  /// </value>
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Hidden)]
  public GeneralAxis MainHorizontalAxis
  {
    get => _horizontalAxis;
    set
    {
      if (_updatingAxis)
      {
        return;
      }

      if (value == null && _horizontalAxis != null)
      {
        Children.Remove(item: _horizontalAxis);
        _horizontalAxis.TicksChanged -= OnHorizontalAxisTicksChanged;
        _horizontalAxis = null;
        UpdateHorizontalTicks(axis: _horizontalAxis);
        return;
      }

      VerifyAxisType(axisPlacement: value.Placement, axisType: AxisType.Horizontal);

      if (value != _horizontalAxis)
      {
        ValidateHorizontalAxis(axis: value);
        _updatingAxis = true;
        if (_horizontalAxis != null)
        {
          _horizontalAxis.TicksChanged -= OnHorizontalAxisTicksChanged;
          SetIsDefaultAxis(obj: _horizontalAxis, value: false);
          if (!_keepOldAxis)
          {
            Children.Remove(item: _horizontalAxis);
          }
          value.Visibility = _horizontalAxis.Visibility;
        }
        SetIsDefaultAxis(obj: value, value: true);
        _horizontalAxis = value;
        _horizontalAxis.TicksChanged += OnHorizontalAxisTicksChanged;

        if (!Children.Contains(item: value))
        {
          Children.Add(content: value);
        }

        UpdateHorizontalTicks(axis: value);
        OnHorizontalAxisChanged();
        _updatingAxis = false;
      }
    }
  }

  protected virtual void OnHorizontalAxisChanged() { }
  protected virtual void ValidateHorizontalAxis(GeneralAxis axis) { }

  private static void VerifyAxisType(AxisPlacement axisPlacement, AxisType axisType)
  {
    var result = false;
    switch (axisPlacement)
    {
      case AxisPlacement.Left:
      case AxisPlacement.Right:
        result = axisType == AxisType.Vertical;
        break;
      case AxisPlacement.Top:
      case AxisPlacement.Bottom:
        result = axisType == AxisType.Horizontal;
        break;
    }

    if (!result)
    {
      throw new ArgumentException(message: Strings.Exceptions.InvalidAxisPlacement);
    }
  }

  protected override void OnIsDefaultAxisChangedCore(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    if (d is GeneralAxis axis)
    {
      var value = (bool)e.NewValue;
      var oldKeepOldAxis = _keepOldAxis;
      var horizontal = axis.Placement == AxisPlacement.Bottom || axis.Placement == AxisPlacement.Top;
      _keepOldAxis = true;

      if (value && horizontal)
      {
        MainHorizontalAxis = axis;
      }
      else if (value && !horizontal)
      {
        MainVerticalAxis = axis;
      }
      else if (!value && horizontal)
      {
        MainHorizontalAxis = null;
      }
      else if (!value && !horizontal)
      {
        MainVerticalAxis = null;
      }

      _keepOldAxis = oldKeepOldAxis;
    }
  }

  public bool NewLegendVisible
  {
    get => Legend.LegendVisible;
    set => Legend.LegendVisible = value;
  }

  private enum AxisType
  {
    Horizontal,
    Vertical
  }
}