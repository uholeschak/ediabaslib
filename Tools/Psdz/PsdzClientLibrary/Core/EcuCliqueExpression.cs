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
			PsdzDatabase database = ClientContext.GetDatabase(vec);
            if (database == null)
            {
                return false;
            }

            PsdzDatabase.EcuClique ecuClique = database.GetEcuCliqueById(this.value.ToString(CultureInfo.InvariantCulture));
			if (vec == null)
			{
				return false;
			}
			if (ecuClique == null)
			{
				return true;
			}
			if (!database.EvaluateXepRulesById(ecuClique.Id, vec, ffmResolver, null))
			{
				return false;
			}
            List<PsdzDatabase.EcuVar> ecuVariantsByEcuCliquesId = ClientContext.GetDatabase(vec)?.GetEcuVariantsByEcuCliquesId(ecuClique.Id);
			if (ecuVariantsByEcuCliquesId == null || ecuVariantsByEcuCliquesId.Count == 0)
			{
				return false;
			}
			if (vec.VCI != null && vec.VCI.VCIType == VCIDeviceType.INFOSESSION && (vec.VehicleIdentLevel == IdentificationLevel.BasicFeatures || vec.VehicleIdentLevel == IdentificationLevel.VINBasedFeatures || (vec.VehicleIdentLevel == IdentificationLevel.VINBasedOnlineUpdated && (vec.ECU == null || (vec.ECU != null && vec.ECU.Count == 0))) || vec.VehicleIdentLevel == IdentificationLevel.VINOnly))
			{
				foreach (PsdzDatabase.EcuVar ecuVar in ecuVariantsByEcuCliquesId)
				{
					flag = database.EvaluateXepRulesById(ecuVar.Id, vec, ffmResolver, null);
					if (flag && !string.IsNullOrEmpty(ecuVar.EcuGroupId))
					{
						flag = database.EvaluateXepRulesById(ecuVar.EcuGroupId, vec, ffmResolver, null);
						if (flag)
						{
							break;
						}
					}
				}
				return flag;
			}
			foreach (PsdzDatabase.EcuVar ecuVar in ecuVariantsByEcuCliquesId)
            {
                flag = vec.getECUbyECU_SGBD(ecuVar.Name) != null;
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

        public override string ToFormula(FormulaConfig formulaConfig)
        {
            PsdzDatabase.EcuClique ecuClique = ClientContext.GetDatabase(this.vecInfo)?.GetEcuCliqueById(this.value.ToString(CultureInfo.InvariantCulture));

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            stringBuilder.Append(formulaConfig.CheckStringFunc);
            stringBuilder.Append("(\"EcuClique\", ");
            stringBuilder.Append("\"");
            if (ecuClique != null)
            {
                stringBuilder.Append(ecuClique.CliqueName);
            }
            stringBuilder.Append("\")");
            stringBuilder.Append(FormulaSeparator(formulaConfig));

            return stringBuilder.ToString();
        }

		public override string ToString()
		{
			return "EcuClique=" + this.value.ToString(CultureInfo.InvariantCulture);
		}
	}
}
