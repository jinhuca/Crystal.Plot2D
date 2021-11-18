using Crystal.Plot2D;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following set of attributes. 
// Change these attribute values to modify the information associated with an assembly.
[assembly: AssemblyTitle("Crystal.Plot2D")]
[assembly: AssemblyDescription("Scientific Data Visualization")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Jin Hu")]
[assembly: AssemblyProduct("Crystal.Plot2D")]
[assembly: AssemblyCopyright("Copyright © Jin Hu 2009 - 2021")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
[assembly: XmlnsDefinition(AssemblyConstants.DefaultXmlNamespace, "Crystal.Plot2D")]
[assembly: XmlnsDefinition(AssemblyConstants.DefaultXmlNamespace, "Crystal.Plot2D.Charts")]
[assembly: XmlnsDefinition(AssemblyConstants.DefaultXmlNamespace, "Crystal.Plot2D.Graphs")]
[assembly: XmlnsDefinition(AssemblyConstants.DefaultXmlNamespace, "Crystal.Plot2D.DataSources")]
[assembly: XmlnsDefinition(AssemblyConstants.DefaultXmlNamespace, "Crystal.Plot2D.Common")]
[assembly: XmlnsPrefix(AssemblyConstants.DefaultXmlNamespace, "Plotter2D")]
[assembly: CLSCompliant(true)]
[assembly: AllowPartiallyTrustedCallers]
[module: SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]

namespace Crystal.Plot2D
{
  public static class AssemblyConstants
  {
    public const string DefaultXmlNamespace = "http://schemas.crystal.com/Plot2D";
  }
}
