using BMW.ISPI.IstaOperation.Impl;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.RheingoldSessionController;
using EdiabasLib;
using PsdzClient;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PsdzClientLibrary;

[PreserveSource(Hint = "Custom code", SuppressWarning = true)]
public class TestModuleRunner
{
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

    public bool Run()
    {
        try
        {
            ParameterContainer inParameters = SetUpModuleInParameters();
            ParameterContainer outParameters = new ParameterContainer();
            ParameterContainer inAndOutParameters = new ParameterContainer();

            //Vehicle vehicle = _moduleParameters.getParameter(ModuleParameter.ParameterName.Vehicle) as Vehicle;

            Assembly assembly = GetModuleAssembly(_moduleName);
            if (assembly == null)
            {
                return false;
            }

            Type type = assembly.GetType(_moduleTypeName, throwOnError: true);
            IIstaModule instance = type?.CreateInstance(new Type[1] { typeof(ParameterContainer) }, new object[1] { inParameters }) as IIstaModule;
            if (instance == null)
            {
                return false;
            }

            MethodInfo method = instance.GetType().GetMethod("run");
            if (method == null)
            {
                return false;
            }
            method.Invoke(instance, new object[3] { inParameters, outParameters, inAndOutParameters });

            _moduleParameters.setParameter(ModuleParameter.ParameterName.OutParameters, outParameters);
            _moduleParameters.setParameter(ModuleParameter.ParameterName.InAndOutParameters, inAndOutParameters);
            _moduleParameters.setParameter(ModuleParameter.ParameterName.ResultSet, instance.ResultSet);
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    public static Assembly GetModuleAssembly(string cleanIstaModuleName)
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

        string assemblyPath = System.IO.Path.Combine(appDir, assemblyModuleName);
        return Assembly.LoadFrom(assemblyPath);
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
