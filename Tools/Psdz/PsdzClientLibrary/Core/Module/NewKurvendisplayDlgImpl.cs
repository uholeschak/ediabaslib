using BMW.ISPI.IstaOperation.Contract.Document;
using BMW.ISPI.IstaOperation.Contract.ServiceProgram;
using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;

#pragma warning disable SYSLIB0006
namespace BMW.Rheingold.Module.ISTA
{
    internal class NewKurvendisplayDlgImpl : ServiceDlgImplBase<NewKurvendisplayDlgModel>
    {
        private static readonly string CURVE_LINE_COLOR_PARAM_TEMPLATE = "Kurve{0:00}_Farbe";

        private static readonly string CURVE_LINE_STYLE_PARAM_TEMPLATE = "Kurve{0:00}_Anzeige";

        private static readonly string CURVE_LINE_WIDTH_PARAM_TEMPLATE = "Kurve{0:00}_Strichstaerke";

        private static readonly string CURVE_TEXT_PARAM_TEMPLATE = "Kurve{0:00}_Text";

        private static readonly string CURVE_YAXIS_PARAM_TEMPLATE = "Kurve{0:00}_YAchse";

        private static readonly string CURVE_YPOINT_PARAM_TEMPLATE = "Kurve{0:00}_Y";

        private static readonly string CURVE_YPOINTS_LIST_PARAM_TEMPLATE = "Kurve{0:00}_Y_List";

        private bool displayLast;

        private bool hasDynamicDisplayBeenInitialized;

        private Thread listenToActionsThread;

        private bool shouldQuitDialog;

        private AutoResetEvent resumeEvent = new AutoResetEvent(initialState: false);

        private bool ShouldQuitDialog
        {
            get
            {
                return shouldQuitDialog;
            }
            set
            {
                if (shouldQuitDialog != value)
                {
                    shouldQuitDialog = value;
                    if (shouldQuitDialog)
                    {
                        resumeEvent.Set();
                    }
                }
            }
        }

        public NewKurvendisplayDlgImpl(ParameterContainer inParam)
            : base(inParam)
        {
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            displayLast = false;
            Log.Debug(Log.CurrentMethod(), "Method " + method + " is called");
            if ("statische_Anzeige".Equals(method))
            {
                Reset();
                base.Model.IsStatic = true;
                base.Model.BackgroundColor = new ObservableCollection<string>(new string[4] { "#CCCCCC", "#CCCCCC", "#CCCCCC", "#CCCCCC" });
                InitializeGraph(inParam);
                UpdateStaticCurvePoints(inParam);
                base.Model.UpdateReferenceVerticalAxis();
            }
            else if ("dynamische_Anzeige".Equals(method))
            {
                base.Model.IsStatic = false;
                base.Model.BackgroundColor = new ObservableCollection<string>(new string[4] { "#FFFFFF", "#FFFFFF", "#FFFFFF", "#FFFFFF" });
                if (!hasDynamicDisplayBeenInitialized)
                {
                    Reset();
                    ListenToActions(isAsync: true);
                    InitializeGraph(inParam);
                    hasDynamicDisplayBeenInitialized = true;
                    base.Model.UpdateReferenceVerticalAxis();
                }
                UpdateDynamicTextValues(inParam);
                UpdateDynamicCurvePoints(inParam);
            }
            else if ("letzte_Anzeige".Equals(method))
            {
                base.Model.BackgroundColor = new ObservableCollection<string>(new string[4] { "#CCCCCC", "#CCCCCC", "#CCCCCC", "#CCCCCC" });
                displayLast = true;
                ShouldQuitDialog = false;
                base.Model.ActionButtons.Clear();
                base.Model.IsStatic = true;
                InitializeGraph(inParam);
                base.Model.UpdateReferenceVerticalAxis();
            }
            else if ("Dialog_Ausblenden".Equals(method))
            {
                Reset();
                base.ServiceDialogUI.IsDialogShown = false;
            }
            if (!Convert.ToBoolean(inParam.getParameter("Bestaetigung", false)))
            {
                int milliseconds = Convert.ToInt32(inParam.getParameter("Anzeigedauer", 0));
                Wait(milliseconds);
                if (ShouldQuitDialog)
                {
                    outParam.setParameter("QUIT", true);
                }
            }
            else
            {
                ListenToActions(isAsync: false);
                base.ServiceDialogUI.IsDialogShown = false;
                outParam.setParameter("QUIT", true);
            }
            SetOutParams(outParam);
        }

        private void AddToActionButtonCollection(int buttonNumber, string contentText)
        {
            if (!string.IsNullOrEmpty(contentText))
            {
                KurvenDisplayActionButton item = new KurvenDisplayActionButton
                {
                    Content = contentText,
                    ButtonNumber = buttonNumber
                };
                base.Model.ActionButtons.Add(item);
            }
        }

        private void CreateSampledPointsContainer(int targetSamplePointsOnXAxis, int maxSamplePointsOnXAxis)
        {
            base.Model.SampledPointsContainer = new SampledPointsIndexContainer(base.Model.MinXValue, base.Model.MaxXValue, base.Model.Curves.Count, targetSamplePointsOnXAxis, maxSamplePointsOnXAxis);
        }

        private List<NewCurveData> GetCurvesForDynamicDisplay(ParameterContainer inParam)
        {
            List<NewCurveData> list = new List<NewCurveData>();
            for (int i = 1; i <= 30; i++)
            {
                double? num = inParam.getParameter(string.Format(CURVE_YPOINT_PARAM_TEMPLATE, i), null) as double?;
                int num2 = Convert.ToInt32(inParam.getParameter(string.Format(CURVE_YAXIS_PARAM_TEMPLATE, i), -1));
                string legendText = (inParam.getParameter(string.Format(CURVE_TEXT_PARAM_TEMPLATE, i), i) as ITextLocator)?.TextContent?.PlainText;
                int thickness = Convert.ToInt32(inParam.getParameter(string.Format(CURVE_LINE_WIDTH_PARAM_TEMPLATE, i), 1));
                int num3 = Convert.ToInt32(inParam.getParameter(string.Format(CURVE_LINE_STYLE_PARAM_TEMPLATE, i), 0));
                int color = Convert.ToInt32(inParam.getParameter(string.Format(CURVE_LINE_COLOR_PARAM_TEMPLATE, i), 1));
                if (num.HasValue && num2 != -1 && num3 != 0)
                {
                    NewCurveData newCurveData = new NewCurveData();
                    newCurveData.LegendText = legendText;
                    newCurveData.YAxis = num2;
                    newCurveData.Thickness = thickness;
                    newCurveData.Style = num3;
                    newCurveData.Color = color;
                    newCurveData.Index = i;
                    newCurveData.YPoints.Add(Math.Round(num.Value, 2));
                    list.Add(newCurveData);
                }
            }
            return list.OrderBy((NewCurveData x) => x.YAxis).ToList();
        }

        private List<NewCurveData> GetCurvesForStaticDisplay(ParameterContainer inParam)
        {
            List<NewCurveData> list = new List<NewCurveData>();
            for (int i = 1; i <= 30; i++)
            {
                List<double> list2 = inParam.getParameter(string.Format(CURVE_YPOINTS_LIST_PARAM_TEMPLATE, i), null) as List<double>;
                int num = Convert.ToInt32(inParam.getParameter(string.Format(CURVE_YAXIS_PARAM_TEMPLATE, i), -1));
                string legendText = (inParam.getParameter(string.Format(CURVE_TEXT_PARAM_TEMPLATE, i), i) as ITextLocator)?.TextContent?.PlainText;
                int thickness = Convert.ToInt32(inParam.getParameter(string.Format(CURVE_LINE_WIDTH_PARAM_TEMPLATE, i), 1));
                int num2 = Convert.ToInt32(inParam.getParameter(string.Format(CURVE_LINE_STYLE_PARAM_TEMPLATE, i), 0));
                int color = Convert.ToInt32(inParam.getParameter(string.Format(CURVE_LINE_COLOR_PARAM_TEMPLATE, i), 1));
                if (list2 != null && num != -1 && num2 != 0)
                {
                    NewCurveData newCurveData = new NewCurveData();
                    newCurveData.LegendText = legendText;
                    newCurveData.YPoints.AddRange(list2);
                    newCurveData.YAxis = num;
                    newCurveData.Thickness = thickness;
                    newCurveData.Style = num2;
                    newCurveData.Color = color;
                    newCurveData.Index = i;
                    list.Add(newCurveData);
                }
            }
            return list.OrderBy((NewCurveData x) => x.YAxis).ToList();
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
                assignAction((T)Convert.ChangeType(value, typeof(T)));
            }
            else if (!displayLast)
            {
                assignAction(defaultValue);
            }
        }

        private void InitializeGraph(ParameterContainer inParam)
        {
            SetNextButtonEnabled(value: true);
            GetValueFromParam(inParam, "Ueberschrift", delegate (string x)
            {
                base.Model.HeaderText = x;
            }, null);
            GetValueFromParam(inParam, "Einleitung", delegate (string x)
            {
                base.Model.IntroductionText = x;
            }, null);
            GetValueFromParam(inParam, "Abschluss", delegate (string x)
            {
                base.Model.ConclusionText = x;
            }, null);
            GetValueFromParam(inParam, "Text_Legende_Y1Achse", delegate (string x)
            {
                base.Model.YAxisLegendName[0] = x;
            }, null);
            GetValueFromParam(inParam, "Text_Legende_Y2Achse", delegate (string x)
            {
                base.Model.YAxisLegendName[1] = x;
            }, null);
            GetValueFromParam(inParam, "Text_Legende_Y3Achse", delegate (string x)
            {
                base.Model.YAxisLegendName[2] = x;
            }, null);
            GetValueFromParam(inParam, "Text_Legende_Y4Achse", delegate (string x)
            {
                base.Model.YAxisLegendName[3] = x;
            }, null);
            GetValueFromParam(inParam, "Text_XAchse", delegate (string x)
            {
                base.Model.XAxisText = x;
            }, null, getPlainText: true);
            GetValueFromParam(inParam, "Text_Y1Achse", delegate (string x)
            {
                base.Model.YAxisText[0] = x;
            }, null, getPlainText: true);
            GetValueFromParam(inParam, "Text_Y2Achse", delegate (string x)
            {
                base.Model.YAxisText[1] = x;
            }, null, getPlainText: true);
            GetValueFromParam(inParam, "Text_Y3Achse", delegate (string x)
            {
                base.Model.YAxisText[2] = x;
            }, null, getPlainText: true);
            GetValueFromParam(inParam, "Text_Y4Achse", delegate (string x)
            {
                base.Model.YAxisText[3] = x;
            }, null, getPlainText: true);
            GetValueFromParam(inParam, "Minwert_XAchse", delegate (double x)
            {
                base.Model.MinXValue = x;
            }, 0.0);
            GetValueFromParam(inParam, "Maxwert_XAchse", delegate (double x)
            {
                base.Model.MaxXValue = x;
            }, 0.0);
            GetValueFromParam(inParam, "Teiler_XAchse", delegate (double x)
            {
                base.Model.XAxisDivision = x;
            }, 0.0);
            GetValueFromParam(inParam, "Teiler_Y1Achse", delegate (double x)
            {
                base.Model.YAxisDivision[0] = x;
            }, 0.0);
            GetValueFromParam(inParam, "Teiler_Y2Achse", delegate (double x)
            {
                base.Model.YAxisDivision[1] = x;
            }, 0.0);
            GetValueFromParam(inParam, "Teiler_Y3Achse", delegate (double x)
            {
                base.Model.YAxisDivision[2] = x;
            }, 0.0);
            GetValueFromParam(inParam, "Teiler_Y4Achse", delegate (double x)
            {
                base.Model.YAxisDivision[3] = x;
            }, 0.0);
            GetValueFromParam(inParam, "Minwert_Y1Achse", delegate (double x)
            {
                base.Model.MinYValue[0] = x;
            }, 0.0);
            GetValueFromParam(inParam, "Minwert_Y2Achse", delegate (double x)
            {
                base.Model.MinYValue[1] = x;
            }, 0.0);
            GetValueFromParam(inParam, "Minwert_Y3Achse", delegate (double x)
            {
                base.Model.MinYValue[2] = x;
            }, 0.0);
            GetValueFromParam(inParam, "Minwert_Y4Achse", delegate (double x)
            {
                base.Model.MinYValue[3] = x;
            }, 0.0);
            GetValueFromParam(inParam, "Maxwert_Y1Achse", delegate (double x)
            {
                base.Model.MaxYValue[0] = x;
            }, 0.0);
            GetValueFromParam(inParam, "Maxwert_Y2Achse", delegate (double x)
            {
                base.Model.MaxYValue[1] = x;
            }, 0.0);
            GetValueFromParam(inParam, "Maxwert_Y3Achse", delegate (double x)
            {
                base.Model.MaxYValue[2] = x;
            }, 0.0);
            GetValueFromParam(inParam, "Maxwert_Y4Achse", delegate (double x)
            {
                base.Model.MaxYValue[3] = x;
            }, 0.0);
            GetValueFromParam(inParam, "Grenze1_Y1Achse", delegate (double x)
            {
                base.Model.UpperLimitY[0] = x;
            }, double.NaN);
            GetValueFromParam(inParam, "Grenze1_Y2Achse", delegate (double x)
            {
                base.Model.UpperLimitY[1] = x;
            }, double.NaN);
            GetValueFromParam(inParam, "Grenze1_Y3Achse", delegate (double x)
            {
                base.Model.UpperLimitY[2] = x;
            }, double.NaN);
            GetValueFromParam(inParam, "Grenze1_Y4Achse", delegate (double x)
            {
                base.Model.UpperLimitY[3] = x;
            }, double.NaN);
            GetValueFromParam(inParam, "Grenze2_Y1Achse", delegate (double x)
            {
                base.Model.LowerLimitY[0] = x;
            }, double.NaN);
            GetValueFromParam(inParam, "Grenze2_Y2Achse", delegate (double x)
            {
                base.Model.LowerLimitY[1] = x;
            }, double.NaN);
            GetValueFromParam(inParam, "Grenze2_Y3Achse", delegate (double x)
            {
                base.Model.LowerLimitY[2] = x;
            }, double.NaN);
            GetValueFromParam(inParam, "Grenze2_Y4Achse", delegate (double x)
            {
                base.Model.LowerLimitY[3] = x;
            }, double.NaN);
            GetValueFromParam(inParam, "Kurvendarstellung", delegate (bool x)
            {
                base.Model.LinearInterpolationEnabled = x;
            }, defaultValue: true);
            GetValueFromParam(inParam, "Refresh", delegate (bool x)
            {
                base.Model.HorizontalOverflowScrollingEnabled = x;
            }, defaultValue: true);
            GetValueFromParam(inParam, "Kurvennummerierung", delegate (bool x)
            {
                base.Model.ShowCurveNumbers = x;
            }, defaultValue: false);
            GetValueFromParam(inParam, "Button03_Text", delegate (string x)
            {
                AddToActionButtonCollection(3, x);
            }, null, getPlainText: true);
            GetValueFromParam(inParam, "Button02_Text", delegate (string x)
            {
                AddToActionButtonCollection(2, x);
            }, null, getPlainText: true);
            GetValueFromParam(inParam, "Button01_Text", delegate (string x)
            {
                AddToActionButtonCollection(1, x);
            }, null, getPlainText: true);
            if (Convert.ToBoolean(inParam.getParameter("Vollbild", false)))
            {
                base.ServiceProgramController.SetDisplayMode(DisplayMode.FullPrimary);
            }
            else
            {
                base.ServiceProgramController.SetDisplayMode(DisplayMode.Split);
            }
        }

        private void ListenToActions(bool isAsync)
        {
            Action listenAction = delegate
            {
                while (!ShouldQuitDialog)
                {
                    ServiceProgramAction serviceProgramAction = base.ServiceProgramController.AwaitUserAction(-1);
                    if (parentTab.ModuleData.IsExecutionCompleted)
                    {
                        ShouldQuitDialog = true;
                    }
                    else if (serviceProgramAction is ServiceProgramNavigationAction)
                    {
                        ShouldQuitDialog = true;
                    }
                    else
                    {
                        ServiceProgramButtonSelectionAction buttonAction = serviceProgramAction as ServiceProgramButtonSelectionAction;
                        if (buttonAction != null)
                        {
                            base.Model.ActionButtons.FirstOrDefault((KurvenDisplayActionButton x) => x.ButtonNumber == buttonAction.SelectedIndex)?.ToogleExecuteState();
                        }
                    }
                }
            };
            if (isAsync)
            {
                parentTab.ModuleData.PropertyChanged += OnParentTabModuleStateChanged;
                listenToActionsThread = new Thread((ThreadStart)delegate
                {
                    listenAction();
                });
                listenToActionsThread.Start();
            }
            else
            {
                StopListeningAsyncActions();
                listenAction();
            }
        }

        private void OnParentTabModuleStateChanged(object o, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ModuleState" && (parentTab.ModuleData.IsExecutionCompleted || parentTab.ModuleData.Status == typeDiagObjectState.Canceled))
            {
                StopListeningAsyncActions();
            }
        }

        private void Reset()
        {
            hasDynamicDisplayBeenInitialized = false;
            ShouldQuitDialog = false;
            base.Model.SampledPointsContainer = null;
            base.Model.ActionButtons.Clear();
            base.Model.XPoints.Clear();
            base.Model.Curves.Clear();
        }

        private void SetOutParams(ParameterContainer outParam)
        {
            foreach (KurvenDisplayActionButton actionButton in base.Model.ActionButtons)
            {
                if (actionButton.ExecuteAction)
                {
                    outParam.setParameter($"Button{actionButton.ButtonNumber:00}", true);
                }
                else
                {
                    outParam.setParameter($"Button{actionButton.ButtonNumber:00}", false);
                }
            }
        }

        private void StopListeningAsyncActions()
        {
            if (listenToActionsThread != null)
            {
                parentTab.ModuleData.PropertyChanged -= OnParentTabModuleStateChanged;
                listenToActionsThread.Abort();
            }
        }

        private void UpdateDynamicCurvePoints(ParameterContainer inParam)
        {
            if (base.Model.Curves == null || base.Model.Curves.Count == 0)
            {
                List<NewCurveData> curvesForDynamicDisplay = GetCurvesForDynamicDisplay(inParam);
                base.Model.Curves.AddRange(curvesForDynamicDisplay);
                CreateSampledPointsContainer(80, 120);
            }
            else
            {
                foreach (NewCurveData curf in base.Model.Curves)
                {
                    double? num = inParam.getParameter(string.Format(CURVE_YPOINT_PARAM_TEMPLATE, curf.Index), null) as double?;
                    if (num.HasValue)
                    {
                        curf.YPoints.Add(Math.Round(num.Value, 2));
                    }
                    int num2 = Convert.ToInt32(inParam.getParameter(string.Format(CURVE_LINE_STYLE_PARAM_TEMPLATE, curf.Index), -1));
                    if (num2 != -1)
                    {
                        curf.Style = num2;
                    }
                }
            }
            double? num3 = inParam.getParameter("Kurven_X", null) as double?;
            if (num3.HasValue)
            {
                base.Model.XPoints.Add(Math.Round(num3.Value, 2));
                base.Model.SampledPointsContainer.TryAddingSampleIndex(base.Model.XPoints.Count - 1, num3.Value);
            }
        }

        private void UpdateDynamicTextValues(ParameterContainer inParam)
        {
            GetValueFromParam(inParam, "Ueberschrift", delegate (string x)
            {
                base.Model.HeaderText = x;
            }, null);
            GetValueFromParam(inParam, "Einleitung", delegate (string x)
            {
                base.Model.IntroductionText = x;
            }, null);
            GetValueFromParam(inParam, "Abschluss", delegate (string x)
            {
                base.Model.ConclusionText = x;
            }, null);
            foreach (KurvenDisplayActionButton button in base.Model.ActionButtons)
            {
                GetValueFromParam(inParam, $"Button{button.ButtonNumber:00}_Text", delegate (string x)
                {
                    button.Content = x;
                }, null, getPlainText: true);
            }
        }

        private void UpdateSampledPoints()
        {
            for (int i = 0; i < base.Model.XPoints.Count; i++)
            {
                base.Model.SampledPointsContainer.TryAddingSampleIndex(i, base.Model.XPoints.ElementAt(i));
            }
        }

        private void UpdateStaticCurvePoints(ParameterContainer inParam)
        {
            if (inParam.getParameter("Kurven_X_List", null) is List<double> items)
            {
                base.Model.XPoints.AddRange(items);
                List<NewCurveData> curvesForStaticDisplay = GetCurvesForStaticDisplay(inParam);
                base.Model.Curves.AddRange(curvesForStaticDisplay);
                CreateSampledPointsContainer(2000, 2000);
                UpdateSampledPoints();
            }
        }

        private void Wait(int milliseconds)
        {
            resumeEvent.WaitOne(milliseconds);
        }
    }
}
