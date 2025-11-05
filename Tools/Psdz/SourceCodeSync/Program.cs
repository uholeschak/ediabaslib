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
        private static Dictionary<string, ClassDeclarationSyntax> _classDict = new Dictionary<string, ClassDeclarationSyntax>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, EnumDeclarationSyntax> _enumDict = new Dictionary<string, EnumDeclarationSyntax>(StringComparer.OrdinalIgnoreCase);

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
                    if (string.IsNullOrEmpty(relPath))
                    {
                        continue;
                    }

                    if (filterParts != null && filterParts.Length > 0)
                    {
                        bool matched = false;
                        foreach (string filterPart in filterParts)
                        {
                            if (relPath.Contains(filterPart, StringComparison.OrdinalIgnoreCase))
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

                    if (showSource)
                    {
                        Console.WriteLine($"Class: {className}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(classSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (_classDict.TryGetValue(className, out ClassDeclarationSyntax oldClassSyntax))
                    {
                        string oldClassSource = oldClassSyntax.ToFullString();
                        if (oldClassSource != classSource)
                        {
                            Console.WriteLine("*** Warning: Duplicate class name with different source: {0}", className);
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

                    if (showSource)
                    {
                        Console.WriteLine($"Enum: {enumName}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(enumSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (!_enumDict.TryAdd(enumName, enumDecl))
                    {
                        Console.WriteLine("*** Warning: Duplicate enum name found: {0}", enumName);
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
                if (fileContent.Contains("//", StringComparison.Ordinal) ||
                    fileContent.Contains("[UH]", StringComparison.Ordinal))
                {
                    Console.WriteLine("Skipping manually modified file: {0}", fileName);
                    return true;
                }

                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(fileContent);
                CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();

                bool fileModified = false;
                CompilationUnitSyntax newRoot = root;

                // Update classes
                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                foreach (ClassDeclarationSyntax cls in classes)
                {
                    string className = GetClassName(cls);
                    string classSource = cls.ToFullString();
                    if (showSource)
                    {
                        Console.WriteLine($"Class: {className}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(classSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (_classDict.TryGetValue(className, out ClassDeclarationSyntax sourceClass))
                    {
                        // Compare if they're different
                        if (cls.ToFullString() != sourceClass.ToFullString())
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
                    string enumSource = enumDecl.ToFullString();
                    if (showSource)
                    {
                        Console.WriteLine($"Enum: {enumName}");
                        Console.WriteLine("Source:");
                        Console.WriteLine(enumSource);
                        Console.WriteLine(new string('-', 80));
                    }

                    if (_enumDict.TryGetValue(enumName, out EnumDeclarationSyntax sourceEnum))
                    {
                        // Compare if they're different
                        if (enumDecl.ToFullString() != sourceEnum.ToFullString())
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
                    string modifiedContent = newRoot.NormalizeWhitespace(indentation: "    ", eol: "\r\n").ToFullString();
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

        public static string GetClassName(ClassDeclarationSyntax classDeclaration)
        {
            string className = classDeclaration.Identifier.ValueText;
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
            return enumName;
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
    }
}
