using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace Crystal.Plot2D.Navigation;

internal sealed partial class AboutWindow
{
  public AboutWindow()
  {
    InitializeComponent();
  }

  private void Hyperlink_Click(object sender, RoutedEventArgs e)
  {
    var source = (Hyperlink)sender;
    Process.Start(fileName: source.NavigateUri.ToString());
  }

  private void Window_KeyDown(object sender, KeyEventArgs e)
  {
    // close on Esc or Enter pressed
    if (e.Key == Key.Escape || e.Key == Key.Enter)
    {
      Close();
    }
  }

  private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
  {
    var source = (Hyperlink)sender;
    Process.Start(fileName: source.NavigateUri.ToString());
  }
}
