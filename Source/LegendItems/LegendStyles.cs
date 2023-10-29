using System;
using System.Windows;

namespace Crystal.Plot2D.LegendItems;

public static class LegendStyles
{
  private static Style _defaultStyle;

  public static Style Default
  {
    get
    {
      if (_defaultStyle == null)
      {
        var legendStyles_ = GetLegendStyles();
        _defaultStyle = (Style)legendStyles_[key: typeof(Legend)];
      }

      return _defaultStyle;
    }
  }

  private static Style _noScrollStyle;

  public static Style NoScroll
  {
    get
    {
      if (_noScrollStyle == null)
      {
        var legendStyles_ = GetLegendStyles();
        _noScrollStyle = (Style)legendStyles_[key: "NoScrollLegendStyle"];
      }

      return _noScrollStyle;
    }
  }

  private static ResourceDictionary GetLegendStyles()
  {
    var legendStyles_ = (ResourceDictionary)Application.LoadComponent(resourceLocator: new Uri(uriString: Constants.Constants.LegendResourceUri, uriKind: UriKind.Relative));
    return legendStyles_;
  }
}
