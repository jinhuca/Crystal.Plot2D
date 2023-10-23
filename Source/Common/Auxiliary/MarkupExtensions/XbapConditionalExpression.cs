using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace Crystal.Plot2D;

[EditorBrowsable(state: EditorBrowsableState.Never)]
public class XbapConditionalExpression : MarkupExtension
{
  public XbapConditionalExpression() { }

  public XbapConditionalExpression(object value)
  {
    Value = value;
  }

  [ConstructorArgument(argumentName: "value")]
  public object Value { get; set; }

  public override object ProvideValue(IServiceProvider serviceProvider)
  {
#if RELEASEXBAP
			return null;
#else
    return ((ResourceDictionary)Application.LoadComponent(resourceLocator: new Uri(uriString: Constants.ThemeUri, uriKind: UriKind.Relative)))[key: Value];
#endif
  }
}
