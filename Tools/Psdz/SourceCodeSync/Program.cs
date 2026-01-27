using CommandLine;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SourceCodeSync
{
    internal class Program
    {
        /// <summary>
        /// Represents a //[-]//[+] comment line with its context (preceding and following lines)
        /// </summary>
        private class CommentedCodeLineInfo
        {
            public string CommentLine { get; set; }
            public string PrecedingCodeLine { get; set; }
            public string FollowingCodeLine { get; set; }
            public int OriginalLineNumber { get; set; }
            public CommentedCodeLineInfo NextInfo { get; set; }
            public int? Index { get; set; }
        }

        private static Dictionary<string, ClassDeclarationSyntax> _classDict = new (StringComparer.Ordinal);
        private static Dictionary<string, ClassDeclarationSyntax> _classBareDict = new (StringComparer.Ordinal);
        private static Dictionary<string, InterfaceDeclarationSyntax> _interfaceDict = new (StringComparer.Ordinal);
        private static Dictionary<string, InterfaceDeclarationSyntax> _interfaceBareDict = new (StringComparer.Ordinal);
        private static Dictionary<string, EnumDeclarationSyntax> _enumDict = new (StringComparer.Ordinal);
        private static Dictionary<string, EnumDeclarationSyntax> _enumBareDict = new (StringComparer.Ordinal);

        private static readonly string[] _ignoreNamespaces =
        [
            @"^BMW\.ISPI\.TRIC\.ISTA\.Contracts\.Enums\.UserLogin$",
            @"^BMW\.Rheingold\.CoreFramework\.OSS$",
            @"^BMW\.Rheingold\.CoreFramework\.IndustrialCustomer\..*",
            @"^BMW\.Rheingold\.CoreFramework\.Contracts\.Programming\.TherapyPlan$",
            @"^BMW\.ISPI\.TRIC\.ISTA\.Contracts\.Models\..*",
            @"^BMW\.Rheingold\.InfoProvider\.(SWT|Tric|Broker|igDom|service|Current|SCC|NOP|EDGE|Properties).*",
        ];

        private static readonly Regex[] _compiledIgnoreNamespaces = _ignoreNamespaces
                .Select(pattern => new Regex(pattern, RegexOptions.Compiled))
                .ToArray();

        // Regex pattern for extracting index from //[-](index) or //[+](index)
        private static readonly Regex _commentedCodeWithIndexPattern = new Regex(
            @"^//\[[-+]\]\((\d+)\)", RegexOptions.Compiled);

        private static readonly Dictionary<string, string> _modifyClassNames = new()
        {
            {"public_static_LicenseHelper", null},
            {"internal_sealed_LicenseManager", null},
            {"public_LicenseStatusChecker", null},
            {"public_LicenseWizardHelper", null },
            {"internal_CharacteristicsGenerator", null },
            {"BMW.Rheingold.Programming.Common.SecureCodingService", null },
            {"BMW.Rheingold.Programming.ProgrammingService", "ProgrammingService2"},
            {"BMW.iLean.CommonServices.Logging.Extensions", null},
            {"BMW.ISPI.TRIC.ISTA.VehicleIdentification.Utility.GearboxUtility", null}
        };

        private static readonly Dictionary<string, string> _modifyInterfaceNames = new()
        {
            {"BMW.Rheingold.CoreFramework.Contracts.Programming.IProgrammingService", "IProgrammingService2"},
            {"BMW.Rheingold.ISTA.CoreFramework.ILogger", null}
        };

        private static readonly Dictionary<string, string> _modifyEnumNames = new()
        {
            {"BMW.iLean.CommonServices.Logging.EventKind", null},
            {"BMW.Rheingold.CoreFramework.Contracts.Programming.TherapyPlan.SwiActionCategory", null}
        };

        private static readonly string[] _decompileAssemblies =
        [
            "BMW.ISPI.TRIC.ISTA.Common",
            "BMW.ISPI.TRIC.ISTA.Contracts",
            "BMW.ISPI.TRIC.ISTA.DiagnosticsBusinessDataCore",
            "BMW.ISPI.TRIC.ISTA.FusionReactor",
            "BMW.ISPI.TRIC.ISTA.MultisourceLogic",
            "BMW.ISPI.TRIC.ISTA.RuleEvaluation",
            "BMW.ISPI.TRIC.ISTA.VehicleIdentification",
            "CommonServices",
            "DiagnosticsBusinessData",
            "IstaServicesClient",
            "IstaServicesContract",
            "RheingoldCoreContracts",
            "RheingoldCoreFramework",
            "RheingoldDiagnostics",
            "RheingoldInfoProvider",
            "RheingoldISPINext",
            "RheingoldISTACoreFramework",
            "RheingoldProgramming",
            "RheingoldPsdzWebApi.Adapter",
            "RheingoldPsdzWebApi.Adapter.Contracts",
            "RheingoldVehicleCommunication"
        ];

        private static readonly Dictionary<string, string> _textReplacements = new Dictionary<string, string>
        {
            { "BMW.Rheingold.CoreFramework.Extensions.AddRange", "Extensions.AddRange" },
            { "RheingoldPsdzWebApi.Adapter.Contracts.Services.IProgrammingService", "IProgrammingService" },
            { "BMW.Rheingold.DiagnosticsBusinessDataCore.DiagnosticsBusinessDataCore", "DiagnosticsBusinessDataCore" },
            { "BMW.Rheingold.CoreFramework.Extensions", "Extensions" },
            { "BMW.ISPI.TRIC.ISTA.Contracts.Enums.NetworkType", "BMW.Rheingold.CoreFramework.Contracts.Vehicle.NetworkType"},
            { "BMW.ISPI.TRIC.ISTA.MultisourceLogic.MultisourceLogic", "MultisourceLogic"}
        };

        private const string _commentedRemoveCodeMarker = "//[-]";

        private const string _commentedAddCodeMarker = "//[+]";

        private const string _attributPreserveSource = "PreserveSource";

        private const string _signatureModifiedProperty = "SignatureModified";

        private const string _accessModifiedProperty = "AccessModified";

        private const string _inheritanceModifiedProperty = "InheritanceModified";

        private const string _attributesModifiedProperty = "AttributesModified";

        private const string _removedProperty = "Removed";

        private const string _suppressWarningProperty = "SuppressWarning";

        private const string _attributesOriginalHashProperty = "OriginalHash";

        private const string _attributesHintProperty = "Hint";

        public class Options
        {
            public Options()
            {
                SourceDir = string.Empty;
                AssemblyDir = string.Empty;
                DestDir = string.Empty;
                Filter = string.Empty;
                Overwrite = false;
                Verbosity = VerbosityOption.Error;
            }

            public enum VerbosityOption
            {
                None,
                Error,
                Important,
                Warning,
                Info,
                Debug
            }

            [Option('s', "sourcedir", Required = true, HelpText = "Source directory.")]
            public string SourceDir { get; set; }

            [Option('a', "assemblydir", Required = true, HelpText = "Assembly directory.")]
            public string AssemblyDir { get; set; }

            [Option('d', "destdir", Required = true, HelpText = "Destination directory.")]
            public string DestDir { get; set; }

            [Option('f', "filter", Required = false, HelpText = "Directory filter.")]
            public string Filter { get; set; }

            [Option('o', "overwrite", Required = false, HelpText = "Option to overwrite existing files.")]
            public bool Overwrite { get; set; }

            [Option('v', "verbosity", Required = false, HelpText = "Option for message verbosity (Error, Warning, Info, Debug)")]
            public VerbosityOption Verbosity { get; set; }
        }

        static Options.VerbosityOption _verbosity = Options.VerbosityOption.Important;

        static int Main(string[] args)
        {
            try
            {
                string sourceDir = null;
                string assemblyDir = null;
                string destDir = null;
                string filter = null;
                bool overwrite = false;
                bool hasErrors = false;

                Parser parser = new Parser(with =>
                {
                    //ignore case for enum values
                    with.CaseInsensitiveEnumValues = true;
                    with.EnableDashDash = true;
                    with.HelpWriter = Console.Out;
                });

                parser.ParseArguments<Options>(args)
                    .WithParsed<Options>(o =>
                    {
                        sourceDir = o.SourceDir;
                        assemblyDir = o.AssemblyDir;
                        destDir = o.DestDir;
                        filter = o.Filter;
                        overwrite = o.Overwrite;
                        _verbosity = o.Verbosity;
                    })
                    .WithNotParsed(errs =>
                    {
                        string errors = string.Join("\n", errs);
                        Console.WriteLine("Option parsing errors:\n{0}", string.Join("\n", errors));
                        if (errors.IndexOf("BadFormatConversion", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Console.WriteLine("Valid verbosity options are: {0}", string.Join(", ", Enum.GetNames(typeof(Options.VerbosityOption)).ToList()));
                        }

                        hasErrors = true;
                    });

                if (hasErrors)
                {
                    return 1;
                }

                if (string.IsNullOrEmpty(sourceDir))
                {
                    if (_verbosity >= Options.VerbosityOption.Error)
                    {
                        Console.WriteLine("Source directory missing");
                    }
                    return 1;
                }

                if (!Path.IsPathRooted(sourceDir))
                {
                    // sourceDir is relative, combine it with assemblyDir
                    sourceDir = Path.GetFullPath(Path.Combine(assemblyDir, sourceDir));
                    if (_verbosity >= Options.VerbosityOption.Info)
                    {
                        Console.WriteLine("Source directory is relative, combined with assembly directory: {0}", sourceDir);
                    }

                    if (!Directory.Exists(sourceDir))
                    {
                        try
                        {
                            Directory.CreateDirectory(sourceDir);
                        }
                        catch (Exception e)
                        {
                            if (_verbosity >= Options.VerbosityOption.Error)
                            {
                                Console.WriteLine("Create source directory failed: {0}, Exception: {1}", sourceDir, e.Message);
                            }
                            return 1;
                        }
                    }
                }
                else
                {
                    if (!Directory.Exists(sourceDir))
                    {
                        if (_verbosity >= Options.VerbosityOption.Error)
                        {
                            Console.WriteLine("Source directory not existing: {0}", sourceDir);
                        }
                        return 1;
                    }
                }

                if (string.IsNullOrEmpty(assemblyDir) || !Directory.Exists(assemblyDir))
                {
                    if (_verbosity >= Options.VerbosityOption.Error)
                    {
                        Console.WriteLine("Assembly directory not existing: {0}", assemblyDir);
                    }
                    return 1;
                }

                if (string.IsNullOrEmpty(destDir) || !Directory.Exists(destDir))
                {
                    if (_verbosity >= Options.VerbosityOption.Error)
                    {
                        Console.WriteLine("Destination directory not existing: {0}", destDir);
                    }
                    return 1;
                }

                string[] filterParts = null;
                if (!string.IsNullOrEmpty(filter))
                {
                    filterParts = filter.Split(';');
                }

                Console.WriteLine("Assembly dir: {0}", assemblyDir);
                Console.WriteLine("Source dir: {0}", sourceDir);
                if (filterParts != null && filterParts.Length > 0)
                {
                    Console.WriteLine("Filter: {0}", string.Join(", ", filterParts));
                }
                Console.WriteLine("Decompiling ...");

                List<string> searchList = new List<string>() { assemblyDir };
                foreach (string assemblyName in _decompileAssemblies)
                {
                    string assemblyPath = Path.Combine(assemblyDir, assemblyName + ".dll");
                    if (File.Exists(assemblyPath))
                    {
                        string outputPath = Path.Combine(sourceDir, assemblyName);
                        if (Directory.Exists(outputPath))
                        {
                            if (overwrite)
                            {
                                Directory.Delete(outputPath, true);
                            }
                            else
                            {
                                if (_verbosity >= Options.VerbosityOption.Info)
                                {
                                    Console.WriteLine("Source directory already exists, skipping decompilation: {0}", outputPath);
                                }
                                continue;
                            }
                        }

                        if (_verbosity >= Options.VerbosityOption.Info)
                        {
                            Console.WriteLine("Decompiling assembly: {0}", assemblyPath);
                        }

                        try
                        {
                            if (!DecompilerHelper.DecompileAssembly(assemblyPath, outputPath, searchList, _textReplacements))
                            {
                                if (_verbosity >= Options.VerbosityOption.Error)
                                {
                                    Console.WriteLine("*** Decompilation failed for assembly: {0}", assemblyPath);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (_verbosity >= Options.VerbosityOption.Error)
                            {
                                Console.WriteLine("*** Decompilation failed for assembly: {0}, Exception: {1}", assemblyPath, ex.Message);
                            }
                        }
                    }
                    else
                    {
                        if (_verbosity >= Options.VerbosityOption.Error)
                        {
                            Console.WriteLine("*** Assembly not found for decompilation: {0}", assemblyPath);
                        }
                    }
                }

                Console.WriteLine("Dest dir: {0}", destDir);
                Console.WriteLine("Updating source ...");

                string[] sourceFiles = Directory.GetFiles(sourceDir, "*.cs", SearchOption.AllDirectories);
                foreach (string file in sourceFiles)
                {
                    if (!GetFileSource(file))
                    {
                        if (_verbosity >= Options.VerbosityOption.Error)
                        {
                            Console.WriteLine("*** Get file source failed: {0}", file);
                        }
                    }
                }

                string[] destFiles = Directory.GetFiles(destDir, "*.cs", SearchOption.AllDirectories);
                foreach (string file in destFiles)
                {
                    if (file.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string relPath = GetRelativePath(destDir, file);
                    string relDir = Path.GetDirectoryName(relPath);
                    if (string.IsNullOrEmpty(relDir))
                    {
                        continue;
                    }

                    if (filterParts != null && filterParts.Length > 0)
                    {
                        bool matched = false;
                        foreach (string filterPart in filterParts)
                        {
                            if (relDir.Contains(filterPart, StringComparison.OrdinalIgnoreCase))
                            {
                                matched = true;
                                break;
                            }
                        }

                        if (!matched)
                        {
                            continue;
                        }
                    }

                    if (!UpdateFile(file))
                    {
                        if (_verbosity >= Options.VerbosityOption.Error)
                        {
                            Console.WriteLine("*** Update file failed: {0}", file);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (_verbosity >= Options.VerbosityOption.Error)
                {
                    Console.WriteLine("*** Exception: {0}", e.Message);
                }
                return 1;
            }

            return 0;
        }

        public static bool GetFileSource(string fileName)
        {
            try
            {
                string fileContent = File.ReadAllText(fileName);
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(fileContent);
                SyntaxNode root = syntaxTree.GetCompilationUnitRoot();

                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                foreach (ClassDeclarationSyntax cls in classes)
                {
                    string classNameFull = GetClassName(cls, includeModifiers: true);
                    string classNameBare = GetClassName(cls);
                    string classSource = cls.ToFullString();
                    string namespaceName = GetNamespace(cls);

                    if (_verbosity >= Options.VerbosityOption.Debug)
                    {
                        Console.WriteLine($"Class: {classNameFull}");
                        Console.WriteLine($"Namespace: {namespaceName}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(classSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (_compiledIgnoreNamespaces.Any(regex => regex.IsMatch(namespaceName)))
                    {
                        continue;
                    }

                    string changeClassName = null;
                    if (_modifyClassNames.TryGetValue(classNameFull, out string newClassName1))
                    {
                        if (string.IsNullOrEmpty(newClassName1))
                        {
                            continue;
                        }
                        changeClassName = newClassName1;
                    }

                    string classNameWithNamespace = GetClassName(cls, includeNamespace: true);
                    if (_modifyClassNames.TryGetValue(classNameWithNamespace, out string newClassName2))
                    {
                        if (string.IsNullOrEmpty(newClassName2))
                        {
                            continue;
                        }
                        changeClassName = newClassName2;
                    }

                    List<Tuple<Dictionary<string, ClassDeclarationSyntax>, string, bool>> dictList = new()
                    {
                        new(_classDict, classNameFull, true),
                        new(_classBareDict, classNameBare, false)
                    };

                    foreach (var tuple in dictList)
                    {
                        Dictionary<string, ClassDeclarationSyntax> dict = tuple.Item1;
                        string name = tuple.Item2;
                        bool isFullName = tuple.Item3;

                        ClassDeclarationSyntax classCopy = cls;
                        if (!string.IsNullOrEmpty(changeClassName))
                        {
                            string pattern = $@"(?<=^|[^a-zA-Z0-9]){Regex.Escape(classNameBare)}(?=[^a-zA-Z0-9]|$)";
                            name = Regex.Replace(name, pattern, changeClassName);

                            SyntaxToken newIdentifier = SyntaxFactory.Identifier(
                                cls.Identifier.LeadingTrivia,
                                changeClassName,
                                cls.Identifier.TrailingTrivia
                            );
                            classCopy = cls.WithIdentifier(newIdentifier);

                            // Rename all constructors to match the new class name
                            classCopy = RenameConstructors(classCopy, classNameBare, changeClassName);
                        }

                        if (dict.TryGetValue(name, out ClassDeclarationSyntax oldClassSyntax))
                        {
                            if (oldClassSyntax != null)
                            {
                                string oldClassSource = oldClassSyntax.ToFullString();
                                if (oldClassSource != classSource)
                                {
                                    if (isFullName)
                                    {
                                        if (_verbosity >= Options.VerbosityOption.Error)
                                        {
                                            Console.WriteLine("*** Warning: Duplicate class name with different source: {0}", name);
                                        }
                                    }
                                    else
                                    {
                                        if (_verbosity >= Options.VerbosityOption.Warning)
                                        {
                                            Console.WriteLine("Warning: Duplicate bare class name with different source: {0}", name);
                                        }
                                    }

                                    dict[name] = null;
                                }
                            }
                        }
                        else
                        {
                            if (!dict.TryAdd(name, classCopy))
                            {
                                if (_verbosity >= Options.VerbosityOption.Error)
                                {
                                    Console.WriteLine("*** Warning: Add class failed: {0}", name);
                                }
                            }
                        }
                    }
                }

                var interfaces = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>();
                foreach (InterfaceDeclarationSyntax interfaceDecl in interfaces)
                {
                    string interfaceNameFull = GetInterfaceName(interfaceDecl, includeModifiers: true);
                    string interfaceBareName = GetInterfaceName(interfaceDecl);
                    string interfaceSource = interfaceDecl.ToFullString();
                    string namespaceName = GetNamespace(interfaceDecl);

                    if (_verbosity >= Options.VerbosityOption.Debug)
                    {
                        Console.WriteLine($"Interface: {interfaceNameFull}");
                        Console.WriteLine($"Namespace: {namespaceName}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(interfaceSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (_compiledIgnoreNamespaces.Any(regex => regex.IsMatch(namespaceName)))
                    {
                        continue;
                    }

                    string changeInterfaceName = null;
                    if (_modifyInterfaceNames.TryGetValue(interfaceNameFull, out string newInterfaceName1))
                    {
                        if (string.IsNullOrEmpty(newInterfaceName1))
                        {
                            continue;
                        }
                        changeInterfaceName = newInterfaceName1;
                    }

                    string interfaceNameWithNamespace = GetInterfaceName(interfaceDecl, includeNamespace: true);
                    if (_modifyInterfaceNames.TryGetValue(interfaceNameWithNamespace, out string newInterfaceName2))
                    {
                        if (string.IsNullOrEmpty(newInterfaceName2))
                        {
                            continue;
                        }
                        changeInterfaceName = newInterfaceName2;
                    }

                    List<Tuple<Dictionary<string, InterfaceDeclarationSyntax>, string, bool>> dictList =
                        new List<Tuple<Dictionary<string, InterfaceDeclarationSyntax>, string, bool>>
                        {
                            new (_interfaceDict, interfaceNameFull, true),
                            new (_interfaceBareDict, interfaceBareName, false)
                        };

                    foreach (var tuple in dictList)
                    {
                        Dictionary<string, InterfaceDeclarationSyntax> dict = tuple.Item1;
                        string name = tuple.Item2;
                        bool isFullName = tuple.Item3;

                        InterfaceDeclarationSyntax interfaceCopy = interfaceDecl;
                        if (!string.IsNullOrEmpty(changeInterfaceName))
                        {
                            string pattern = $@"(?<=^|[^a-zA-Z0-9]){Regex.Escape(interfaceBareName)}(?=[^a-zA-Z0-9]|$)";
                            name = Regex.Replace(name, pattern, changeInterfaceName);

                            SyntaxToken newIdentifier = SyntaxFactory.Identifier(
                                interfaceDecl.Identifier.LeadingTrivia,
                                changeInterfaceName,
                                interfaceDecl.Identifier.TrailingTrivia
                            );
                            interfaceCopy = interfaceDecl.WithIdentifier(newIdentifier);
                        }

                        if (dict.TryGetValue(name, out InterfaceDeclarationSyntax oldInterfaceSyntax))
                        {
                            if (oldInterfaceSyntax != null)
                            {
                                string oldInterfaceSource = oldInterfaceSyntax.ToFullString();
                                if (oldInterfaceSource != interfaceSource)
                                {
                                    if (isFullName)
                                    {
                                        if (_verbosity >= Options.VerbosityOption.Error)
                                        {
                                            Console.WriteLine("*** Warning: Duplicate interface name with different source: {0}", name);
                                        }
                                    }
                                    else
                                    {
                                        if (_verbosity >= Options.VerbosityOption.Warning)
                                        {
                                            Console.WriteLine("Warning: Duplicate bare interface name with different source: {0}", name);
                                        }
                                    }

                                    dict[name] = null;
                                }
                            }
                        }
                        else
                        {
                            if (!dict.TryAdd(name, interfaceCopy))
                            {
                                if (_verbosity >= Options.VerbosityOption.Error)
                                {
                                    Console.WriteLine("*** Warning: Add interface failed: {0}", name);
                                }
                            }
                        }
                    }
                }

                var enums = root.DescendantNodes().OfType<EnumDeclarationSyntax>();
                foreach (EnumDeclarationSyntax enumDecl in enums)
                {
                    string enumNameFull = GetEnumName(enumDecl, includeModifiers: true);
                    string enumBareName = GetEnumName(enumDecl);
                    string enumSource = enumDecl.ToFullString();
                    string namespaceName = GetNamespace(enumDecl);

                    if (_verbosity >= Options.VerbosityOption.Debug)
                    {
                        Console.WriteLine($"Enum: {enumNameFull}");
                        Console.WriteLine($"Namespace: {namespaceName}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(enumSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (_compiledIgnoreNamespaces.Any(regex => regex.IsMatch(namespaceName)))
                    {
                        continue;
                    }

                    string changeEnumName = null;
                    if (_modifyEnumNames.TryGetValue(enumNameFull, out string newEnumName1))
                    {
                        if (string.IsNullOrEmpty(newEnumName1))
                        {
                            continue;
                        }
                        changeEnumName = newEnumName1;
                    }

                    string enumNameWithNamespace = GetEnumName(enumDecl, includeNamespace: true);
                    if (_modifyEnumNames.TryGetValue(enumNameWithNamespace, out string newEnumName2))
                    {
                        if (string.IsNullOrEmpty(newEnumName2))
                        {
                            continue;
                        }
                        changeEnumName = newEnumName2;
                    }

                    List<Tuple<Dictionary<string, EnumDeclarationSyntax>, string, bool>> dictList =
                        new List<Tuple<Dictionary<string, EnumDeclarationSyntax>, string, bool>>
                        {
                            new (_enumDict, enumNameFull, true),
                            new (_enumBareDict, enumBareName, false)
                        };

                    foreach (var tuple in dictList)
                    {
                        Dictionary<string, EnumDeclarationSyntax> dict = tuple.Item1;
                        string name = tuple.Item2;
                        bool isFullName = tuple.Item3;

                        EnumDeclarationSyntax enumCopy = enumDecl;
                        if (!string.IsNullOrEmpty(changeEnumName))
                        {
                            string pattern = $@"(?<=^|[^a-zA-Z0-9]){Regex.Escape(enumBareName)}(?=[^a-zA-Z0-9]|$)";
                            name = Regex.Replace(name, pattern, changeEnumName);
                            SyntaxToken newIdentifier = SyntaxFactory.Identifier(
                                enumDecl.Identifier.LeadingTrivia,
                                changeEnumName,
                                enumDecl.Identifier.TrailingTrivia
                            );
                            enumCopy = enumDecl.WithIdentifier(newIdentifier);
                        }

                        if (dict.TryGetValue(name, out EnumDeclarationSyntax oldEnumSyntax))
                        {
                            if (oldEnumSyntax != null)
                            {
                                string oldEnumSource = oldEnumSyntax.ToFullString();
                                if (oldEnumSource != enumSource)
                                {
                                    if (isFullName)
                                    {
                                        if (_verbosity >= Options.VerbosityOption.Error)
                                        {
                                            Console.WriteLine("*** Warning: Duplicate enum name with different source: {0}", name);
                                        }
                                    }
                                    else
                                    {
                                        if (_verbosity >= Options.VerbosityOption.Warning)
                                        {
                                            Console.WriteLine("Warning: Duplicate bare enum name with different source: {0}", name);
                                        }
                                    }
                                    dict[name] = null;
                                }
                            }
                        }
                        else
                        {
                            if (!dict.TryAdd(name, enumCopy))
                            {
                                if (_verbosity >= Options.VerbosityOption.Error)
                                {
                                    Console.WriteLine("*** Warning: Add enum failed: {0}", name);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (_verbosity >= Options.VerbosityOption.Error)
                {
                    Console.WriteLine("*** Error parsing file: {0}, Exception {1}", fileName, e.Message);
                }
                return false;
            }
            return true;
        }

        public static bool UpdateFile(string fileName)
        {
            try
            {
                string fileContent = File.ReadAllText(fileName);
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(fileContent);
                CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();

                bool fileModified = false;
                CompilationUnitSyntax newRoot = root;

                // Update classes
                List<ClassDeclarationSyntax> classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                foreach (ClassDeclarationSyntax cls in classes)
                {
                    string classNameFull = GetClassName(cls, includeModifiers: true);
                    string classNameBare = GetClassName(cls);
                    string classSource = cls.NormalizeWhitespace().ToFullString();
                    string namespaceName = GetNamespace(cls);

                    if (_verbosity >= Options.VerbosityOption.Debug)
                    {
                        Console.WriteLine($"Class: {classNameFull}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(classSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (HasSpecialTrivia(cls, out string reason, out Options.VerbosityOption verbosity))
                    {
                        if (_verbosity >= verbosity)
                        {
                            Console.WriteLine("Skipping class {0}:{1} reason: {2}", namespaceName, classNameFull, reason);
                        }
                        continue;
                    }

                    if (!_classDict.TryGetValue(classNameFull, out ClassDeclarationSyntax sourceClass))
                    {
                        if (!_classBareDict.TryGetValue(classNameBare, out sourceClass))
                        {
                            sourceClass = null;
                        }
                    }

                    if (sourceClass != null)
                    {
                        ClassDeclarationSyntax sourceClassCopy = sourceClass;
                        bool hasSpecialSourceAttribute = HasSpecialSourceAttribute(cls.AttributeLists);
                        if (!hasSpecialSourceAttribute)
                        {
                            bool hasContract = HasContractAttribute(cls.AttributeLists);
                            bool sourceHasContract = HasContractAttribute(sourceClassCopy.AttributeLists);
                            if (hasContract && !sourceHasContract)
                            {
                                if (_verbosity >= Options.VerbosityOption.Important)
                                {
                                    Console.WriteLine("Skipping class {0}:{1} with removed Contract from file: {2}", namespaceName, classNameFull, Path.GetFileName(fileName));
                                }
                                continue;
                            }
                        }

                        bool specialAttribute = HasSpecialSourceAttribute(cls.AttributeLists);
                        if (specialAttribute)
                        {
                            if (_verbosity >= Options.VerbosityOption.Info)
                            {
                                Console.WriteLine("Class {0}:{1} special attribute detected", namespaceName, classNameFull);
                            }

                            // Update modifiers and attributes from destination class
                            sourceClassCopy = sourceClassCopy.WithAttributeLists(cls.AttributeLists);

                            // Update attributes if AccessModified is set
                            if (GetAttributeBoolPropertyFromAttributeLists(cls.AttributeLists, _accessModifiedProperty))
                            {
                                sourceClassCopy = sourceClassCopy.WithModifiers(cls.Modifiers);
                            }

                            // Update inheritance/base types if InheritanceModified is set
                            if (GetAttributeBoolPropertyFromAttributeLists(cls.AttributeLists, _inheritanceModifiedProperty))
                            {
                                sourceClassCopy = sourceClassCopy.WithBaseList(cls.BaseList);

                                if (_verbosity >= Options.VerbosityOption.Info)
                                {
                                    Console.WriteLine("Class {0}:{1} inheritance updated", namespaceName, classNameFull);
                                }
                            }
                        }

                        // Check if destination class has any preserved members
                        bool hasPreservedMembers = cls.Members.Any(m => ShouldPreserveMember(m, out string _, out Options.VerbosityOption _) || GetAllCommentedCodeLines(m).Any());
                        ClassDeclarationSyntax mergedClass;

                        if (hasPreservedMembers)
                        {
                            // Merge with preserved members
                            mergedClass = MergeClassPreservingMarked(cls, sourceClassCopy);
                            if (_verbosity >= Options.VerbosityOption.Info)
                            {
                                int preservedCount = cls.Members.Count(m => ShouldPreserveMember(m, out string _, out Options.VerbosityOption _));
                                Console.WriteLine($"Merging class {namespaceName}:{classNameFull} while preserving {preservedCount} marked member(s)");
                            }
                        }
                        else
                        {
                            // No preserved members, use source as-is
                            mergedClass = sourceClassCopy;
                        }

                        // Compare if they're different
                        string mergedClassStr = mergedClass.NormalizeWhitespace().ToFullString();
                        if (classSource != mergedClassStr)
                        {
                            if (_verbosity >= Options.VerbosityOption.Info)
                            {
                                Console.WriteLine($"Updating class: {namespaceName}:{classNameFull}");
                            }
                            newRoot = newRoot.ReplaceNode(cls, mergedClass);
                            fileModified = true;
                        }
                    }
                    else
                    {
                        if (_verbosity >= Options.VerbosityOption.Error)
                        {
                            Console.WriteLine("*** Warning: Class not found in source files: {0}:{1}", namespaceName, classNameFull);
                        }
                    }
                }

                // Need to refresh the tree after class replacements
                if (fileModified)
                {
                    root = newRoot;
                }

                // Update interfaces
                var interfaces = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>().ToList();
                foreach (InterfaceDeclarationSyntax interfaceDecl in interfaces)
                {
                    string interfaceNameFull = GetInterfaceName(interfaceDecl, includeModifiers: true);
                    string interfaceNameBare = GetInterfaceName(interfaceDecl);
                    string interfaceSource = interfaceDecl.NormalizeWhitespace().ToFullString();
                    string namespaceName = GetNamespace(interfaceDecl);

                    if (_verbosity >= Options.VerbosityOption.Debug)
                    {
                        Console.WriteLine($"Interface: {interfaceNameFull}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(interfaceSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (HasSpecialTrivia(interfaceDecl, out string reason, out Options.VerbosityOption verbosity))
                    {
                        if (_verbosity >= verbosity)
                        {
                            Console.WriteLine("Skipping interface {0}:{1}, reason: {2}", namespaceName, interfaceNameFull, reason);
                        }
                        continue;
                    }

                    if (!_interfaceDict.TryGetValue(interfaceNameFull, out InterfaceDeclarationSyntax sourceInterface))
                    {
                        if (!_interfaceBareDict.TryGetValue(interfaceNameBare, out sourceInterface))
                        {
                            sourceInterface = null;
                        }
                    }

                    if (sourceInterface != null)
                    {
                        InterfaceDeclarationSyntax sourceInterfaceCopy = sourceInterface;
                        bool specialAttribute = HasSpecialSourceAttribute(interfaceDecl.AttributeLists);
                        if (specialAttribute)
                        {
                            if (_verbosity >= Options.VerbosityOption.Info)
                            {
                                Console.WriteLine("Interface {0}:{1} special attribute detected", namespaceName, interfaceNameFull);
                            }

                            // Update modifiers and attributes from destination interface
                            sourceInterfaceCopy = sourceInterfaceCopy.WithAttributeLists(interfaceDecl.AttributeLists);

                            // Update attributes if AccessModified is set
                            if (GetAttributeBoolPropertyFromAttributeLists(interfaceDecl.AttributeLists, _accessModifiedProperty))
                            {
                                sourceInterfaceCopy = sourceInterfaceCopy.WithModifiers(interfaceDecl.Modifiers);
                            }

                            // Update inheritance/base types if InheritanceModified is set
                            if (GetAttributeBoolPropertyFromAttributeLists(interfaceDecl.AttributeLists, _inheritanceModifiedProperty))
                            {
                                sourceInterfaceCopy = sourceInterfaceCopy.WithBaseList(interfaceDecl.BaseList);

                                if (_verbosity >= Options.VerbosityOption.Info)
                                {
                                    Console.WriteLine("Interface {0}:{1} inheritance updated", namespaceName, interfaceNameFull);
                                }
                            }
                        }

                        // Check for preserved members
                        bool hasPreservedMembers = interfaceDecl.Members.Any(m => ShouldPreserveMember(m, out string _, out Options.VerbosityOption _));
                        InterfaceDeclarationSyntax mergedInterface;

                        if (hasPreservedMembers)
                        {
                            mergedInterface = MergeInterfacePreservingMarked(interfaceDecl, sourceInterfaceCopy);
                            if (_verbosity >= Options.VerbosityOption.Info)
                            {
                                int preservedCount = interfaceDecl.Members.Count(m => ShouldPreserveMember(m, out string _, out Options.VerbosityOption _));
                                Console.WriteLine($"Merging interface {namespaceName}:{interfaceNameFull} while preserving {preservedCount} marked member(s)");
                            }
                        }
                        else
                        {
                            mergedInterface = sourceInterfaceCopy;
                        }

                        // Compare if they're different
                        string mergedInterfaceStr = mergedInterface.NormalizeWhitespace().ToFullString();
                        if (interfaceSource != mergedInterfaceStr)
                        {
                            if (_verbosity >= Options.VerbosityOption.Info)
                            {
                                Console.WriteLine($"Updating interface: {namespaceName}:{interfaceNameFull}");
                            }
                            newRoot = newRoot.ReplaceNode(interfaceDecl, mergedInterface);
                            fileModified = true;
                        }
                    }
                    else
                    {
                        if (_verbosity >= Options.VerbosityOption.Error)
                        {
                            Console.WriteLine("*** Warning: Interface not found in source files: {0}:{1}", namespaceName, interfaceNameFull);
                        }
                    }
                }

                // Update enums (not merge required)
                var enums = root.DescendantNodes().OfType<EnumDeclarationSyntax>().ToList();
                foreach (EnumDeclarationSyntax enumDecl in enums)
                {
                    string enumName = GetEnumName(enumDecl, includeModifiers: true);
                    string enumSource = enumDecl.NormalizeWhitespace().ToFullString();
                    string namespaceName = GetNamespace(enumDecl);

                    if (_verbosity >= Options.VerbosityOption.Debug)
                    {
                        Console.WriteLine($"Enum: {enumName}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(enumSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (HasSpecialTrivia(enumDecl, out string reason, out Options.VerbosityOption verbosity))
                    {
                        if (_verbosity >= verbosity)
                        {
                            Console.WriteLine("Skipping enum {0}:{1} reason: {2}", namespaceName, enumName, reason);
                        }
                        continue;
                    }

                    if (!_enumDict.TryGetValue(enumName, out EnumDeclarationSyntax sourceEnum))
                    {
                        if (!_enumBareDict.TryGetValue(GetEnumName(enumDecl), out sourceEnum))
                        {
                            sourceEnum = null;
                        }
                    }

                    if (sourceEnum != null)
                    {
                        EnumDeclarationSyntax sourceEnumCopy = sourceEnum;
                        bool specialAttribute = HasSpecialSourceAttribute(enumDecl.AttributeLists);
                        if (specialAttribute)
                        {
                            if (_verbosity >= Options.VerbosityOption.Info)
                            {
                                Console.WriteLine("Enum {0}:{1} special attribute detected", namespaceName, enumName);
                            }
                            // Update modifiers and attributes from destination enum
                            sourceEnumCopy = sourceEnumCopy
                                .WithModifiers(enumDecl.Modifiers)
                                .WithAttributeLists(enumDecl.AttributeLists);
                        }

                        // Compare if they're different
                        string sourceEnumStr = sourceEnumCopy.NormalizeWhitespace().ToFullString();
                        if (enumSource != sourceEnumStr)
                        {
                            if (_verbosity >= Options.VerbosityOption.Info)
                            {
                                Console.WriteLine($"Updating enum: {namespaceName}:{enumName}");
                            }
                            newRoot = newRoot.ReplaceNode(enumDecl, sourceEnumCopy);
                            fileModified = true;
                        }
                    }
                    else
                    {
                        if (_verbosity >= Options.VerbosityOption.Error)
                        {
                            Console.WriteLine("*** Warning: Enum not found in source files: {0}:{1}", namespaceName, enumName);
                        }
                    }
                }

                // Write the modified file back if changes were made
                if (fileModified)
                {
                    string modifiedContent = newRoot.NormalizeWhitespace().ToFullString();

                    File.WriteAllText(fileName, modifiedContent, new UTF8Encoding(true));
                    if (_verbosity >= Options.VerbosityOption.Info)
                    {
                        Console.WriteLine($"File updated: {fileName}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("*** Error parsing file: {0}, Exception {1}", fileName, e.Message);
                return false;
            }
            return true;
        }

        public static string GetModifiersText(SyntaxTokenList syntaxTokenList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (SyntaxToken modifier in syntaxTokenList)
            {
                if (modifier.IsKeyword())
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("_");
                    }
                    sb.Append(modifier.ValueText);
                }
            }

            return sb.ToString();
        }

        public static string GetBaseTypesText(SeparatedSyntaxList<BaseTypeSyntax> baseTypeList)
        {
            List<string> baseTypeNames = baseTypeList
                .Select(bt => bt.Type.ToString())
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();

            StringBuilder sb = new StringBuilder();
            foreach (string typeName in baseTypeNames)
            {
                if (sb.Length > 0)
                {
                    sb.Append("_");
                }
                sb.Append(typeName);
            }

            return sb.ToString();
        }

        public static string GetClassName(ClassDeclarationSyntax classDeclaration, bool includeModifiers = false, bool includeNamespace = false)
        {
            string className = classDeclaration.Identifier.ValueText;
            if (includeNamespace)
            {
                string namespaceName = GetNamespace(classDeclaration);
                if (!string.IsNullOrEmpty(namespaceName))
                {
                    className = $"{namespaceName}.{className}";
                }
            }
            else
            {
                if (includeModifiers)
                {
                    string modifiers = GetModifiersText(classDeclaration.Modifiers);
                    if (modifiers.Length > 0)
                    {
                        className = $"{modifiers}_{className}";
                    }
                }

                int typeParamCount = classDeclaration.TypeParameterList?.Parameters.Count ?? 0;
                if (typeParamCount > 0)
                {
                    className += $"_<{typeParamCount}>";
                }

                if (includeModifiers)
                {
                    if (classDeclaration.BaseList != null)
                    {
                        className += ":" + GetBaseTypesText(classDeclaration.BaseList.Types);
                    }
                }
            }
            return className;
        }

        public static string GetInterfaceName(InterfaceDeclarationSyntax interfaceDeclaration, bool includeModifiers = false, bool includeNamespace = false)
        {
            string interfaceName = interfaceDeclaration.Identifier.ValueText;
            if (includeNamespace)
            {
                string namespaceName = GetNamespace(interfaceDeclaration);
                if (!string.IsNullOrEmpty(namespaceName))
                {
                    interfaceName = $"{namespaceName}.{interfaceName}";
                }
            }
            else
            {
                if (includeModifiers)
                {
                    string modifiers = GetModifiersText(interfaceDeclaration.Modifiers);
                    if (modifiers.Length > 0)
                    {
                        interfaceName = $"{modifiers}_{interfaceName}";
                    }
                }

                int typeParamCount = interfaceDeclaration.TypeParameterList?.Parameters.Count ?? 0;
                if (typeParamCount > 0)
                {
                    interfaceName += $"_<{typeParamCount}>";
                }

                if (interfaceDeclaration.BaseList != null)
                {
                    interfaceName += ":" + GetBaseTypesText(interfaceDeclaration.BaseList.Types);
                }
            }

            return interfaceName;
        }

        public static string GetEnumName(EnumDeclarationSyntax enumDeclaration, bool includeModifiers = false, bool includeNamespace = false)
        {
            string enumName = enumDeclaration.Identifier.ValueText;
            if (includeNamespace)
            {
                string namespaceName = GetNamespace(enumDeclaration);
                if (!string.IsNullOrEmpty(namespaceName))
                {
                    enumName = $"{namespaceName}.{enumName}";
                }
            }
            else
            {
                if (includeModifiers)
                {
                    string modifiers = GetModifiersText(enumDeclaration.Modifiers);
                    if (modifiers.Length > 0)
                    {
                        enumName = $"{modifiers}_{enumName}";
                    }
                }
            }

            return enumName;
        }

        public static string GetNamespace(SyntaxNode node)
        {
            var namespaceDeclaration = node.Ancestors()
                .OfType<BaseNamespaceDeclarationSyntax>()
                .FirstOrDefault();

            return namespaceDeclaration?.Name.ToString() ?? string.Empty;
        }

        public static string GetRelativePath(string basePath, string fullPath)
        {
            // Require trailing backslash for path
            if (!basePath.EndsWith("\\"))
            {
                basePath += "\\";
            }

            Uri baseUri = new Uri(basePath);
            Uri fullUri = new Uri(fullPath);

            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);

            // Uri's use forward slashes so convert back to backward slashes
            return relativeUri.ToString().Replace("/", "\\");
        }

        /// <summary>
        /// Checks if a class declaration has any special trivia
        /// </summary>
        public static bool HasSpecialTrivia(ClassDeclarationSyntax classDeclaration, out string reason, out Options.VerbosityOption verbosity)
        {
            reason = string.Empty;
            verbosity = Options.VerbosityOption.Important;

            if (ShouldPreserveMember(classDeclaration, out reason, out verbosity))
            {
                return true;
            }

            if ((classDeclaration.HasLeadingTrivia && HasSpecialTrivia(classDeclaration.GetLeadingTrivia())) ||
                (classDeclaration.HasTrailingTrivia && HasSpecialTrivia(classDeclaration.GetTrailingTrivia())))
            {
                reason = "Comment in class declaration";
                return true;
            }

            // Check all descendant tokens (comments inside the class)
            foreach (SyntaxToken token in classDeclaration.DescendantTokens(descendIntoTrivia: true))
            {
                if ((token.HasLeadingTrivia && HasSpecialTrivia(token.LeadingTrivia)) ||
                    (token.HasTrailingTrivia && HasSpecialTrivia(token.TrailingTrivia)))
                {
                    reason = "Comment in class";
                    return true;
                }
            }

            return false;
        }

        public static bool HasSpecialTrivia(InterfaceDeclarationSyntax interfaceDeclaration, out string reason, out Options.VerbosityOption verbosity)
        {
            reason = string.Empty;
            verbosity = Options.VerbosityOption.Important;

            if (ShouldPreserveMember(interfaceDeclaration, out reason, out verbosity))
            {
                return true;
            }

            if ((interfaceDeclaration.HasLeadingTrivia && HasSpecialTrivia(interfaceDeclaration.GetLeadingTrivia())) ||
                (interfaceDeclaration.HasTrailingTrivia && HasSpecialTrivia(interfaceDeclaration.GetTrailingTrivia())))
            {
                reason = "Comment in interface declaration";
                return true;
            }

            // Check all descendant tokens (comments inside the interface)
            foreach (var token in interfaceDeclaration.DescendantTokens(descendIntoTrivia: true))
            {
                if ((token.HasLeadingTrivia && HasSpecialTrivia(token.LeadingTrivia)) ||
                    (token.HasTrailingTrivia && HasSpecialTrivia(token.TrailingTrivia)))
                {
                    reason = "Comment in interface";
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Same comment detection methods for enums
        /// </summary>
        public static bool HasSpecialTrivia(EnumDeclarationSyntax enumDeclaration, out string reason, out Options.VerbosityOption verbosity)
        {
            reason = string.Empty;
            verbosity = Options.VerbosityOption.Important;

            if (ShouldPreserveMember(enumDeclaration, out reason, out verbosity))
            {
                return true;
            }

            if ((enumDeclaration.HasLeadingTrivia && HasSpecialTrivia(enumDeclaration.GetLeadingTrivia())) ||
                (enumDeclaration.HasTrailingTrivia && HasSpecialTrivia(enumDeclaration.GetTrailingTrivia())))
            {
                reason = "Comment in enum declaration";
                return true;
            }

            foreach (var token in enumDeclaration.DescendantTokens(descendIntoTrivia: true))
            {
                if ((token.HasLeadingTrivia && HasSpecialTrivia(token.LeadingTrivia)) ||
                    (token.HasTrailingTrivia && HasSpecialTrivia(token.TrailingTrivia)))
                {
                    reason = "Comment in enum";
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a trivia list contains special trivia
        /// </summary>
        public static bool HasSpecialTrivia(SyntaxTriviaList triviaList, bool includeComments = true, bool includePreprocessor = false)
        {
            if (includeComments)
            {
                foreach (SyntaxTrivia trivia in triviaList)
                {
                    if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||           // //
                        trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||   // /* */
                        trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || // ///
                        trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))    // /** */
                    {
                        string comment = trivia.ToString();
                        if (comment.Contains("TODO:", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (comment.Contains("[IGNORE]", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (comment.TrimStart().StartsWith(_commentedRemoveCodeMarker, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (comment.TrimStart().StartsWith(_commentedAddCodeMarker, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        return true;
                    }
                }
            }

            if (includePreprocessor)
            {
                foreach (SyntaxTrivia trivia in triviaList)
                {
                    if (trivia.IsKind(SyntaxKind.IfDirectiveTrivia) ||
                        trivia.IsKind(SyntaxKind.ElifDirectiveTrivia) ||
                        trivia.IsKind(SyntaxKind.ElseDirectiveTrivia) ||
                        trivia.IsKind(SyntaxKind.EndIfDirectiveTrivia) ||
                        trivia.IsKind(SyntaxKind.DefineDirectiveTrivia) ||
                        trivia.IsKind(SyntaxKind.UndefDirectiveTrivia) ||
                        trivia.IsKind(SyntaxKind.RegionDirectiveTrivia) ||
                        trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia) ||
                        trivia.IsKind(SyntaxKind.PragmaWarningDirectiveTrivia) ||
                        trivia.IsKind(SyntaxKind.PragmaChecksumDirectiveTrivia) ||
                        trivia.IsKind(SyntaxKind.ReferenceDirectiveTrivia) ||
                        trivia.IsKind(SyntaxKind.LoadDirectiveTrivia) ||
                        trivia.IsKind(SyntaxKind.NullableDirectiveTrivia) ||
                        trivia.IsKind(SyntaxKind.BadDirectiveTrivia))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool HasContractAttribute(SyntaxList<AttributeListSyntax> attributeLists)
        {
            return attributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr =>
                {
                    string attrName = attr.Name.ToString();
                    switch (attrName)
                    {
                        case "ServiceContract":
                        case "DataContract":
                        case "OperationContract":
                            return true;
                    }
                    return false;
                });
        }

        public static bool HasSpecialSourceAttribute(SyntaxList<AttributeListSyntax> attributeLists)
        {
            return attributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr =>
                {
                    string attrName = attr.Name.ToString();
                    switch (attrName)
                    {
                        case _attributPreserveSource:
                            if (GetAttributeBoolProperty(attr, _accessModifiedProperty) ||
                                GetAttributeBoolProperty(attr, _inheritanceModifiedProperty) ||
                                GetAttributeBoolProperty(attr, _attributesModifiedProperty))
                            {
                                return true;
                            }
                            break;
                    }
                    return false;
                });
        }

        public static string GetOriginalHashAttribute(SyntaxList<AttributeListSyntax> attributeLists)
        {
            foreach (AttributeListSyntax attributeListSyntax in attributeLists)
            {
                foreach (AttributeSyntax attr in attributeListSyntax.Attributes)
                {
                    string attrName = attr.Name.ToString();
                    switch (attrName)
                    {
                        case _attributPreserveSource:
                        {
                            string hash = GetAttributeStringProperty(attr, _attributesOriginalHashProperty);
                            if (hash != null)
                            {
                                return hash;
                            }
                            break;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a boolean property value from attribute lists
        /// </summary>
        private static bool GetAttributeBoolPropertyFromAttributeLists(SyntaxList<AttributeListSyntax> attributeLists, string propertyName)
        {
            return attributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr =>
                {
                    string attrName = attr.Name.ToString();
                    if (attrName == _attributPreserveSource)
                    {
                        return GetAttributeBoolProperty(attr, propertyName);
                    }
                    return false;
                });
        }

        /// <summary>
        /// Renames all constructors in a class to match the new class name
        /// </summary>
        private static ClassDeclarationSyntax RenameConstructors(
            ClassDeclarationSyntax classDeclaration,
            string oldClassName,
            string newClassName)
        {
            var constructors = classDeclaration.Members
                .OfType<ConstructorDeclarationSyntax>()
                .Where(c => c.Identifier.Text == oldClassName)
                .ToList();

            if (!constructors.Any())
            {
                return classDeclaration;
            }

            ClassDeclarationSyntax updatedClass = classDeclaration;

            foreach (var constructor in constructors)
            {
                var newIdentifier = SyntaxFactory.Identifier(
                    constructor.Identifier.LeadingTrivia,
                    newClassName,
                    constructor.Identifier.TrailingTrivia
                );

                var newConstructor = constructor.WithIdentifier(newIdentifier);
                updatedClass = updatedClass.ReplaceNode(constructor, newConstructor);
            }

            return updatedClass;
        }

        /// <summary>
        /// Merges signature from source member with body from preserved member
        /// </summary>
        private static MemberDeclarationSyntax MergeMemberSignatureWithBody(
            MemberDeclarationSyntax sourceMember,
            MemberDeclarationSyntax preservedMember)
        {
            MemberDeclarationSyntax result = sourceMember switch
            {
                MethodDeclarationSyntax sourceMethod when preservedMember is MethodDeclarationSyntax preservedMethod =>
                    sourceMethod
                        .WithParameterList(preservedMethod.ParameterList)
                        .WithReturnType(preservedMethod.ReturnType)
                        .WithModifiers(preservedMethod.Modifiers)
                        .WithExpressionBody(preservedMethod.ExpressionBody)
                        .WithSemicolonToken(preservedMethod.SemicolonToken)
                        .WithAttributeLists(preservedMember.AttributeLists),

                PropertyDeclarationSyntax sourceProperty when preservedMember is PropertyDeclarationSyntax preservedProperty =>
                    sourceProperty
                        .WithAccessorList(preservedProperty.AccessorList)
                        .WithModifiers(preservedProperty.Modifiers)
                        .WithExpressionBody(preservedProperty.ExpressionBody)
                        .WithInitializer(preservedProperty.Initializer)
                        .WithSemicolonToken(preservedProperty.SemicolonToken)
                        .WithAttributeLists(preservedMember.AttributeLists),

                ConstructorDeclarationSyntax sourceCtor when preservedMember is ConstructorDeclarationSyntax preservedCtor =>
                    sourceCtor
                        .WithParameterList(preservedCtor.ParameterList)
                        .WithModifiers(preservedCtor.Modifiers)
                        .WithExpressionBody(preservedCtor.ExpressionBody)
                        .WithInitializer(preservedCtor.Initializer)
                        .WithSemicolonToken(preservedCtor.SemicolonToken)
                        .WithAttributeLists(preservedMember.AttributeLists),

                IndexerDeclarationSyntax sourceIndexer when preservedMember is IndexerDeclarationSyntax preservedIndexer =>
                    sourceIndexer
                        .WithParameterList(preservedIndexer.ParameterList)
                        .WithModifiers(preservedIndexer.Modifiers)
                        .WithAccessorList(preservedIndexer.AccessorList)
                        .WithExpressionBody(preservedIndexer.ExpressionBody)
                        .WithSemicolonToken(preservedIndexer.SemicolonToken)
                        .WithAttributeLists(preservedMember.AttributeLists),

                FieldDeclarationSyntax sourceField when preservedMember is FieldDeclarationSyntax preservedField =>
                    sourceField
                        .WithModifiers(preservedField.Modifiers)
                        .WithDeclaration(sourceField.Declaration
                            .WithVariables(SyntaxFactory.SeparatedList(
                                sourceField.Declaration.Variables.Zip(preservedField.Declaration.Variables,
                                    (sourceVar, preservedVar) => sourceVar.WithInitializer(preservedVar.Initializer)))))
                        .WithAttributeLists(preservedMember.AttributeLists),

                _ => preservedMember
            };

            return result;
        }

        /// <summary>
        /// Merges source class into destination, preserving marked members and //[-]//[+] comments
        /// </summary>
        public static ClassDeclarationSyntax MergeClassPreservingMarked(
            ClassDeclarationSyntax destClass,
            ClassDeclarationSyntax sourceClass)
        {
            // Get all members from destination that should be preserved
            List<MemberDeclarationSyntax> preservedMembers = destClass.Members
                .Where(m => ShouldPreserveMember(m, out string _, out Options.VerbosityOption _))
                .ToList();

            // Build a new member list, maintaining source order
            List<MemberDeclarationSyntax> newMembers = new List<MemberDeclarationSyntax>();
            HashSet<MemberDeclarationSyntax> processedPreservedMembers = new HashSet<MemberDeclarationSyntax>();

            // Iterate through source members and replace with preserved versions where needed
            foreach (MemberDeclarationSyntax sourceMember in sourceClass.Members)
            {
                string sourceMemberName = GetMemberName(sourceMember);

                MemberDeclarationSyntax destMember = destClass.Members
                    .FirstOrDefault(dm => !processedPreservedMembers.Contains(dm) &&
                                          GetMemberName(dm) == sourceMemberName);

                // Find matching preserved member by name
                MemberDeclarationSyntax matchingPreservedMember = preservedMembers
                    .FirstOrDefault(pm => !processedPreservedMembers.Contains(pm) &&
                                          GetMemberName(pm) == sourceMemberName);

                if (matchingPreservedMember != null)
                {
                    MemberDeclarationSyntax memberToAdd = matchingPreservedMember;

                    // Hash validation
                    string originalHash = GetOriginalHashAttribute(memberToAdd.AttributeLists);
                    if (originalHash != null)
                    {
                        // Calculate current hash of source member
                        string currentHash = sourceMember switch
                        {
                            MethodDeclarationSyntax method => CalculateMethodHash(method),
                            FieldDeclarationSyntax field => CalculateMethodHash(field),
                            PropertyDeclarationSyntax property => CalculatePropertyHash(property),
                            ConstructorDeclarationSyntax constructor => CalculateConstructorHash(constructor),
                            _ => null
                        };

                        if (currentHash != null && string.Compare(currentHash, originalHash, StringComparison.Ordinal) != 0)
                        {
                            if (_verbosity >= Options.VerbosityOption.Error)
                            {
                                string className = GetClassName(sourceClass);
                                Console.WriteLine($"Class {className}, Member {sourceMemberName}, hash mismatch: '{currentHash}'");
                            }
                        }
                    }

                    // Check if SignatureModified is set - if so, merge signature from source with body from preserved
                    if (GetAttributeBoolPropertyFromAttributeLists(matchingPreservedMember.AttributeLists, _signatureModifiedProperty))
                    {
                        memberToAdd = MergeMemberSignatureWithBody(sourceMember, matchingPreservedMember);
                        if (_verbosity >= Options.VerbosityOption.Info)
                        {
                            Console.WriteLine($"Member {sourceMemberName} signature preserved, body updated");
                        }

                        if (destMember != null)
                        {
                            // Additionally preserve //[-]//[+] comments inside body
                            string className = GetClassName(destClass);
                            memberToAdd = PreserveCommentedCodeInsideBody(destMember, memberToAdd, className);
                        }
                    }

                    // Use preserved version (or merged version) at the same position as source
                    newMembers.Add(memberToAdd);
                    processedPreservedMembers.Add(matchingPreservedMember);
                }
                else
                {
                    // Not preserved - use source, but preserve //[-]//[+] comments inside body
                    MemberDeclarationSyntax memberToAdd = sourceMember;
                    if (destMember != null)
                    {
                        // Then preserve //[-]//[+] comments inside body
                        string className = GetClassName(destClass);
                        memberToAdd = PreserveCommentedCodeInsideBody(destMember, memberToAdd, className);
                    }
                    newMembers.Add(memberToAdd);
                }
            }

            foreach (var preservedMember in preservedMembers)
            {
                if (!processedPreservedMembers.Contains(preservedMember))
                {
                    newMembers.Add(preservedMember);
                }
            }

            return sourceClass.WithMembers(SyntaxFactory.List(newMembers));
        }

        /// <summary>
        /// Merges source interface into destination, preserving marked members at their original positions
        /// </summary>
        public static InterfaceDeclarationSyntax MergeInterfacePreservingMarked(
            InterfaceDeclarationSyntax destInterface,
            InterfaceDeclarationSyntax sourceInterface)
        {
            var preservedMembers = destInterface.Members
                .Where(m => ShouldPreserveMember(m, out string _, out Options.VerbosityOption _))
                .ToList();

            if (!preservedMembers.Any())
            {
                return sourceInterface;
            }

            var newMembers = new List<MemberDeclarationSyntax>();
            var processedPreservedMembers = new HashSet<MemberDeclarationSyntax>();

            // Iterate through source members and replace with preserved versions where needed
            foreach (var sourceMember in sourceInterface.Members)
            {
                string sourceMemberName = GetMemberName(sourceMember);

                // Find matching preserved member by name
                var matchingPreservedMember = preservedMembers
                    .FirstOrDefault(pm => !processedPreservedMembers.Contains(pm) &&
                                          GetMemberName(pm) == sourceMemberName);

                if (matchingPreservedMember != null)
                {
                    // Use preserved version at the same position as source
                    newMembers.Add(matchingPreservedMember);
                    processedPreservedMembers.Add(matchingPreservedMember);
                }
                else
                {
                    // Use source version
                    newMembers.Add(sourceMember);
                }
            }

            // Add any preserved members that didn't exist in source at the end
            foreach (var preservedMember in preservedMembers)
            {
                if (!processedPreservedMembers.Contains(preservedMember))
                {
                    newMembers.Add(preservedMember);
                }
            }

            return sourceInterface.WithMembers(SyntaxFactory.List(newMembers));
        }

        /// <summary>
        /// Checks if a member has a preserve marker
        /// </summary>
        public static bool ShouldPreserveMember(SyntaxNode member, out string reason, out Options.VerbosityOption verbosity)
        {
            reason = string.Empty;
            verbosity = Options.VerbosityOption.Important;

            // Check for attributes like [Preserve] or [DoNotSync]
            if (member is MemberDeclarationSyntax memberDecl)
            {
                AttributeSyntax preserveAttribute = memberDecl.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .FirstOrDefault(attr =>
                    {
                        string attrName = attr.Name.ToString();
                        return attrName == _attributPreserveSource;
                    });

                if (preserveAttribute != null)
                {
                    // Check if AccessModified property is set to true
                    if (GetAttributeBoolProperty(preserveAttribute, _accessModifiedProperty) ||
                        GetAttributeBoolProperty(preserveAttribute, _inheritanceModifiedProperty) ||
                        GetAttributeBoolProperty(preserveAttribute, _attributesModifiedProperty))
                    {
                        return false;
                    }

                    StringBuilder sbReason = new StringBuilder();
                    string hint = GetAttributeStringProperty(preserveAttribute, _attributesHintProperty);
                    if (!string.IsNullOrEmpty(hint))
                    {
                        sbReason.Append($"\"{hint}\"");
                    }

                    if (GetAttributeBoolProperty(preserveAttribute, _removedProperty))
                    {
                        verbosity = Options.VerbosityOption.Info;
                        if (sbReason.Length > 0)
                        {
                            sbReason.Append(" ");
                        }
                        sbReason.Append($"[{_removedProperty}]");
                    }

                    if (GetAttributeBoolProperty(preserveAttribute, _suppressWarningProperty))
                    {
                        verbosity = Options.VerbosityOption.Info;
                        if (sbReason.Length > 0)
                        {
                            sbReason.Append(" ");
                        }
                        sbReason.Append($"[{_suppressWarningProperty}]");
                    }

                    if (GetAttributeBoolProperty(preserveAttribute, _attributesModifiedProperty))
                    {
                        if (sbReason.Length > 0)
                        {
                            sbReason.Append(" ");
                        }
                        sbReason.Append($"[{_attributesModifiedProperty}]");
                    }

                    reason = sbReason.ToString();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Preserves //[-]//[+] commented code lines inside method bodies and other members
        /// </summary>
        private static MemberDeclarationSyntax PreserveCommentedCodeInsideBody(
            MemberDeclarationSyntax destMember,
            MemberDeclarationSyntax sourceMember,
            string className)
        {
            // Get all //[-]//[+] comments from destination member (including inside body)
            List<string> destCommentedLines = GetAllCommentedCodeLines(destMember);

            if (!destCommentedLines.Any())
            {
                return sourceMember;
            }

            string destMemberName = GetMemberName(destMember);
            string locationInfo = $"{className}.{destMemberName}";
            // Get the source code as string and work with it line by line
            string sourceCode = sourceMember.ToFullString();
            string destCode = destMember.ToFullString();

            // Parse dest code to find //[-]//[+] lines and their context
            List<CommentedCodeLineInfo> linesToPreserve = FindCommentedCodeLinesWithContext(destCode);

            if (!linesToPreserve.Any())
            {
                return sourceMember;
            }

            // Try to insert //[-]//[+] lines into source code at appropriate positions
            string mergedCode = MergeCommentedCodeLines(sourceCode, linesToPreserve, locationInfo);

            if (mergedCode == sourceCode)
            {
                return sourceMember;
            }

            // Wrap in appropriate context for parsing
            string wrapperClassName = sourceMember is ConstructorDeclarationSyntax ctor
                ? ctor.Identifier.Text
                : "__DummyClass__";

            string wrappedCode = $@"class {wrapperClassName}
{{
{mergedCode}
}}";

            // Parse the wrapped code
            SyntaxTree mergedTree = CSharpSyntaxTree.ParseText(wrappedCode);
            var diagnostics = mergedTree.GetDiagnostics();
            List<Diagnostic> errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

            if (errors.Any())
            {
                if (_verbosity >= Options.VerbosityOption.Error)
                {
                    string memberName = GetMemberName(sourceMember);
                    Console.WriteLine($"*** Parsing errors in merged code for member {memberName}:");
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"    {error.GetMessage()}");
                    }

                    Console.WriteLine("-------------------------------");
                    Console.Write(mergedCode);
                    Console.WriteLine("-------------------------------");
                }

                // Return original source member if parsing failed
                return sourceMember;
            }

            SyntaxNode mergedRoot = mergedTree.GetRoot();

            // Extract the member from the wrapper class
            ClassDeclarationSyntax wrapperClass = mergedRoot.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();

            MemberDeclarationSyntax mergedMember = wrapperClass?.Members.FirstOrDefault();

            return mergedMember ?? sourceMember;
        }

        /// <summary>
        /// Gets all //[-]//[+] commented code lines from a member, including inside bodies
        /// </summary>
        private static List<string> GetAllCommentedCodeLines(SyntaxNode member)
        {
            List<string> result = new List<string>();

            foreach (SyntaxTrivia trivia in member.DescendantTrivia(descendIntoTrivia: true))
            {
                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia))
                {
                    string comment = trivia.ToString();
                    if (comment.TrimStart().StartsWith(_commentedRemoveCodeMarker, StringComparison.OrdinalIgnoreCase) ||
                        comment.TrimStart().StartsWith(_commentedAddCodeMarker, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(comment);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Finds all //[-]//[+] comment lines with their surrounding context
        /// </summary>
        private static List<CommentedCodeLineInfo> FindCommentedCodeLinesWithContext(string code)
        {
            List<CommentedCodeLineInfo> result = new List<CommentedCodeLineInfo>();
            string[] lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            CommentedCodeLineInfo rootAddInfo = null;
            CommentedCodeLineInfo lastAddInfo = null;
            bool ignoreNextLine = false;

            for (int i = 0; i < lines.Length; i++)
            {
                if (ignoreNextLine)
                {
                    ignoreNextLine = false;
                    continue;
                }

                string trimmedLine = lines[i].Trim();
                bool isRemoveLine = trimmedLine.StartsWith(_commentedRemoveCodeMarker, StringComparison.OrdinalIgnoreCase);
                bool isAddLine = trimmedLine.StartsWith(_commentedAddCodeMarker, StringComparison.OrdinalIgnoreCase);

                int? index = null;
                Match match = _commentedCodeWithIndexPattern.Match(trimmedLine);
                if (match.Success && match.Groups.Count > 1)
                {
                    if (int.TryParse(match.Groups[1].Value, out int parsedIndex))
                    {
                        index = parsedIndex;
                    }
                }

                if (isRemoveLine || isAddLine)
                {
                    int nextLine = isAddLine ? 2 : 1;
                    CommentedCodeLineInfo info = new CommentedCodeLineInfo
                    {
                        CommentLine = lines[i],
                        PrecedingCodeLine = i > 0 ? NormalizeCodeLine(lines[i - 1], true) : null,
                        FollowingCodeLine = i < lines.Length - nextLine ? NormalizeCodeLine(lines[i + nextLine], true) : null,
                        OriginalLineNumber = i,
                        Index = index
                    };

                    if (isAddLine && lastAddInfo != null)
                    {
                        // Chain added lines
                        lastAddInfo.NextInfo = info;
                        rootAddInfo.FollowingCodeLine = info.FollowingCodeLine;
                    }
                    else
                    {
                        result.Add(info);
                    }

                    if (isAddLine)
                    {
                        if (rootAddInfo == null)
                        {
                            rootAddInfo = info;
                        }

                        lastAddInfo = info;
                        ignoreNextLine = true;
                    }
                    else
                    {
                        rootAddInfo = null;
                        lastAddInfo = null;
                    }
                }
                else
                {
                    rootAddInfo = null;
                    lastAddInfo = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Normalizes a code line for comparison (removes whitespace)
        /// </summary>
        private static string NormalizeCodeLine(string line, bool ignoreAddedLine = false)
        {
            if (string.IsNullOrEmpty(line))
            {
                return null;
            }

            string trimmed = line.Trim();

            // Skip empty lines and other comments
            if (string.IsNullOrEmpty(trimmed) ||
                trimmed.StartsWith("//", StringComparison.Ordinal) ||
                trimmed.StartsWith("/*", StringComparison.Ordinal))
            {
                if (trimmed.StartsWith(_commentedRemoveCodeMarker, StringComparison.Ordinal))
                {
                    // Remove the marker and optional (index) part
                    trimmed = RemoveCommentMarkerWithIndex(trimmed, _commentedRemoveCodeMarker);
                }
                else if (trimmed.StartsWith(_commentedAddCodeMarker, StringComparison.Ordinal))
                {
                    if (ignoreAddedLine)
                    {
                        return null;
                    }

                    // Remove the marker and optional (index) part
                    trimmed = RemoveCommentMarkerWithIndex(trimmed, _commentedAddCodeMarker);
                }
                else
                {
                    return null;
                }
            }

            return Regex.Replace(trimmed, @"\s+", "");
        }

        /// <summary>
        /// Removes the comment marker and optional (index) part from a line
        /// </summary>
        private static string RemoveCommentMarkerWithIndex(string line, string marker)
        {
            // First remove the marker
            string result = line.Substring(marker.Length);

            // Check if there's an (index) part to remove
            if (result.StartsWith("(", StringComparison.Ordinal))
            {
                int closeParenIndex = result.IndexOf(')', StringComparison.Ordinal);
                if (closeParenIndex > 0)
                {
                    // Remove the (index) part
                    result = result.Substring(closeParenIndex + 1);
                }
            }

            return result.TrimStart();
        }

        /// <summary>
        /// Merges //[-]//[+] commented code lines from dest into source code
        /// </summary>
        private static string MergeCommentedCodeLines(string sourceCode, List<CommentedCodeLineInfo> linesToPreserve, string locationInfo)
        {
            List<string> sourceLines = sourceCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> normalizedLines = sourceLines
                .Select(line => NormalizeCodeLine(line, true))
                .Where(line => !string.IsNullOrEmpty(line))
                .ToList();
            List<(int lineIndex, string commentLine)> insertions = new List<(int lineIndex, string commentLine)>();

            foreach (CommentedCodeLineInfo commentInfo in linesToPreserve)
            {
                string trimmedLine = commentInfo.CommentLine.Trim();
                bool isRemoveLine = trimmedLine.StartsWith(_commentedRemoveCodeMarker, StringComparison.OrdinalIgnoreCase);
                bool isAddLine = trimmedLine.StartsWith(_commentedAddCodeMarker, StringComparison.OrdinalIgnoreCase);

                if (isRemoveLine)
                {
                    // Try to find and remove the corresponding code line in source
                    string normalizedCodeLineToRemove = NormalizeCodeLine(commentInfo.CommentLine);
                    bool lineRemoved = false;
                    int matchCount = 0;
                    for (int i = 0; i < sourceLines.Count; i++)
                    {
                        string normalizedSourceLine = NormalizeCodeLine(sourceLines[i]);
                        if (normalizedSourceLine != null &&
                            string.Compare(normalizedSourceLine, normalizedCodeLineToRemove, StringComparison.Ordinal) == 0)
                        {
                            string sourceTrimmed = sourceLines[i].Trim();
                            if (sourceTrimmed.StartsWith(_commentedRemoveCodeMarker, StringComparison.OrdinalIgnoreCase) ||
                                sourceTrimmed.StartsWith(_commentedAddCodeMarker, StringComparison.OrdinalIgnoreCase))
                            {
                                matchCount++;
                                continue;
                            }

                            string precedingLine = commentInfo.PrecedingCodeLine;
                            string followingLine = commentInfo.FollowingCodeLine;
                            // Verify context lines match
                            bool precedingMatches =
                                precedingLine == null ||
                                (i > 0 && string.Compare(NormalizeCodeLine(sourceLines[i - 1]), precedingLine, StringComparison.Ordinal) == 0);
                            bool followingMatches =
                                followingLine == null ||
                                (i + 1 < sourceLines.Count && string.Compare(NormalizeCodeLine(sourceLines[i + 1]), followingLine, StringComparison.Ordinal) == 0);

                            int matchLines = normalizedLines.Count(line => string.Compare(line, normalizedSourceLine, StringComparison.Ordinal) == 0);
                            bool validLine = precedingMatches || followingMatches;
                            if (matchLines > 1)
                            {
                                validLine = precedingMatches && followingMatches;
                            }

                            if (validLine)
                            {
                                if (commentInfo.Index != null && commentInfo.Index != matchCount)
                                {
                                    matchCount++;
                                    continue;
                                }

                                // Replace the code line with the comment line
                                sourceLines[i] = commentInfo.CommentLine;
                                lineRemoved = true;
                                break;
                            }
                        }
                    }

                    if (!lineRemoved)
                    {
                        if (_verbosity >= Options.VerbosityOption.Error)
                        {
                            Console.WriteLine($"No matching line found in '{locationInfo}' to remove: {trimmedLine}");
                        }
                    }

                    continue;
                }

                if (isAddLine)
                {
                    int insertIndex = -1;

                    // Strategy 1: Find by preceding line
                    if (commentInfo.PrecedingCodeLine != null)
                    {
                        int matchCount = 0;
                        for (int i = 0; i < sourceLines.Count; i++)
                        {
                            string normalizedSourceLine = NormalizeCodeLine(sourceLines[i]);
                            if (normalizedSourceLine != null &&
                                string.Compare(normalizedSourceLine, commentInfo.PrecedingCodeLine, StringComparison.Ordinal) == 0)
                            {
                                if (i + 1 < sourceLines.Count)
                                {
                                    insertIndex = i + 1;
                                    if (commentInfo.Index != null && commentInfo.Index == matchCount)
                                    {
                                        matchCount = 1;
                                        break;
                                    }
                                }

                                matchCount++;
                            }
                        }

                        // If multiple matches found, invalidate this strategy
                        if (matchCount > 1)
                        {
                            insertIndex = -1;
                        }
                    }

                    // Strategy 2: Find by following line
                    if (insertIndex == -1 && commentInfo.FollowingCodeLine != null)
                    {
                        int matchCount = 0;
                        for (int i = 0; i < sourceLines.Count; i++)
                        {
                            string normalizedSourceLine = NormalizeCodeLine(sourceLines[i]);
                            if (normalizedSourceLine != null &&
                                string.Compare(normalizedSourceLine, commentInfo.FollowingCodeLine, StringComparison.Ordinal) == 0)
                            {
                                if (i > 0)
                                {
                                    insertIndex = i;
                                    if (commentInfo.Index != null && commentInfo.Index == matchCount)
                                    {
                                        matchCount = 1;
                                        break;
                                    }
                                }

                                matchCount++;
                            }
                        }

                        // If multiple matches found, invalidate this strategy
                        if (matchCount > 1)
                        {
                            insertIndex = -1;
                        }
                    }

                    if (insertIndex >= 0)
                    {
                        // add the line without the comment marker
                        StringBuilder sbAddLines = new StringBuilder();
                        CommentedCodeLineInfo info = commentInfo;
                        while (info != null)
                        {
                            string trimmed = info.CommentLine.Trim();
                            string addLine = RemoveCommentMarkerWithIndex(trimmed, _commentedRemoveCodeMarker).TrimStart();
                            if (!string.IsNullOrEmpty(addLine))
                            {
                                if (sbAddLines.Length > 0)
                                {
                                    sbAddLines.AppendLine();
                                }

                                sbAddLines.Append(info.CommentLine);
                                sbAddLines.AppendLine();
                                sbAddLines.Append(addLine);
                            }

                            info = info.NextInfo;
                        }

                        insertions.Add((insertIndex, sbAddLines.ToString()));
                    }
                    else
                    {
                        if (_verbosity >= Options.VerbosityOption.Error)
                        {
                            Console.WriteLine($"No valid insertion point found in '{locationInfo}': {trimmedLine}");
                        }
                    }
                }
            }

            // Sort insertions by line index (descending) to insert from bottom to top
            insertions = insertions.OrderByDescending(x => x.lineIndex).ToList();

            foreach (var (lineIndex, commentLine) in insertions)
            {
                sourceLines.Insert(lineIndex, commentLine);
            }

            return string.Join(Environment.NewLine, sourceLines);
        }

        /// <summary>
        /// Gets the name of a member for comparison purposes, including interface name for explicit implementations
        /// </summary>
        private static string GetMemberName(SyntaxNode member)
        {
            return member switch
            {
                MethodDeclarationSyntax method => GetMethodName(method),
                PropertyDeclarationSyntax property => GetPropertyName(property),
                FieldDeclarationSyntax field => GetFieldName(field),
                EventDeclarationSyntax eventDecl => eventDecl.Identifier.Text,
                ConstructorDeclarationSyntax ctor => ctor.Identifier.Text,
                _ => null
            };
        }

        /// <summary>
        /// Gets the field name (supports multiple variable declarators)
        /// </summary>
        private static string GetFieldName(FieldDeclarationSyntax field)
        {
            // For fields with multiple variables (e.g., int x, y, z;), 
            // use the first variable's name
            var firstVariable = field.Declaration.Variables.FirstOrDefault();
            if (firstVariable != null)
            {
                return firstVariable.Identifier.Text;
            }
            return null;
        }

        /// <summary>
        /// Gets the property name, including interface prefix for explicit implementations
        /// </summary>
        private static string GetPropertyName(PropertyDeclarationSyntax property)
        {
            // Check for explicit interface implementation
            if (property.ExplicitInterfaceSpecifier != null)
            {
                // Format: IInterfaceName.PropertyName
                return $"{property.ExplicitInterfaceSpecifier.Name}.{property.Identifier.Text}";
            }

            return property.Identifier.Text;
        }

        /// <summary>
        /// Gets the method name, including interface prefix for explicit implementations
        /// </summary>
        private static string GetMethodName(MethodDeclarationSyntax method)
        {
            string typeParameterSuffix = "";
            int typeParamCount = method.TypeParameterList?.Parameters.Count ?? 0;
            if (typeParamCount > 0)
            {
                typeParameterSuffix = $"<{typeParamCount}>";
            }

            // Check for explicit interface implementation
            if (method.ExplicitInterfaceSpecifier != null)
            {
                // Format: IInterfaceName.MethodName
                return $"{method.ExplicitInterfaceSpecifier.Name}.{method.Identifier.Text}{typeParameterSuffix}";
            }

            return $"{method.Identifier.Text}{typeParameterSuffix}";
        }

        /// <summary>
        /// Gets a boolean property value from an attribute
        /// </summary>
        private static bool GetAttributeBoolProperty(AttributeSyntax attribute, string propertyName)
        {
            if (attribute.ArgumentList == null)
            {
                return false;
            }

            foreach (var argument in attribute.ArgumentList.Arguments)
            {
                if (argument.NameEquals != null &&
                    argument.NameEquals.Name.Identifier.Text == propertyName)
                {
                    // Check if the expression is a literal (true/false)
                    if (argument.Expression is LiteralExpressionSyntax literal)
                    {
                        return literal.Kind() == SyntaxKind.TrueLiteralExpression;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the string value of a property from an attribute
        /// </summary>
        private static string GetAttributeStringProperty(AttributeSyntax attribute, string propertyName)
        {
            if (attribute.ArgumentList == null)
            {
                return null;
            }

            foreach (var argument in attribute.ArgumentList.Arguments)
            {
                if (argument.NameEquals != null &&
                    argument.NameEquals.Name.Identifier.Text == propertyName)
                {
                    if (argument.Expression is LiteralExpressionSyntax literal)
                    {
                        // Remove quotes from string literal
                        return literal.Token.ValueText;
                    }
                }
            }

            return null;
        }

        public static string CalculateMethodHash(MethodDeclarationSyntax method)
        {
            string normalized = method.NormalizeWhitespace().ToFullString();
            string noSpaces = Regex.Replace(normalized, @"\s+", "");

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(noSpaces));
                return BitConverter.ToString(hashBytes, 0, 16).Replace("-", "").ToUpperInvariant();
            }
        }

        public static string CalculateMethodHash(FieldDeclarationSyntax field)
        {
            string normalized = field.NormalizeWhitespace().ToFullString();
            string noSpaces = Regex.Replace(normalized, @"\s+", "");

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(noSpaces));
                return BitConverter.ToString(hashBytes, 0, 16).Replace("-", "").ToUpperInvariant();
            }
        }

        public static string CalculatePropertyHash(PropertyDeclarationSyntax property)
        {
            // Normalize and remove whitespaces for hash calculation
            string normalized = property.NormalizeWhitespace().ToFullString();
            string noSpaces = Regex.Replace(normalized, @"\s+", "");

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(noSpaces));
                return BitConverter.ToString(hashBytes, 0, 16).Replace("-", "").ToUpperInvariant();
            }
        }

        public static string CalculateConstructorHash(ConstructorDeclarationSyntax constructor)
        {
            string normalized = constructor.NormalizeWhitespace().ToFullString();
            string noSpaces = Regex.Replace(normalized, @"\s+", "");

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(noSpaces));
                return BitConverter.ToString(hashBytes, 0, 16).Replace("-", "").ToUpperInvariant();
            }
        }
    }
}
