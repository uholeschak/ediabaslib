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

        // Erstellen Sie einen Decompiler mit Einstellungen
        CSharpDecompiler decompiler = new CSharpDecompiler(dllPath, resolver, new DecompilerSettings
        {
            ThrowOnAssemblyResolveErrors = false,
            CSharpFormattingOptions = FormattingOptionsFactory.CreateAllman(),
            UseDebugSymbols = true,
            RemoveDeadCode = false,
            ArrayInitializers = true,
            AutomaticEvents = true,
            AutomaticProperties = true,
            UsingDeclarations = true
        });

        // Dekompilieren Sie die gesamte Assembly
        string decompiledCode = decompiler.DecompileWholeModuleAsString();

        return decompiledCode;
    }

    public static void DecompileToFile(string dllPath, string outputPath)
    {
        var decompiledCode = DecompileAssembly(dllPath);
        File.WriteAllText(outputPath, decompiledCode);
    }
}
