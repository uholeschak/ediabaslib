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
	public class EcuVariantExpression : SingleAssignmentExpression
	{
		public EcuVariantExpression()
		{
		}

		public EcuVariantExpression(long ecuVariantId)
		{
			this.value = ecuVariantId;
		}

		public string VariantName
		{
			get
			{
				if (string.IsNullOrEmpty(this.variantName))
				{
                    PdszDatabase.EcuVar ecuVariantById = ClientContext.GetDatabase(this.vecInfo)?.GetEcuVariantById(this.value.ToString(CultureInfo.InvariantCulture));
					if (ecuVariantById != null)
					{
						this.variantName = ecuVariantById.Name;
					}
					else
					{
						this.variantName = string.Empty;
					}
					return this.variantName;
				}
				return this.variantName;
			}
		}

		public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
        {
            this.vecInfo = vec;
            PdszDatabase database = ClientContext.GetDatabase(this.vecInfo);
            if (database == null)
            {
                return false;
            }
            PdszDatabase.EcuVar ecuVariantById = database.GetEcuVariantById(this.value.ToString(CultureInfo.InvariantCulture));
			if (ecuVariantById == null)
			{
				return false;
			}
			if (vec.VCI != null && (vec.VehicleIdentLevel == IdentificationLevel.BasicFeatures || vec.VehicleIdentLevel == IdentificationLevel.VINBasedFeatures || vec.VehicleIdentLevel == IdentificationLevel.VINOnly))
			{
				if (!database.EvaluateXepRulesById(ecuVariantById.Id, vec, ffmResolver, null))
				{
					return false;
				}
				IEcuVariantLocator ecuVariantLocator = new EcuVariantLocator(ecuVariantById, vec, ffmResolver);
				if (ecuVariantLocator == null)
				{
					return true;
				}
				if (ecuVariantLocator.Parents != null && ecuVariantLocator.Parents.Any<ISPELocator>())
				{
					foreach (ISPELocator ispelocator in ecuVariantLocator.Parents)
					{
						IEcuGroupLocator ecuGroupLocator = ispelocator as IEcuGroupLocator;
						if (ecuGroupLocator != null)
						{
							if (database.EvaluateXepRulesById(ecuGroupLocator.SignedId, vec, ffmResolver, null))
							{
								return true;
							}
						}
					}
					return false;
				}
				return true;
			}
			else
			{
				if (vec.VehicleIdentLevel == IdentificationLevel.VINBasedOnlineUpdated && vec.ECU != null && vec.ECU.Count == 0)
				{
					return true;
				}
				bool flag;
				if (!(flag = (vec.getECUbyECU_SGBD(this.VariantName) != null)) && "EWS3".Equals(this.VariantName, StringComparison.OrdinalIgnoreCase) && vec.BNType == BNType.BN2000_PGO)
				{
					//Log.Info("EcuVariantExpression.Evaluate()", "check for EWS3 => EWS3P", Array.Empty<object>());
					flag = true;
				}
				return flag;
			}
		}

		public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
		{
			if (ecus.EcuVariants.ToList<long>().BinarySearch(this.value) >= 0)
			{
				return EEvaluationResult.VALID;
			}
			if (ecus.UnknownEcuVariants.ToList<long>().BinarySearch(this.value) >= 0)
			{
				return EEvaluationResult.MISSING_VARIANT;
			}
			return EEvaluationResult.INVALID;
		}

		public override IList<long> GetUnknownVariantIds(EcuConfiguration ecus)
		{
			List<long> list = new List<long>();
			if (ecus.UnknownEcuVariants.ToList<long>().BinarySearch(this.value) >= 0)
			{
				list.Add(this.value);
			}
			return list;
		}

		public override void Serialize(MemoryStream ms)
		{
			ms.WriteByte(11);
			base.Serialize(ms);
		}

        public override string ToFormula(FormulaConfig formulaConfig)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(formulaConfig.CheckLongFunc);
            stringBuilder.Append("(\"EcuVariant\", ");
            stringBuilder.Append(value.ToString(CultureInfo.InvariantCulture));
            stringBuilder.Append(")");
            stringBuilder.Append(FormulaSeparator(formulaConfig));

            return stringBuilder.ToString();
        }

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"EcuVariant=",
				this.value.ToString(CultureInfo.InvariantCulture),
				" (",
				this.VariantName,
				")"
			});
		}

		private string variantName;
    }
}
