using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Events;
using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient.Core;

namespace PsdzClient.Programming
{
    internal class VehicleProgrammingEventHandler : IPsdzEventListener
    {
        private readonly Dictionary<long, EcuProgrammingInfo> diagAddrToEcuMap;
        private readonly PsdzContext psdzContext;
        public VehicleProgrammingEventHandler(EcuProgrammingInfos ecuProgrammingInfos, PsdzContext psdzContext, bool ecusSeveralTimesPossible = false)
        {
            if (ecuProgrammingInfos == null)
            {
                throw new ArgumentNullException("ecuProgrammingInfos");
            }

            this.psdzContext = psdzContext;
            diagAddrToEcuMap = new Dictionary<long, EcuProgrammingInfo>();
            if (!ecusSeveralTimesPossible)
            {
                foreach (EcuProgrammingInfo ecuProgrammingInfo in ecuProgrammingInfos)
                {
                    if (!diagAddrToEcuMap.ContainsKey(ecuProgrammingInfo.Ecu.ID_SG_ADR))
                    {
                        diagAddrToEcuMap.Add(ecuProgrammingInfo.Ecu.ID_SG_ADR, ecuProgrammingInfo);
                    }
                }

                return;
            }

            FillEcusIfSeveralTimesPossible(ecuProgrammingInfos);
        }

        public void SetPsdzEvent(IPsdzEvent psdzEvent)
        {
            if (psdzEvent is IPsdzTransactionProgressEvent)
            {
                UpdateProgrammingProgress((IPsdzTransactionProgressEvent)psdzEvent);
            }
            else if (psdzEvent is IPsdzTransactionEvent)
            {
                UpdateProgrammingAction((IPsdzTransactionEvent)psdzEvent);
            }
        }

        private ProgrammingActionType? Map(PsdzTaCategories cat)
        {
            switch (cat)
            {
                case PsdzTaCategories.GatewayTableDeploy:
                case PsdzTaCategories.SwDeploy:
                case PsdzTaCategories.EcuActivate:
                case PsdzTaCategories.EcuPoll:
                case PsdzTaCategories.EcuMirrorDeploy:
                case PsdzTaCategories.SmacTransferStart:
                case PsdzTaCategories.SmacTransferStatus:
                    return ProgrammingActionType.Programming;
                case PsdzTaCategories.BlFlash:
                    return ProgrammingActionType.BootloaderProgramming;
                case PsdzTaCategories.CdDeploy:
                    return ProgrammingActionType.Coding;
                case PsdzTaCategories.FscBackup:
                    return ProgrammingActionType.FscBakup;
                case PsdzTaCategories.FscDeploy:
                    return ProgrammingActionType.FscActivate;
                case PsdzTaCategories.HddUpdate:
                    return ProgrammingActionType.HddUpdate;
                case PsdzTaCategories.HwDeinstall:
                    return ProgrammingActionType.Unmounting;
                case PsdzTaCategories.HwInstall:
                    return ProgrammingActionType.Mounting;
                case PsdzTaCategories.IbaDeploy:
                    return ProgrammingActionType.IbaDeploy;
                case PsdzTaCategories.IdBackup:
                    return ProgrammingActionType.IdSave;
                case PsdzTaCategories.IdRestore:
                    return ProgrammingActionType.IdRestore;
                case PsdzTaCategories.SFADeploy:
                    return ProgrammingActionType.SFAWrite;
                default:
                    Log.Warning("VehicleProgrammingEventHandler.SetPsdzException()", "TA category '{0}' not yet supported!", cat);
                    return null;
            }
        }

        private EcuProgrammingInfo GetCorrespondingEcu(IPsdzEvent psdzEvent)
        {
            int num = psdzEvent.EcuId?.DiagAddrAsInt ?? 255;
            if (!diagAddrToEcuMap.ContainsKey(num))
            {
                return null;
            }

            return diagAddrToEcuMap[num];
        }

        private void UpdateProgrammingAction(IPsdzTransactionEvent psdzEvent)
        {
            try
            {
                EcuProgrammingInfo correspondingEcu = GetCorrespondingEcu(psdzEvent);
                if (correspondingEcu == null)
                {
                    return;
                }

                PsdzTransactionInfo transactionInfo = psdzEvent.TransactionInfo;
                PsdzTaCategories transactionType = psdzEvent.TransactionType;
                Log.Debug("VehicleProgrammingEventHandler.UpdateProgrammingAction", "ECU: 0x{0:X2} - transaction info: {1} - action type: {2}", correspondingEcu.Ecu.ID_SG_ADR, transactionInfo, transactionType);
                if (transactionType == PsdzTaCategories.FscDeploy || transactionType == PsdzTaCategories.SFADeploy)
                {
                    IEnumerable<IPsdzTalLine> talLines = psdzContext.Tal.TalLines.Where((IPsdzTalLine talLine) => talLine.EcuIdentifier.Equals(psdzEvent.EcuId));
                    correspondingEcu.UpdateProgrammingActions(talLines, isTalExecuted: false);
                    return;
                }

                ProgrammingActionType? programmingActionType = Map(transactionType);
                ProgrammingActionState? programmingActionState = Map(transactionInfo);
                if (programmingActionType.HasValue && programmingActionState.HasValue)
                {
                    if (IsBootloaderFlashAndSwDeployCase(transactionType, psdzEvent.EcuId))
                    {
                        programmingActionType = ProgrammingActionType.Programming;
                    }

                    correspondingEcu.UpdateSingleProgrammingAction(programmingActionType.Value, programmingActionState.Value, executed: false);
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("VehicleProgrammingEventHandler.UpdateProgrammingAction", exception);
            }
        }

        private bool IsBootloaderFlashAndSwDeployCase(PsdzTaCategories transactionType, IPsdzEcuIdentifier ecuId)
        {
            bool flag = false;
            if (transactionType == PsdzTaCategories.BlFlash || transactionType == PsdzTaCategories.SwDeploy || transactionType == PsdzTaCategories.EcuMirrorDeploy)
            {
                IEnumerable<IPsdzTalLine> source = psdzContext.Tal.TalLines.Where((IPsdzTalLine talLine) => talLine.EcuIdentifier.Equals(ecuId));
                bool flag2 = source.Any((IPsdzTalLine a) => a.BlFlash != null);
                bool flag3 = source.Any((IPsdzTalLine a) => a.SwDeploy != null);
                flag = flag2 && flag3;
            }

            bool flag4 = false;
            if (transactionType == PsdzTaCategories.BlFlash || transactionType == PsdzTaCategories.EcuMirrorDeploy)
            {
                IEnumerable<IPsdzTalLine> source2 = psdzContext.Tal.TalLines.Where((IPsdzTalLine talLine) => talLine.EcuIdentifier.Equals(ecuId));
                bool flag5 = source2.Any((IPsdzTalLine a) => a.BlFlash != null);
                bool flag6 = source2.Any((IPsdzTalLine a) => a.EcuMirrorDeploy != null);
                flag4 = flag5 && flag6;
            }

            return flag || flag4;
        }

        private void UpdateProgrammingProgress(IPsdzTransactionProgressEvent psdzEvent)
        {
            EcuProgrammingInfo correspondingEcu = GetCorrespondingEcu(psdzEvent);
            if (correspondingEcu != null)
            {
                Log.Info("VehicleProgrammingEventHandler.UpdateProgrammingProgress()", "ECU: 0x{0:X2} - Progress: {1}", correspondingEcu.Ecu.ID_SG_ADR, psdzEvent.Progress);
                correspondingEcu.ProgressValue = (double)psdzEvent.Progress * 0.01;
            }
        }

        private ProgrammingActionState? Map(PsdzTransactionInfo transactionInfo)
        {
            ProgrammingActionState? result = null;
            switch (transactionInfo)
            {
                case PsdzTransactionInfo.Started:
                case PsdzTransactionInfo.Repeating:
                case PsdzTransactionInfo.ProgressInfo:
                    result = ProgrammingActionState.ActionInProcess;
                    break;
                case PsdzTransactionInfo.FinishedWithError:
                    result = ProgrammingActionState.ActionFailed;
                    break;
                case PsdzTransactionInfo.Finished:
                    result = ProgrammingActionState.ActionSuccessful;
                    break;
                default:
                    Log.Warning("VehicleProgrammingEventHandler.UpdateProgrammingAction()", "Transaction info '{0}' not yet supported!", transactionInfo);
                    break;
            }

            return result;
        }

        private void FillEcusIfSeveralTimesPossible(EcuProgrammingInfos ecuProgrammingInfos)
        {
            foreach (EcuProgrammingInfo ecuProgrammingInfo in ecuProgrammingInfos)
            {
                bool flag = false;
                foreach (IProgrammingAction programmingAction in ecuProgrammingInfo.GetProgrammingActions(null))
                {
                    if (programmingAction.IsSelected)
                    {
                        flag = true;
                        break;
                    }
                }

                if (flag)
                {
                    diagAddrToEcuMap.Add(ecuProgrammingInfo.Ecu.ID_SG_ADR, ecuProgrammingInfo);
                }
            }
        }
    }
}