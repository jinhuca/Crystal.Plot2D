using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace Crystal.Plot2D.Common;

public class UndoProvider : INotifyPropertyChanged
{
  public UndoProvider()
  {
    UndoStack.IsEmptyChanged += OnUndoStackIsEmptyChanged;
    RedoStack.IsEmptyChanged += OnRedoStackIsEmptyChanged;
  }
  public bool IsEnabled { get; set; } = true;

  private void OnUndoStackIsEmptyChanged(object sender, EventArgs e)
  {
    PropertyChanged.Raise(sender: this, propertyName: "CanUndo");
  }

  private void OnRedoStackIsEmptyChanged(object sender, EventArgs e)
  {
    PropertyChanged.Raise(sender: this, propertyName: "CanRedo");
  }

  public void AddAction(UndoAction action)
  {
    if (!IsEnabled)
    {
      return;
    }

    if (State != UndoState.None)
    {
      return;
    }

    UndoStack.Push(action: action);
    RedoStack.Clear();
  }

  public void Undo()
  {
    var action = UndoStack.Pop();
    RedoStack.Push(action: action);

    State = UndoState.Undoing;
    try
    {
      action.Undo();
    }
    finally
    {
      State = UndoState.None;
    }
  }

  public void Redo()
  {
    var action = RedoStack.Pop();
    UndoStack.Push(action: action);

    State = UndoState.Redoing;
    try
    {
      action.Do();
    }
    finally
    {
      State = UndoState.None;
    }
  }

  public bool CanUndo => !UndoStack.IsEmpty;
  public bool CanRedo => !RedoStack.IsEmpty;
  public UndoState State { get; private set; } = UndoState.None;
  public ActionStack UndoStack { get; } = new();
  public ActionStack RedoStack { get; } = new();

  private Dictionary<CaptureKeyHolder, object> CaptureHolders { get; } = new();

  #region INotifyPropertyChanged Members

  public event PropertyChangedEventHandler PropertyChanged;

  #endregion

  public void CaptureOldValue(DependencyObject target, DependencyProperty property, object oldValue)
  {
    CaptureHolders[key: new CaptureKeyHolder { Target = target, Property = property }] = oldValue;
  }

  public void CaptureNewValue(DependencyObject target, DependencyProperty property, object newValue)
  {
    var holder = new CaptureKeyHolder { Target = target, Property = property };
    if (CaptureHolders.ContainsKey(key: holder))
    {
      object oldValue = CaptureHolders[key: holder];
      CaptureHolders.Remove(key: holder);

      if (!object.Equals(objA: oldValue, objB: newValue))
      {
        var action = new DependencyPropertyChangedUndoAction(target: target, property: property, oldValue: oldValue, newValue: newValue);
        AddAction(action: action);
      }
    }
  }

  private sealed class CaptureKeyHolder
  {
    public DependencyObject Target { get; init; }
    public DependencyProperty Property { get; init; }

    public override int GetHashCode()
    {
      return Target.GetHashCode() ^ Property.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      if (obj == null)
      {
        return false;
      }

      if (obj is not CaptureKeyHolder other)
      {
        return false;
      }

      return Target == other.Target && Property == other.Property;
    }
  }
}

public enum UndoState
{
  None,
  Undoing,
  Redoing
}
