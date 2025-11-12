using System.Collections.Generic;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using ICSharpCode.Decompiler.TypeSystem;
using System.IO;
using ICSharpCode.Decompiler.Metadata;

namespace SourceCodeSync;

public class DecompilerHelper
{
    public static string DecompileAssembly(string dllPath, List<string> searchList = null)
    {
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
        DecompilerSettings settings = new DecompilerSettings
        {
            ThrowOnAssemblyResolveErrors = false,
            UseDebugSymbols = true,
        };
        settings.SetLanguageVersion(LanguageVersion.CSharp7_3);

        // Erstellen Sie einen Decompiler mit Einstellungen
        CSharpDecompiler decompiler = new CSharpDecompiler(dllPath, resolver, settings);

        // Dekompilieren Sie die gesamte Assembly
        string decompiledCode = decompiler.DecompileWholeModuleAsString();
        return decompiledCode;
    }

    public static void DecompileToFile(string dllPath, string outputPath, List<string> searchList = null)
    {
        string decompiledCode = DecompileAssembly(dllPath, searchList);
        File.WriteAllText(outputPath, decompiledCode);
    }
}
