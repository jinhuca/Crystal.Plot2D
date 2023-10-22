using Crystal.Plot2D;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following set of attributes. 
// Change these attribute values to modify the information associated with an assembly.
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
[assembly: XmlnsDefinition(AssemblyConstants.DefaultXmlNamespace, "Crystal.Plot2D")]
[assembly: XmlnsDefinition(AssemblyConstants.DefaultXmlNamespace, "Crystal.Plot2D.Charts")]
[assembly: XmlnsDefinition(AssemblyConstants.DefaultXmlNamespace, "Crystal.Plot2D.DataSources")]
[assembly: XmlnsDefinition(AssemblyConstants.DefaultXmlNamespace, "Crystal.Plot2D.Common")]
[assembly: XmlnsPrefix(AssemblyConstants.DefaultXmlNamespace, "Plotter2D")]
[assembly: AllowPartiallyTrustedCallers]

namespace Crystal.Plot2D;

public static class AssemblyConstants
{
  public const string DefaultXmlNamespace = "http://schemas.crystal.com/Plot2D";
}
