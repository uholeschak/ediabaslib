using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.Module.ISTA;
using BMW.Rheingold.RheingoldSessionController;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Threading;

namespace BMW.Rheingold.Module.ISTA
{
    public abstract class ISTAServiceDialog : ISTAModule, IServiceDialog
    {
        private SidisPanel sidisPanel;

        private DateTime lastInvokeExecutionTime = DateTime.Now;

        public IModuleExecutionStep CurrentStep { get; set; }

        internal SidisPanel Panel
        {
            get
            {
                if (sidisPanel == null)
                {
                    sidisPanel = new SidisPanel(_globalTabModuleISTA);
                }
                return sidisPanel;
            }
        }

        internal ISPEUserInterface UserInterface => base.SPEUserInterface;

        public IModuleExecutionStep ServiceDialogUI { get; set; }

        public virtual void SetResultSetFromServiceProgram(IResult resultSet)
        {
        }

        protected IProtocolBasic RetrieveFasta(ParameterContainer inParameters)
        {
            if (inParameters.getParameter("FASTA") is IFastaGrouping fastaGrouping)
            {
                return fastaGrouping.ProtocolingInstance;
            }
            return null;
        }

        public override InfoObject GetInfoObjStarted()
        {
            InfoObject obj = (_globalModuleInParameter.getParameter("__RheinGoldCoreModuleParameters__") as ModuleParameter).getParameter(ModuleParameter.ParameterName.InfoObjStarted) as InfoObject;
            if (obj == null)
            {
                Log.Error("ISTAServiceDialog.GetInfoObjStarted()", "Infoobject is null.");
            }
            return obj;
        }

        public abstract void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam);

        public void InvokeMain(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            BeforeInvoke(method, inParam, inoutParam);
            Invoke(method, inParam, outParam, inoutParam);
            ParameterContainer parameterContainer = AfterInvoke(method);
            if (parameterContainer != null)
            {
                outParam.cloneParameters(parameterContainer);
            }
        }

        protected virtual ParameterContainer AfterInvoke(string method)
        {
            return null;
        }

        protected virtual void BeforeInvoke(string method, ParameterContainer inParam, ParameterContainer inoutParam)
        {
        }

        protected virtual void DelayInvoke(double desiredDelay)
        {
            double num = desiredDelay - (DateTime.Now - lastInvokeExecutionTime).TotalMilliseconds;
            if (num >= 1.0)
            {
                Thread.Sleep((int)num);
            }
            lastInvokeExecutionTime = DateTime.Now;
        }

        protected ITextContent GetText(ParameterContainer inParam, string key)
        {
            ITextContent textContent = TextLocator.Empty.TextContent;
            if (inParam.getParameter(key) is List<ITextLocator> list && list.Count > 0)
            {
                textContent = list[0].TextContent;
            }
            return textContent;
        }

        protected string GetContent(ITextContent content)
        {
            string result = string.Empty;
            if (content is TextContent textContent)
            {
                result = textContent.GetTextForUI(logic.Lang)[0].TextItem;
            }
            else
            {
                Log.Error("ISTAServiceDialog.GetContent()", "Couldn't retrieve text from textconent.");
            }
            return result;
        }
    }
}
