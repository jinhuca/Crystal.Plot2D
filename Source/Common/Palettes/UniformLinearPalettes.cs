﻿using System.Windows.Media;

namespace Crystal.Plot2D.Common;

public static class UniformLinearPalettes
{
  static UniformLinearPalettes()
  {
    BlackAndWhitePalette.IncreaseBrightness = false;
    RedGreenBluePalette.IncreaseBrightness = false;
    BlueOrangePalette.IncreaseBrightness = false;
  }

  public static UniformLinearPalette BlackAndWhitePalette { get; } = new(colors: new[]
    { Colors.Black, Colors.White });
  public static UniformLinearPalette RedGreenBluePalette { get; } = new(colors: new[]
    { Colors.Blue, Color.FromRgb(r: 0, g: 255, b: 0), Colors.Red });
  public static UniformLinearPalette BlueOrangePalette { get; } = new(colors: new[]
    { Colors.Blue, Colors.Cyan, Colors.Yellow, Colors.Orange });
}
