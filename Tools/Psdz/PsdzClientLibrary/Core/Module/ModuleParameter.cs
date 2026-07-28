using PsdzClient.Core;
using System;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework
{
    public class ModuleParameter
    {
        public enum ParameterName
        {
            data,
            tabControl,
            ECUKom,
            Vehicle,
            Fasta,
            runParameter,
            Protocol,
            Owner,
            MIBKom,
            InfoObjStarted,
            XepInfoObjectStarted,
            Logic,
            ForegroundThread,
            OutParameters,
            InAndOutParameters,
            ResultSet,
            FastaAblauf,
            FastaService2,
            Configuration,
            MeasurementLauncher,
            ServiceProgramController,
            IN_konfig,
            IN_pause,
            IN_automode,
            IN_automaticRun,
            CcmIdExtern,
            inTextausgabe
        }

        private Dictionary<string, object> parameters;

        public object[] callParameter => new object[1] { parameters };

        public ModuleParameter(Dictionary<string, object> parameters)
        {
            this.parameters = parameters;
        }

        public ModuleParameter()
        {
            parameters = new Dictionary<string, object>();
        }

        public ModuleParameter Clone()
        {
            ModuleParameter moduleParameter = new ModuleParameter();
            try
            {
                lock (parameters)
                {
                    foreach (ParameterName value in Enum.GetValues(typeof(ParameterName)))
                    {
                        if (ContainsParameter(value))
                        {
                            moduleParameter.setParameter(value, getParameter(value));
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ModuleParameter.Clone()", exception);
            }
            return moduleParameter;
        }

        public bool ContainsParameter(ParameterName name)
        {
            return parameters.ContainsKey(name.ToString());
        }

        public object getParameter(ParameterName name)
        {
            return getParameter(name, null);
        }

        public object getParameter(ParameterName name, object defaultValue)
        {
            try
            {
                if (parameters.ContainsKey(name.ToString()))
                {
                    return parameters[name.ToString()];
                }
                Log.Warning("ModuleParameter.getParameter()", "parameter {0} not found in module parameters; will return default value {1}", name, defaultValue);
            }
            catch (Exception exception)
            {
                Log.WarningException("ModuleParameter.getParameter()", exception);
            }
            return defaultValue;
        }

        public void removeParameter(ParameterName name)
        {
            try
            {
                if (parameters.ContainsKey(name.ToString()))
                {
                    parameters.Remove(name.ToString());
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ModuleParameter.removeParameter()", exception);
            }
        }

        public void setParameter(ParameterName name, object parameter)
        {
            try
            {
                lock (parameters)
                {
                    if (parameters.ContainsKey(name.ToString()))
                    {
                        parameters[name.ToString()] = parameter;
                    }
                    else
                    {
                        parameters.Add(name.ToString(), parameter);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ModuleParameter.setParameter()", exception);
            }
        }
    }
}
