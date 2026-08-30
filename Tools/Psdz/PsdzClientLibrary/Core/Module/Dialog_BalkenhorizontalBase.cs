using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.Module.ISTA;
using BMW.Rheingold.RheingoldSessionController;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.Module.ISTA
{
    internal class Dialog_BalkenhorizontalBase : ServiceDlgImplBase<BalkenHorizontalDlgModel>
    {
        private const string NEWROW = "<br/>";

        private const string SEP = ";";

        private const string FirstMinMaxLastPlaceholder = "{MinFirstLastMax}";

        private bool isProtocoled;

        private int barCount;

        private double?[] firstValues;

        private double[] lastValues;

        private double[] smallestValues;

        private double[] largestValues;

        private Queue<string>[] savedValues;

        protected DateTime startTime;

        public Dialog_BalkenhorizontalBase(ParameterContainer InParameter, int barCount)
            : base(InParameter)
        {
            isProtocoled = false;
            this.barCount = barCount;
            savedValues = new Queue<string>[barCount];
            for (int i = 0; i < barCount; i++)
            {
                base.Model.Balken.Add(new BalkenHorizontalControlModel());
                savedValues[i] = new Queue<string>();
            }
            firstValues = new double?[barCount];
            lastValues = new double[barCount];
            smallestValues = new double[barCount];
            largestValues = new double[barCount];
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            startTime = DateTime.Now;
            if ($"AnzeigeEin_Formular_{barCount}BalkenH".Equals(method))
            {
                AnzeigeEin_Formular_BalkenH(inParam, outParam, inoutParam);
                HandleSavedValues(inoutParam);
                outParam.setParameter("i_Weiter", IsNextButtonPressedWithinTimePeriod());
                ResetLastTimeNextButtonPressed();
                return;
            }
            if ($"AnzeigeAus_Formular_{barCount}BalkenH".Equals(method))
            {
                AnzeigeAus_Formular_BalkenH(outParam);
                if (base.ServiceDialogUI != null)
                {
                    base.ServiceDialogUI.IsDialogShown = false;
                }
                return;
            }
            throw new ServiceDialogMethodUnsupportedException(method);
        }

        protected void AnzeigeEin_Formular_BalkenH(ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            Logger.WriteInformation($"AnzeigeEin_Formular_{barCount}BalkenH" + " called");
            base.Model.SetValues(logic.Lang, inParam, outParam, inoutParam);
            SetNextButtonEnabled(value: true);
            Logger.WriteInformation("_ExitIndex is: {0}", 0);
        }

        private void HandleSavedValues(ParameterContainer inoutParam)
        {
            for (int i = 0; i < barCount; i++)
            {
                double barValue = base.Model.Balken[i].BarValue;
                inoutParam.setParameter($"i_Balkenwert{i + 1}", barValue);
                savedValues[i].Enqueue(FormatBarValue(barValue, base.Model.Balken[i].BarValueFormat));
                if (savedValues[i].Count > 100)
                {
                    savedValues[i].Dequeue();
                }
                firstValues[i] = (firstValues[i].HasValue ? firstValues[i].Value : barValue);
                lastValues[i] = barValue;
                smallestValues[i] = ((smallestValues[i] < barValue) ? smallestValues[i] : barValue);
                largestValues[i] = ((largestValues[i] > barValue) ? largestValues[i] : barValue);
            }
        }

        protected void AnzeigeAus_Formular_BalkenH(ParameterContainer outParam)
        {
            Logger.WriteInformation($"AnzeigeAus_Formular_{barCount}BalkenH" + " called");
            SetNextButtonEnabled(value: false);
            if (!isProtocoled)
            {
                WriteFasta($"AnzeigeAus_Formular_{barCount}BalkenH", outParam);
            }
            Logger.WriteInformation("_ExitIndex is: {0}", 0);
        }

        private void WriteFasta(string method, ParameterContainer outParam)
        {
            ParameterContainer parameter = new ParameterContainer();
            outParam.setParameter("_FASTA", parameter);
            IAction<IUiDialog> action = FastaProtocoler.CreateAndAddUiDialogFromServiceProgram($"Dialog_{barCount}Balkenhorizontal", method);
            action.StartTime = startTime;
            List<LocalizedText> list = new List<LocalizedText>();
            string fastaMessage = ConstructFastaMessage();
            IList<LocalizedText> minFirstLastMaxLocalized = new FormatedData("#MinFirstLastMax").Localize(logic.Lang);
            list.AddRange(logic.Lang.Select((string x) => new LocalizedText(fastaMessage.Replace("{MinFirstLastMax}", minFirstLastMaxLocalized.FirstOrDefault((LocalizedText l) => l.Language == x).TextItem), x)));
            action.SpecialAction.CreateAndAddMessageText(list);
            isProtocoled = true;
        }

        private string ConstructFastaMessage()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.Model.TxtObereTextbox);
            stringBuilder.Append("<br/>");
            for (int i = 0; i < barCount; i++)
            {
                if (!string.IsNullOrWhiteSpace(base.Model.Balken[i].TxtOverBalkenTextbox))
                {
                    stringBuilder.Append(base.Model.Balken[i].TxtOverBalkenTextbox);
                    stringBuilder.Append("<br/>");
                }
                stringBuilder.Append("{MinFirstLastMax}");
                stringBuilder.Append("<br/>");
                string barValueFormat = base.Model.Balken[i].BarValueFormat;
                string text = FormatBarValue(firstValues[i].HasValue ? firstValues[i].Value : 0.0, barValueFormat) ?? "";
                stringBuilder.Append(string.IsNullOrWhiteSpace(text) ? "-" : text);
                stringBuilder.Append(";");
                string text2 = FormatBarValue(smallestValues[i], barValueFormat) ?? "";
                stringBuilder.Append(string.IsNullOrWhiteSpace(text2) ? "-" : text2);
                stringBuilder.Append(";");
                string text3 = FormatBarValue(largestValues[i], barValueFormat) ?? "";
                stringBuilder.Append(string.IsNullOrWhiteSpace(text3) ? "-" : text3);
                stringBuilder.Append(";");
                string text4 = FormatBarValue(lastValues[i], barValueFormat) ?? "";
                stringBuilder.Append(string.IsNullOrWhiteSpace(text4) ? "-" : text4);
                stringBuilder.Append("<br/>");
                stringBuilder.Append("<br/>");
                stringBuilder.Append(string.Join(";", savedValues[i].Where((string v) => !string.IsNullOrWhiteSpace(v))));
                stringBuilder.Append("<br/>");
                stringBuilder.Append("<br/>");
            }
            stringBuilder.Append(base.Model.TxtUntereTextbox);
            return stringBuilder.ToString();
        }

        private string FormatBarValue(double barValue, string format)
        {
            if (!string.IsNullOrWhiteSpace(format))
            {
                return string.Format("{0:" + format + "}", barValue);
            }
            return barValue.ToString();
        }
    }
}
