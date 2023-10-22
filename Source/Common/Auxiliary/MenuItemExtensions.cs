﻿using System.Linq;
using System.Windows.Controls;

namespace Crystal.Plot2D.Common;

internal static class MenuItemExtensions
{
  public static MenuItem FindChildByHeader(this MenuItem parent, string header)
  {
    return parent.Items.OfType<MenuItem>().FirstOrDefault(subMenu => subMenu.Header.ToString() == header);
  }
}
