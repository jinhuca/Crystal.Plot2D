using System;
using System.Globalization;

namespace Crystal.Plot2D.Common;

internal static class TokenizerHelper
{
  public static char GetNumericListSeparator(IFormatProvider provider)
  {
    char separator = ',';

    NumberFormatInfo numberInfo = NumberFormatInfo.GetInstance(formatProvider: provider);
    if (numberInfo.NumberDecimalSeparator.Length > 0 && separator == numberInfo.NumberDecimalSeparator[index: 0])
    {
      separator = ';';
    }

    return separator;
  }
}
