using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JinHu.Visualization.Plotter2D.Common
{
  /// <summary>
  ///   Implementation of <see cref="INotifyPropertyChanged"/> to simplify models.
  /// </summary>
  public abstract class BindableBase : INotifyPropertyChanged
  {
    /// <summary>
    ///   Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///   Checks if a property already matches a desired value. Sets the property and notifies listeners only when necessary.
    /// </summary>
    /// <typeparam name="T">
    ///   Type of the property.
    /// </typeparam>
    /// <param name="storage">
    ///   Reference to a property with both getter and setter.
    /// </param>
    /// <param name="value">
    ///   Desired value for the property.
    /// </param>
    /// <param name="propertyName">
    ///   Name of the property used to notify listeners. This value is optional and can 
    ///   be provided automatically when invoked from compilers that support CallerMemberName.
    /// </param>
    /// <returns>
    ///   True if the value was changed, false if the existing value matched the desired value.
    /// </returns>
    protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
      if (Equals(storage, value))
      {
        return false;
      }

      storage = value;
      OnPropertyChanged(propertyName);
      return true;
    }

    /// <summary>
    ///   Notifies listeners that a property value has changed.
    /// </summary>
    /// <param name="propertyName">
    ///   Name of the property used to notify listeners. This value is optional and can be provided 
    ///   automatically when invoked from compilers that support <see cref="CallerMemberNameAttribute"/>.
    /// </param>
    protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    ///   Raises this object's PropertyChanged event.
    /// </summary>
    /// <typeparam name="T">
    ///   The type of the property that has a new value
    /// </typeparam>
    /// <param name="propertyExpression">
    ///   A Lambda expression representing the property that has a new value.
    /// </param>
    protected virtual void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
    {
      var propertyName = PropertySupport.ExtractPropertyName(propertyExpression);
      OnPropertyChanged(propertyName);
    }
  }
}
