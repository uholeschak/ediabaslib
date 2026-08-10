using BMW.ISPI.IstaOperation.Impl;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.RheingoldSessionController;
using EdiabasLib;
using log4net;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using PsdzClient;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace PsdzClientLibrary;

[PreserveSource(Hint = "Custom code", SuppressWarning = true)]
public class TestModuleRunner
{
    private static readonly ILog log = LogManager.GetLogger(typeof(TestModuleRunner));
    private static readonly string[] additionalAssemblies =
    {
        "mscorlib.dll",
        "netstandard.dll",
        "System.dll",
        "System.Core.dll",
        "System.Runtime.dll",
        "System.Collections.dll",
        "System.Xml.dll"
    };

    private readonly ClientContext _clientContext;
    private readonly ProgrammingJobs _programmingJobs;
    private readonly PsdzDatabase.SwiInfoObj _swiInfoObj;
    private readonly ILogic _logic;
    private readonly ServiceProgramController _serviceProgramController;
    private readonly string _moduleName;
    private readonly string _moduleTypeName;
    private readonly ModuleParameter _moduleParameters;

    public ModuleParameter ModuleParameters => _moduleParameters;

    public TestModuleRunner(ClientContext clientContext, ProgrammingJobs programmingJobs, string controlId, Dictionary<string, object> parametersDict = null)
    {
        _clientContext = clientContext;
        _programmingJobs = programmingJobs;
        _swiInfoObj = _clientContext?.Database?.GetInfoObjectByControlId(controlId);
        if (_swiInfoObj == null)
        {
            log.ErrorFormat("TestModuleRunner: No SwiInfoObj found for controlId: {0}", controlId);
            throw new ArgumentException($"No SwiInfoObj found for controlId: {controlId}");
        }

        _logic = new Logic(clientContext, programmingJobs);
        _serviceProgramController = new ServiceProgramController();
        _moduleName = IstaModuleBase.ModuleNameTransformator(_swiInfoObj.Identificator);
        _moduleTypeName = "BMW.Rheingold.Module.ISTA." + _moduleName;

        Dictionary<string, object> useParametersDict = parametersDict ?? new Dictionary<string, object>();
        _moduleParameters = new ModuleParameter(useParametersDict);
        _moduleParameters.setParameter(ModuleParameter.ParameterName.Logic, _logic);
        _moduleParameters.setParameter(ModuleParameter.ParameterName.Vehicle, programmingJobs.PsdzContext.VecInfo);
        _moduleParameters.setParameter(ModuleParameter.ParameterName.ServiceProgramController, _serviceProgramController);
    }

    public bool IsValid()
    {
        try
        {
            Assembly assembly = GetModuleAssembly(_clientContext, _moduleName);
            if (assembly == null)
            {
                log.ErrorFormat("IsValid: GetModuleAssembly returned null for: {0}", _moduleName);
                return false;
            }

            Type type = assembly.GetType(_moduleTypeName, throwOnError: false);
            if (type == null)
            {
                log.ErrorFormat("IsValid: GetType returned null for: {0}", _moduleTypeName);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            log.ErrorFormat("IsValid: Exception: {0}", ex);
            return false;
        }
    }

    public bool Run()
    {
        try
        {
            ParameterContainer inParameters = SetUpModuleInParameters();
            ParameterContainer outParameters = new ParameterContainer();
            ParameterContainer inAndOutParameters = new ParameterContainer();

            //Vehicle vehicle = _moduleParameters.getParameter(ModuleParameter.ParameterName.Vehicle) as Vehicle;

            Assembly assembly = GetModuleAssembly(_clientContext, _moduleName);
            if (assembly == null)
            {
                log.ErrorFormat("Run: GetModuleAssembly returned null for: {0}", _moduleName);
                return false;
            }

            Type type = assembly.GetType(_moduleTypeName, throwOnError: true);
            IIstaModule instance = type?.CreateInstance(new Type[1] { typeof(ParameterContainer) }, new object[1] { inParameters }) as IIstaModule;
            if (instance == null)
            {
                log.ErrorFormat("Run: CreateInstance returned null for: {0}", _moduleTypeName);
                return false;
            }

            MethodInfo method = instance.GetType().GetMethod("run");
            if (method == null)
            {
                log.ErrorFormat("Run: GetMethod run returned null for: {0}", _moduleTypeName);
                return false;
            }
            method.Invoke(instance, new object[3] { inParameters, outParameters, inAndOutParameters });

            _moduleParameters.setParameter(ModuleParameter.ParameterName.OutParameters, outParameters);
            _moduleParameters.setParameter(ModuleParameter.ParameterName.InAndOutParameters, inAndOutParameters);
            _moduleParameters.setParameter(ModuleParameter.ParameterName.ResultSet, instance.ResultSet);
        }
        catch (Exception ex)
        {
            log.ErrorFormat("Run: Exception: {0}", ex);
            return false;
        }
        return true;
    }

    public static Assembly GetModuleAssembly(ClientContext clientContext, string cleanIstaModuleName)
    {
        Assembly compiledAssembly = CompileModuleAssembly(clientContext, cleanIstaModuleName);
        return compiledAssembly;
    }

    /*
        Filter:
        <Compile Remove="$(Src)\ABL_GEN_AG*.cs" />
        <Compile Remove="$(Src)\ABL_GEN_AT*.cs" />
        <Compile Remove="$(Src)\ABL_GEN_AU*.cs" />
        <Compile Remove="$(Src)\ABL_GEN_G*.cs" />
        <Compile Remove="$(Src)\ABL_GEN_G*.cs" />
        <Compile Remove="$(Src)\ABL_GEN_BIKE_SET_SERVICEDATA*" />
        <Compile Remove="$(Src)\ABL_GEN_LIB_BIKE_UXP_COMPLETECODING.cs" />
        <Compile Remove="$(Src)\ABL_GEN_RESTOREINDIVDATA.cs" />

        <Compile Remove="$(Src)\ABL_LIF_ASEC__LCS_STATUS*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_AUTHORING*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_BACK*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_BIKE*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_CERT*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_CLEARERRORINFOMEMORY*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_DATA*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_DOC*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_EOS*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_GET__DISPLAYTEXT_DOB*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_GET__EFUSE_VERBRAUCHERLISTE*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_GETIBOOLRESULTOBJECTPROPERTIES*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_IDENTIFY_ISTA_OPERATIONALMODE*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_KOMPONENTENDIEBSTAHLSCHUTZ*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_KI__RANDOMFOREST*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_OBFCM*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_POLYNOM*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_PARALLELGENERIERUNG_SECURETOKEN*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_PROVISIONING*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_PC__SOUNDPLAYER*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_QDM*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_QMD*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_SEND_SPEEDLINKDATA*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_SPEZIAL*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_SWITCH_ECUS_TO_FIELDMODE*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_WRITESECURETOKENAUTOMATIK*.cs" />
        <Compile Remove="$(Src)\ABL_LIF_WRITE_PRG*.cs" />
     */

    public static Assembly CompileModuleAssembly(ClientContext clientContext, string cleanIstaModuleName)
    {
        if (string.IsNullOrEmpty(cleanIstaModuleName))
        {
            log.ErrorFormat("CompileModuleAssembly: cleanIstaModuleName is null or empty");
            return null;
        }

        string appDir = EdiabasNet.AssemblyDirectory;
        if (string.IsNullOrEmpty(appDir))
        {
            log.ErrorFormat("CompileModuleAssembly: AssemblyDirectory is null or empty");
            return null;
        }

        string testModulesPath = Path.Combine(clientContext.Database.DatabaseExtractPath, "Testmodule");
#if DEBUG
        OptimizationLevel optimizationLevel = OptimizationLevel.Debug;
        string outputPath = Path.Combine(testModulesPath, "Debug");
#else
        OptimizationLevel optimizationLevel = OptimizationLevel.Release;
        string outputPath = Path.Combine(testModulesPath, "Release");
#endif
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        string sourcePath = Path.Combine(testModulesPath, cleanIstaModuleName + ".cs");
        string assemblyPath = Path.Combine(outputPath, cleanIstaModuleName + ".dll");
        string logFilePath = Path.Combine(outputPath, cleanIstaModuleName + ".log");
        if (!File.Exists(sourcePath))
        {
            log.ErrorFormat("CompileModuleAssembly: Source file does not exist: {0}", sourcePath);
            return null;
        }

        foreach (Assembly loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                if (!loadedAssembly.IsDynamic &&
                    string.Equals(loadedAssembly.Location, assemblyPath, StringComparison.OrdinalIgnoreCase))
                {
                    log.InfoFormat("CompileModuleAssembly: Assembly already loaded: {0}", assemblyPath);
                    return loadedAssembly;
                }
            }
            catch (Exception)
            {
                // Assemblies ohne Location überspringen
            }
        }

        if (File.Exists(assemblyPath))
        {
            // Neukompilierung erzwingen, wenn die Quelldatei neuer ist als die Assembly
            DateTime sourceTimeUtc = File.GetLastWriteTimeUtc(sourcePath);
            DateTime assemblyTimeUtc = File.GetLastWriteTimeUtc(assemblyPath);
            if (sourceTimeUtc > assemblyTimeUtc)
            {
                try
                {
                    File.Delete(assemblyPath);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("CompileModuleAssembly: File.Delete Exception: {0}", ex);
                    return null;
                }
            }
        }

        if (File.Exists(assemblyPath))
        {
            try
            {
                return Assembly.LoadFrom(assemblyPath);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("CompileModuleAssembly: Assembly.LoadFrom Exception: {0}", ex);
                try
                {
                    File.Delete(assemblyPath);
                }
                catch (Exception ex2)
                {
                    log.ErrorFormat("CompileModuleAssembly: File.Delete Exception: {0}", ex2);
                    return null;
                }
            }
        }

        try
        {
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }

            string sourceCode = File.ReadAllText(sourcePath);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

            string assemblyInfo = $"""
                                   using System.Reflection;
                                   [assembly: AssemblyTitle("{cleanIstaModuleName}")]
                                   [assembly: AssemblyProduct("ISTA test module")]
                                   [assembly: AssemblyDescription("Compiled from {cleanIstaModuleName}.cs")]
                                   [assembly: AssemblyCompany("EdiabasLib")]
                                   [assembly: AssemblyVersion("1.0.0.0")]
                                   [assembly: AssemblyFileVersion("1.0.0.0")]
                                   [assembly: AssemblyInformationalVersion("Compiled {DateTime.Now:yyyy-MM-dd HH:mm:ss}")]
                                   """;
            SyntaxTree assemblyInfoTree = CSharpSyntaxTree.ParseText(assemblyInfo);

            List<MetadataReference> references = new List<MetadataReference>();
            HashSet<string> addedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
            if (string.IsNullOrEmpty(runtimeDir))
            {
                log.ErrorFormat("CompileModuleAssembly: Runtime directory is null or empty");
                return null;
            }

            foreach (string assembly in additionalAssemblies)
            {
                AddReference(ref references, ref addedPaths, Path.Combine(runtimeDir, assembly));
            }

            foreach (Assembly loaded in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (!loaded.IsDynamic)
                    {
                        AddReference(ref references, ref addedPaths, loaded.Location);
                    }
                }
                catch (Exception)
                {
                    // Assemblies ohne Location überspringen
                }
            }

            CSharpCompilation compilation = CSharpCompilation.Create(
                cleanIstaModuleName,
                new[] { syntaxTree, assemblyInfoTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: optimizationLevel));

            EmitResult result;
            using (Stream win32Resources = compilation.CreateDefaultWin32Resources(
                       versionResource: true,
                       noManifest: true,
                       manifestContents: null,
                       iconInIcoFormat: null))
            {
                using (FileStream peStream = new FileStream(assemblyPath, FileMode.Create, FileAccess.ReadWrite))
                {
                    result = compilation.Emit(peStream, win32Resources: win32Resources);
                }
            }

            if (!result.Success)
            {
                log.ErrorFormat("CompileModuleAssembly: Compilation of '{0}' failed", sourcePath);

                List<string> errorLines = new List<string>
                {
                    $"Compilation of '{sourcePath}' failed at {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                    string.Empty
                };

                foreach (Diagnostic diagnostic in result.Diagnostics)
                {
                    if (diagnostic.Severity == DiagnosticSeverity.Error)
                    {
                        errorLines.Add(diagnostic.ToString());
                    }
                }

                try
                {
                    File.WriteAllLines(logFilePath, errorLines);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("CompileModuleAssembly: Writing log file Exception: {0}", ex);
                }

                try
                {
                    File.Delete(assemblyPath);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("CompileModuleAssembly: File.Delete Exception: {0}", ex);
                    return null;
                }

                return null;
            }

            return Assembly.LoadFrom(assemblyPath);
        }
        catch (Exception ex)
        {
            log.ErrorFormat("CompileModuleAssembly: Exception: {0}", ex);
            return null;
        }
    }

    private static void AddReference(ref List<MetadataReference> references, ref HashSet<string> addedPaths, string path)
    {
        if (!string.IsNullOrEmpty(path) && File.Exists(path) && addedPaths.Add(path))
        {
            references.Add(MetadataReference.CreateFromFile(path));
        }
    }

    private ParameterContainer SetUpModuleInParameters()
    {
        ParameterContainer parameterContainer = new ParameterContainer();
        Dictionary<string, object> dictionary = _moduleParameters.Clone().callParameter[0] as Dictionary<string, object>;
        foreach (string key in dictionary.Keys)
        {
            parameterContainer.setParameter(key, dictionary[key]);
        }

        parameterContainer.setParameter("__RheinGoldCoreModuleParameters__", _moduleParameters.Clone());

        //parameterContainer.setParameter("__RheinGoldTabModuleISTA__", parent);
        //parameterContainer.setParameter("FASTA", fasta2);
        //parameterContainer.setParameter("MeasurementLauncher", measurementService);
        parameterContainer.setParameter("ISTAModule.Me", _swiInfoObj);
        return parameterContainer;
    }
}
