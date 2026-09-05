using BMW.Rheingold.Module.ISTA;
using PsdzClient.Core.Container;

namespace BMW.Rheingold.Module.ISTA
{
    internal class MultiselectServiceDlgImpl : QuestionSelectServiceDlgImpl<MultiselectServiceDlgModel>
    {
        public MultiselectServiceDlgImpl(ParameterContainer inParameters)
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
            if ("WithButtonLabel_25".Equals(method) || "OnlyButtonText_25".Equals(method))
            {
                Invoke(method, inParam, outParam, inoutParam, isNextButtonEnabled: true);
            }
        }
    }
}
