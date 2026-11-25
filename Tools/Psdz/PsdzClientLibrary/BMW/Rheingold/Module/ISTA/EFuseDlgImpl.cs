using BMW.ISPI.IstaOperation.Contract.ServiceProgram;
using BMW.Rheingold.CoreFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using BMW.Rheingold.CoreFramework.TableDataHelper;

namespace BMW.Rheingold.Module.ISTA
{
    internal class EFuseDlgImpl : ServiceDlgImplBase<EFuseDlgModel>
    {
        public EFuseDlgImpl(ParameterContainer inParam) : base(inParam)
        {
        }

        public override void Invoke(string method, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            bool.TryParse(inParam.getKeyValuePair("Protocol")?.Value.ToString(), out var _);
            string infoTitle = "EFuseDlgImpl";
            Dictionary<string, TableData> tableData = TableDataHelper.GetTableData(inParam, logic.Lang, Vehicle, DBProvider, FFMDynamicResolver);
            if (tableData != null && tableData.Any())
            {
                Model.ResultData = tableData.Values.FirstOrDefault();
                if (FastaProtocoler != null)
                {
                    FastaProtocoler.AddMultiLanguageEFuseInfoTable(infoTitle, tableData, DateTime.Now);
                }

                SetNextButtonEnabled(value: true);
                WaitOnUserInteraction();
            }
        }

        private void WaitOnUserInteraction()
        {
            try
            {
                ServiceProgramNavigationAction serviceProgramNavigationAction;
                do
                {
                    serviceProgramNavigationAction = ServiceProgramController.AwaitUserAction(-1) as ServiceProgramNavigationAction;
                }
                while (serviceProgramNavigationAction == null);
                Log.Info("EFuseDlgImpl.WaitOnUserInteraction()", "Navigation action: {0}", serviceProgramNavigationAction.NavigationAction);
            }
            catch (Exception exception)
            {
                Log.WarningException("EFuseDlgImpl.WaitOnUserInteraction()", exception);
            }
        }
    }
}