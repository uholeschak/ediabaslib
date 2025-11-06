using CommandLine;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SourceCodeSync
{
    internal class Program
    {
        private static Dictionary<string, ClassDeclarationSyntax> _classDict = new Dictionary<string, ClassDeclarationSyntax>(StringComparer.Ordinal);
        private static Dictionary<string, EnumDeclarationSyntax> _enumDict = new Dictionary<string, EnumDeclarationSyntax>(StringComparer.Ordinal);

        private static readonly string[] _ignoreNamespaces =
        [
            "BMW.ISPI.TRIC.ISTA.Contracts.Enums.UserLogin",
            "BMW.Rheingold.CoreFramework.OSS",
        ];

        private static readonly string[] _ignoreClassNames =
        [
            "public_ParameterContainer",
            "public_Address",
            "public_Contract",
            "public_Outlet",
            "public_Phone",
            "public_Contact",
            "public_Property",
            "public_static_LicenseHelper",
            "internal_sealed_LicenseManager",
            "public_LicenseStatusChecker",
            "public_LicenseWizardHelper"
        ];

        private static readonly string[] _ignoreEnumNames =
        [
        ];

        public class Options
        {
            public Options()
            {
                SourceDir = string.Empty;
                DestDir = string.Empty;
                Filter = string.Empty;
                ShowSource = false;
            }

            [Option('s', "sourcedir", Required = true, HelpText = "Source directory.")]
            public string SourceDir { get; set; }

            [Option('d', "destdir", Required = true, HelpText = "Destination directory.")]
            public string DestDir { get; set; }

            [Option('f', "filter", Required = false, HelpText = "Directory filter.")]
            public string Filter { get; set; }

            [Option("source", Required = false, HelpText = "Show source.")]
            public bool ShowSource { get; set; }
        }

        static int Main(string[] args)
        {
            try
            {
                string sourceDir = null;
                string destDir = null;
                string filter = null;
                bool showSource = false;
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
                        showSource = o.ShowSource;
                    })
                    .WithNotParsed(errs =>
                    {
                        string errors = string.Join("\n", errs);
                        Console.WriteLine("Option parsing errors:\n{0}", string.Join("\n", errors));

                        hasErrors = true;
                    });

                if (hasErrors)
                {
                    return 1;
                }

                if (string.IsNullOrEmpty(sourceDir) || !Directory.Exists(sourceDir))
                {
                    Console.WriteLine("Source directory not existing: {0}", sourceDir);
                    return 1;
                }

                if (string.IsNullOrEmpty(destDir) || !Directory.Exists(destDir))
                {
                    Console.WriteLine("Destination directory not existing: {0}", destDir);
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
                    if (!GetFileSource(file, showSource))
                    {
                        Console.WriteLine("*** Get file source failed: {0}", file);
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

                    if (!UpdateFile(file, showSource))
                    {
                        Console.WriteLine("*** Update file failed: {0}", file);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("*** Exception: {0}", e.Message);
                return 1;
            }

            return 0;
        }

        public static bool GetFileSource(string fileName, bool showSource)
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

                    if (showSource)
                    {
                        Console.WriteLine($"Class: {className}");
                        Console.WriteLine($"Namespace: {namespaceName}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(classSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (_ignoreNamespaces.Contains(namespaceName))
                    {
                        continue;
                    }

                    if (_ignoreClassNames.Contains(className))
                    {
                        continue;
                    }

                    if (_classDict.TryGetValue(className, out ClassDeclarationSyntax oldClassSyntax))
                    {
                        string oldClassSource = oldClassSyntax.ToFullString();
                        if (oldClassSource != classSource)
                        {
                            Console.WriteLine("*** Warning: Duplicate class name with different source: {0}", className);
                            _classDict[className] = null;
                        }
                    }
                    else
                    {
                        if (!_classDict.TryAdd(className, cls))
                        {
                            Console.WriteLine("*** Warning: Add class failed: {0}", className);
                        }
                    }
                }

                var enums = root.DescendantNodes().OfType<EnumDeclarationSyntax>();
                foreach (EnumDeclarationSyntax enumDecl in enums)
                {
                    string enumName = GetEnumName(enumDecl);
                    string enumSource = enumDecl.ToFullString();
                    string namespaceName = GetNamespace(enumDecl);

                    if (showSource)
                    {
                        Console.WriteLine($"Enum: {enumName}");
                        Console.WriteLine($"Namespace: {namespaceName}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(enumSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (_ignoreNamespaces.Contains(namespaceName))
                    {
                        continue;
                    }

                    if (_ignoreEnumNames.Contains(enumName))
                    {
                        continue;
                    }

                    if (_enumDict.TryGetValue(enumName, out EnumDeclarationSyntax oldEnumSyntax))
                    {
                        string oldEnumSource = oldEnumSyntax.ToFullString();
                        if (oldEnumSource != enumSource)
                        {
                            Console.WriteLine("*** Warning: Duplicate enum name with different source: {0}", enumName);
                            _enumDict[enumName] = null;
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
                Console.WriteLine("*** Error parsing file: {0}, Exception {1}", fileName, e.Message);
                return false;
            }
            return true;
        }

        public static bool UpdateFile(string fileName, bool showSource)
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
                    if (showSource)
                    {
                        Console.WriteLine($"Class: {className}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(classSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (HasComments(cls))
                    {
                        Console.WriteLine("Skipping class {0} with comments: {1}", className, fileName);
                        continue;
                    }

                    if (_classDict.TryGetValue(className, out ClassDeclarationSyntax sourceClass) && sourceClass != null)
                    {
                        // Compare if they're different
                        string classSourceStr = sourceClass.NormalizeWhitespace().ToFullString();
                        if (classSource != classSourceStr)
                        {
                            Console.WriteLine($"Updating class: {className}");
                            newRoot = newRoot.ReplaceNode(cls, sourceClass);
                            fileModified = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("*** Warning: Class not found in source files: {0}", className);
                    }
                }

                // Need to refresh the tree after class replacements
                if (fileModified)
                {
                    root = newRoot;
                }

                // Update enums
                var enums = root.DescendantNodes().OfType<EnumDeclarationSyntax>().ToList();
                foreach (EnumDeclarationSyntax enumDecl in enums)
                {
                    string enumName = GetEnumName(enumDecl);
                    string enumSource = enumDecl.NormalizeWhitespace().ToFullString();
                    if (showSource)
                    {
                        Console.WriteLine($"Enum: {enumName}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(enumSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (HasComments(enumDecl))
                    {
                        Console.WriteLine("Skipping enum {0} with comments: {1}", enumName, fileName);
                        continue;
                    }

                    if (_enumDict.TryGetValue(enumName, out EnumDeclarationSyntax sourceEnum) && sourceEnum != null)
                    {
                        // Compare if they're different
                        string sourceEnumStr = sourceEnum.NormalizeWhitespace().ToFullString();
                        if (enumSource != sourceEnumStr)
                        {
                            Console.WriteLine($"Updating enum: {enumName}");
                            newRoot = newRoot.ReplaceNode(enumDecl, sourceEnum);
                            fileModified = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("*** Warning: Enum not found in source files: {0}", enumName);
                    }
                }

                // Write the modified file back if changes were made
                if (fileModified)
                {
                    // Normalize whitespace: use spaces instead of tabs
                    string modifiedContent = newRoot.NormalizeWhitespace().ToFullString();
                    //modifiedContent += Environment.NewLine; // Ensure file ends with a newline

                    // Write with UTF-8 BOM encoding
                    File.WriteAllText(fileName, modifiedContent, new UTF8Encoding(true));
                    Console.WriteLine($"File updated: {fileName}");
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

        public static string GetClassName(ClassDeclarationSyntax classDeclaration)
        {
            string className = classDeclaration.Identifier.ValueText;
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
            return className;
        }

        public static string GetEnumName(EnumDeclarationSyntax enumDeclaration)
        {
            string enumName = enumDeclaration.Identifier.ValueText;
            string modifiers = GetModifiersText(enumDeclaration.Modifiers);
            if (modifiers.Length > 0)
            {
                enumName = $"{modifiers}_{enumName}";
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
        /// Checks if a class declaration has any comments (single-line, multi-line, or XML documentation)
        /// </summary>
        public static bool HasComments(ClassDeclarationSyntax classDeclaration)
        {
            // Check leading trivia (comments before the class)
            if (classDeclaration.HasLeadingTrivia)
            {
                if (HasCommentTrivia(classDeclaration.GetLeadingTrivia()))
                {
                    return true;
                }
            }

            // Check trailing trivia (comments after the class declaration line)
            if (classDeclaration.HasTrailingTrivia)
            {
                if (HasCommentTrivia(classDeclaration.GetTrailingTrivia()))
                {
                    return true;
                }
            }

            // Check all descendant tokens (comments inside the class)
            foreach (var token in classDeclaration.DescendantTokens(descendIntoTrivia: true))
            {
                if (token.HasLeadingTrivia && HasCommentTrivia(token.LeadingTrivia))
                {
                    return true;
                }
                if (token.HasTrailingTrivia && HasCommentTrivia(token.TrailingTrivia))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a trivia list contains any comment trivia
        /// </summary>
        public static bool HasCommentTrivia(SyntaxTriviaList triviaList)
        {
            foreach (SyntaxTrivia trivia in triviaList)
            {
                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||           // //
                    trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||   // /* */
                    trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || // ///
                    trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))    // /** */
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Same comment detection methods for enums
        /// </summary>
        public static bool HasComments(EnumDeclarationSyntax enumDeclaration)
        {
            if (enumDeclaration.HasLeadingTrivia && HasCommentTrivia(enumDeclaration.GetLeadingTrivia()))
            {
                return true;
            }

            if (enumDeclaration.HasTrailingTrivia && HasCommentTrivia(enumDeclaration.GetTrailingTrivia()))
            {
                return true;
            }

            foreach (var token in enumDeclaration.DescendantTokens(descendIntoTrivia: true))
            {
                if (token.HasLeadingTrivia && HasCommentTrivia(token.LeadingTrivia))
                {
                    return true;
                }
                if (token.HasTrailingTrivia && HasCommentTrivia(token.TrailingTrivia))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
