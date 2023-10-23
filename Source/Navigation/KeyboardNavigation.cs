using Crystal.Plot2D.Charts;
using Crystal.Plot2D.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Crystal.Plot2D;

///<summary>
/// Provides keyboard navigation around viewport of Plotter.
///</summary>
public class KeyboardNavigation : IPlotterElement
{
  ///<summary>
  /// Initializes a new instance of the <see cref="KeyboardNavigation"/> class.
  ///</summary>
  public KeyboardNavigation() { }

  private bool isReversed = true;
  /// <summary>
  /// Gets or sets a value indicating whether panning directions are reversed.
  /// </summary>
  /// <value>
  /// 	<c>true</c> if panning directions are reversed; otherwise, <c>false</c>.
  /// </value>
  public bool IsReversed
  {
    get => isReversed;
    set => isReversed = value;
  }

  private readonly List<CommandBinding> addedBindings = new();
  private void AddBinding(CommandBinding binding)
  {
    plotter2D.CommandBindings.Add(commandBinding: binding);
    addedBindings.Add(item: binding);
  }

  private void InitCommands()
  {
    if (plotter2D == null)
    {
      return;
    }

    var zoomOutToMouseCommandBinding = new CommandBinding(
      command: ChartCommands.ZoomOutToMouse,
      executed: ZoomOutToMouseExecute,
      canExecute: ZoomOutToMouseCanExecute);
    AddBinding(binding: zoomOutToMouseCommandBinding);

    var zoomInToMouseCommandBinding = new CommandBinding(
      command: ChartCommands.ZoomInToMouse,
      executed: ZoomInToMouseExecute,
      canExecute: ZoomInToMouseCanExecute);
    AddBinding(binding: zoomInToMouseCommandBinding);

    var zoomWithParamCommandBinding = new CommandBinding(
      command: ChartCommands.ZoomWithParameter,
      executed: ZoomWithParamExecute,
      canExecute: ZoomWithParamCanExecute);
    AddBinding(binding: zoomWithParamCommandBinding);

    var zoomInCommandBinding = new CommandBinding(
      command: ChartCommands.ZoomIn,
      executed: ZoomInExecute,
      canExecute: ZoomInCanExecute);
    AddBinding(binding: zoomInCommandBinding);

    var zoomOutCommandBinding = new CommandBinding(
      command: ChartCommands.ZoomOut,
      executed: ZoomOutExecute,
      canExecute: ZoomOutCanExecute);
    AddBinding(binding: zoomOutCommandBinding);

    var fitToViewCommandBinding = new CommandBinding(
      command: ChartCommands.FitToView,
      executed: FitToViewExecute,
      canExecute: FitToViewCanExecute);
    AddBinding(binding: fitToViewCommandBinding);

    var ScrollLeftCommandBinding = new CommandBinding(
        command: ChartCommands.ScrollLeft,
        executed: ScrollLeftExecute,
        canExecute: ScrollLeftCanExecute);
    AddBinding(binding: ScrollLeftCommandBinding);

    var ScrollRightCommandBinding = new CommandBinding(
      command: ChartCommands.ScrollRight,
      executed: ScrollRightExecute,
      canExecute: ScrollRightCanExecute);
    AddBinding(binding: ScrollRightCommandBinding);

    var ScrollUpCommandBinding = new CommandBinding(
      command: ChartCommands.ScrollUp,
      executed: ScrollUpExecute,
      canExecute: ScrollUpCanExecute);
    AddBinding(binding: ScrollUpCommandBinding);

    var ScrollDownCommandBinding = new CommandBinding(
      command: ChartCommands.ScrollDown,
      executed: ScrollDownExecute,
      canExecute: ScrollDownCanExecute);
    AddBinding(binding: ScrollDownCommandBinding);

    var SaveScreenshotCommandBinding = new CommandBinding(
      command: ChartCommands.SaveScreenshot,
      executed: SaveScreenshotExecute,
      canExecute: SaveScreenshotCanExecute);
    AddBinding(binding: SaveScreenshotCommandBinding);

    var CopyScreenshotCommandBinding = new CommandBinding(
      command: ChartCommands.CopyScreenshot,
      executed: CopyScreenshotExecute,
      canExecute: CopyScreenshotCanExecute);
    AddBinding(binding: CopyScreenshotCommandBinding);

    var ShowHelpCommandBinding = new CommandBinding(
      command: ChartCommands.ShowHelp,
      executed: ShowHelpExecute,
      canExecute: ShowHelpCanExecute);
    AddBinding(binding: ShowHelpCommandBinding);

    var UndoCommandBinding = new CommandBinding(
      command: ApplicationCommands.Undo,
      executed: UndoExecute,
      canExecute: UndoCanExecute);
    AddBinding(binding: UndoCommandBinding);

    var RedoCommandBinding = new CommandBinding(
      command: ApplicationCommands.Redo,
      executed: RedoExecute,
      canExecute: RedoCanExecute);
    AddBinding(binding: RedoCommandBinding);
  }

  #region Zoom Out To Mouse

  private void ZoomToPoint(double coeff)
  {
    Point pt = Mouse.GetPosition(relativeTo: plotter2D.CentralGrid);
    Point dataPoint = Viewport.Transform.ScreenToData(screenPoint: pt);
    DataRect visible = Viewport.Visible;

    Viewport.Visible = visible.Zoom(to: dataPoint, ratio: coeff);
  }

  private void ZoomOutToMouseExecute(object target, ExecutedRoutedEventArgs e)
  {
    ZoomToPoint(coeff: zoomOutCoeff);
    e.Handled = true;
  }

  private void ZoomOutToMouseCanExecute(object target, CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = true;
  }

  #endregion

  #region Zoom In To Mouse

  private void ZoomInToMouseExecute(object target, ExecutedRoutedEventArgs e)
  {
    ZoomToPoint(coeff: zoomInCoeff);
    e.Handled = true;
  }

  private void ZoomInToMouseCanExecute(object target, CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = true;
  }

  #endregion

  #region Zoom With param

  private void ZoomWithParamExecute(object target, ExecutedRoutedEventArgs e)
  {
    double zoomParam = (double)e.Parameter;
    plotter2D.Viewport.Zoom(factor: zoomParam);
    e.Handled = true;
  }

  private void ZoomWithParamCanExecute(object target, CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = true;
  }

  #endregion

  #region Zoom in

  private const double zoomInCoeff = 0.9;
  private void ZoomInExecute(object target, ExecutedRoutedEventArgs e)
  {
    Viewport.Zoom(factor: zoomInCoeff);
    e.Handled = true;
  }

  private void ZoomInCanExecute(object target, CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = true;
  }

  #endregion

  #region Zoom out

  private const double zoomOutCoeff = 1 / zoomInCoeff;
  private void ZoomOutExecute(object target, ExecutedRoutedEventArgs e)
  {
    Viewport.Zoom(factor: zoomOutCoeff);
    e.Handled = true;
  }

  private void ZoomOutCanExecute(object target, CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = true;
  }

  #endregion

  #region Fit to view

  private void FitToViewExecute(object target, ExecutedRoutedEventArgs e)
  {
    // todo сделать нормально.
    (Viewport as Viewport2D).FitToView();
    e.Handled = true;
  }

  private void FitToViewCanExecute(object target, CanExecuteRoutedEventArgs e)
  {
    // todo add a check if viewport is already fitted to view.
    e.CanExecute = true;
  }

  #endregion

  #region Scroll

  private readonly double scrollCoeff = 0.05;
  private void ScrollVisibleProportionally(double xShiftCoeff, double yShiftCoeff)
  {
    DataRect visible = Viewport.Visible;
    DataRect oldVisible = visible;
    double width = visible.Width;
    double height = visible.Height;

    double reverseCoeff = isReversed ? -1 : 1;
    visible.Offset(offsetX: reverseCoeff * xShiftCoeff * width, offsetY: reverseCoeff * yShiftCoeff * height);

    Viewport.Visible = visible;
    plotter2D.UndoProvider.AddAction(action: new DependencyPropertyChangedUndoAction(target: Viewport, property: Viewport2D.VisibleProperty, oldValue: oldVisible, newValue: visible));
  }

  #region ScrollLeft

  private void ScrollLeftExecute(object target, ExecutedRoutedEventArgs e)
  {
    ScrollVisibleProportionally(xShiftCoeff: scrollCoeff, yShiftCoeff: 0);
    e.Handled = true;
  }

  private void ScrollLeftCanExecute(object target, CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = true;
  }

  #endregion

  #region ScrollRight

  private void ScrollRightExecute(object target, ExecutedRoutedEventArgs e)
  {
    ScrollVisibleProportionally(xShiftCoeff: -scrollCoeff, yShiftCoeff: 0);
    e.Handled = true;
  }

  private void ScrollRightCanExecute(object target, CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = true;
  }

  #endregion

  #region ScrollUp

  private void ScrollUpExecute(object target, ExecutedRoutedEventArgs e)
  {
    ScrollVisibleProportionally(xShiftCoeff: 0, yShiftCoeff: -scrollCoeff);
    e.Handled = true;
  }

  private void ScrollUpCanExecute(object target, CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = true;
  }

  #endregion

  #region ScrollDown

  private void ScrollDownExecute(object target, ExecutedRoutedEventArgs e)
  {
    ScrollVisibleProportionally(xShiftCoeff: 0, yShiftCoeff: scrollCoeff);
    e.Handled = true;
  }

  private void ScrollDownCanExecute(object target, CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = true;
  }

  #endregion

  #endregion

  #region SaveScreenshot

  private void SaveScreenshotExecute(object target, ExecutedRoutedEventArgs e)
  {
    SaveFileDialog dlg = new();
    dlg.Filter = "PNG (*.png)|*.png|JPEG (*.jpg)|*.jpg|BMP (*.bmp)|*.bmp|GIF (*.gif)|*.gif";
    dlg.FilterIndex = 1;
    dlg.AddExtension = true;
    if (dlg.ShowDialog().GetValueOrDefault(defaultValue: false))
    {
      string filePath = dlg.FileName;
      plotter2D.SaveScreenshot(filePath: filePath);
      e.Handled = true;
    }
  }

  private void SaveScreenshotCanExecute(object target, CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = true;
  }

  #endregion

  #region CopyScreenshot

  private void CopyScreenshotExecute(object target, ExecutedRoutedEventArgs e)
  {
    plotter2D.CopyScreenshotToClipboard();
    e.Handled = true;
  }

  private void CopyScreenshotCanExecute(object target, CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = true;
  }

  #endregion

  #region ShowHelp

  private bool aboutWindowOpened;
  private void ShowHelpExecute(object target, ExecutedRoutedEventArgs e)
  {
    if (!aboutWindowOpened)
    {
      AboutWindow window = new();
      window.Closed += aboutWindow_Closed;
      window.DataContext = plotter2D;

      window.Owner = Window.GetWindow(dependencyObject: plotter2D);

      aboutWindowOpened = true;
      window.Show();

      e.Handled = true;
    }
  }

  void aboutWindow_Closed(object sender, EventArgs e)
  {
    Window window = (Window)sender;
    window.Closed -= aboutWindow_Closed;
    aboutWindowOpened = false;
  }

  private void ShowHelpCanExecute(object target, CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = !aboutWindowOpened;
  }

  #endregion

  #region Undo

  private void UndoExecute(object target, ExecutedRoutedEventArgs e)
  {
    plotter2D.UndoProvider.Undo();
    e.Handled = true;
  }

  private void UndoCanExecute(object target, CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = plotter2D.UndoProvider.CanUndo;
  }

  #endregion

  #region Redo

  private void RedoExecute(object target, ExecutedRoutedEventArgs e)
  {
    plotter2D.UndoProvider.Redo();
    e.Handled = true;
  }

  private void RedoCanExecute(object target, CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = plotter2D.UndoProvider.CanRedo;
  }

  #endregion

  #region IPlotterElement Members

  private Viewport2D Viewport => plotter2D.Viewport;

  private PlotterBase plotter2D;
  void IPlotterElement.OnPlotterAttached(PlotterBase plotter)
  {
    plotter2D = (PlotterBase)plotter;

    InitCommands();
  }

  void IPlotterElement.OnPlotterDetaching(PlotterBase plotter)
  {
    foreach (var commandBinding in addedBindings)
    {
      plotter.CommandBindings.Remove(commandBinding: commandBinding);
    }
    addedBindings.Clear();

    plotter2D = null;
  }

  PlotterBase IPlotterElement.Plotter => plotter2D;

  #endregion
}
