using System;
using System.Windows;

namespace Crystal.Plot2D.LegendItems;

public static class LegendStyles
{
  private static Style defaultStyle;

  public static Style Default
  {
    get
    {
      if (defaultStyle == null)
      {
        var legendStyles = GetLegendStyles();
        defaultStyle = (Style)legendStyles[key: typeof(Legend)];
      }

      return defaultStyle;
    }
  }

  private static Style noScrollStyle;
  public static Style NoScroll
  {
    get
    {
      if (noScrollStyle == null)
      {
        var legendStyles = GetLegendStyles();
        noScrollStyle = (Style)legendStyles[key: "NoScrollLegendStyle"];
      }

      return noScrollStyle;
    }
  }

  private static ResourceDictionary GetLegendStyles()
  {
    var legendStyles = (ResourceDictionary)Application.LoadComponent(resourceLocator: new Uri(uriString: Constants.Constants.LegendResourceUri, uriKind: UriKind.Relative));
    return legendStyles;
  }
}
