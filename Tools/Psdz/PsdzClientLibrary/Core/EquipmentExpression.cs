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
	public class EquipmentExpression : SingleAssignmentExpression
	{
		public EquipmentExpression()
		{
		}

		public EquipmentExpression(long equipmentId)
		{
			this.value = equipmentId;
		}

		public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
		{
            PsdzDatabase database = ClientContext.GetDatabase(vec);
            if (database == null)
            {
                return false;
            }
			if (vec == null)
			{
				//Log.Error("EquipmentExpression.Evaluate()", "vec was null", Array.Empty<object>());
				return false;
			}
			if (vec.VehicleIdentLevel == IdentificationLevel.None)
			{
				return false;
			}
            PsdzDatabase.Equipment equipmentById = database.GetEquipmentById(this.value.ToString(CultureInfo.InvariantCulture));
			if (equipmentById == null)
			{
				return false;
			}
			object obj = EquipmentExpression.evaluationLockObject;
			bool result;
			lock (obj)
			{
				bool? flag2 = vec.hasFFM(equipmentById.Name);
				if (flag2 != null)
				{
					result = flag2.Value;
				}
				else
				{
					bool flag3 = database.EvaluateXepRulesById(this.value.ToString(CultureInfo.InvariantCulture), vec, ffmResolver, null);
#if false
					if (ffmResolver != null && flag3)
					{
                        List<SwiInfoObj> infoObjectsByDiagObjectControlId = database.GetInfoObjectsByDiagObjectControlId(this.value.ToString(CultureInfo.InvariantCulture), vec, ffmResolver, true, null);
						if (infoObjectsByDiagObjectControlId != null && infoObjectsByDiagObjectControlId.Count != 0)
						{
							bool? flag4 = ffmResolver.Resolve(this.value, infoObjectsByDiagObjectControlId.First<IXepInfoObject>());
							vec.AddOrUpdateFFM(new FFMResult(equipmentById.Id, equipmentById.Name, "FFMResolver", flag4, false));
							if (flag4 != null)
							{
								result = flag4.Value;
							}
							else
							{
								result = true;
							}
						}
						else
						{
							result = false;
						}
					}
					else
#endif
					if (flag3)
					{
						result = true;
					}
					else
					{
						result = false;
					}
				}
			}
			return result;
		}

		public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
		{
			if (ecus.Equipments.ToList<long>().BinarySearch(this.value) >= 0)
			{
				return EEvaluationResult.VALID;
			}
			if (ecus.UnknownEquipments.ToList<long>().BinarySearch(this.value) >= 0)
			{
				return EEvaluationResult.MISSING_VARIANT;
			}
			return EEvaluationResult.INVALID;
		}

		public override IList<long> GetUnknownVariantIds(EcuConfiguration ecus)
		{
			List<long> list = new List<long>();
			if (ecus.UnknownEquipments.ToList<long>().BinarySearch(this.value) >= 0)
			{
				list.Add(this.value);
			}
			return list;
		}

		public override void Serialize(MemoryStream ms)
		{
			ms.WriteByte(13);
			base.Serialize(ms);
		}

        public override string ToFormula(FormulaConfig formulaConfig)
        {
            PsdzDatabase.Equipment equipmentById = ClientContext.GetDatabase(this.vecInfo)?.GetEquipmentById(this.value.ToString(CultureInfo.InvariantCulture));
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            stringBuilder.Append(formulaConfig.RuleValidFunc);
            stringBuilder.Append("(\"");
            if (equipmentById != null)
            {
                string ruleId = this.value.ToString(CultureInfo.InvariantCulture);
                stringBuilder.Append(ruleId);
                if (formulaConfig.SubRuleIds != null && !formulaConfig.SubRuleIds.Contains(ruleId))
                {
                    formulaConfig.SubRuleIds.Add(ruleId);
                }
            }
            stringBuilder.Append("\")");
            stringBuilder.Append(FormulaSeparator(formulaConfig));

            return stringBuilder.ToString();
        }

		public override string ToString()
		{
			return "Equipment=" + this.value.ToString(CultureInfo.InvariantCulture);
		}

		private static object evaluationLockObject = new object();
	}
}
