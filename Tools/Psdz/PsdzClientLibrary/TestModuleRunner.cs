using BMW.Rheingold.CoreFramework;
using EdiabasLib;
using PsdzClient;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Reflection;
using PsdzClient.Programming;

namespace PsdzClientLibrary;

[PreserveSource(Hint = "Custom code", SuppressWarning = true)]
public class TestModuleRunner
{
    private readonly ClientContext _clientContext;
    private readonly PsdzDatabase.SwiInfoObj _swiInfoObj;
    private readonly string _moduleName;
    private readonly string _moduleTypeName;
    private readonly ModuleParameter _moduleParameters;

    public ModuleParameter ModuleParameters => _moduleParameters;

    public TestModuleRunner(ClientContext clientContext, PsdzContext psdzContext, string controlId, Dictionary<string, object> parametersDict = null)
    {
        _clientContext = clientContext;
        _swiInfoObj = _clientContext?.Database?.GetInfoObjectByControlId(controlId);
        if (_swiInfoObj == null)
        {
            throw new ArgumentException($"No SwiInfoObj found for controlId: {controlId}");
        }

        _moduleName = IstaModuleBase.ModuleNameTransformator(_swiInfoObj.Identificator);
        _moduleTypeName = "BMW.Rheingold.Module.ISTA." + _moduleName;
        _moduleParameters = new ModuleParameter(parametersDict);
        _moduleParameters.setParameter(ModuleParameter.ParameterName.Vehicle, psdzContext.VecInfo);
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
        if (_moduleParameters != null)
        {
            Dictionary<string, object> dictionary = _moduleParameters.Clone().callParameter[0] as Dictionary<string, object>;
            foreach (string key in dictionary.Keys)
            {
                parameterContainer.setParameter(key, dictionary[key]);
            }

            parameterContainer.setParameter("__RheinGoldCoreModuleParameters__", _moduleParameters.Clone());
        }

        //parameterContainer.setParameter("__RheinGoldTabModuleISTA__", parent);
        //parameterContainer.setParameter("FASTA", fasta2);
        //parameterContainer.setParameter("MeasurementLauncher", measurementService);
        parameterContainer.setParameter("ISTAModule.Me", _swiInfoObj);
        return parameterContainer;
    }
}
