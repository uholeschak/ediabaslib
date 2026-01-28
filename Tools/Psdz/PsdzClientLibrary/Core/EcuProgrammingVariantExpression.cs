using PsdzClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    public class EcuProgrammingVariantExpression : SingleAssignmentExpression
    {
        [PreserveSource(Hint = "Database replaced", SuppressWarning = true)]
        private PsdzDatabase.EcuPrgVar programmingVariant;

        [PreserveSource(Hint = "Database replaced", SuppressWarning = true)]
        private PsdzDatabase.EcuVar ecuVariant;

        [PreserveSource(Hint = "Database modified", SignatureModified = true)]
        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationServices, ValidationRuleInternalResults internalResult)
        {
            ILogger logger = ruleEvaluationServices.Logger;
            try
            {
                if (vec == null)
                {
                    logger.Warning(logger.CurrentMethod(), "vec was null");
                    return false;
                }

                //[-] programmingVariant = dataProvider.GetEcuProgrammingVariantById(value, vec, ffmResolver);
                //[+] programmingVariant = ClientContext.GetDatabase(vec)?.GetEcuProgrammingVariantById(value.ToString(CultureInfo.InvariantCulture), vec, ffmResolver);
                programmingVariant = ClientContext.GetDatabase(vec)?.GetEcuProgrammingVariantById(value.ToString(CultureInfo.InvariantCulture), vec, ffmResolver);
                if (programmingVariant == null)
                {
                    logger.Warning(logger.CurrentMethod(), "no valid programming variant information found for id: {0}", value);
                    return false;
                }

                //[-] ecuVariant = dataProvider.GetEcuVariantById(programmingVariant.EcuVariantId);
                //[+] ecuVariant = ClientContext.GetDatabase(vec)?.GetEcuVariantById(programmingVariant.EcuVarId);
                ecuVariant = ClientContext.GetDatabase(vec)?.GetEcuVariantById(programmingVariant.EcuVarId);
                if (ecuVariant == null)
                {
                    logger.Warning(logger.CurrentMethod(), "no valid EcuVariant information found for id: {0}", value);
                    return false;
                }

                IEnumerable<IIdentEcu> source = vec.ECU.Where((IIdentEcu c) => c.ProgrammingVariantName != null && c.VARIANTE != null && c.ProgrammingVariantName.Equals(programmingVariant.Name, StringComparison.OrdinalIgnoreCase) && c.VARIANTE.Equals(ecuVariant.Name, StringComparison.OrdinalIgnoreCase));
                if (source.Any())
                {
                    return true;
                }
                return false;
            }
            catch (Exception exception)
            {
                logger.WarningException(logger.CurrentMethod(), exception);
                return false;
            }
            finally
            {
                logger.Info(logger.CurrentMethod(), ToString());
            }
        }

        public override string ToString()
        {
            if (programmingVariant != null && ecuVariant != null)
            {
                return "EcuProgrammingVariant: ProgrammingVariantName= " + programmingVariant.Name + " And VARIANTE= " + ecuVariant.Name;
            }

            return $"EcuProgrammingVariant: ID= {value}";
        }

        [PreserveSource(Added = true)]
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
    }
}