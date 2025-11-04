using CommandLine;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;
using System.Linq;

namespace SourceCodeSync
{
    internal class Program
    {
        public class Options
        {
            public Options()
            {
                SourceDir = string.Empty;
                DestDir = string.Empty;
            }

            [Option('s', "sourcedir", Required = true, HelpText = "Source directory.")]
            public string SourceDir { get; set; }

            [Option('d', "destdir", Required = true, HelpText = "Destination directory.")]
            public string DestDir { get; set; }
        }

        static int Main(string[] args)
        {
            try
            {
                string sourceDir = null;
                string destDir = null;
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

                string[] files = Directory.GetFiles(sourceDir, "*.cs", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    try
                    {
                        string fileContent = File.ReadAllText(file);
                        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(fileContent);
                        SyntaxNode root = syntaxTree.GetCompilationUnitRoot();

                        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                        foreach (var cls in classes)
                        {
                            string className = cls.Identifier.ValueText;
                            string classSource = cls.ToFullString();
                            Console.WriteLine($"Class: {className}");
                            Console.WriteLine("Source:");
                            Console.WriteLine(classSource);
                            Console.WriteLine(new string('-', 80));
                        }

                        var enums = root.DescendantNodes().OfType<EnumDeclarationSyntax>();
                        foreach (var enumDecl in enums)
                        {
                            string enumName = enumDecl.Identifier.ValueText;
                            string enumSource = enumDecl.ToFullString();
                            Console.WriteLine($"Enum: {enumName}");
                            Console.WriteLine("Source:");
                            Console.WriteLine(enumSource);
                            Console.WriteLine(new string('-', 80));
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("*** Error parsing file: {0}, Exception {1}", file, e.Message);
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
    }
}
