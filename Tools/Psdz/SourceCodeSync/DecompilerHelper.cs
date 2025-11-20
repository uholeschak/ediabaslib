using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.Metadata;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ICSharpCode.Decompiler.TypeSystem;

namespace SourceCodeSync;

public class DecompilerHelper
{
    public static bool DecompileAssembly(string dllPath, string outputPath, List<string> searchList = null, Dictionary<string, string> textReplacements = null)
    {
        if (string.IsNullOrEmpty(outputPath))
        {
            return false;
        }

        UniversalAssemblyResolver resolver = new UniversalAssemblyResolver(
            dllPath,
            throwOnError: false,
            targetFramework: null);

        if (searchList != null)
        {
            foreach (var path in searchList)
            {
                resolver.AddSearchDirectory(path);
            }
        }

        CSharpFormattingOptions formattingOptions = FormattingOptionsFactory.CreateAllman();
        formattingOptions.IndentationString = "    "; // 4 Leerzeichen statt Tabs

        DecompilerSettings settings = new DecompilerSettings
        {
            ThrowOnAssemblyResolveErrors = false,
            UseDebugSymbols = true,
            UsingDeclarations = true,
            UsingStatement = true,
            CSharpFormattingOptions = formattingOptions
        };
        settings.SetLanguageVersion(LanguageVersion.CSharp7_3);

        // Erstellen Sie einen Decompiler mit Einstellungen
        CSharpDecompiler decompiler = new CSharpDecompiler(dllPath, resolver, settings);

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        MetadataModule module = decompiler.TypeSystem.MainModule;
        foreach (var type in module.TopLevelTypeDefinitions)
        {
            if (type.Name == "<Module>")
            {
                continue;
            }


            if (type.IsCompilerGenerated())
            {
                continue;
            }

            if (type.Kind == TypeKind.Delegate)
            {
                continue;
            }

            // Erstelle Namespace-Verzeichnis
            string namespacePath = type.Namespace.Replace('.', Path.DirectorySeparatorChar);
            string typeDirectory = Path.Combine(outputPath, namespacePath);
            Directory.CreateDirectory(typeDirectory);

            // Dateiname basierend auf Typnamen
            string typeName = type.Name;
            if (type.TypeArguments.Count > 0)
            {
                typeName += "_" + type.TypeArguments.Count;
            }

            string fileName = SanitizeFileName(typeName) + ".cs";
            string filePath = Path.Combine(typeDirectory, fileName);

            // Dekompiliere den Typ
            string typeCode = decompiler.DecompileTypeAsString(type.FullTypeName);
            if (textReplacements != null)
            {
                foreach (var kvp in textReplacements)
                {
                    typeCode = typeCode.Replace(kvp.Key, kvp.Value);
                }
            }

            File.WriteAllText(filePath, typeCode);
        }

        return true;
    }

    private static string SanitizeFileName(string fileName)
    {
        // Entferne ungültige Zeichen aus Dateinamen
        char[] invalidChars = Path.GetInvalidFileNameChars();
        foreach (char c in invalidChars)
        {
            fileName = fileName.Replace(c, '_');
        }
        // Entferne generische Parameter-Marker
        fileName = fileName.Replace('<', '_').Replace('>', '_').Replace('`', '_');
        return fileName;
    }
}
