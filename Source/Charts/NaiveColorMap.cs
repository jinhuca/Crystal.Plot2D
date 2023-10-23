using Crystal.Plot2D.Common;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Crystal.Plot2D.Charts;

public class NaiveColorMap
{
  public double[,] Data { get; set; }

  public IPalette Palette { get; set; }

  public BitmapSource BuildImage()
  {
    if (Data == null)
    {
      throw new ArgumentNullException(paramName: "Data");
    }
    if (Palette == null)
    {
      throw new ArgumentNullException(paramName: "Palette");
    }

    int width = Data.GetLength(dimension: 0);
    int height = Data.GetLength(dimension: 1);

    int[] pixels = new int[width * height];
    var minMax = Data.GetMinMax();
    var min = minMax.Min;
    var rangeDelta = minMax.GetLength();

    int pointer = 0;
    for (int iy = 0; iy < height; iy++)
    {
      for (int ix = 0; ix < width; ix++)
      {
        double value = Data[ix, height - 1 - iy];
        double ratio = (value - min) / rangeDelta;
        Color color = Palette.GetColor(t: ratio);
        int argb = color.ToArgb();
        pixels[pointer++] = argb;
      }
    }

    WriteableBitmap bitmap = new(pixelWidth: width, pixelHeight: height, dpiX: 96, dpiY: 96, pixelFormat: PixelFormats.Pbgra32, palette: null);
    int bpp = (bitmap.Format.BitsPerPixel + 7) / 8;
    int stride = bitmap.PixelWidth * bpp;
    bitmap.WritePixels(sourceRect: new Int32Rect(x: 0, y: 0, width: width, height: height), pixels: pixels, stride: stride, offset: 0);
    return bitmap;
  }
}