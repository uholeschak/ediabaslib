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
    private readonly PsdzDatabase.SwiInfoObj _swiInfoObj;
    private readonly string _moduleName;
    private readonly ModuleParameter _moduleParameters;

    public TestModuleRunner(PsdzDatabase.SwiInfoObj swiInfoObj, Dictionary<string, object> parametersDict = null)
    {
        _swiInfoObj = swiInfoObj;
        _moduleName = "BMW.Rheingold.Module.ISTA." + IstaModuleBase.ModuleNameTransformator(_swiInfoObj.Identification);
        _moduleParameters = new ModuleParameter(parametersDict);
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
