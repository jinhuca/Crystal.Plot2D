﻿using System.Windows.Controls;
using Crystal.Plot2D.Descriptions;

namespace Crystal.Plot2D.Charts;

/// <summary>
///   <see cref="LegendItem"/> is a base class for item in legend, that represents some chart. 
/// </summary>
public abstract class LegendItem : ContentControl
{
  /// <summary>
  ///   Initializes a new instance of the <see cref="LegendItem"/> class.
  /// </summary>
  protected LegendItem() { }

  /// <summary>
  ///   Initializes a new instance of the <see cref="LegendItem"/> class.
  /// </summary>
  /// <param name="description">
  ///   The description.
  /// </param>
  protected LegendItem(Description description)
  {
    Description = description;
  }

  private Description description;

  /// <summary>
  ///   Gets or sets the description.
  /// </summary>
  /// <value>
  ///   The description.
  /// </value>
  public Description Description
  {
    get => description;
    set
    {
      description = value;
      Content = description;
    }
  }
}
