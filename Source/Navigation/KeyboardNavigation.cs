using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Common.UndoSystem;
using Microsoft.Win32;

namespace Crystal.Plot2D.Navigation;

///<summary>
/// Provides keyboard navigation around viewport of Plotter.
///</summary>
public sealed class KeyboardNavigation : IPlotterElement
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

    var zoomOutToMouseCommandBinding_ = new CommandBinding(
      command: ChartCommands.ZoomOutToMouse,
      executed: ZoomOutToMouseExecute,
      canExecute: ZoomOutToMouseCanExecute);
    AddBinding(binding: zoomOutToMouseCommandBinding_);

    var zoomInToMouseCommandBinding_ = new CommandBinding(
      command: ChartCommands.ZoomInToMouse,
      executed: ZoomInToMouseExecute,
      canExecute: ZoomInToMouseCanExecute);
    AddBinding(binding: zoomInToMouseCommandBinding_);

    var zoomWithParamCommandBinding_ = new CommandBinding(
      command: ChartCommands.ZoomWithParameter,
      executed: ZoomWithParamExecute,
      canExecute: ZoomWithParamCanExecute);
    AddBinding(binding: zoomWithParamCommandBinding_);

    var zoomInCommandBinding_ = new CommandBinding(
      command: ChartCommands.ZoomIn,
      executed: ZoomInExecute,
      canExecute: ZoomInCanExecute);
    AddBinding(binding: zoomInCommandBinding_);

    var zoomOutCommandBinding_ = new CommandBinding(
      command: ChartCommands.ZoomOut,
      executed: ZoomOutExecute,
      canExecute: ZoomOutCanExecute);
    AddBinding(binding: zoomOutCommandBinding_);

    var fitToViewCommandBinding_ = new CommandBinding(
      command: ChartCommands.FitToView,
      executed: FitToViewExecute,
      canExecute: FitToViewCanExecute);
    AddBinding(binding: fitToViewCommandBinding_);

    var scrollLeftCommandBinding_ = new CommandBinding(
        command: ChartCommands.ScrollLeft,
        executed: ScrollLeftExecute,
        canExecute: ScrollLeftCanExecute);
    AddBinding(binding: scrollLeftCommandBinding_);

    var scrollRightCommandBinding_ = new CommandBinding(
      command: ChartCommands.ScrollRight,
      executed: ScrollRightExecute,
      canExecute: ScrollRightCanExecute);
    AddBinding(binding: scrollRightCommandBinding_);

    var scrollUpCommandBinding_ = new CommandBinding(
      command: ChartCommands.ScrollUp,
      executed: ScrollUpExecute,
      canExecute: ScrollUpCanExecute);
    AddBinding(binding: scrollUpCommandBinding_);

    var scrollDownCommandBinding_ = new CommandBinding(
      command: ChartCommands.ScrollDown,
      executed: ScrollDownExecute,
      canExecute: ScrollDownCanExecute);
    AddBinding(binding: scrollDownCommandBinding_);

    var saveScreenshotCommandBinding_ = new CommandBinding(
      command: ChartCommands.SaveScreenshot,
      executed: SaveScreenshotExecute,
      canExecute: SaveScreenshotCanExecute);
    AddBinding(binding: saveScreenshotCommandBinding_);

    var copyScreenshotCommandBinding_ = new CommandBinding(
      command: ChartCommands.CopyScreenshot,
      executed: CopyScreenshotExecute,
      canExecute: CopyScreenshotCanExecute);
    AddBinding(binding: copyScreenshotCommandBinding_);

    var showHelpCommandBinding_ = new CommandBinding(
      command: ChartCommands.ShowHelp,
      executed: ShowHelpExecute,
      canExecute: ShowHelpCanExecute);
    AddBinding(binding: showHelpCommandBinding_);

    var undoCommandBinding_ = new CommandBinding(
      command: ApplicationCommands.Undo,
      executed: UndoExecute,
      canExecute: UndoCanExecute);
    AddBinding(binding: undoCommandBinding_);

    var redoCommandBinding_ = new CommandBinding(
      command: ApplicationCommands.Redo,
      executed: RedoExecute,
      canExecute: RedoCanExecute);
    AddBinding(binding: redoCommandBinding_);
  }

  #region Zoom Out To Mouse

  private void ZoomToPoint(double coeff)
  {
    var pt_ = Mouse.GetPosition(relativeTo: plotter2D.CentralGrid);
    var dataPoint_ = Viewport.Transform.ScreenToData(screenPoint: pt_);
    var visible_ = Viewport.Visible;

    Viewport.Visible = visible_.Zoom(to: dataPoint_, ratio: coeff);
  }

  private void ZoomOutToMouseExecute(object target, ExecutedRoutedEventArgs e)
  {
    ZoomToPoint(coeff: ZoomOutCoeff);
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
    ZoomToPoint(coeff: ZoomInCoeff);
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
    var zoomParam_ = (double)e.Parameter;
    plotter2D.Viewport.Zoom(factor: zoomParam_);
    e.Handled = true;
  }

  private void ZoomWithParamCanExecute(object target, CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = true;
  }

  #endregion

  #region Zoom in

  private const double ZoomInCoeff = 0.9;
  private void ZoomInExecute(object target, ExecutedRoutedEventArgs e)
  {
    Viewport.Zoom(factor: ZoomInCoeff);
    e.Handled = true;
  }

  private void ZoomInCanExecute(object target, CanExecuteRoutedEventArgs e)
  {
    e.CanExecute = true;
  }

  #endregion

  #region Zoom out

  private const double ZoomOutCoeff = 1 / ZoomInCoeff;

  private void ZoomOutExecute(object target, ExecutedRoutedEventArgs e)
  {
    Viewport.Zoom(factor: ZoomOutCoeff);
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
    // todo: do it right.
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

  private const double ScrollCoeff = 0.05;

  private void ScrollVisibleProportionally(double xShiftCoeff, double yShiftCoeff)
  {
    var visible_ = Viewport.Visible;
    var oldVisible_ = visible_;
    var width_ = visible_.Width;
    var height_ = visible_.Height;

    double reverseCoeff_ = isReversed ? -1 : 1;
    visible_.Offset(offsetX: reverseCoeff_ * xShiftCoeff * width_, offsetY: reverseCoeff_ * yShiftCoeff * height_);

    Viewport.Visible = visible_;
    plotter2D.UndoProvider.AddAction(action: new DependencyPropertyChangedUndoAction(target: Viewport, property: Viewport2D.VisibleProperty, oldValue: oldVisible_, newValue: visible_));
  }

  #region ScrollLeft

  private void ScrollLeftExecute(object target, ExecutedRoutedEventArgs e)
  {
    ScrollVisibleProportionally(xShiftCoeff: ScrollCoeff, yShiftCoeff: 0);
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
    ScrollVisibleProportionally(xShiftCoeff: -ScrollCoeff, yShiftCoeff: 0);
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
    ScrollVisibleProportionally(xShiftCoeff: 0, yShiftCoeff: -ScrollCoeff);
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
    ScrollVisibleProportionally(xShiftCoeff: 0, yShiftCoeff: ScrollCoeff);
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
    SaveFileDialog dlg_ = new();
    dlg_.Filter = "PNG (*.png)|*.png|JPEG (*.jpg)|*.jpg|BMP (*.bmp)|*.bmp|GIF (*.gif)|*.gif";
    dlg_.FilterIndex = 1;
    dlg_.AddExtension = true;
    if (dlg_.ShowDialog().GetValueOrDefault(defaultValue: false))
    {
      var filePath_ = dlg_.FileName;
      plotter2D.SaveScreenShot(filePath: filePath_);
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
      AboutWindow window_ = new();
      window_.Closed += aboutWindow_Closed;
      window_.DataContext = plotter2D;

      window_.Owner = Window.GetWindow(dependencyObject: plotter2D);

      aboutWindowOpened = true;
      window_.Show();

      e.Handled = true;
    }
  }

  private void aboutWindow_Closed(object sender, EventArgs e)
  {
    var window_ = (Window)sender;
    window_.Closed -= aboutWindow_Closed;
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
    foreach (var commandBinding_ in addedBindings)
    {
      plotter.CommandBindings.Remove(commandBinding: commandBinding_);
    }
    addedBindings.Clear();

    plotter2D = null;
  }

  PlotterBase IPlotterElement.Plotter => plotter2D;

  #endregion
}
