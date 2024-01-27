using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClientLibrary.Core;

namespace PsdzClient.Core
{
	public class EcuProgrammingVariantExpression : SingleAssignmentExpression
	{
		public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
		{
			bool result;
			try
			{
				if (vec == null)
				{
					Log.Warning("EcuProgrammingVariantExpression.Evaluate()", "vec was null", Array.Empty<object>());
					result = false;
				}
				else
                {
					this.programmingVariant = ClientContext.GetDatabase(vec)?.GetEcuProgrammingVariantById(this.value.ToString(CultureInfo.InvariantCulture), vec, ffmResolver);
                    if (this.programmingVariant == null)
					{
						result = false;
					}
					else
					{
						this.ecuVariant = ClientContext.GetDatabase(vec)?.GetEcuVariantById(this.programmingVariant.EcuVarId);
						if (this.ecuVariant == null)
						{
							result = false;
						}
						else if ((from c in vec.ECU
								  where c.ProgrammingVariantName != null && c.VARIANTE != null && c.ProgrammingVariantName.Equals(this.programmingVariant.Name, StringComparison.OrdinalIgnoreCase) && c.VARIANTE.Equals(this.ecuVariant.Name, StringComparison.OrdinalIgnoreCase)
								  select c).Any<ECU>())
						{
							result = true;
						}
						else
						{
							result = false;
						}
					}
				}
			}
			catch (Exception exception)
			{
				Log.WarningException("EcuProgrammingVariantExpression.Evaluate()", exception);
				result = false;
			}
			finally
			{
				Log.Info(Log.CurrentMethod(), this.ToString(), Array.Empty<object>());
			}
			return result;
		}

        public override string ToFormula(FormulaConfig formulaConfig)
        {
            StringBuilder stringBuilder = new StringBuilder();
            PsdzDatabase database = ClientContext.GetDatabase(this.vecInfo);

            PsdzDatabase.EcuPrgVar ecuPrgVar = database.GetEcuProgrammingVariantById(this.value.ToString(CultureInfo.InvariantCulture), null, null);
            PsdzDatabase.EcuVar ecuVar = null;
            if (ecuPrgVar != null)
            {
                ecuVar = database?.GetEcuVariantById(ecuPrgVar.EcuVarId);
            }

            if (ecuPrgVar != null && ecuVar != null)
            {
                stringBuilder.Append(formulaConfig.RuleValidFunc);
                stringBuilder.Append("(");
                if (!formulaConfig.IsRuleValidNumFunc)
                {
                    stringBuilder.Append("\"");
                }
                string ruleId = ecuPrgVar.EcuVarId;
                stringBuilder.Append(ruleId);
                if (formulaConfig.SubRuleIds != null && !formulaConfig.SubRuleIds.Contains(ruleId))
                {
                    formulaConfig.SubRuleIds.Add(ruleId);
                }
                if (!formulaConfig.IsRuleValidNumFunc)
                {
                    stringBuilder.Append("\"");
                }
                stringBuilder.Append(")");

                stringBuilder.Append(" && ");

                stringBuilder.Append(formulaConfig.CheckStringFunc);
                stringBuilder.Append("(\"EcuVariant\", ");
                stringBuilder.Append("\"");
                stringBuilder.Append(ecuVar.Name);
                stringBuilder.Append("\")");
            }
            else
            {
                stringBuilder.Append(formulaConfig.CheckLongFunc);
                stringBuilder.Append("(\"EcuProgrammingVariant\", ");
                stringBuilder.Append(this.value.ToString(CultureInfo.InvariantCulture));
                stringBuilder.Append(")");
            }

            return stringBuilder.ToString();
        }

		public override string ToString()
		{
			if (this.programmingVariant != null && this.ecuVariant != null)
			{
				return "EcuProgrammingVariant: ProgrammingVariantName= " + this.programmingVariant.Name + " And VARIANTE= " + this.ecuVariant.Name;
			}
			return string.Format("EcuProgrammingVariant: ID= {0}", this.value);
		}

		private PsdzDatabase.EcuPrgVar programmingVariant;

		private PsdzDatabase.EcuVar ecuVariant;
	}
}
