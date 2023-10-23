using System;
using System.Diagnostics;

namespace Crystal.Plot2D;

[Conditional(conditionString: "DEBUG")]
[AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class NotNullAttribute : Attribute
{
}
