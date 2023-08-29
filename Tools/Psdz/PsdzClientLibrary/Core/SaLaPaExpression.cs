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
	public class SaLaPaExpression : SingleAssignmentExpression
	{
		public SaLaPaExpression()
		{
		}

		public SaLaPaExpression(long saLaPaId)
		{
			this.value = saLaPaId;
		}

		public override void Serialize(MemoryStream ms)
		{
			ms.WriteByte(14);
			base.Serialize(ms);
		}

        public override string ToFormula(FormulaConfig formulaConfig)
        {
            PsdzDatabase.SaLaPa saLaPaById = ClientContext.GetDatabase(this.vecInfo)?.GetSaLaPaById(this.value.ToString(CultureInfo.InvariantCulture));
            
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            stringBuilder.Append(formulaConfig.CheckStringFunc);
            stringBuilder.Append("(\"SALAPA\", ");
            stringBuilder.Append("\"");
            if (saLaPaById != null)
            {
                stringBuilder.Append(saLaPaById.Name);
            }
            stringBuilder.Append("\")");
            stringBuilder.Append(FormulaSeparator(formulaConfig));

            return stringBuilder.ToString();
        }

		public override string ToString()
		{
			return "SALAPA=" + this.value.ToString(CultureInfo.InvariantCulture);
		}

		public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
		{
			if (ecus.SaLaPas.Count != 0 && ecus.SaLaPas.ToList<long>().BinarySearch(this.value) < 0)
			{
				return EEvaluationResult.INVALID;
			}
			return EEvaluationResult.VALID;
		}

		public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
		{
			if (vec == null)
			{
				return false;
			}
			PsdzDatabase.SaLaPa saLaPaById = ClientContext.GetDatabase(vec)?.GetSaLaPaById(this.value.ToString(CultureInfo.InvariantCulture));
			if (saLaPaById == null)
			{
				return false;
			}
			if (saLaPaById.ProductType != vec.Prodart)
			{
				return false;
			}
			bool flag;
			if (vec.FA != null && vec.VehicleIdentLevel != IdentificationLevel.BasicFeatures && vec.VehicleIdentLevel != IdentificationLevel.VINOnly)
			{
				if (vec.VehicleIdentLevel != IdentificationLevel.VINBasedFeatures)
				{
					flag = vec.hasSA(saLaPaById.Name);
					return flag;
				}
			}
			flag = true;
			return flag;
		}
	}
}
