using System.Windows;
using System.Windows.Media;
using Crystal.Plot2D.Common;

namespace Crystal.Plot2D.Navigation;

/// <inheritdoc />
/// <summary>
///  Base class for all navigation providers.
/// </summary>
public abstract class NavigationBase : Viewport2DElement
{
  protected NavigationBase()
  {
    ManualTranslate = true;
    ManualClip = true;
    Loaded += NavigationBase_Loaded;
  }

  private void NavigationBase_Loaded(object sender, RoutedEventArgs e)
  {
    OnLoaded();
  }

  private void OnLoaded()
  {
    // this call enables contextMenu to be shown after loading and
    // before any changes to Viewport - without this call 
    // context menu was not shown.
    InvalidateVisual();
  }

  protected override void OnRenderCore(DrawingContext dc, RenderState state)
  {
    dc.DrawRectangle(brush: Brushes.Transparent, pen: null, rectangle: state.Output);
  }
}
