using System.Windows.Input;

namespace Crystal.Plot2D;

// TODO: probably optimize memory usage by replacing implicit creation of 
// all commands on first usage of this class - 
// create each command on first access directly to it.

/// <summary>Common chart plotter commands</summary>
public static class ChartCommands
{

  #region Auxiliary code for creation of commands

  private static RoutedUICommand CreateCommand(string name)
  {
    return new RoutedUICommand(text: name, name: name, ownerType: typeof(ChartCommands));
  }

  private static RoutedUICommand CreateCommand(string name, params Key[] keys)
  {
    InputGestureCollection gestures = new();
    foreach (var key in keys)
    {
      gestures.Add(inputGesture: new KeyGesture(key: key));
    }
    return new RoutedUICommand(text: name, name: name, ownerType: typeof(ChartCommands), inputGestures: gestures);
  }

  private static RoutedUICommand CreateCommand(string name, MouseAction mouseAction)
  {
    return new RoutedUICommand(text: name, name: name, ownerType: typeof(ChartCommands), inputGestures: new InputGestureCollection { new MouseGesture(mouseAction: mouseAction) });
  }

  private static RoutedUICommand CreateCommand(string name, params InputGesture[] gestures)
  {
    return new RoutedUICommand(text: name, name: name, ownerType: typeof(ChartCommands), inputGestures: new InputGestureCollection(inputGestures: gestures));
  }

  #endregion

  private static readonly RoutedUICommand zoomOutToMouse = CreateCommand(name: "ZoomOutToMouse", mouseAction: MouseAction.RightDoubleClick);
  /// <summary>
  /// Gets the value that represents the Zoom Out To Mouse command.
  /// </summary>
  public static RoutedUICommand ZoomOutToMouse => zoomOutToMouse;

  private static readonly RoutedUICommand zoomInToMouse = CreateCommand(name: "ZoomInToMouse", mouseAction: MouseAction.LeftDoubleClick);
  /// <summary>
  /// Gets the value that represents the zoom in to mouse command.
  /// </summary>
  /// <value></value>
  public static RoutedUICommand ZoomInToMouse => zoomInToMouse;

  private static readonly RoutedUICommand zoomWithParam = CreateCommand(name: "ZoomWithParam");
  /// <summary>
  /// Gets the value that represents the zoom with parameter command.
  /// </summary>
  /// <value></value>
  public static RoutedUICommand ZoomWithParameter => zoomWithParam;

  private static readonly RoutedUICommand zoomIn = CreateCommand(name: "ZoomIn", keys: Key.OemPlus);
  /// <summary>
  /// Gets the value that represents the zoom in command.
  /// </summary>
  /// <value></value>
  public static RoutedUICommand ZoomIn => zoomIn;

  private static readonly RoutedUICommand zoomOut = CreateCommand(name: "ZoomOut", keys: Key.OemMinus);
  /// <summary>
  /// Gets the value that represents the zoom out command.
  /// </summary>
  /// <value></value>
  public static RoutedUICommand ZoomOut => zoomOut;

  private static readonly RoutedUICommand fitToView = CreateCommand(name: "FitToView", keys: Key.Home);
  /// <summary>
  /// Gets the value that represents the fit to view command.
  /// </summary>
  /// <value></value>
  public static RoutedUICommand FitToView => fitToView;

  private static readonly RoutedUICommand scrollLeft = CreateCommand(name: "ScrollLeft", keys: Key.Left);
  /// <summary>
  /// Gets the value that represents the scroll left command.
  /// </summary>
  /// <value></value>
  public static RoutedUICommand ScrollLeft => scrollLeft;

  private static readonly RoutedUICommand scrollRight = CreateCommand(name: "ScrollRight", keys: Key.Right);
  /// <summary>
  /// Gets the value that represents the scroll right command.
  /// </summary>
  /// <value></value>
  public static RoutedUICommand ScrollRight => scrollRight;

  private static readonly RoutedUICommand scrollUp = CreateCommand(name: "ScrollUp", keys: Key.Up);
  /// <summary>
  /// Gets the value that represents the scroll up command.
  /// </summary>
  /// <value></value>
  public static RoutedUICommand ScrollUp => scrollUp;

  private static readonly RoutedUICommand scrollDown = CreateCommand(name: "ScrollDown", keys: Key.Down);
  /// <summary>
  /// Gets the value that represents the scroll down command.
  /// </summary>
  /// <value></value>
  public static RoutedUICommand ScrollDown => scrollDown;

  private static readonly RoutedUICommand saveScreenshot = CreateCommand(name: "SaveScreenshot", gestures: new KeyGesture(key: Key.S, modifiers: ModifierKeys.Control));
  /// <summary>
  /// Gets the value that represents the save screenshot command.
  /// </summary>
  /// <value></value>
  public static RoutedUICommand SaveScreenshot => saveScreenshot;

  private static readonly RoutedUICommand copyScreenshot = CreateCommand(name: "CopyScreenshot", keys: Key.F11);
  /// <summary>
  /// Gets the value that represents the copy screenshot command.
  /// </summary>
  /// <value></value>
  public static RoutedUICommand CopyScreenshot => copyScreenshot;

  private static readonly RoutedUICommand showHelp = CreateCommand(name: "ShowHelp", keys: Key.F1);
  /// <summary>
  /// Gets the value that represents the show help command.
  /// </summary>
  /// <value></value>
  public static RoutedUICommand ShowHelp => showHelp;
}
