using System.Reflection;
using System.Windows.Media.Imaging;

namespace Crystal.Plot2D;

public static class D3IconHelper
{
  private static BitmapFrame icon;
  public static BitmapFrame TheIcon
  {
    get
    {
      if (icon == null)
      {
        Assembly currentAssembly = typeof(D3IconHelper).Assembly;
        icon = BitmapFrame.Create(bitmapStream: currentAssembly.GetManifestResourceStream(name: "Crystal.Plot2D.Resources.D3-icon.ico"));
      }
      return icon;
    }
  }

  private static BitmapFrame whiteIcon;
  public static BitmapFrame WhiteIcon
  {
    get
    {
      if (whiteIcon == null)
      {
        Assembly currentAssembly = typeof(D3IconHelper).Assembly;
        whiteIcon = BitmapFrame.Create(bitmapStream: currentAssembly.GetManifestResourceStream(name: "Crystal.Plot2D.Resources.D3-icon-white.ico"));
      }

      return whiteIcon;
    }
  }
}
