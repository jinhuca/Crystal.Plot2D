using System.Reflection;
using System.Windows.Media.Imaging;

namespace Crystal.Plot2D
{
  public static class D3IconHelper
  {
    private static BitmapFrame icon = null;
    public static BitmapFrame TheIcon
    {
      get
      {
        if (icon == null)
        {
          Assembly currentAssembly = typeof(D3IconHelper).Assembly;
          icon = BitmapFrame.Create(currentAssembly.GetManifestResourceStream("Crystal.Plot2D.Resources.D3-icon.ico"));
        }
        return icon;
      }
    }

    private static BitmapFrame whiteIcon = null;
    public static BitmapFrame WhiteIcon
    {
      get
      {
        if (whiteIcon == null)
        {
          Assembly currentAssembly = typeof(D3IconHelper).Assembly;
          whiteIcon = BitmapFrame.Create(currentAssembly.GetManifestResourceStream("Crystal.Plot2D.Resources.D3-icon-white.ico"));
        }

        return whiteIcon;
      }
    }
  }
}
