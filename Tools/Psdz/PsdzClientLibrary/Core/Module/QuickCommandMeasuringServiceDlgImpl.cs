using BMW.ISPI.IstaOperation.Contract.ServiceProgram;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.ConnectionManagement;
using BMW.Rheingold.Measurement.Common;
using BMW.Rheingold.Measurement.Common.Contract;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.CoreFramework.Interaction.Responses;
using BMW.Rheingold.CoreFramework.Localization;
using BMW.Rheingold.xVM;

namespace BMW.Rheingold.Module.ISTA
{
    internal class QuickCommandMeasuringServiceDlgImpl : ServiceDlgImplBase<QuickCommandMeasuringServiceDlgModel>
    {
        private static readonly Dictionary<MeasuringFunction, string> UNIT_DICTIONARY = new Dictionary<MeasuringFunction, string>
        {
            {
                MeasuringFunction.Current,
                "A"
            },
            {
                MeasuringFunction.Diode,
                "V"
            },
            {
                MeasuringFunction.None,
                string.Empty
            },
            {
                MeasuringFunction.Pressure,
                "bar"
            },
            {
                MeasuringFunction.LowPressure,
                "bar"
            },
            {
                MeasuringFunction.Resistance,
                new string (new char[1] { 'Ω' })
            },
            {
                MeasuringFunction.Temperature,
                new string (new char[2] { '°', 'C' })
            },
            {
                MeasuringFunction.Voltage,
                "V"
            }
        };
        private ITextLocator additionalText;
        private ISTAModule callingModule;
        private bool canReadNewValueFromImib;
        private string command;
        private bool display;
        private BMW.Rheingold.Measurement.Common.MeasurementResult lastReadResult;
        private ITextLocator measurementPointText1;
        private ITextLocator measurementPointText2;
        private string method;
        private ParameterContainer outParameter;
        private ITextLocator questionText;
        public MeasuringFunction MeasuringType => CommandToMeasuringFunction(command);

        public QuickCommandMeasuringServiceDlgImpl(ParameterContainer inParam) : base(inParam)
        {
            _globalModuleInParameter = inParam;
            __handleInParameter();
            callingModule = inParam.getParameter("__CallingModule__") as ISTAModule;
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            this.method = method;
            outParameter = outParam;
            command = inParam.getParameter("Command", null) as string;
            display = (bool)inParam.getParameter("Display", false);
            measurementPointText1 = inParam.getParameter("MeasurementPointText1", null) as ITextLocator;
            Model.MeasurementPointPin1 = inParam.getParameter("MeasurementPointPin1", null) as int? ;
            measurementPointText2 = inParam.getParameter("MeasurementPointText2", null) as ITextLocator;
            Model.MeasurementPointPin2 = inParam.getParameter("MeasurementPointPin2", null) as int? ;
            additionalText = inParam.getParameter("AdditionalText", null) as ITextLocator;
            questionText = inParam.getParameter("QuestionText", null) as ITextLocator;
            Model.AreMeasurmentIconsVisible = measurementPointText1 != null && measurementPointText2 != null;
            IDeviceImib deviceImib = InitializeImib();
            if (display)
            {
                InitializeDisplayText();
                if (deviceImib == null)
                {
                    Model.IsManualInput = true;
                }
                else
                {
                    StartReadingDataFromImib(deviceImib);
                }

                NavigateTo(Model);
                WaitForAction();
            }
            else if (deviceImib != null)
            {
                lastReadResult = SendCommand(deviceImib);
            }

            SetOutParameters(outParam);
        }

        private void CallConnectionManager(ILogic logic, Vehicle vecInfo, ConnectionTargetTypes connectionTargetTypes, IStartMeasurementService measurementService)
        {
            InteractionConnectionManagerResponse interactionConnectionManagerResponse = ConnectionManagerHandler.ShowConnectionManager(null, logic, logic.Services.InteractionService, vecInfo?.VCI, vecInfo?.MIB, connectionTargetTypes);
            ConnectionManagerResponseAction? connectionManagerResponseAction = interactionConnectionManagerResponse?.Action;
            if (connectionManagerResponseAction.HasValue && connectionManagerResponseAction.GetValueOrDefault() == ConnectionManagerResponseAction.Connect)
            {
                VCIDevice device = interactionConnectionManagerResponse.VciDevice;
                if (logic.HandleVCI(ref device, continueVecInfo: true).ErrorCodeInt != 0)
                {
                    IList<string> lang = new string[1]
                    {
                        ConfigSettings.CurrentUICulture
                    }.ToList();
                    IList<LocalizedText> titleList = new FormatedData("#Error").Localize(lang);
                    IList<LocalizedText> msgList = new FormatedData("#043").Localize(lang);
                    logic.Services.InteractionService.RegisterMessage(titleList, msgList);
                }
                else
                {
                    measurementService.InitQuickCommandMeasuringType(MeasuringType);
                }
            }
            else
            {
                Log.Warning(Log.CurrentMethod(), "Response action '{0}' is not allowed.", interactionConnectionManagerResponse?.Action);
            }
        }

        private bool CheckImibConnection(IStartMeasurementServiceServer measurementService)
        {
            if (!measurementService.CheckConnectionToImibInServiceDialog(MeasuringType))
            {
                if (!(callingModule.__RheinGoldCoreModuleParameters__.getParameter(ModuleParameter.ParameterName.Logic)is ILogic logic))
                {
                    Log.Error(Log.CurrentMethod(), "Logic must not be null.");
                    return false;
                }

                CallConnectionManager(logic, logic.VecInfo, ConnectionTargetTypes.MIB, measurementService);
            }

            return measurementService.IsConnectedToImib;
        }

        private MeasuringFunction CommandToMeasuringFunction(string command)
        {
            MeasuringFunction result = MeasuringFunction.None;
            if (string.IsNullOrEmpty(command))
            {
                return result;
            }

            if (command.Contains("VOLTage"))
            {
                result = MeasuringFunction.Voltage;
            }
            else if (command.Contains("TEMPerature"))
            {
                result = MeasuringFunction.Temperature;
            }
            else if (command.Contains("CURRent"))
            {
                result = MeasuringFunction.Current;
            }
            else if (command.Contains("PRESsure"))
            {
                result = MeasuringFunction.Pressure;
            }
            else if (command.Contains("RESistance"))
            {
                result = MeasuringFunction.Resistance;
            }
            else if (command.Contains("DIODe"))
            {
                result = MeasuringFunction.Diode;
            }
            else if (command.Contains("LOWPressure"))
            {
                result = MeasuringFunction.LowPressure;
            }

            return result;
        }

        private string GetUnit(string command)
        {
            return UNIT_DICTIONARY[CommandToMeasuringFunction(command)];
        }

        private void InitializeDisplayText()
        {
            Model.AdditionalText = GetContent(additionalText?.TextContent);
            Model.QuestionText = GetContent(questionText?.TextContent);
            Model.MeasurementPointText1 = GetContent(measurementPointText1?.TextContent);
            Model.MeasurementPointText2 = GetContent(measurementPointText2?.TextContent);
            if (!string.IsNullOrEmpty(Model.QuestionText))
            {
                SetNextButtonEnabled(value: false);
                Model.Button1Text = FormatedData.Localize("#Yes");
                Model.Button2Text = FormatedData.Localize("#No");
            }
            else
            {
                SetNextButtonEnabled(value: true);
            }

            Model.MeasuredUnit = GetUnit(command);
        }

        private IDeviceImib InitializeImib()
        {
            string text = Log.CurrentMethod();
            IStartMeasurementServiceServer measurementLauncher = callingModule.MeasurementLauncher;
            if (measurementLauncher == null)
            {
                Log.Error(text, "The measurement service is not available.");
            }
            else if (CheckImibConnection(measurementLauncher))
            {
                IDeviceImib deviceImib = measurementLauncher.ReserveMeasurementDevice();
                if (deviceImib == null)
                {
                    Log.Warning(text, "No IMIB available or no connection to IMIB established.");
                }
                else
                {
                    if (deviceImib.GenericMeasurementDevice != null)
                    {
                        return deviceImib;
                    }

                    Log.Warning(text, "The measurement device is not available.");
                }
            }

            return null;
        }

        private BMW.Rheingold.Measurement.Common.MeasurementResult SendCommand(IDeviceImib imib)
        {
            try
            {
                return imib.GenericMeasurementDevice.Command(command + ";:SYST:ERR:ALL?");
            }
            catch (Exception ex)
            {
                Log.Error(Log.CurrentMethod(), ex.Message);
            }

            return null;
        }

        private void SetOutParameters(ParameterContainer outParam)
        {
            if (Model.IsManualInput)
            {
                if (double.TryParse(Model.MeasuredValue, out var result))
                {
                    outParam.setParameter("Result", result);
                }
                else
                {
                    Log.Error(Log.CurrentMethod(), "Failed to parse {0} to double.", Model.MeasuredValue);
                }
            }
            else if (lastReadResult != null)
            {
                outParam.setParameter("Result", lastReadResult.Value);
                outParam.setParameter("Fehlergrund", lastReadResult.Fehlergrund);
                outParam.setParameter("Status", lastReadResult.Status);
            }

            outParam.setParameter("IOResult", Model.IsAnswer1);
        }

        private void StartReadingDataFromImib(IDeviceImib imib)
        {
            canReadNewValueFromImib = true;
            Task.Run(() =>
            {
                while (canReadNewValueFromImib && parentTab.ModuleData.Status != typeDiagObjectState.Canceled)
                {
                    lastReadResult = SendCommand(imib);
                    Model.MeasuredValue = Math.Round(lastReadResult.Value, 3).ToString();
                }
            });
        }

        private void StopReadingDataFromImib()
        {
            canReadNewValueFromImib = false;
        }

        private void WaitForAction()
        {
            try
            {
                DisplayWaitCursor(value: false);
                while (true)
                {
                    ServiceProgramAction serviceProgramAction = ServiceProgramController.AwaitUserAction(-1);
                    if (serviceProgramAction is ServiceProgramNavigationAction)
                    {
                        break;
                    }

                    if (serviceProgramAction is ServiceProgramMeasuringDlgAction serviceProgramMeasuringDlgAction)
                    {
                        Model.IsAnswer1 = serviceProgramMeasuringDlgAction.IsAnswer1;
                        Model.IsAnswer2 = serviceProgramMeasuringDlgAction.IsAnswer2;
                        Model.MeasuredValue = serviceProgramMeasuringDlgAction.Value1;
                        if (serviceProgramMeasuringDlgAction.IsAnswer1 || serviceProgramMeasuringDlgAction.IsAnswer2)
                        {
                            SetNextButtonEnabled(value: true);
                        }
                    }
                }

                StopReadingDataFromImib();
                callingModule.ResultSet.CollectiveResult = CollectiveResultSet.Ok;
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
            }
        }
    }
}