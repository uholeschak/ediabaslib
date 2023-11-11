using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Events;
using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClientLibrary.Core;

namespace PsdzClient.Programming
{
	public class VehicleProgrammingEventHandler : IPsdzEventListener
	{
		public VehicleProgrammingEventHandler(EcuProgrammingInfos ecuProgrammingInfos, PsdzContext psdzContext, bool ecusSeveralTimesPossible = false)
		{
			this.psdzContext = psdzContext;
			this.diagAddrToEcuMap = new Dictionary<long, EcuProgrammingInfo>();
            if (!ecusSeveralTimesPossible)
            {
                using (IEnumerator<IEcuProgrammingInfo> enumerator = ecuProgrammingInfos.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        IEcuProgrammingInfo ecuProgrammingInfo = enumerator.Current;
                        EcuProgrammingInfo ecuProgrammingInfo2 = (EcuProgrammingInfo)ecuProgrammingInfo;
                        if (!this.diagAddrToEcuMap.ContainsKey(ecuProgrammingInfo2.Ecu.ID_SG_ADR))
                        {
                            this.diagAddrToEcuMap.Add(ecuProgrammingInfo2.Ecu.ID_SG_ADR, ecuProgrammingInfo2);
                        }
                    }
                    return;
                }
            }
            this.FillEcusIfSeveralTimesPossible(ecuProgrammingInfos);
		}

		public void SetPsdzEvent(IPsdzEvent psdzEvent)
		{
			if (psdzEvent is IPsdzTransactionProgressEvent)
			{
				this.UpdateProgrammingProgress((IPsdzTransactionProgressEvent)psdzEvent);
				return;
			}
			if (psdzEvent is IPsdzTransactionEvent)
			{
				this.UpdateProgrammingAction((IPsdzTransactionEvent)psdzEvent);
			}
		}

		private ProgrammingActionType? Map(PsdzTaCategories cat)
		{
			switch (cat)
			{
				case PsdzTaCategories.BlFlash:
					return new ProgrammingActionType?(ProgrammingActionType.BootloaderProgramming);
				case PsdzTaCategories.CdDeploy:
					return new ProgrammingActionType?(ProgrammingActionType.Coding);
				case PsdzTaCategories.FscBackup:
					return new ProgrammingActionType?(ProgrammingActionType.FscBakup);
				case PsdzTaCategories.FscDeploy:
					return new ProgrammingActionType?(ProgrammingActionType.FscActivate);
				case PsdzTaCategories.GatewayTableDeploy:
				case PsdzTaCategories.SwDeploy:
					return new ProgrammingActionType?(ProgrammingActionType.Programming);
				case PsdzTaCategories.HddUpdate:
					return new ProgrammingActionType?(ProgrammingActionType.HddUpdate);
				case PsdzTaCategories.HwDeinstall:
					return new ProgrammingActionType?(ProgrammingActionType.Unmounting);
				case PsdzTaCategories.HwInstall:
					return new ProgrammingActionType?(ProgrammingActionType.Mounting);
				case PsdzTaCategories.IbaDeploy:
					return new ProgrammingActionType?(ProgrammingActionType.IbaDeploy);
				case PsdzTaCategories.IdBackup:
					return new ProgrammingActionType?(ProgrammingActionType.IdSave);
				case PsdzTaCategories.IdRestore:
					return new ProgrammingActionType?(ProgrammingActionType.IdRestore);
				case PsdzTaCategories.SFADeploy:
					return new ProgrammingActionType?(ProgrammingActionType.SFAWrite);
				case PsdzTaCategories.EcuActivate:
				case PsdzTaCategories.EcuPoll:
				case PsdzTaCategories.EcuMirrorDeploy:
					Log.Warning(Log.CurrentMethod(), string.Format("Unimplemented TA category type {0}.", cat), Array.Empty<object>());
					return null;
			}
			return null;
		}

		private EcuProgrammingInfo GetCorrespondingEcu(IPsdzEvent psdzEvent)
		{
			IPsdzEcuIdentifier ecuId = psdzEvent.EcuId;
			int num = (ecuId != null) ? ecuId.DiagAddrAsInt : 255;
			if (!this.diagAddrToEcuMap.ContainsKey((long)num))
			{
				return null;
			}
			return this.diagAddrToEcuMap[(long)num];
		}

		private void UpdateProgrammingAction(IPsdzTransactionEvent psdzEvent)
		{
			try
			{
				EcuProgrammingInfo correspondingEcu = this.GetCorrespondingEcu(psdzEvent);
				if (correspondingEcu != null)
				{
					PsdzTransactionInfo transactionInfo = psdzEvent.TransactionInfo;
					PsdzTaCategories transactionType = psdzEvent.TransactionType;
					if (transactionType != PsdzTaCategories.FscDeploy)
					{
						if (transactionType != PsdzTaCategories.SFADeploy)
						{
							ProgrammingActionType? programmingActionType = this.Map(transactionType);
							ProgrammingActionState? programmingActionState = this.Map(transactionInfo);
							if (programmingActionType != null && programmingActionState != null)
							{
								correspondingEcu.UpdateSingleProgrammingAction(programmingActionType.Value, programmingActionState.Value, false);
								return;
							}
                            return;
						}
					}
					IEnumerable<IPsdzTalLine> talLines = from talLine in this.psdzContext.Tal.TalLines
														 where talLine.EcuIdentifier.Equals(psdzEvent.EcuId)
														 select talLine;
					correspondingEcu.UpdateProgrammingActions(talLines, false, 0);
				}
                return;
			}
			catch (Exception)
			{
			}
		}

		private void UpdateProgrammingProgress(IPsdzTransactionProgressEvent psdzEvent)
		{
			EcuProgrammingInfo correspondingEcu = this.GetCorrespondingEcu(psdzEvent);
			if (correspondingEcu != null)
			{
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
					result = new ProgrammingActionState?(ProgrammingActionState.ActionInProcess);
					break;
				case PsdzTransactionInfo.Finished:
					result = new ProgrammingActionState?(ProgrammingActionState.ActionSuccessful);
					break;
				case PsdzTransactionInfo.FinishedWithError:
					result = new ProgrammingActionState?(ProgrammingActionState.ActionFailed);
					break;
				default:
					break;
			}
			return result;
		}

        private void FillEcusIfSeveralTimesPossible(EcuProgrammingInfos ecuProgrammingInfos)
        {
            using (IEnumerator<IEcuProgrammingInfo> enumerator = ecuProgrammingInfos.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    EcuProgrammingInfo ecuProgrammingInfo = (EcuProgrammingInfo)enumerator.Current;
                    bool flag = false;
                    using (IEnumerator<IProgrammingAction> enumerator2 = ecuProgrammingInfo.GetProgrammingActions(null).GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            if (enumerator2.Current.IsSelected)
                            {
                                flag = true;
                                break;
                            }
                        }

                        if (flag)
                        {
                            this.diagAddrToEcuMap.Add(ecuProgrammingInfo.Ecu.ID_SG_ADR, ecuProgrammingInfo);
                        }
                    }
                }
            }
        }

		private readonly Dictionary<long, EcuProgrammingInfo> diagAddrToEcuMap;

		private readonly PsdzContext psdzContext;
	}
}
