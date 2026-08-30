using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;

namespace BMW.Rheingold.Module.ISTA
{
    internal class KurvendisplayDlgImpl : ServiceDlgImplBase<KurvendisplayDlgModel>
    {
        private bool startStoppRegistrated;

        private bool displayed;

        private double intervall;

        private bool isProtocoled;

        private const string SEPR = ";";

        private const string NEWROW = "\n";

        private string fastaMessage;

        private readonly CurveDisplayData data;

        private IList<LocalizedText> ueberschriftTextList;

        private IList<LocalizedText> einleitungTextList;

        private IList<LocalizedText> abschlussTextList;

        private double invokeDelay = 300.0;

        public bool EnableFullscreen
        {
            set
            {
                if (parentTab is IDiagnosticsModuleCoreTab diagnosticsModuleCoreTab && value != diagnosticsModuleCoreTab.IsAblTabsFullscreenEnabled)
                {
                    diagnosticsModuleCoreTab.IsScreenModeSelectionEnabled = value;
                    diagnosticsModuleCoreTab.IsAblTabsFullscreenEnabled = value;
                    diagnosticsModuleCoreTab.UpdateButtonLine();
                }
            }
        }

        public bool ManuelFreeze { get; set; }

        public double InitialMinValue { get; set; }

        public KurvendisplayDlgImpl(ParameterContainer inParam)
            : base(inParam)
        {
            ManuelFreeze = false;
            startStoppRegistrated = false;
            isProtocoled = false;
            data = base.Model.Data;
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            try
            {
                switch (method)
                {
                    case "Anzeige_4_Kurven":
                    case "Anzeige_4_Kurven_mit_Array":
                    case "Anzeige_12_Kurven":
                    case "Anzeige_12_Kurven_mit_Array":
                        {
                            ReadInParameter(inParam);
                            bool refresh = Convert.ToBoolean(inParam.getParameter("refresh", false));
                            bool protocol = Convert.ToBoolean(inParam.getParameter("protocol", true));
                            bool startstopp = Convert.ToBoolean(inParam.getParameter("startstopp", false));
                            EnableFullscreen = Convert.ToBoolean(inParam.getParameter("vollbild", false));
                            ICollection<CurveData> curves = GetCurves(method, inParam, inoutParam);
                            bool flag = ShowCurves(method, curves, refresh, protocol, startstopp);
                            outParam.setParameter("i_Weiter", flag);
                            break;
                        }
                    case "Anzeige_aus_Kurven":
                        Anzeige_aus_Kurven();
                        outParam.setParameter("i_Weiter", false);
                        invokeDelay = 0.0;
                        break;
                    default:
                        throw new ServiceDialogMethodUnsupportedException(method);
                }
            }
            finally
            {
                if (invokeDelay >= 1.0)
                {
                    base.DelayInvoke(invokeDelay);
                }
            }
        }

        private CurveData GetCurveData(CurveData curve)
        {
            CurveData curveData = null;
            if (base.Model.Data != null && base.Model.Curves.Any())
            {
                curveData = base.Model.Curves.SingleOrDefault((CurveData x) => x.Name.Equals(curve.Name));
            }
            if (curveData == null)
            {
                curveData = curve;
                base.Model.AddCurve(curveData);
            }
            else
            {
                curveData.Update(curve);
            }
            return curveData;
        }

        private void InitCurves(string methodName, ICollection<CurveData> curves)
        {
            switch (methodName)
            {
                case "Anzeige_4_Kurven":
                case "Anzeige_12_Kurven":
                    {
                        double minValueX = InitialMinValue - intervall;
                        foreach (CurveData curf in curves)
                        {
                            GetCurveData(curf).AddToCurve(curf.X, curf.Y, minValueX);
                        }
                        base.Model.UpdateCurves();
                        break;
                    }
                case "Anzeige_4_Kurven_mit_Array":
                case "Anzeige_12_Kurven_mit_Array":
                    foreach (CurveData curf2 in curves)
                    {
                        GetCurveData(curf2);
                    }
                    base.Model.UpdateCurves();
                    break;
                default:
                    throw new ServiceDialogMethodUnsupportedException(methodName);
            }
        }

        private bool ShowCurves(string methodName, ICollection<CurveData> curves, bool refresh, bool protocol, bool startstopp)
        {
            DateTime now = DateTime.Now;
            int num = 0;
            bool flag = false;
            if (!displayed)
            {
                Logger.WriteInformation(methodName + " called");
                displayed = true;
                ResetNextButtonLatency();
            }
            InitCurves(methodName, curves);
            if (startstopp)
            {
                ActivateStartStop();
            }
            DrawChart();
            SetNextButtonEnabled(value: true);
            if (data.MaxXValue <= base.Model.Curves.Max((CurveData x) => x.X))
            {
                if (!refresh)
                {
                    ActivateScrolling();
                    if (startstopp)
                    {
                        DeactivateStartStop();
                    }
                    flag = WaitForContinueButton();
                    DeactivateScrolling();
                }
                else if (data.MaxXValue < curves.Max((CurveData x) => x.X))
                {
                    intervall += data.XTeiler;
                }
            }
            while (ManuelFreeze && !IsNextButtonPressedWithinTimePeriod())
            {
                Thread.Sleep(50);
            }
            bool flag2 = flag || IsNextButtonPressedWithinTimePeriod();
            if (flag2)
            {
                ResetLastTimeNextButtonPressed();
                if (methodName.Equals("Anzeige_4_Kurven") && !isProtocoled)
                {
                    WriteFasta(base.Model.Curves, methodName, protocol, now);
                }
                ResetButtons();
                EnableFullscreen = false;
                if (startstopp)
                {
                    DeregisterStartStop();
                }
            }
            else if (methodName.Equals("Anzeige_4_Kurven"))
            {
                SaveInfosForFasta(base.Model.Curves, protocol);
            }
            if (!displayed)
            {
                Logger.WriteInformation("_ExitIndex is: {0}", num);
            }
            return flag2;
        }

        private void Anzeige_aus_Kurven()
        {
            Logger.WriteInformation("Anzeige_aus_Kurven called");
            intervall = 0.0;
            DeregisterStartStop();
            ClearView();
            ResetNextButtonLatency();
            displayed = false;
            if (base.ServiceDialogUI != null)
            {
                base.ServiceDialogUI.IsDialogShown = false;
            }
            Logger.WriteInformation("_ExitIndex is: {0}", 0);
        }

        private void WriteFasta(IEnumerable<CurveData> curves, string method, bool protocol, DateTime startTime)
        {
            IEnumerable<string> enumerable = curves.Select((CurveData curve) => GetStatisticForFasta(curve));
            string finalFastaMessage = string.Empty;
            if (protocol)
            {
                finalFastaMessage = fastaMessage + "\n";
                foreach (string item in enumerable)
                {
                    finalFastaMessage = finalFastaMessage + "\n" + item;
                }
                finalFastaMessage += "\n";
            }
            else
            {
                foreach (string item2 in enumerable)
                {
                    finalFastaMessage = finalFastaMessage + "\n" + item2;
                }
                finalFastaMessage += "\n";
            }
            IAction<IUiDialog> action = FastaProtocoler.CreateAndAddUiDialogFromServiceProgram("Dialog_Kurvendisplay", method);
            action.StartTime = startTime;
            List<LocalizedText> list = new List<LocalizedText>();
            list.AddRange(logic.Lang.Select((string x) => new LocalizedText(finalFastaMessage, x)));
            action.SpecialAction.CreateAndAddMessageText(list);
            isProtocoled = true;
        }

        private void SaveInfosForFasta(IEnumerable<CurveData> curves, bool protocol)
        {
            string text = string.Empty;
            foreach (CurveData curf in curves)
            {
                text = text + curf.X + ";" + curf.Y + ";";
                curf.Points.Add(new Tuple<double, double>(curf.X, curf.Y));
            }
            text += "\n";
            if (protocol)
            {
                fastaMessage += text;
            }
        }

        private string GetStatisticForFasta(CurveData data)
        {
            string name = data.Name;
            string text = ((!string.IsNullOrEmpty(data.Text)) ? data.Text : name);
            ICollection<Tuple<double, double>> points = data.Points;
            if (points.Count == 0)
            {
                return text + ": no values";
            }
            Tuple<double, double> tuple = points.First();
            Tuple<double, double> tuple2 = points.First();
            double num = 0.0;
            int num2 = 0;
            string text2 = "";
            string text3 = "";
            foreach (Tuple<double, double> item in points)
            {
                if (item.Item2 < tuple.Item2)
                {
                    tuple = item;
                }
                if (item.Item2 > tuple2.Item2)
                {
                    tuple2 = item;
                }
                num += item.Item2;
                num2++;
            }
            foreach (Tuple<double, double> item2 in points)
            {
                if (item2.Item2.Equals(tuple.Item2))
                {
                    if (string.IsNullOrEmpty(text2))
                    {
                        text2 = "(" + item2.Item1 + "," + item2.Item2 + ")";
                    }
                    else if (!tuple.Equals(new Point(0.0, 0.0)))
                    {
                        text2 = text2 + "," + "(" + item2.Item1 + "," + item2.Item2 + ")";
                    }
                }
                if (item2.Item2.Equals(tuple2.Item2))
                {
                    if (string.IsNullOrEmpty(text3))
                    {
                        text3 = "(" + item2.Item1 + "," + item2.Item2 + ")";
                    }
                    else if (!tuple2.Equals(new Point(0.0, 0.0)))
                    {
                        text3 = text3 + "," + "(" + item2.Item1 + "," + item2.Item2 + ")";
                    }
                }
            }
            double num3 = num / (double)num2;
            return text + ": MIN: " + text2 + "; MAX: " + text3 + "; MID: " + num3;
        }

        private void DrawChart()
        {
            base.Model.IsCurveDisplayVisible = true;
        }

        private void ClearView()
        {
            base.Model.Ueberschrift = null;
            base.Model.Einleitung = null;
            base.Model.ClearCurves();
            base.Model.Abschluss = null;
            base.Model.IsCurveDisplayVisible = false;
            EnableFullscreen = false;
        }

        private void ActivateScrolling()
        {
            if (parentTab is IDiagnosticsModuleCoreTab diagnosticsModuleCoreTab)
            {
                if (data.MinXValue == InitialMinValue)
                {
                    diagnosticsModuleCoreTab.CustomButton1.IsEnabled = false;
                }
                else
                {
                    diagnosticsModuleCoreTab.CustomButton1.IsEnabled = true;
                }
                diagnosticsModuleCoreTab.CustomButton1.Click += Button_Click_back;
                diagnosticsModuleCoreTab.CustomButton0.IsEnabled = true;
                diagnosticsModuleCoreTab.CustomButton0.Click += Button_Click_forward;
            }
        }

        private void Button_Click_back(object sender, RoutedEventArgs e)
        {
            if (parentTab is IDiagnosticsModuleCoreTab diagnosticsModuleCoreTab)
            {
                diagnosticsModuleCoreTab.CustomButton1.IsEnabled = data.MinXValue - data.XTeiler > InitialMinValue;
            }
            data.MinXValue -= data.XTeiler;
            data.MaxXValue -= data.XTeiler;
            base.Model.UpdateCurves();
        }

        private void Button_Click_forward(object sender, RoutedEventArgs e)
        {
            if (parentTab is IDiagnosticsModuleCoreTab diagnosticsModuleCoreTab)
            {
                diagnosticsModuleCoreTab.CustomButton1.IsEnabled = true;
            }
            data.MinXValue += data.XTeiler;
            data.MaxXValue += data.XTeiler;
            base.Model.UpdateCurves();
        }

        private void Button_Click_freeze(object sender, RoutedEventArgs e)
        {
            ManuelFreeze = !ManuelFreeze;
            if (ManuelFreeze)
            {
                ActivateScrolling();
            }
            else
            {
                DeactivateScrolling();
            }
        }

        private void DeactivateScrolling()
        {
            if (parentTab is IDiagnosticsModuleCoreTab diagnosticsModuleCoreTab)
            {
                diagnosticsModuleCoreTab.CustomButton1.IsEnabled = false;
                diagnosticsModuleCoreTab.CustomButton1.Click -= Button_Click_back;
                diagnosticsModuleCoreTab.CustomButton0.IsEnabled = false;
                diagnosticsModuleCoreTab.CustomButton0.Click -= Button_Click_forward;
            }
        }

        private void ResetButtons()
        {
            if (parentTab is IDiagnosticsModuleCoreTab diagnosticsModuleCoreTab)
            {
                diagnosticsModuleCoreTab.CustomButton1.Visibility = Visibility.Collapsed;
                diagnosticsModuleCoreTab.CustomButton1.IsEnabled = false;
                diagnosticsModuleCoreTab.CustomButton0.Visibility = Visibility.Collapsed;
                diagnosticsModuleCoreTab.CustomButton0.IsEnabled = false;
                diagnosticsModuleCoreTab.CustomButton2.Visibility = Visibility.Collapsed;
                diagnosticsModuleCoreTab.CustomButton2.IsEnabled = false;
            }
        }

        private void DeregisterStartStop()
        {
            if (startStoppRegistrated)
            {
                if (parentTab is IDiagnosticsModuleCoreTab diagnosticsModuleCoreTab)
                {
                    diagnosticsModuleCoreTab.CustomButton2.Click -= Button_Click_freeze;
                }
                startStoppRegistrated = false;
                ManuelFreeze = false;
                DeactivateScrolling();
            }
        }

        private void DeactivateStartStop()
        {
            if (parentTab is IDiagnosticsModuleCoreTab diagnosticsModuleCoreTab)
            {
                diagnosticsModuleCoreTab.CustomButton2.IsEnabled = false;
            }
        }

        private void ActivateStartStop()
        {
            if (parentTab is IDiagnosticsModuleCoreTab diagnosticsModuleCoreTab)
            {
                diagnosticsModuleCoreTab.KeyboardButton.Visibility = Visibility.Collapsed;
                diagnosticsModuleCoreTab.UpdateDetailsButton.Visibility = Visibility.Collapsed;
                diagnosticsModuleCoreTab.ReloadButton.Visibility = Visibility.Collapsed;
                diagnosticsModuleCoreTab.CustomButton1.Content = "Scroll Back";
                diagnosticsModuleCoreTab.CustomButton1.Visibility = Visibility.Visible;
                diagnosticsModuleCoreTab.CustomButton1.IsEnabled = false;
                diagnosticsModuleCoreTab.CustomButton0.Content = "Scroll Forward";
                diagnosticsModuleCoreTab.CustomButton0.Visibility = Visibility.Visible;
                diagnosticsModuleCoreTab.CustomButton0.IsEnabled = false;
                diagnosticsModuleCoreTab.CustomButton2.Content = "Start/Stop";
                diagnosticsModuleCoreTab.CustomButton2.Visibility = Visibility.Visible;
                diagnosticsModuleCoreTab.CustomButton2.IsEnabled = true;
                if (!startStoppRegistrated)
                {
                    diagnosticsModuleCoreTab.CustomButton2.Click += Button_Click_freeze;
                    startStoppRegistrated = true;
                }
            }
        }

        private void ReadInParameter(ParameterContainer inParam)
        {
            ueberschriftTextList = GetLocalizedText(inParam.getParameter("text_Ueberschrift", null) as ITextLocator);
            einleitungTextList = GetLocalizedText(inParam.getParameter("text_Einleitung", null) as ITextLocator);
            abschlussTextList = GetLocalizedText(inParam.getParameter("text_Abschluss", null) as ITextLocator);
            base.Model.Ueberschrift = ((ueberschriftTextList?.FirstOrDefault() != null) ? ueberschriftTextList.FirstOrDefault().TextItem : string.Empty);
            base.Model.Einleitung = ((einleitungTextList?.FirstOrDefault() != null) ? einleitungTextList.FirstOrDefault().TextItem : string.Empty);
            base.Model.Abschluss = ((abschlussTextList?.FirstOrDefault() != null) ? abschlussTextList.FirstOrDefault().TextItem : string.Empty);
            ITextLocator textLocator = inParam.getParameter("text_YAchse", null) as ITextLocator;
            data.UnitY = ((textLocator != null) ? textLocator.ToString() : string.Empty);
            data.YTeiler = Convert.ToDouble(inParam.getParameter("teiler_YAchse", 1));
            data.MinYValue = Convert.ToDouble(inParam.getParameter("minwert_YAchse", 0));
            data.MaxYValue = Convert.ToDouble(inParam.getParameter("maxwert_YAchse", 0.0));
            int yAxisColor = Convert.ToInt32(inParam.getParameter("farbe_YAchse", 1));
            data.YAxisColor = yAxisColor;
            if (inParam.getParameter("text_Y2Achse", null) is ITextLocator textLocator2)
            {
                data.UnitY2 = textLocator2.ToString();
            }
            data.Y2Teiler = Convert.ToDouble(inParam.getParameter("teiler_Y2Achse", 1));
            data.MinY2Value = Convert.ToDouble(inParam.getParameter("minwert_Y2Achse", 0));
            data.MaxY2Value = Convert.ToDouble(inParam.getParameter("maxwert_Y2Achse", 0.0));
            int y2AxisColor = Convert.ToInt32(inParam.getParameter("farbe_Y2Achse", 1));
            data.Y2AxisColor = y2AxisColor;
            if (inParam.getParameter("text_XAchse", null) is ITextLocator textLocator3)
            {
                data.UnitX = textLocator3.ToString();
            }
            data.XTeiler = Convert.ToDouble(inParam.getParameter("teiler_XAchse", 1));
            double num = Convert.ToDouble(inParam.getParameter("minwert_XAchse", 0));
            data.MinXValue = num + intervall;
            InitialMinValue = num;
            data.MaxXValue = Convert.ToDouble(inParam.getParameter("maxwert_XAchse", 0.0)) + intervall;
            data.CurveThickness = Convert.ToInt32(inParam.getParameter("strichstaerke_kurven", 2));
            data.CurveInPoints = !Convert.ToBoolean(inParam.getParameter("kurvendarstellung", true));
            data.Border1 = Convert.ToDouble(inParam.getParameter("grenze1_YAchse", 0.0));
            data.Border2 = Convert.ToDouble(inParam.getParameter("grenze2_YAchse", 0.0));
        }

        private ICollection<CurveData> GetCurves(string methodName, ParameterContainer inParam, ParameterContainer inoutParam)
        {
            ICollection<CurveData> result = new Collection<CurveData>();
            switch (methodName)
            {
                case "Anzeige_4_Kurven":
                    result = GetCurves(4, inParam, inoutParam);
                    break;
                case "Anzeige_4_Kurven_mit_Array":
                    result = GetCurvesWithArray(4, inParam, inoutParam);
                    break;
                case "Anzeige_12_Kurven":
                    result = GetCurves(12, inParam, inoutParam);
                    break;
                case "Anzeige_12_Kurven_mit_Array":
                    result = GetCurvesWithArray(12, inParam, inoutParam);
                    break;
                default:
                    Log.Error("KurvendisplayDlgImpl.GetCurves", "Invalid method name: {0}", methodName);
                    break;
            }
            return result;
        }

        private ICollection<CurveData> GetCurves(int numCurves, ParameterContainer inParam, ParameterContainer inoutParam)
        {
            ICollection<CurveData> collection = new Collection<CurveData>();
            for (int i = 1; i <= numCurves; i++)
            {
                int strokeColor = Convert.ToInt32(inParam.getParameter(string.Format(CultureInfo.InvariantCulture, "kurve{0:00}_farbe", i), 1));
                bool isY = Convert.ToBoolean(inParam.getParameter(string.Format(CultureInfo.InvariantCulture, "kurve{0:00}_BezugzuY2", i), false));
                IList<LocalizedText> localizedText = GetLocalizedText(inParam.getParameter(string.Format(CultureInfo.InvariantCulture, "kurve{0:00}_text", i), null) as ITextLocator);
                double x = Convert.ToDouble(inoutParam.getParameter(string.Format(CultureInfo.InvariantCulture, "kurve{0:00}_x", i), double.NaN));
                double y = Convert.ToDouble(inoutParam.getParameter(string.Format(CultureInfo.InvariantCulture, "kurve{0:00}_y", i), double.NaN));
                string name = string.Format(CultureInfo.InvariantCulture, "kurve{0:00}", i);
                CurveData curveData = new CurveData
                {
                    Name = name,
                    StrokeColor = strokeColor,
                    IsY2 = isY,
                    X = x,
                    Y = y
                };
                curveData.Text = ((localizedText != null && localizedText.Any()) ? localizedText[0].TextItem : string.Empty);
                curveData.IsVisible = !string.IsNullOrEmpty(curveData.Text);
                collection.Add(curveData);
            }
            return collection;
        }

        private ICollection<CurveData> GetCurvesWithArray(int numCurves, ParameterContainer inParam, ParameterContainer inoutParam)
        {
            ICollection<CurveData> collection = new Collection<CurveData>();
            for (int i = 1; i <= numCurves; i++)
            {
                int strokeColor = Convert.ToInt32(inParam.getParameter(string.Format(CultureInfo.InvariantCulture, "kurve{0:00}_farbe", i), 1));
                bool isY = Convert.ToBoolean(inParam.getParameter(string.Format(CultureInfo.InvariantCulture, "kurve{0:00}_BezugzuY2", i), false));
                IList<LocalizedText> localizedText = GetLocalizedText(inParam.getParameter(string.Format(CultureInfo.InvariantCulture, "kurve{0:00}_text", i), null) as ITextLocator);
                string name = string.Format(CultureInfo.InvariantCulture, "kurve{0:00}", i);
                double[,] curveNewArg = inoutParam.getParameter(name, null) as double[,];
                CurveData curveData = new CurveData
                {
                    Name = name,
                    StrokeColor = strokeColor,
                    IsY2 = isY
                };
                curveData.Text = ((localizedText != null && localizedText.Any()) ? localizedText[0].TextItem : string.Empty);
                curveData.IsVisible = !string.IsNullOrEmpty(curveData.Text);
                CurveNewDataCotainer curveNewContainer = new CurveNewDataCotainer(curveNewArg);
                curveData.CutCurve(curveNewContainer);
                collection.Add(curveData);
            }
            return collection;
        }

        private IList<LocalizedText> GetLocalizedText(ITextLocator textLocator)
        {
            IList<LocalizedText> result = new List<LocalizedText>();
            TextContent textContent = ((textLocator != null && textLocator.TextContent is TextContent) ? ((TextContent)textLocator.TextContent) : null);
            if (textContent != null)
            {
                result = textContent.GetTextForUI(logic.Lang);
            }
            return result;
        }
    }
}
