using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Tal;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;

namespace PsdzClient.Programming
{
	public class ProgrammingUtils
	{
        public static bool IsFlashableOverMost(IEcu ecu)
		{
			if (ecu.BUS != BusType.MOST)
			{
				if (ecu.BUS != BusType.VIRTUAL || ecu.ID_SG_ADR != 160L)
				{
					return false;
				}
			}
			return true;
		}

		public static bool IsUsedSpecificRoutingTable(IEcu ecu)
		{
			IList<long> list = new List<long>();
			list.Add(41L);
			return ecu != null && !list.Contains(ecu.ID_SG_ADR) && !ecu.IsVirtualOrVirtualBusCheck();
		}

		// ProgrammingTaskFlags.Mount | ProgrammingTaskFlags.Unmount | ProgrammingTaskFlags.Replace | ProgrammingTaskFlags.Flash | Programming.ProgrammingTaskFlags.Code | ProgrammingTaskFlags.DataRecovery | ProgrammingTaskFlags.Fsc
		public static IPsdzTalFilter CreateTalFilter(ProgrammingTaskFlags programmingTaskFlags, IPsdzObjectBuilder objectBuilder)
		{
			ISet<TaCategories> set = new HashSet<TaCategories>();
			if (programmingTaskFlags.HasFlag(ProgrammingTaskFlags.EnforceCoding))
			{
				set.Add(TaCategories.CdDeploy);
			}
			IPsdzTalFilter psdzTalFilter = objectBuilder.DefineFilterForAllEcus(set.ToArray<TaCategories>(), TalFilterOptions.Must, null);
			ISet<TaCategories> set2 = new HashSet<TaCategories>();
			if (programmingTaskFlags.HasFlag(ProgrammingTaskFlags.Mount))
			{
				set2.Add(TaCategories.HwInstall);
			}
			if (programmingTaskFlags.HasFlag(ProgrammingTaskFlags.Unmount))
			{
				set2.Add(TaCategories.HwDeinstall);
			}
			if (programmingTaskFlags.HasFlag(ProgrammingTaskFlags.Replace))
			{
				set2.Add(TaCategories.HwInstall);
				set2.Add(TaCategories.HwDeinstall);
			}
			if (programmingTaskFlags.HasFlag(ProgrammingTaskFlags.Flash))
			{
				set2.Add(TaCategories.BlFlash);
				set2.Add(TaCategories.SwDeploy);
				set2.Add(TaCategories.IbaDeploy);
			}
			if (programmingTaskFlags.HasFlag(ProgrammingTaskFlags.Code))
			{
				set2.Add(TaCategories.CdDeploy);
			}
			if (programmingTaskFlags.HasFlag(ProgrammingTaskFlags.DataRecovery))
			{
				set2.Add(TaCategories.IdBackup);
				set2.Add(TaCategories.IdRestore);
				set2.Add(TaCategories.FscBackup);
			}
			if (programmingTaskFlags.HasFlag(ProgrammingTaskFlags.Fsc))
			{
				set2.Add(TaCategories.FscDeploy);
				set2.Add(TaCategories.FscDeployPrehwd);
			}
			ISet<TaCategories> set3 = new HashSet<TaCategories>(ProgrammingUtils.AllowedTaCategories);
			set3.ExceptWith(set);
			set3.ExceptWith(set2);
			set3.Add(TaCategories.EcuActivate);
			set3.Add(TaCategories.EcuPoll);
			set3.Add(TaCategories.EcuMirrorDeploy);
			psdzTalFilter = objectBuilder.DefineFilterForAllEcus(set3.ToArray<TaCategories>(), TalFilterOptions.MustNot, psdzTalFilter);
			return psdzTalFilter;
		}

        public static ProgrammingTaskFlags RetrieveProgrammingTaskFlagsFromTasks(IEnumerable<IProgrammingTask> programmingTasks)
		{
			ProgrammingTaskFlags programmingTaskFlags = (ProgrammingTaskFlags)0;
			if (programmingTasks != null)
			{
				foreach (IProgrammingTask programmingTask in programmingTasks)
				{
					programmingTaskFlags |= programmingTask.Flags;
				}
			}
			return programmingTaskFlags;
		}

        public static string NormalizeXmlText(string xmlText)
		{
			if (string.IsNullOrEmpty(xmlText))
			{
				return xmlText;
			}
			return Regex.Replace(xmlText.Trim(), ">\\s+<", "><");
		}

        public static IFa ModifyFa(IFa faInput, List<string> faModList, bool addEntry)
        {
            if (faInput == null)
            {
                return null;
            }

            IFa faResult = faInput.Clone();
            foreach (string modEntry in faModList)
            {
                IList<string> faList = null;
                string item = modEntry.Trim();
                char prefix = item[0];
                string itemName = item.Substring(1);
                switch (prefix)
                {
                    case '-':
                        faList = faResult.EWords;
                        break;

                    case '+':
                        faList = faResult.HOWords;
                        break;

                    case '$':
                        faList = faResult.Salapas;
                        break;
                }

                if (faList == null)
                {
                    return null;
                }

                if (addEntry)
                {
                    if (!faList.Contains(itemName))
                    {
                        faList.Add(itemName);
                    }
                }
                else
                {
                    if (faList.Contains(itemName))
                    {
                        faList.Remove(itemName);
                    }
                }
            }

            return faResult;
        }

        static ProgrammingUtils()
        {
            ProgrammingUtils.AllowedTaCategories = new TaCategories[]
            {
                TaCategories.BlFlash,
                TaCategories.CdDeploy,
                TaCategories.FscBackup,
                TaCategories.FscDeploy,
                TaCategories.FscDeployPrehwd,
                TaCategories.GatewayTableDeploy,
                TaCategories.HddUpdate,
                TaCategories.HwDeinstall,
                TaCategories.HwInstall,
                TaCategories.IbaDeploy,
                TaCategories.IdBackup,
                TaCategories.IdRestore,
                TaCategories.SwDeploy,
                TaCategories.SFADeploy,
            };
		}

		public static readonly TaCategories[] AllowedTaCategories;
	}
}
