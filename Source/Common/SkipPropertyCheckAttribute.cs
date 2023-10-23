﻿using System;
using System.Diagnostics;

namespace Crystal.Plot2D.Common;

[Conditional(conditionString: "DEBUG")]
[AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class SkipPropertyCheckAttribute : Attribute
{
}
