using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Crystal.Plot2D.Common
{
  public sealed class DataRectSerializer : ValueSerializer
  {
    public override bool CanConvertFromString(string value, IValueSerializerContext context) => true;

    public override bool CanConvertToString(object value, IValueSerializerContext context) => value is DataRect;

    public override object ConvertFromString(string value, IValueSerializerContext context)
    {
      if (value != null)
      {
        return DataRect.Parse(value);
      }
      return base.ConvertFromString(value, context);
    }

    public override string ConvertToString(object value, IValueSerializerContext context)
    {
      if (value is DataRect)
      {
        DataRect rect = (DataRect)value;
        return rect.ConvertToString(null, CultureInfo.GetCultureInfo("en-us"));
      }
      return base.ConvertToString(value, context);
    }
  }
}
