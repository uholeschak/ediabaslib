using PsdzClient.Core.Container;

namespace BMW.Rheingold.Module.ISTA
{
    internal class MehrfachAuswahlDlgImpl : QuestionSelectServiceDlgImpl<MehrfachAuswahlDlgModel>
    {
        public MehrfachAuswahlDlgImpl(ParameterContainer inParameters)
            : base(inParameters)
        {
        }

        public override ParameterContainer FinishDialog(ParameterContainer inoutParam)
        {
            ParameterContainer parameterContainer = new ParameterContainer();
            parameterContainer.setParameter("SelektionAuswahl", GetSelectionSettings());
            Dispose();
            return parameterContainer;
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            if ("ButtonLabel_Vorbelegung".Equals(method))
            {
                Invoke(method, inParam, outParam, inoutParam, isNextButtonEnabled: true);
            }
            else if ("InitializeDialog2".Equals(method))
            {
                Invoke(method, inParam, outParam, inoutParam, isNextButtonEnabled: false);
            }
            else if ("WithButtonLabel_25".Equals(method) || "OnlyButtonText_25".Equals(method))
            {
                Invoke(method, inParam, outParam, inoutParam, isNextButtonEnabled: true);
            }
        }
    }
}
