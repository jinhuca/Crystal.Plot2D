using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Crystal.Plot2D.Charts;

public abstract class AxisControlBase : ContentControl
{
  #region Properties

  public HorizontalAlignment LabelsHorizontalAlignment
  {
    get => (HorizontalAlignment)GetValue(dp: LabelsHorizontalAlignmentProperty);
    set => SetValue(dp: LabelsHorizontalAlignmentProperty, value: value);
  }

  public static readonly DependencyProperty LabelsHorizontalAlignmentProperty = DependencyProperty.Register(
    name: nameof(LabelsHorizontalAlignment),
    propertyType: typeof(HorizontalAlignment),
    ownerType: typeof(AxisControlBase),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: HorizontalAlignment.Center));


  public VerticalAlignment LabelsVerticalAlignment
  {
    get => (VerticalAlignment)GetValue(dp: LabelsVerticalAlignmentProperty);
    set => SetValue(dp: LabelsVerticalAlignmentProperty, value: value);
  }

  public static readonly DependencyProperty LabelsVerticalAlignmentProperty = DependencyProperty.Register(
    name: nameof(LabelsVerticalAlignment),
    propertyType: typeof(VerticalAlignment),
    ownerType: typeof(AxisControlBase),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: VerticalAlignment.Center));

  public abstract Path TicksPath { get; }

  #endregion // end of Properties
}
