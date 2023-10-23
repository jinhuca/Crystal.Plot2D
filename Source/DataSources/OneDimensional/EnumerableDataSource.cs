using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace Crystal.Plot2D.DataSources;

public class EnumerableDataSource<T> : EnumerableDataSourceBase<T>
{
  public EnumerableDataSource(IEnumerable<T> data) : base(data: data) { }
  public EnumerableDataSource(IEnumerable data) : base(data: data) { }

  #region Property XYMapping

  private Func<T, Point> _xyMapping;
  public Func<T, Point> XYMapping
  {
    get => _xyMapping;
    set
    {
      _xyMapping = value ?? throw new ArgumentNullException(paramName: nameof(XYMapping));
      RaiseDataChanged();
    }
  }

  #endregion Property XYMapping

  #region Property XMapping

  private Func<T, double> _xMapping;

  [NotNull]
  public Func<T, double> XMapping
  {
    get => _xMapping;
    set
    {
      _xMapping = value ?? throw new ArgumentNullException(paramName: nameof(XMapping));
      RaiseDataChanged();
    }
  }

  #endregion Property XMapping

  #region Property YMapping

  private Func<T, double> _yMapping;

  [NotNull]
  public Func<T, double> YMapping
  {
    get => _yMapping;
    set
    {
      _yMapping = value ?? throw new ArgumentNullException(paramName: nameof(YMapping));
      RaiseDataChanged();
    }
  }

  #endregion Property YMapping

  /// <summary>
  /// Method to apply a mapping from a object value to a DependencyProperty of DependencyObject.
  /// </summary>
  /// <param name="property">DependencyProperty</param>
  /// <param name="mapping">mapping from value to dp</param>
  public void AddMapping(DependencyProperty property, Func<T, object> mapping)
  {
    if (property == null)
    {
      throw new ArgumentNullException(paramName: nameof(property));
    }
    if (mapping == null)
    {
      throw new ArgumentNullException(paramName: nameof(mapping));
    }
    Mappings.Add(item: new Mapping<T> { Property = property, F = mapping });
  }

  public override IPointEnumerator GetEnumerator(DependencyObject context) => new EnumerablePointEnumerator<T>(_dataSource: this);

  internal List<Mapping<T>> Mappings { get; } = new();

  internal void FillPoint(T elem, ref Point point)
  {
    if (XYMapping != null)
    {
      point = XYMapping(arg: elem);
    }
    else
    {
      if (XMapping != null)
      {
        point.X = XMapping(arg: elem);
      }
      if (YMapping != null)
      {
        point.Y = YMapping(arg: elem);
      }
    }
  }

  internal void ApplyMappings(DependencyObject target, T elem)
  {
    if (target != null)
    {
      foreach (var mapping in Mappings)
      {
        target.SetValue(dp: mapping.Property, value: mapping.F(arg: elem));
      }
    }
  }
}