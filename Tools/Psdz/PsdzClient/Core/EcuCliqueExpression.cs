using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
	public class EcuCliqueExpression : SingleAssignmentExpression
	{
		public EcuCliqueExpression()
		{
		}

		public EcuCliqueExpression(long ecuCliqueId)
		{
			this.value = ecuCliqueId;
		}

		public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
		{
			bool flag = false;
			IDatabaseProvider instance = DatabaseProviderFactory.Instance;
			XEP_ECUCLIQUES ecuClique = instance.GetEcuClique(this.value);
			if (vec == null)
			{
				return false;
			}
			if (ecuClique == null)
			{
				return true;
			}
			if (!instance.EvaluateXepRulesById(ecuClique.ID, vec, ffmResolver, null))
			{
				return false;
			}
			ICollection<XEP_ECUVARIANTS> ecuVariantsByEcuCliquesId = instance.GetEcuVariantsByEcuCliquesId(ecuClique.ID);
			if (ecuVariantsByEcuCliquesId == null || ecuVariantsByEcuCliquesId.Count == 0)
			{
				return false;
			}
			if (vec.VCI != null && vec.VCI.VCIType == VCIDeviceType.INFOSESSION && (vec.VehicleIdentLevel == IdentificationLevel.BasicFeatures || vec.VehicleIdentLevel == IdentificationLevel.VINBasedFeatures || (vec.VehicleIdentLevel == IdentificationLevel.VINBasedOnlineUpdated && (vec.ECU == null || (vec.ECU != null && vec.ECU.Count == 0))) || vec.VehicleIdentLevel == IdentificationLevel.VINOnly))
			{
				foreach (XEP_ECUVARIANTS xep_ECUVARIANTS in ecuVariantsByEcuCliquesId)
				{
					flag = instance.EvaluateXepRulesById(xep_ECUVARIANTS.Id, vec, ffmResolver, null);
					if (flag && xep_ECUVARIANTS.EcuGroupId != null)
					{
						decimal? ecuGroupId = xep_ECUVARIANTS.EcuGroupId;
						decimal d = 0m;
						if (ecuGroupId.GetValueOrDefault() > d & ecuGroupId != null)
						{
							flag = instance.EvaluateXepRulesById(xep_ECUVARIANTS.EcuGroupId.Value, vec, ffmResolver, null);
							if (flag)
							{
								break;
							}
						}
					}
				}
				return flag;
			}
			foreach (XEP_ECUVARIANTS xep_ECUVARIANTS2 in ecuVariantsByEcuCliquesId)
			{
				if (!(flag = (vec.getECUbyECU_SGBD(xep_ECUVARIANTS2.Name) != null)) && "EWS3".Equals(xep_ECUVARIANTS2.Name, StringComparison.OrdinalIgnoreCase) && vec.BNType == BNType.BN2000_PGO)
				{
					//Log.Info("EcuCliqueExpression.Evaluate()", "check for EWS3 => EWS3P", Array.Empty<object>());
					flag = true;
				}
				if (flag)
				{
					break;
				}
			}
			return flag;
		}

		public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
		{
			if (ecus.EcuCliques.ToList<long>().BinarySearch(this.value) >= 0)
			{
				return EEvaluationResult.VALID;
			}
			if (ecus.UnknownEcuCliques.ToList<long>().BinarySearch(this.value) >= 0)
			{
				return EEvaluationResult.MISSING_VARIANT;
			}
			return EEvaluationResult.INVALID;
		}

		public override IList<long> GetUnknownVariantIds(EcuConfiguration ecus)
		{
			List<long> list = new List<long>();
			if (ecus.UnknownEcuCliques.ToList<long>().BinarySearch(this.value) >= 0)
			{
				list.Add(this.value);
			}
			return list;
		}

		public override void Serialize(MemoryStream ms)
		{
			ms.WriteByte(12);
			base.Serialize(ms);
		}

		public override string ToString()
		{
			return "EcuClique=" + this.value.ToString(CultureInfo.InvariantCulture);
		}
	}
}
