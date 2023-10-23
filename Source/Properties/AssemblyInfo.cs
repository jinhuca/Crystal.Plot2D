﻿using Crystal.Plot2D;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following set of attributes. 
// Change these attribute values to modify the information associated with an assembly.
[assembly: ComVisible(visibility: false)]
[assembly: CLSCompliant(isCompliant: true)]
[assembly: ThemeInfo(themeDictionaryLocation: ResourceDictionaryLocation.None, genericDictionaryLocation: ResourceDictionaryLocation.SourceAssembly)]
[assembly: XmlnsDefinition(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, clrNamespace: "Crystal.Plot2D")]
[assembly: XmlnsDefinition(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, clrNamespace: "Crystal.Plot2D.Charts")]
[assembly: XmlnsDefinition(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, clrNamespace: "Crystal.Plot2D.DataSources")]
[assembly: XmlnsDefinition(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, clrNamespace: "Crystal.Plot2D.Common")]
[assembly: XmlnsPrefix(xmlNamespace: AssemblyConstants.DefaultXmlNamespace, prefix: "Plotter2D")]
[assembly: AllowPartiallyTrustedCallers]

namespace Crystal.Plot2D;

public static class AssemblyConstants
{
  public const string DefaultXmlNamespace = "http://schemas.crystal.com/Plot2D";
}
