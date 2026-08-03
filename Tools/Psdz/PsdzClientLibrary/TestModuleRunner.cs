using BMW.Rheingold.CoreFramework;
using PsdzClient;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PsdzClientLibrary;

[PreserveSource(Hint = "Custom code", SuppressWarning = true)]
public class TestModuleRunner
{
    private readonly ClientContext _clientContext;
    private readonly PsdzDatabase.SwiInfoObj _swiInfoObj;
    private readonly string _moduleName;
    private readonly ModuleParameter _moduleParameters;

    public TestModuleRunner(ClientContext clientContext, string controlId, Dictionary<string, object> parametersDict = null)
    {
        _clientContext = clientContext;
        _swiInfoObj = _clientContext?.Database?.GetInfoObjectByControlId(controlId);
        if (_swiInfoObj == null)
        {
            throw new ArgumentException($"No SwiInfoObj found for controlId: {controlId}");
        }

        _moduleName = "BMW.Rheingold.Module.ISTA." + IstaModuleBase.ModuleNameTransformator(_swiInfoObj.Identificator);
        _moduleParameters = new ModuleParameter(parametersDict);
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

        return Assembly.Load(assemblyModuleName);
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
