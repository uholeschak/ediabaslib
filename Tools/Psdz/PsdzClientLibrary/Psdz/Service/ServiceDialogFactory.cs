using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Globalization;
using PsdzClient;

namespace BMW.Rheingold.Module.ISTA
{
    public class ServiceDialogFactory : IServiceDialogFactory
    {
        public IServiceDialog CreateServiceDialog(ISTAModule callingModule, string methodName, string path, IModuleExecutionParent globalTabModuleISTA, int elementNo, ParameterContainer inParameters, ParameterContainer inoutParameters)
        {
            string text = ResolveDialogRef(path, logMissing: true);
            ServiceDialogConfiguration serviceDialogConfiguration;
            try
            {
                serviceDialogConfiguration = ServiceDialogConfiguration.GetRegisteredConfiguration(text);
                //[-] IDatabaseProvider instance = DatabaseProviderFactory.Instance;
                if (serviceDialogConfiguration.TextCollection == null && callingModule != null)
                {
                    //[-] IXepInfoObject infoObjectByControlId = instance.GetInfoObjectByControlId(serviceDialogConfiguration.ControlId);
                    //[+] PsdzDatabase instance = callingModule.DBProvider;
                    PsdzDatabase instance = callingModule.DBProvider;
                    //[+] PsdzDatabase.SwiInfoObj infoObjectByControlId = instance.GetInfoObjectByControlId(serviceDialogConfiguration.ControlId.ToString());
                    PsdzDatabase.SwiInfoObj infoObjectByControlId = instance.GetInfoObjectByControlId(serviceDialogConfiguration.ControlId.ToString());
                    serviceDialogConfiguration.TextCollection = TextContentManager.Create(instance, callingModule.logic.Lang, infoObjectByControlId, serviceDialogConfiguration.Name);
                }
            }
            catch (Exception ex)
            {
                serviceDialogConfiguration = ServiceDialogConfiguration.GetDefault(text);
                Log.Error("ServiceDialogFactory.CreateServiceDialog()", "Unknown service dialog {0} using type {1} instead: {2}", text, serviceDialogConfiguration.DialogType.Name, ex);
            }

            if (ConfigSettings.IsProgrammingEnabled() || ConfigSettings.IsLogisticBaseEnabled())
            {
                bool flag = false;
                ModuleParameter moduleParameter = null;
                if (callingModule == null)
                {
                    Log.Warning("ServiceDialogFactory.CreateServiceDialog()", "Parameter \"callingModule\" is null.");
                }
                else
                {
                    moduleParameter = callingModule.__RheinGoldCoreModuleParameters__;
                }

                if (moduleParameter != null && moduleParameter.getParameter(ModuleParameter.ParameterName.ForegroundThread) != null)
                {
                    flag = (bool)moduleParameter.getParameter(ModuleParameter.ParameterName.ForegroundThread);
                }
                else
                {
                    Log.Warning("ServiceDialogFactory.CreateServiceDialog()", "No module parameter with name \"{0}\" found. Using value \"{1}\".", ModuleParameter.ParameterName.ForegroundThread, flag);
                }

                object parameter = inParameters.getParameter("Display");
                if (flag && serviceDialogConfiguration.IsShowingGui && parameter is IConvertible && Convert.ToBoolean(parameter))
                {
                    throw new Exception($"Module {callingModule} running in background must not create a service dialog with GUI ({text}) and Display parameter {parameter}.");
                }
            }

            ServiceDialogCmdBase serviceDialogCmdBase;
            if (serviceDialogConfiguration.ControllerType == null)
            {
                serviceDialogCmdBase = new ServiceDialogCmdBase(callingModule, methodName, path, globalTabModuleISTA, elementNo);
            }
            else
            {
                object[] constructorParam = new object[5]
                {
                    callingModule,
                    methodName,
                    path,
                    globalTabModuleISTA,
                    elementNo
                };
                Type[] constructorParamType = new Type[5]
                {
                    typeof(ISTAModule),
                    typeof(string),
                    typeof(string),
                    typeof(IModuleExecutionParent),
                    typeof(int)
                };
                serviceDialogCmdBase = serviceDialogConfiguration.ControllerType.CreateInstance(constructorParamType, constructorParam) as ServiceDialogCmdBase;
            }

            serviceDialogCmdBase.ServiceDialogConfig = serviceDialogConfiguration;
            Exception ex2 = null;
            try
            {
                serviceDialogCmdBase.CreateDialog(inParameters, inoutParameters);
            }
            catch (Exception ex3)
            {
                ex2 = ex3;
            }

            if (ex2 != null)
            {
                throw new Exception(string.Format(CultureInfo.InvariantCulture, "Failed to create service dialog {0}.", text), ex2);
            }

            return serviceDialogCmdBase;
        }

        public static string ResolveDialogRef(string idStr, bool logMissing = false)
        {
            string text;
            switch (idStr)
            {
                case "MessageServiceDlg":
                case "MessageServiceDlg_flackerfrei":
                case "FlackerfreieMeldung":
                case "Meldung":
                    text = "51915403";
                    break;
                case "OsziServiceDlgPage2":
                    text = "-1";
                    break;
                default:
                    text = idStr;
                    break;
            }

            try
            {
                ServiceDialogConfiguration serviceDialogConfiguration = ((!decimal.TryParse(text, out var result)) ? ServiceDialogConfiguration.GetRegisteredConfiguration(text) : ServiceDialogConfiguration.GetRegisteredConfigurationById(result));
                if (serviceDialogConfiguration != null)
                {
                    return serviceDialogConfiguration.Name;
                }

                if (logMissing)
                {
                    Log.Warning("ServiceDialogFactory.ResolveDialogRef()", "Failed to find service dialog \"{0}\".", text);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ServiceDialogFactory.ResolveDialogRef()", "Failed to find service dialog \"{0}\": {1}", text, ex);
            }

            return null;
        }
    }
}