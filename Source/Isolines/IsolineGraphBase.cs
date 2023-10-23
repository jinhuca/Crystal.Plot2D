using Crystal.Plot2D.Common;
using Crystal.Plot2D.DataSources;
using System;
using System.Windows;
using DataSource = Crystal.Plot2D.DataSources.IDataSource2D<double>;

namespace Crystal.Plot2D.Charts;

public abstract class IsolineGraphBase : ContentGraph
{
  protected IsolineGraphBase() { }

  private IsolineCollection collection = new();
  protected IsolineCollection Collection
  {
    get => collection;
    set => collection = value;
  }

  private readonly IsolineBuilder isolineBuilder = new();
  protected IsolineBuilder IsolineBuilder => isolineBuilder;

  private readonly IsolineTextAnnotater annotater = new();
  protected IsolineTextAnnotater Annotater => annotater;

  #region Properties

  #region IsolineCollection property

  public IsolineCollection IsolineCollection
  {
    get => (IsolineCollection)GetValue(dp: IsolineCollectionProperty);
    set => SetValue(dp: IsolineCollectionProperty, value: value);
  }

  public static readonly DependencyProperty IsolineCollectionProperty = DependencyProperty.Register(
    name: nameof(IsolineCollection),
    propertyType: typeof(IsolineCollection),
    ownerType: typeof(IsolineGraphBase),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: null, flags: FrameworkPropertyMetadataOptions.Inherits));

  #endregion // end of IsolineCollection property

  #region WayBeforeTextMultiplier

  public double WayBeforeTextMultiplier
  {
    get => (double)GetValue(dp: WayBeforeTextMultiplierProperty);
    set => SetValue(dp: WayBeforeTextMultiplierProperty, value: value);
  }

  public static readonly DependencyProperty WayBeforeTextMultiplierProperty = DependencyProperty.Register(
    name: "WayBeforeTextCoeff",
    propertyType: typeof(double),
    ownerType: typeof(IsolineGraphBase),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: 1.0, flags: FrameworkPropertyMetadataOptions.Inherits, propertyChangedCallback: OnIsolinePropertyChanged));

  #endregion // end of WayBeforeTextCoeff

  private static void OnIsolinePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    // todo do smth here
  }

  #region Palette property

  public IPalette Palette
  {
    get => (IPalette)GetValue(dp: PaletteProperty);
    set => SetValue(dp: PaletteProperty, value: value);
  }

  public static readonly DependencyProperty PaletteProperty = DependencyProperty.Register(
    name: nameof(Palette),
    propertyType: typeof(IPalette),
    ownerType: typeof(IsolineGraphBase),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: new HsbPalette(), flags: FrameworkPropertyMetadataOptions.Inherits, propertyChangedCallback: OnIsolinePropertyChanged), validateValueCallback: ValidatePalette);

  private static bool ValidatePalette(object value)
  {
    return value != null;
  }

  #endregion // end of Palette property

  #region DataSource property

  public DataSource DataSource
  {
    get => (DataSource)GetValue(dp: DataSourceProperty);
    set => SetValue(dp: DataSourceProperty, value: value);
  }

  public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register(
    name: nameof(DataSource),
    propertyType: typeof(DataSource),
    ownerType: typeof(IsolineGraphBase),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: null, flags: FrameworkPropertyMetadataOptions.Inherits, propertyChangedCallback: OnDataSourceChanged));

  private static void OnDataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    IsolineGraphBase owner = (IsolineGraphBase)d;
    owner.OnDataSourceChanged(prevDataSource: (DataSource)e.OldValue, currDataSource: (DataSource)e.NewValue);
  }

  protected virtual void OnDataSourceChanged(IDataSource2D<double> prevDataSource, IDataSource2D<double> currDataSource)
  {
    if (prevDataSource != null)
    {
      prevDataSource.Changed -= OnDataSourceChanged;
    }

    if (currDataSource != null)
    {
      currDataSource.Changed += OnDataSourceChanged;
    }

    UpdateDataSource();
    CreateUIRepresentation();

    RaiseEvent(e: new RoutedEventArgs(routedEvent: BackgroundRenderer.UpdateRequested));
  }

  #endregion // end of DataSource property

  #region DrawLabels property

  public bool DrawLabels
  {
    get => (bool)GetValue(dp: DrawLabelsProperty);
    set => SetValue(dp: DrawLabelsProperty, value: value);
  }

  public static readonly DependencyProperty DrawLabelsProperty = DependencyProperty.Register(
      name: nameof(DrawLabels),
      propertyType: typeof(bool),
      ownerType: typeof(IsolineGraphBase),
      typeMetadata: new FrameworkPropertyMetadata(defaultValue: true, flags: FrameworkPropertyMetadataOptions.Inherits, propertyChangedCallback: OnIsolinePropertyChanged));

  #endregion // end of DrawLabels property

  #region LabelStringFormat

  public string LabelStringFormat
  {
    get => (string)GetValue(dp: LabelStringFormatProperty);
    set => SetValue(dp: LabelStringFormatProperty, value: value);
  }

  public static readonly DependencyProperty LabelStringFormatProperty = DependencyProperty.Register(
    name: nameof(LabelStringFormat),
    propertyType: typeof(string),
    ownerType: typeof(IsolineGraphBase),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: "F", flags: FrameworkPropertyMetadataOptions.Inherits, propertyChangedCallback: OnIsolinePropertyChanged));

  #endregion // end of LabelStringFormat

  #region UseBezierCurves

  public bool UseBezierCurves
  {
    get => (bool)GetValue(dp: UseBezierCurvesProperty);
    set => SetValue(dp: UseBezierCurvesProperty, value: value);
  }

  public static readonly DependencyProperty UseBezierCurvesProperty = DependencyProperty.Register(
    name: nameof(UseBezierCurves),
    propertyType: typeof(bool),
    ownerType: typeof(IsolineGraphBase),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: false, flags: FrameworkPropertyMetadataOptions.Inherits));

  #endregion // end of UseBezierCurves

  public double LabelsScaling
  {
    get => (double)GetValue(dp: LabelsScalingProperty);
    set => SetValue(dp: LabelsScalingProperty, value: value);
  }

  public static readonly DependencyProperty LabelsScalingProperty = DependencyProperty.Register(
    name: nameof(LabelsScaling),
    propertyType: typeof(double),
    ownerType: typeof(IsolineGraphBase),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: 1.0, flags: FrameworkPropertyMetadataOptions.Inherits));


  #endregion // end of Properties

  #region DataSource

  //private DataSource dataSource = null;
  ///// <summary>
  ///// Gets or sets the data source.
  ///// </summary>
  ///// <value>The data source.</value>
  //public DataSource DataSource
  //{
  //    get { return dataSource; }
  //    set
  //    {
  //        if (dataSource != value)
  //        {
  //            DetachDataSource(dataSource);
  //            dataSource = value;
  //            AttachDataSource(dataSource);

  //            UpdateDataSource();
  //        }
  //    }
  //}

  #region MissineValue property

  public double MissingValue
  {
    get => (double)GetValue(dp: MissingValueProperty);
    set => SetValue(dp: MissingValueProperty, value: value);
  }

  public static readonly DependencyProperty MissingValueProperty = DependencyProperty.Register(
    name: nameof(MissingValue),
    propertyType: typeof(double),
    ownerType: typeof(IsolineGraphBase),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: double.NaN, flags: FrameworkPropertyMetadataOptions.Inherits, propertyChangedCallback: OnMissingValueChanged));

  private static void OnMissingValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    IsolineGraphBase owner = (IsolineGraphBase)d;
    owner.UpdateDataSource();
  }

  #endregion // end of MissineValue property

  public void SetDataSource(DataSource dataSource, double missingValue)
  {
    DataSource = dataSource;
    MissingValue = missingValue;

    UpdateDataSource();
  }

  /// <summary>
  /// This method is called when data source changes.
  /// </summary>
  protected virtual void UpdateDataSource()
  {
  }

  protected virtual void CreateUIRepresentation() { }

  protected virtual void OnDataSourceChanged(object sender, EventArgs e)
  {
    UpdateDataSource();
  }

  #endregion

  #region StrokeThickness

  /// <summary>
  /// Gets or sets thickness of isoline lines.
  /// </summary>
  /// <value>The stroke thickness.</value>
  public double StrokeThickness
  {
    get => (double)GetValue(dp: StrokeThicknessProperty);
    set => SetValue(dp: StrokeThicknessProperty, value: value);
  }

  /// <summary>
  /// Identifies the StrokeThickness dependency property.
  /// </summary>
  public static readonly DependencyProperty StrokeThicknessProperty =
      DependencyProperty.Register(
        name: nameof(StrokeThickness),
        propertyType: typeof(double),
        ownerType: typeof(IsolineGraphBase),
        typeMetadata: new FrameworkPropertyMetadata(defaultValue: 2.0, propertyChangedCallback: OnLineThicknessChanged));

  private static void OnLineThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    IsolineGraphBase graph = (IsolineGraphBase)d;
    graph.OnLineThicknessChanged();
  }

  protected virtual void OnLineThicknessChanged() { }

  #endregion
}
