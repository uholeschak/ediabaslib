using BMW.Authoring.Helper;
using BMW.ISPI.IstaOperation.Contract.Document;
using BMW.ISPI.IstaOperation.Contract.ServiceProgram;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.Module.ISTA;
using BMW.Rheingold.RheingoldSessionController;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;

namespace BMW.Rheingold.Module.ISTA
{
    internal class HealthIndicatorDlgImpl : ServiceDlgImplBase<HealthIndicatorDlgModel>
    {
        private Task _listenToActionsTask;

        private CancellationTokenSource _listenToActionsToken;

        private bool m_ShouldQuitDialog;

        private Dictionary<int, KeyValuePair<int, int>> m_BlockBarIndecesByBarIndices = new Dictionary<int, KeyValuePair<int, int>>();

        private HealthIndicatorProtocoller m_Protocoller;

        private List<BarProtocolData> m_CurrentBarProtocolData = new List<BarProtocolData>();

        private DateTime m_StartTimeForProtocol;

        private int m_LastCountOfParameters;

        private int m_CurrentCountOfParameters;

        private bool IsConfirmNecessary;

        private bool IsProtocollingEnabled = true;

        private int Timeout = -1;

        private const int AWAIT_USER_ACTION_TIMEOUT = 100;

        private readonly AutoResetEvent resumeEvent = new AutoResetEvent(initialState: false);

        private bool IsInitialized => base.Model.BlockContainerData != null;

        private bool ShouldQuitDialog
        {
            get
            {
                return m_ShouldQuitDialog;
            }
            set
            {
                if (value != m_ShouldQuitDialog)
                {
                    m_ShouldQuitDialog = value;
                    if (m_ShouldQuitDialog)
                    {
                        resumeEvent.Set();
                    }
                }
            }
        }

        public HealthIndicatorDlgImpl(ParameterContainer inParam)
            : base(inParam)
        {
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            try
            {
                ReadGeneralParameters(inParam);
                ToggleFullscreenOrSplitScreen();
                if ("show".Equals(method.ToLower()))
                {
                    HandleShowMethod(inParam);
                }
                else if ("hide".Equals(method.ToLower()))
                {
                    HandleHideMethod(outParam);
                    return;
                }
                HandleFastaProtocolling();
                if (!IsConfirmNecessary)
                {
                    HandleDynamicUsecase(outParam);
                }
                else
                {
                    HandleStaticUsecase(outParam);
                }
                m_LastCountOfParameters = m_CurrentCountOfParameters;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
            }
        }

        private void OnParentTabModuleStateChanged(object o, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ModuleState" && (parentTab.ModuleData.IsExecutionCompleted || parentTab.ModuleData.Status == typeDiagObjectState.Canceled))
            {
                StopListeningAsyncActions();
            }
        }

        private void ClearCurrentBarProtocolData()
        {
            m_CurrentBarProtocolData.Clear();
        }

        private void HardReset()
        {
            ClearCurrentBarProtocolData();
            ShouldQuitDialog = false;
            base.Model.BlockContainerData = null;
            base.Model.PastText = string.Empty;
            base.Model.PriorText = string.Empty;
            m_BlockBarIndecesByBarIndices.Clear();
            m_Protocoller = null;
            if (_listenToActionsTask != null && _listenToActionsTask.Status == TaskStatus.Running)
            {
                _listenToActionsToken?.Cancel();
            }
        }

        private void HandleShowMethod(ParameterContainer inParam)
        {
            ClearCurrentBarProtocolData();
            ReadBarParameters(inParam);
            SetNextButtonEnabled(value: true);
        }

        private void HandleHideMethod(ParameterContainer outParam)
        {
            if (m_Protocoller != null && IsProtocollingEnabled)
            {
                m_Protocoller.EndCollectingBarProtocolData(base.Model.PriorText, base.Model.PastText);
            }
            ProtocolToFasta("hide");
            HardReset();
            outParam.setParameter("Quit", true);
            base.ServiceDialogUI.IsDialogShown = false;
        }

        private void HandleStaticUsecase(ParameterContainer outParam)
        {
            ListenToActions(isAsync: false);
            base.ServiceDialogUI.IsDialogShown = false;
            outParam.setParameter("Quit", true);
        }

        private void HandleDynamicUsecase(ParameterContainer outParam)
        {
            ListenToActions(isAsync: true);
            Wait(Timeout);
            if (ShouldQuitDialog)
            {
                ShouldQuitDialog = false;
                base.ServiceDialogUI.IsDialogShown = false;
                outParam.setParameter("Quit", true);
            }
            else
            {
                outParam.setParameter("Quit", false);
            }
        }

        private void ReadGeneralParameters(ParameterContainer inParam)
        {
            m_CurrentCountOfParameters = inParam.Count;
            GetValueFromParam(inParam, "Confirm", delegate (bool x)
            {
                IsConfirmNecessary = x;
            }, defaultValue: false);
            GetValueFromParam(inParam, "Timeout", delegate (int x)
            {
                Timeout = x;
            }, 0);
            GetValueFromParam(inParam, "Protocol", delegate (bool x)
            {
                IsProtocollingEnabled = x;
            }, defaultValue: false);
            GetValueFromParam(inParam, "Fullscreen", delegate (bool x)
            {
                base.Model.IsFullScreen = x;
            }, defaultValue: false);
        }

        private void ReadBarParameters(ParameterContainer inParam)
        {
            SetPriorAndPastText(inParam);
            if (base.Model.BlockContainerData == null || m_CurrentCountOfParameters != m_LastCountOfParameters)
            {
                InitializeBarDisplay(inParam);
            }
            else
            {
                UpdateBarDisplay(inParam);
            }
        }

        private void InitializeBarDisplay(ParameterContainer inParam)
        {
            BlockContainerControlViewModel blockContainerControlViewModel = CreateBlockContainerAndBlocks();
            SetBlockHeader(inParam, blockContainerControlViewModel);
            CreateBars(inParam, blockContainerControlViewModel);
            base.Model.BlockContainerData = blockContainerControlViewModel;
        }

        private void UpdateBarDisplay(ParameterContainer inParam)
        {
            if (inParam == null || base.Model.BlockContainerData == null || m_BlockBarIndecesByBarIndices.Count == 0)
            {
                return;
            }
            SetBlockHeader(inParam, base.Model.BlockContainerData);
            for (int i = 0; i < 32; i++)
            {
                if (m_BlockBarIndecesByBarIndices.ContainsKey(i))
                {
                    int key = m_BlockBarIndecesByBarIndices[i].Key;
                    int value = m_BlockBarIndecesByBarIndices[i].Value;
                    SetBarValues(inParam, base.Model.BlockContainerData.Blocks[key].Bars[value], i + 1);
                }
            }
        }

        private BlockContainerControlViewModel CreateBlockContainerAndBlocks()
        {
            BlockContainerControlViewModel blockContainerControlViewModel = new BlockContainerControlViewModel();
            for (int i = 0; i < 4; i++)
            {
                blockContainerControlViewModel.Blocks.Add(new BlockControlViewModel(i));
            }
            return blockContainerControlViewModel;
        }

        private void SetBlockHeader(ParameterContainer inParam, BlockContainerControlViewModel blockContainer)
        {
            if (inParam != null && blockContainer != null)
            {
                GetValueFromParam(inParam, "Blockheader1", delegate (string x)
                {
                    blockContainer.Blocks[0].BlockHeader = x;
                }, null);
                GetValueFromParam(inParam, "Blockheader2", delegate (string x)
                {
                    blockContainer.Blocks[1].BlockHeader = x;
                }, null);
                GetValueFromParam(inParam, "Blockheader3", delegate (string x)
                {
                    blockContainer.Blocks[2].BlockHeader = x;
                }, null);
                GetValueFromParam(inParam, "Blockheader4", delegate (string x)
                {
                    blockContainer.Blocks[3].BlockHeader = x;
                }, null);
            }
        }

        private void SetPriorAndPastText(ParameterContainer inParam)
        {
            if (inParam != null)
            {
                GetValueFromParam(inParam, "PriorText", delegate (string x)
                {
                    base.Model.PriorText = x;
                }, null);
                GetValueFromParam(inParam, "PastText", delegate (string x)
                {
                    base.Model.PastText = x;
                }, null);
            }
        }

        private void CreateBars(ParameterContainer inParam, BlockContainerControlViewModel blockContainer)
        {
            m_BlockBarIndecesByBarIndices.Clear();
            int[] array = new int[4];
            for (int i = 1; i <= 32; i++)
            {
                BarControlViewModel barControlViewModel = new BarControlViewModel();
                if (SetBarValues(inParam, barControlViewModel, i))
                {
                    int num = barControlViewModel.BlockNumber - 1;
                    m_BlockBarIndecesByBarIndices.Add(i - 1, new KeyValuePair<int, int>(num, array[num]));
                    while (blockContainer.Blocks[num].Bars.Count <= array[num])
                    {
                        blockContainer.Blocks[num].Bars.Add(new BarControlViewModel());
                    }
                    SetBarConstantData(barControlViewModel);
                    blockContainer.Blocks[num].Bars[array[num]] = barControlViewModel;
                    barControlViewModel.BlockBarIndex = array[num];
                    array[num]++;
                }
            }
        }

        private bool SetBarValues(ParameterContainer inParam, BarControlViewModel bar, int index)
        {
            if (inParam == null || bar == null || index < 1 || index > 32)
            {
                return false;
            }
            bar.BarIndex = index;
            GetValueFromParam(inParam, "Bar" + index.ToString("00") + "Blocknumber", delegate (int x)
            {
                bar.BlockNumber = x;
            }, 0);
            GetValueFromParam(inParam, "Bar" + index.ToString("00") + "RightText", delegate (string x)
            {
                bar.RightText = x;
            }, null);
            GetValueFromParam(inParam, "Bar" + index.ToString("00") + "TopText", delegate (string x)
            {
                bar.TopText = x;
            }, null);
            GetValueFromParam(inParam, "Bar" + index.ToString("00") + "Minimum", delegate (double x)
            {
                bar.MinValue = x;
            }, double.NaN);
            GetValueFromParam(inParam, "Bar" + index.ToString("00") + "Maximum", delegate (double x)
            {
                bar.MaxValue = x;
            }, double.NaN);
            GetValueFromParam(inParam, "Bar" + index.ToString("00") + "MajorDivisions", delegate (int x)
            {
                bar.MajorDivisions = x;
            }, 0);
            GetValueFromParam(inParam, "Bar" + index.ToString("00") + "MinorDivisions", delegate (int x)
            {
                bar.MinorDivisions = x;
            }, 0);
            GetValueFromParam(inParam, "Bar" + index.ToString("00") + "ScaleLabelPosition", delegate (int x)
            {
                bar.ScalarValuesPosition = x;
            }, 0);
            GetValueFromParam(inParam, "Bar" + index.ToString("00") + "ColorAreas", delegate (Dictionary<double, ServiceDialogColor> x)
            {
                bar.BarColorsControlViewModel.ColorPositions = new Dictionary<double, ServiceDialogColor>(x);
            }, null);
            GetValueFromParam(inParam, "Bar" + index.ToString("00") + "Value", delegate (double x)
            {
                bar.CurrentValue = x;
            }, double.NaN);
            if (bar.IsActive)
            {
                m_CurrentBarProtocolData.Add(new BarProtocolData(bar.BarIndex, bar.CurrentValue));
            }
            return bar.IsActive;
        }

        private void StopListeningAsyncActions()
        {
            if (_listenToActionsTask != null && _listenToActionsTask.Status == TaskStatus.Running)
            {
                parentTab.ModuleData.PropertyChanged -= OnParentTabModuleStateChanged;
                ShouldQuitDialog = true;
                _listenToActionsToken?.Cancel();
            }
        }

        private static void SetBarConstantData(BarControlViewModel bar)
        {
            bar.IndicatorBorderColor = "Black";
        }

        private void GetValueFromParam<T>(ParameterContainer inParam, string paramString, Action<T> assignAction, T defaultValue, bool getPlainText = false)
        {
            object parameter = inParam.getParameter(paramString, null);
            if (parameter != null)
            {
                object value = parameter;
                if (parameter is ITextLocator textLocator)
                {
                    value = ((!getPlainText) ? GetContent(textLocator.TextContent) : textLocator.TextContent?.PlainText);
                }
                else if (parameter is Dictionary<double, string>)
                {
                    value = (parameter as Dictionary<double, string>).ToDictionary((KeyValuePair<double, string> x) => x.Key, (KeyValuePair<double, string> x) => (ServiceDialogColor)Enum.Parse(typeof(ServiceDialogColor), CultureInfo.CurrentCulture.TextInfo.ToTitleCase(x.Value.ToString())));
                }
                assignAction((T)Convert.ChangeType(value, typeof(T)));
            }
            else
            {
                assignAction(defaultValue);
            }
        }

        private void ListenToActions(bool isAsync)
        {
            Action listenAction = delegate
            {
                int millisecondsTimeout = ((Timeout > 0) ? Timeout : 100);
                while (!ShouldQuitDialog)
                {
                    ServiceProgramAction serviceProgramAction = base.ServiceProgramController.AwaitUserAction(millisecondsTimeout);
                    if (_listenToActionsToken != null && _listenToActionsToken.Token.IsCancellationRequested)
                    {
                        break;
                    }
                    if (parentTab.ModuleData.IsExecutionCompleted)
                    {
                        ShouldQuitDialog = true;
                    }
                    else if (serviceProgramAction is ServiceProgramNavigationAction)
                    {
                        ShouldQuitDialog = true;
                    }
                }
            };
            if (isAsync)
            {
                if (_listenToActionsTask == null || _listenToActionsTask.Status != TaskStatus.Running)
                {
                    parentTab.ModuleData.PropertyChanged += OnParentTabModuleStateChanged;
                    _listenToActionsToken = new CancellationTokenSource();
                    _listenToActionsTask = Task.Run(delegate
                    {
                        listenAction();
                    }, _listenToActionsToken.Token);
                }
            }
            else
            {
                StopListeningAsyncActions();
                listenAction();
            }
        }

        private void Wait(int milliseconds)
        {
            resumeEvent.WaitOne(milliseconds);
        }

        private void ProtocolToFasta(string method)
        {
            if (IsProtocollingEnabled && FastaProtocoler != null && m_Protocoller != null && m_Protocoller.HasData)
            {
                IAction<IUiDialog> action = FastaProtocoler.CreateAndAddUiDialogFromServiceProgram("HealthIndicator", method);
                action.SpecialAction.SetTitle(new TextContent("HealthIndicator").GetTextForUI(logic.Lang));
                action.StartTime = m_StartTimeForProtocol;
                List<LocalizedText> list = new List<LocalizedText>();
                list.AddRange(logic.Lang.Select((string x) => new LocalizedText("n/a", x)));
                action.SpecialAction.CreateAndAddMessageText(list);
                action.SpecialAction.AddAnswer(m_Protocoller.ProtocolLocalized, null);
            }
        }

        private void HandleFastaProtocolling()
        {
            if (IsProtocollingEnabled)
            {
                if (m_Protocoller == null)
                {
                    m_Protocoller = new HealthIndicatorProtocoller(logic.Lang);
                    m_StartTimeForProtocol = DateTime.Now;
                }
                m_Protocoller.CollectBarProtocolData(m_CurrentBarProtocolData);
            }
        }

        private void ToggleFullscreenOrSplitScreen()
        {
            if (!IsInitialized)
            {
                if (base.Model.IsFullScreen)
                {
                    base.ServiceProgramController.SetDisplayMode(DisplayMode.FullPrimary);
                }
                else
                {
                    base.ServiceProgramController.SetDisplayMode(DisplayMode.Split);
                }
            }
        }
    }
}
