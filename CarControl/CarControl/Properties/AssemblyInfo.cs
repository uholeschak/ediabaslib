using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Allgemeine Informationen über eine Assembly werden über die folgenden
// Attribute gesteuert. Ändern Sie diese Attributwerte, um die Informationen zu ändern,
// die mit einer Assembly verknüpft sind.
[assembly: AssemblyTitle("CarControl")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Ulrich Holeschak")]
[assembly: AssemblyProduct("CarControl")]
[assembly: AssemblyCopyright("Copyright © UH 2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Durch Festlegen von ComVisible auf "false" werden die Typen in dieser Assembly unsichtbar
// für COM-Komponenten. Wenn Sie auf einen Typ in dieser Assembly von
// COM zugreifen müssen, legen Sie das ComVisible-Attribut für diesen Typ auf "true" fest.
[assembly: ComVisible(false)]

// Die folgende GUID bestimmt die ID der Typbibliothek, wenn dieses Projekt für COM verfügbar gemacht wird
[assembly: Guid("fd34db26-0b80-4539-ae8f-304858663168")]

// Versionsinformationen für eine Assembly bestehen aus den folgenden vier Werten:
//
//      Hauptversion
//      Nebenversion
//      Buildnummer
//      Revision
//
[assembly: AssemblyVersion("1.0.0.0")]

// Mit dem nachstehenden Attribut wird die FxCop-Warnung "CA2232 : Microsoft.Usage : Fügen Sie ein STAThreadAttribute zu der Assembly hinzu" unterdrückt,
// da die Geräteanwendung keine STA-Threads unterstützt.
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2232:MarkWindowsFormsEntryPointsWithStaThread")]
