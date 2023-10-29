using System;
using System.Windows;

namespace Crystal.Plot2D.Descriptions;

public class StandardDescription : Description
{
  public StandardDescription() { }

  public StandardDescription(string description)
  {
    DescriptionString = description ?? throw new ArgumentNullException(paramName: nameof(description));
  }

  protected override void AttachCore(UIElement element)
  {
    if (DescriptionString == null)
    {
      var str = element.GetType().Name;
      DescriptionString = str;
    }
  }

  public string DescriptionString { get; set; }

  public sealed override string Brief => DescriptionString;

  public sealed override string Full => DescriptionString;
}
