﻿using System.Windows.Data;

namespace Crystal.Plot2D.Common.Auxiliary.MarkupExtensions;

public class TemplateBinding : Binding
{
  public TemplateBinding()
  {
    RelativeSource = new RelativeSource { Mode = RelativeSourceMode.TemplatedParent };
  }

  public TemplateBinding(string propertyPath) : this()
  {
    Path = new System.Windows.PropertyPath(path: propertyPath);
  }
}
