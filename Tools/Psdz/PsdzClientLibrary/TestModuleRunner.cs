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
        if (string.IsNullOrEmpty(cleanIstaModuleName))
        {
            return null;
        }

        string assemblyModuleName = null;
        if (cleanIstaModuleName.StartsWith("ABL_AUS_"))
        {
            assemblyModuleName = "TestmodulesAblAus.dll";
        }
        else if (cleanIstaModuleName.StartsWith("ABL_GEN_"))
        {
            assemblyModuleName = "TestmodulesAblGen.dll";
        }
        else if (cleanIstaModuleName.StartsWith("ABL_LIF_"))
        {
            assemblyModuleName = "TestmodulesAblLif.dll";
        }

        if (string.IsNullOrEmpty(assemblyModuleName))
        {
            return null;
        }

        string appDir = EdiabasNet.AssemblyDirectory;
        if (string.IsNullOrEmpty(appDir))
        {
            return null;
        }

        string assemblyPath = Path.Combine(appDir, assemblyModuleName);
        return Assembly.LoadFrom(assemblyPath);
    }

    public static Assembly CompileModuleAssembly(ClientContext clientContext, string cleanIstaModuleName)
    {
        if (string.IsNullOrEmpty(cleanIstaModuleName))
        {
            return null;
        }

        string appDir = EdiabasNet.AssemblyDirectory;
        if (string.IsNullOrEmpty(appDir))
        {
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
        if (!File.Exists(sourcePath))
        {
            return null;
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
                catch (Exception)
                {
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
            catch (Exception)
            {
                try
                {
                    File.Delete(assemblyPath);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        try
        {
            string sourceCode = File.ReadAllText(sourcePath);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

            // Referenzen: aktuell geladene Assembly + Basis-Laufzeitreferenzen
            Assembly currentAssembly = typeof(TestModuleRunner).Assembly;
            List<MetadataReference> references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(currentAssembly.Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            };

            // Zusätzliche Kern-Assemblies (nötig bei .NET Core/.NET 10, harmlos bei .NET FW)
            string runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
            foreach (string name in new[] { "System.Runtime.dll", "System.Collections.dll", "netstandard.dll", "mscorlib.dll" })
            {
                string refPath = Path.Combine(runtimeDir, name);
                if (File.Exists(refPath))
                {
                    references.Add(MetadataReference.CreateFromFile(refPath));
                }
            }

            CSharpCompilation compilation = CSharpCompilation.Create(
                cleanIstaModuleName,
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: optimizationLevel));

            EmitResult result = compilation.Emit(assemblyPath);
            if (!result.Success)
            {
                // Optional: result.Diagnostics auswerten/loggen
                return null;
            }

            return Assembly.LoadFrom(assemblyPath);
        }
        catch (Exception)
        {
            return null;
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
