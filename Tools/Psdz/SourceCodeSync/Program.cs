using CommandLine;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SourceCodeSync
{
    internal class Program
    {
        private static Dictionary<string, ClassDeclarationSyntax> _classDict = new Dictionary<string, ClassDeclarationSyntax>(StringComparer.Ordinal);
        private static Dictionary<string, InterfaceDeclarationSyntax> _interfaceDict = new Dictionary<string, InterfaceDeclarationSyntax>(StringComparer.Ordinal);
        private static Dictionary<string, EnumDeclarationSyntax> _enumDict = new Dictionary<string, EnumDeclarationSyntax>(StringComparer.Ordinal);

        private static readonly string[] _ignoreNamespaces =
        [
            @"^BMW\.ISPI\.TRIC\.ISTA\.Contracts\.Enums\.UserLogin$",
            @"^BMW\.Rheingold\.CoreFramework\.OSS$",
            @"^BMW\.Rheingold\.CoreFramework\.IndustrialCustomer\..*",
            @"^BMW\.ISPI\.TRIC\.ISTA\.Contracts\.Models\..*",
            @"^BMW\.Rheingold\.InfoProvider\.(SWT|Tric|Broker|igDom|service|Current|SCC|NOP|EDGE|Properties).*",
        ];

        private static readonly Regex[] _compiledIgnoreNamespaces = _ignoreNamespaces
                .Select(pattern => new Regex(pattern, RegexOptions.Compiled))
                .ToArray();

        private static readonly string[] _ignoreClassNames =
        [
            "public_static_LicenseHelper",
            "internal_sealed_LicenseManager",
            "public_LicenseStatusChecker",
            "public_LicenseWizardHelper",
            "BMW.Rheingold.Programming.Common.SecureCodingService",
            "BMW.Rheingold.Programming.ProgrammingService",
        ];

        private static readonly string[] _ignoreInterfaceNames =
        [
            "public_ILogger",
            "RheingoldPsdzWebApi.Adapter.Contracts.Services.IProgrammingService"
        ];

        private static readonly string[] _ignoreEnumNames =
        [
            "BMW.iLean.CommonServices.Logging.EventKind",
        ];

        public class Options
        {
            public Options()
            {
                SourceDir = string.Empty;
                DestDir = string.Empty;
                Filter = string.Empty;
                Verbosity = VerbosityOption.Error;
            }

            public enum VerbosityOption
            {
                None,
                Error,
                Warning,
                Info,
                Debug
            }

            [Option('s', "sourcedir", Required = true, HelpText = "Source directory.")]
            public string SourceDir { get; set; }

            [Option('d', "destdir", Required = true, HelpText = "Destination directory.")]
            public string DestDir { get; set; }

            [Option('f', "filter", Required = false, HelpText = "Directory filter.")]
            public string Filter { get; set; }

            [Option('v', "verbosity", Required = false, HelpText = "Option for message verbosity (Error, Warning, Info, Debug)")]
            public VerbosityOption Verbosity { get; set; }
        }

        static Options.VerbosityOption _verbosity = Options.VerbosityOption.Error;

        static int Main(string[] args)
        {
            try
            {
                string sourceDir = null;
                string destDir = null;
                string filter = null;
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
                        destDir = o.DestDir;
                        filter = o.Filter;
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

                if (string.IsNullOrEmpty(sourceDir) || !Directory.Exists(sourceDir))
                {
                    if (_verbosity >= Options.VerbosityOption.Error)
                    {
                        Console.WriteLine("Source directory not existing: {0}", sourceDir);
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

                Console.WriteLine("Source dir: {0}", sourceDir);
                Console.WriteLine();

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

                Console.WriteLine("Dest dir: {0}", destDir);
                Console.WriteLine();

                string[] destFiles = Directory.GetFiles(destDir, "*.cs", SearchOption.AllDirectories);
                foreach (string file in destFiles)
                {
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
                    string className = GetClassName(cls);
                    string classSource = cls.ToFullString();
                    string namespaceName = GetNamespace(cls);

                    if (_verbosity >= Options.VerbosityOption.Debug)
                    {
                        Console.WriteLine($"Class: {className}");
                        Console.WriteLine($"Namespace: {namespaceName}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(classSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (_compiledIgnoreNamespaces.Any(regex => regex.IsMatch(namespaceName)))
                    {
                        continue;
                    }

                    if (_ignoreClassNames.Contains(className))
                    {
                        continue;
                    }

                    string classNameWithNamespace = GetClassName(cls, includeNamespace: true);
                    if (_ignoreClassNames.Contains(classNameWithNamespace))
                    {
                        continue;
                    }

                    if (_classDict.TryGetValue(className, out ClassDeclarationSyntax oldClassSyntax))
                    {
                        if (oldClassSyntax != null)
                        {
                            string oldClassSource = oldClassSyntax.ToFullString();
                            if (oldClassSource != classSource)
                            {
                                if (_verbosity >= Options.VerbosityOption.Error)
                                {
                                    Console.WriteLine("*** Warning: Duplicate class name with different source: {0}", className);
                                }
                                _classDict[className] = null;
                            }
                        }
                    }
                    else
                    {
                        if (!_classDict.TryAdd(className, cls))
                        {
                            if (_verbosity >= Options.VerbosityOption.Error)
                            {
                                Console.WriteLine("*** Warning: Add class failed: {0}", className);
                            }
                        }
                    }
                }


                var interfaces = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>();
                foreach (InterfaceDeclarationSyntax interfaceDecl in interfaces)
                {
                    string interfaceName = GetInterfaceName(interfaceDecl);
                    string interfaceSource = interfaceDecl.ToFullString();
                    string namespaceName = GetNamespace(interfaceDecl);

                    if (_verbosity >= Options.VerbosityOption.Debug)
                    {
                        Console.WriteLine($"Interface: {interfaceName}");
                        Console.WriteLine($"Namespace: {namespaceName}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(interfaceSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (_compiledIgnoreNamespaces.Any(regex => regex.IsMatch(namespaceName)))
                    {
                        continue;
                    }

                    if (_ignoreInterfaceNames.Contains(interfaceName))
                    {
                        continue;
                    }

                    string interfaceNameWithNamespace = GetInterfaceName(interfaceDecl, includeNamespace: true);
                    if (_ignoreInterfaceNames.Contains(interfaceNameWithNamespace))
                    {
                        continue;
                    }

                    if (_interfaceDict.TryGetValue(interfaceName, out InterfaceDeclarationSyntax oldInterfaceSyntax))
                    {
                        if (oldInterfaceSyntax != null)
                        {
                            string oldInterfaceSource = oldInterfaceSyntax.ToFullString();
                            if (oldInterfaceSource != interfaceSource)
                            {
                                if (_verbosity >= Options.VerbosityOption.Error)
                                {
                                    Console.WriteLine("*** Warning: Duplicate interface name with different source: {0}", interfaceName);
                                }
                                _interfaceDict[interfaceName] = null;
                            }
                        }
                    }
                    else
                    {
                        if (!_interfaceDict.TryAdd(interfaceName, interfaceDecl))
                        {
                            if (_verbosity >= Options.VerbosityOption.Error)
                            {
                                Console.WriteLine("*** Warning: Add interface failed: {0}", interfaceName);
                            }
                        }
                    }
                }

                var enums = root.DescendantNodes().OfType<EnumDeclarationSyntax>();
                foreach (EnumDeclarationSyntax enumDecl in enums)
                {
                    string enumName = GetEnumName(enumDecl);
                    string enumSource = enumDecl.ToFullString();
                    string namespaceName = GetNamespace(enumDecl);

                    if (_verbosity >= Options.VerbosityOption.Debug)
                    {
                        Console.WriteLine($"Enum: {enumName}");
                        Console.WriteLine($"Namespace: {namespaceName}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(enumSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (_compiledIgnoreNamespaces.Any(regex => regex.IsMatch(namespaceName)))
                    {
                        continue;
                    }

                    if (_ignoreEnumNames.Contains(enumName))
                    {
                        continue;
                    }

                    string enumNameWithNamespace = GetEnumName(enumDecl, includeNamespace: true);
                    if (_ignoreEnumNames.Contains(enumNameWithNamespace))
                    {
                        continue;
                    }

                    if (_enumDict.TryGetValue(enumName, out EnumDeclarationSyntax oldEnumSyntax))
                    {
                        if (oldEnumSyntax != null)
                        {
                            string oldEnumSource = oldEnumSyntax.ToFullString();
                            if (oldEnumSource != enumSource)
                            {
                                if (_verbosity >= Options.VerbosityOption.Error)
                                {
                                    Console.WriteLine("*** Warning: Duplicate enum name with different source: {0}", enumName);
                                }
                                _enumDict[enumName] = null;
                            }
                        }
                    }
                    else
                    {
                        if (!_enumDict.TryAdd(enumName, enumDecl))
                        {
                            Console.WriteLine("*** Warning: Add enum failed: {0}", enumName);
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
                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                foreach (ClassDeclarationSyntax cls in classes)
                {
                    string className = GetClassName(cls);
                    string classSource = cls.NormalizeWhitespace().ToFullString();

                    if (_verbosity >= Options.VerbosityOption.Debug)
                    {
                        Console.WriteLine($"Class: {className}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(classSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (HasSpecialTrivia(cls))
                    {
                        if (_verbosity >= Options.VerbosityOption.Warning)
                        {
                            Console.WriteLine("Skipping class {0} with comments: {1}", className, fileName);
                        }
                        continue;
                    }

                    if (_classDict.TryGetValue(className, out ClassDeclarationSyntax sourceClass) && sourceClass != null)
                    {
                        bool hasContract = HasContractAttribute(cls.AttributeLists);
                        bool sourceHasContract = HasContractAttribute(sourceClass.AttributeLists);
                        if (hasContract && !sourceHasContract)
                        {
                            if (_verbosity >= Options.VerbosityOption.Warning)
                            {
                                Console.WriteLine("Skipping class {0} with removed Contract", className);
                            }
                            continue;
                        }

                        // Check if destination class has any preserved members
                        bool hasPreservedMembers = cls.Members.Any(m => ShouldPreserveMember(m));
                        ClassDeclarationSyntax mergedClass;

                        if (hasPreservedMembers)
                        {
                            // Merge with preserved members
                            mergedClass = MergeClassPreservingMarked(cls, sourceClass);
                            if (_verbosity >= Options.VerbosityOption.Info)
                            {
                                int preservedCount = cls.Members.Count(m => ShouldPreserveMember(m));
                                Console.WriteLine($"Merging class {className} while preserving {preservedCount} marked member(s)");
                            }
                        }
                        else
                        {
                            // No preserved members, use source as-is
                            mergedClass = sourceClass;
                        }

                        // Compare if they're different
                        string mergedClassStr = mergedClass.NormalizeWhitespace().ToFullString();
                        if (classSource != mergedClassStr)
                        {
                            if (_verbosity >= Options.VerbosityOption.Info)
                            {
                                Console.WriteLine($"Updating class: {className}");
                            }
                            newRoot = newRoot.ReplaceNode(cls, mergedClass);
                            fileModified = true;
                        }
                    }
                    else
                    {
                        if (_verbosity >= Options.VerbosityOption.Error)
                        {
                            Console.WriteLine("*** Warning: Class not found in source files: {0}", className);
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
                    string interfaceName = GetInterfaceName(interfaceDecl);
                    string interfaceSource = interfaceDecl.NormalizeWhitespace().ToFullString();

                    if (_verbosity >= Options.VerbosityOption.Debug)
                    {
                        Console.WriteLine($"Interface: {interfaceName}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(interfaceSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (HasSpecialTrivia(interfaceDecl))
                    {
                        if (_verbosity >= Options.VerbosityOption.Warning)
                        {
                            Console.WriteLine("Skipping interface {0} with comments: {1}", interfaceName, fileName);
                        }
                        continue;
                    }

                    if (_interfaceDict.TryGetValue(interfaceName, out InterfaceDeclarationSyntax sourceInterface) && sourceInterface != null)
                    {
                        bool hasContract = HasContractAttribute(interfaceDecl.AttributeLists);
                        bool sourceHasContract = HasContractAttribute(sourceInterface.AttributeLists);
                        if (hasContract && !sourceHasContract)
                        {
                            if (_verbosity >= Options.VerbosityOption.Warning)
                            {
                                Console.WriteLine("Skipping interface {0} with removed Contract", interfaceName);
                            }
                            continue;
                        }

                        // Check for preserved members
                        bool hasPreservedMembers = interfaceDecl.Members.Any(m => ShouldPreserveMember(m));
                        InterfaceDeclarationSyntax mergedInterface;

                        if (hasPreservedMembers)
                        {
                            mergedInterface = MergeInterfacePreservingMarked(interfaceDecl, sourceInterface);
                            if (_verbosity >= Options.VerbosityOption.Info)
                            {
                                int preservedCount = interfaceDecl.Members.Count(m => ShouldPreserveMember(m));
                                Console.WriteLine($"Merging interface {interfaceName} while preserving {preservedCount} marked member(s)");
                            }
                        }
                        else
                        {
                            mergedInterface = sourceInterface;
                        }

                        // Compare if they're different
                        string mergedInterfaceStr = mergedInterface.NormalizeWhitespace().ToFullString();
                        if (interfaceSource != mergedInterfaceStr)
                        {
                            if (_verbosity >= Options.VerbosityOption.Info)
                            {
                                Console.WriteLine($"Updating interface: {interfaceName}");
                            }
                            newRoot = newRoot.ReplaceNode(interfaceDecl, mergedInterface);
                            fileModified = true;
                        }
                    }
                    else
                    {
                        if (_verbosity >= Options.VerbosityOption.Error)
                        {
                            Console.WriteLine("*** Warning: Interface not found in source files: {0}", interfaceName);
                        }
                    }
                }

                // Update enums (not merge required)
                var enums = root.DescendantNodes().OfType<EnumDeclarationSyntax>().ToList();
                foreach (EnumDeclarationSyntax enumDecl in enums)
                {
                    string enumName = GetEnumName(enumDecl);
                    string enumSource = enumDecl.NormalizeWhitespace().ToFullString();

                    if (_verbosity >= Options.VerbosityOption.Debug)
                    {
                        Console.WriteLine($"Enum: {enumName}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(enumSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (HasSpecialTrivia(enumDecl))
                    {
                        if (_verbosity >= Options.VerbosityOption.Warning)
                        {
                            Console.WriteLine("Skipping enum {0} with comments: {1}", enumName, fileName);
                        }
                        continue;
                    }

                    if (_enumDict.TryGetValue(enumName, out EnumDeclarationSyntax sourceEnum) && sourceEnum != null)
                    {
                        // Compare if they're different
                        string sourceEnumStr = sourceEnum.NormalizeWhitespace().ToFullString();
                        if (enumSource != sourceEnumStr)
                        {
                            if (_verbosity >= Options.VerbosityOption.Info)
                            {
                                Console.WriteLine($"Updating enum: {enumName}");
                            }
                            newRoot = newRoot.ReplaceNode(enumDecl, sourceEnum);
                            fileModified = true;
                        }
                    }
                    else
                    {
                        if (_verbosity >= Options.VerbosityOption.Error)
                        {
                            Console.WriteLine("*** Warning: Enum not found in source files: {0}", enumName);
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

        public static string GetClassName(ClassDeclarationSyntax classDeclaration, bool includeNamespace = false)
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
                string modifiers = GetModifiersText(classDeclaration.Modifiers);
                if (modifiers.Length > 0)
                {
                    className = $"{modifiers}_{className}";
                }

                int typeParamCount = classDeclaration.TypeParameterList?.Parameters.Count ?? 0;
                if (typeParamCount > 0)
                {
                    className += $"_<{typeParamCount}>";
                }

                if (classDeclaration.BaseList != null)
                {
                    className += ":" + GetBaseTypesText(classDeclaration.BaseList.Types);
                }
            }
            return className;
        }

        public static string GetInterfaceName(InterfaceDeclarationSyntax interfaceDeclaration, bool includeNamespace = false)
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
                string modifiers = GetModifiersText(interfaceDeclaration.Modifiers);
                if (modifiers.Length > 0)
                {
                    interfaceName = $"{modifiers}_{interfaceName}";
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

        public static string GetEnumName(EnumDeclarationSyntax enumDeclaration, bool includeNamespace = false)
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
                string modifiers = GetModifiersText(enumDeclaration.Modifiers);
                if (modifiers.Length > 0)
                {
                    enumName = $"{modifiers}_{enumName}";
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
        public static bool HasSpecialTrivia(ClassDeclarationSyntax classDeclaration)
        {
            // Check leading trivia (comments before the class)
            if (classDeclaration.HasLeadingTrivia)
            {
                if (HasSpecialTrivia(classDeclaration.GetLeadingTrivia()))
                {
                    return true;
                }
            }

            // Check trailing trivia (comments after the class declaration line)
            if (classDeclaration.HasTrailingTrivia)
            {
                if (HasSpecialTrivia(classDeclaration.GetTrailingTrivia()))
                {
                    return true;
                }
            }

            // Check all descendant tokens (comments inside the class)
            foreach (var token in classDeclaration.DescendantTokens(descendIntoTrivia: true))
            {
                if (token.HasLeadingTrivia && HasSpecialTrivia(token.LeadingTrivia))
                {
                    return true;
                }
                if (token.HasTrailingTrivia && HasSpecialTrivia(token.TrailingTrivia))
                {
                    return true;
                }
            }

            if (ShouldPreserveMember(classDeclaration))
            {
                return true;
            }

            return false;
        }

        public static bool HasSpecialTrivia(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            // Check leading trivia (comments before the interface)
            if (interfaceDeclaration.HasLeadingTrivia)
            {
                if (HasSpecialTrivia(interfaceDeclaration.GetLeadingTrivia()))
                {
                    return true;
                }
            }

            // Check trailing trivia (comments after the interface declaration line)
            if (interfaceDeclaration.HasTrailingTrivia)
            {
                if (HasSpecialTrivia(interfaceDeclaration.GetTrailingTrivia()))
                {
                    return true;
                }
            }

            // Check all descendant tokens (comments inside the interface)
            foreach (var token in interfaceDeclaration.DescendantTokens(descendIntoTrivia: true))
            {
                if (token.HasLeadingTrivia && HasSpecialTrivia(token.LeadingTrivia))
                {
                    return true;
                }
                if (token.HasTrailingTrivia && HasSpecialTrivia(token.TrailingTrivia))
                {
                    return true;
                }
            }

            if (ShouldPreserveMember(interfaceDeclaration))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Same comment detection methods for enums
        /// </summary>
        public static bool HasSpecialTrivia(EnumDeclarationSyntax enumDeclaration)
        {
            if (enumDeclaration.HasLeadingTrivia && HasSpecialTrivia(enumDeclaration.GetLeadingTrivia()))
            {
                return true;
            }

            if (enumDeclaration.HasTrailingTrivia && HasSpecialTrivia(enumDeclaration.GetTrailingTrivia()))
            {
                return true;
            }

            foreach (var token in enumDeclaration.DescendantTokens(descendIntoTrivia: true))
            {
                if (token.HasLeadingTrivia && HasSpecialTrivia(token.LeadingTrivia))
                {
                    return true;
                }
                if (token.HasTrailingTrivia && HasSpecialTrivia(token.TrailingTrivia))
                {
                    return true;
                }
            }

            if (ShouldPreserveMember(enumDeclaration))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a trivia list contains special trivia
        /// </summary>
        public static bool HasSpecialTrivia(SyntaxTriviaList triviaList, bool includeComments = true, bool includePreprocessor = true)
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
                        string comment = trivia.ToString().ToUpperInvariant();
                        if (comment.Contains("TODO:"))
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

        /// <summary>
        /// Merges source class into destination, preserving marked members
        /// </summary>
        public static ClassDeclarationSyntax MergeClassPreservingMarked(
            ClassDeclarationSyntax destClass,
            ClassDeclarationSyntax sourceClass)
        {
                // Get all members from destination that should be preserved
            var preservedMembers = destClass.Members
                .Where(m => ShouldPreserveMember(m))
                .ToDictionary(m => GetMemberName(m), m => m);

            if (!preservedMembers.Any())
            {
                return sourceClass;
            }

            // Build a new member list
            List<MemberDeclarationSyntax> newMembers = new List<MemberDeclarationSyntax>();
            HashSet<string> preservedMemberNames = new HashSet<string>(preservedMembers.Keys);

            // First, add all source members that are NOT being replaced by preserved ones
            foreach (var sourceMember in sourceClass.Members)
            {
                string memberName = GetMemberName(sourceMember);

                if (preservedMemberNames.Contains(memberName))
                {
                    // Use preserved version instead
                    newMembers.Add(preservedMembers[memberName]);
                    preservedMemberNames.Remove(memberName); // Mark as added
                }
                else
                {
                    // Use source version
                    newMembers.Add(sourceMember);
                }
            }

            // Add any preserved members that didn't exist in source
            foreach (string remainingName in preservedMemberNames)
            {
                newMembers.Add(preservedMembers[remainingName]);
            }

            // Replace all members at once
            return sourceClass.WithMembers(SyntaxFactory.List(newMembers));
        }

        /// <summary>
        /// Similar merge for interfaces (batch version)
        /// </summary>
        public static InterfaceDeclarationSyntax MergeInterfacePreservingMarked(
            InterfaceDeclarationSyntax destInterface,
            InterfaceDeclarationSyntax sourceInterface)
        {
            var preservedMembers = destInterface.Members
                .Where(m => ShouldPreserveMember(m))
                .ToDictionary(m => GetMemberName(m), m => m);

            if (!preservedMembers.Any())
            {
                return sourceInterface;
            }

            var newMembers = new List<MemberDeclarationSyntax>();
            var preservedMemberNames = new HashSet<string>(preservedMembers.Keys);

            // Add all source members, replacing with preserved ones where applicable
            foreach (var sourceMember in sourceInterface.Members)
            {
                string memberName = GetMemberName(sourceMember);

                if (preservedMemberNames.Contains(memberName))
                {
                    newMembers.Add(preservedMembers[memberName]);
                    preservedMemberNames.Remove(memberName);
                }
                else
                {
                    newMembers.Add(sourceMember);
                }
            }

            // Add preserved members that don't exist in source
            foreach (var remainingName in preservedMemberNames)
            {
                newMembers.Add(preservedMembers[remainingName]);
            }

            return sourceInterface.WithMembers(SyntaxFactory.List(newMembers));
        }

        /// <summary>
        /// Checks if a member has a preserve marker
        /// </summary>
        public static bool ShouldPreserveMember(SyntaxNode member)
        {
            // Check for attributes like [Preserve] or [DoNotSync]
            if (member is MemberDeclarationSyntax memberDecl)
            {
                var hasPreserveAttribute = memberDecl.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Any(attr =>
                    {
                        string attrName = attr.Name.ToString();
                        return attrName == "PreserveSource";
                    });

                if (hasPreserveAttribute)
                {
                    return true;
                }
            }

            // Check for special comments
            if (HasPreserveComment(member))
            {
                return true;
            }

            // Check for naming convention
            if (HasPreserveNamingConvention(member))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the name of a member for comparison purposes
        /// </summary>
        private static string GetMemberName(SyntaxNode member)
        {
            return member switch
            {
                MethodDeclarationSyntax method => method.Identifier.Text,
                PropertyDeclarationSyntax property => property.Identifier.Text,
                FieldDeclarationSyntax field => field.Declaration.Variables.FirstOrDefault()?.Identifier.Text,
                EventDeclarationSyntax eventDecl => eventDecl.Identifier.Text,
                ConstructorDeclarationSyntax ctor => ctor.Identifier.Text,
                _ => null
            };
        }

        /// <summary>
        /// Checks if member has preserve comment markers
        /// </summary>
        public static bool HasPreserveComment(SyntaxNode member)
        {
            if (member.HasLeadingTrivia)
            {
                var trivia = member.GetLeadingTrivia();
                foreach (var t in trivia)
                {
                    if (t.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                        t.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                        t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                        t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                    {
                        string comment = t.ToString().ToUpperInvariant();
                        if (comment.Contains("[PRESERVE]"))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if member name follows preserve naming convention
        /// </summary>
        public static bool HasPreserveNamingConvention(SyntaxNode member)
        {
#if false
            string memberName = member switch
            {
                MethodDeclarationSyntax method => method.Identifier.Text,
                PropertyDeclarationSyntax property => property.Identifier.Text,
                FieldDeclarationSyntax field => field.Declaration.Variables.FirstOrDefault()?.Identifier.Text,
                _ => null
            };

            if (string.IsNullOrEmpty(memberName))
                return false;

            // Check for naming conventions
            if (memberName.StartsWith("Custom_", StringComparison.Ordinal))
            {
                return true;
            }
#endif
            return false;
        }
    }
}
