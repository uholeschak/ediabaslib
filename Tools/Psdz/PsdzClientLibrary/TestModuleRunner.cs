using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;

namespace PsdzClientLibrary;

[PreserveSource(Hint = "Custom code", SuppressWarning = true)]
public class TestModuleRunner
{
    private string _moduleName;
    private ModuleParameter _moduleParameters;
    private PsdzDatabase.SwiInfoObj _swiInfoObj;

    public TestModuleRunner(string moduleName, Dictionary<string, object> parametersDict, PsdzDatabase.SwiInfoObj swiInfoObj)
    {
        _moduleName = moduleName;
        _moduleParameters = new ModuleParameter(parametersDict);
        _swiInfoObj = swiInfoObj;
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
