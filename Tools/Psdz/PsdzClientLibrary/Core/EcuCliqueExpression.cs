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

		public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationServices, ValidationRuleInternalResults internalResult)
		{
			PsdzDatabase database = ClientContext.GetDatabase(vec);
            if (database == null)
            {
                return false;
            }

            bool flag = false;
            PsdzDatabase.EcuClique ecuClique = database.GetEcuClique(this.value.ToString(CultureInfo.InvariantCulture));
			if (vec == null)
			{
				return false;
			}
			if (ecuClique == null)
			{
				return true;
			}
            RuleEvaluationUtill ruleEvaluationUtill = new RuleEvaluationUtill(ruleEvaluationServices, database);
            if (!ruleEvaluationUtill.EvaluateSingleRuleExpression(vec, ecuClique.Id, ffmResolver))
            {
                return false;
            }
            List<PsdzDatabase.EcuVar> ecuVariantsByEcuCliquesId = ClientContext.GetDatabase(vec)?.GetEcuVariantsByEcuCliquesId(ecuClique.Id);
			if (ecuVariantsByEcuCliquesId == null || ecuVariantsByEcuCliquesId.Count == 0)
			{
                ruleEvaluationServices.Logger.Info("EcuCliqueExpression.Evaluate()", "Unable to find ECU variants for ECU clique id: {0}", ecuClique.Id);
				return false;
			}
            if (vec.VCI != null && vec.VCI.VCIType == VCIDeviceType.INFOSESSION && (vec.VehicleIdentLevel == IdentificationLevel.BasicFeatures || vec.VehicleIdentLevel == IdentificationLevel.VINBasedFeatures || (vec.VehicleIdentLevel == IdentificationLevel.VINBasedOnlineUpdated && (vec.ECU == null || (vec.ECU != null && !vec.ECU.Any()))) || vec.VehicleIdentLevel == IdentificationLevel.VINOnly))
			{
                foreach (PsdzDatabase.EcuVar ecuVar in ecuVariantsByEcuCliquesId)
				{
                    flag = ruleEvaluationUtill.EvaluateSingleRuleExpression(vec, ecuVar.Id, ffmResolver);
                    if (!flag)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(ecuVar.EcuGroupId))
                    {
                        flag = ruleEvaluationUtill.EvaluateSingleRuleExpression(vec, ecuVar.EcuGroupId, ffmResolver);
                        if (flag)
                        {
                            break;
                        }
                    }
                }
                ruleEvaluationServices.Logger.Debug("EcuCliqueExpression.Evaluate()", "ECU Clique: {0} Result: {1} [original rule: {2}]", ecuClique.CliqueName, flag, value);
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
            ruleEvaluationServices.Logger.Debug("EcuCliqueExpression.Evaluate()", "ECU Clique: {0} Result: {1} [original rule: {2}]", ecuClique.CliqueName, flag, value);
			return flag;
		}

		public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
		{
            if (ecus.EcuCliques.ToList().BinarySearch(value) >= 0)
            {
                return EEvaluationResult.VALID;
            }
            if (ecus.UnknownEcuCliques.ToList().BinarySearch(value) >= 0)
            {
                return EEvaluationResult.MISSING_VARIANT;
            }
            return EEvaluationResult.INVALID;
		}

        public override IList<long> GetUnknownVariantIds(EcuConfiguration ecus)
		{
            List<long> list = new List<long>();
            if (ecus.UnknownEcuCliques.ToList().BinarySearch(value) >= 0)
            {
                list.Add(value);
            }
            return list;
		}

        public override void Serialize(MemoryStream ms)
		{
            ms.WriteByte(12);
            base.Serialize(ms);
		}

        // [UH] added
        public override string ToFormula(FormulaConfig formulaConfig)
        {
            PsdzDatabase.EcuClique ecuClique = ClientContext.GetDatabase(this.vecInfo)?.GetEcuClique(this.value.ToString(CultureInfo.InvariantCulture));

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
            return "EcuClique=" + value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
