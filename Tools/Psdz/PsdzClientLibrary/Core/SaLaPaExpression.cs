using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Utility;
using PsdzClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    public class SaLaPaExpression : SingleAssignmentExpression
    {
        public SaLaPaExpression()
        {
        }

        public SaLaPaExpression(long saLaPaId)
        {
            value = saLaPaId;
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.WriteByte(14);
            base.Serialize(ms);
        }

        public override string ToString()
        {
            return "SALAPA=" + value.ToString(CultureInfo.InvariantCulture);
        }

        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            if (ecus.SaLaPas.Count == 0 || ecus.SaLaPas.ToList().BinarySearch(value) >= 0)
            {
                return EEvaluationResult.VALID;
            }

            return EEvaluationResult.INVALID;
        }

        [PreserveSource(Hint = "dataProvider removed, vec added", OriginalHash = "")]
        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationServices, ValidationRuleInternalResults internalResult)
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

            ILogger logger = ruleEvaluationServices.Logger;
            if (saLaPaById.ProductType != vec.Prodart)
            {
                logger.Info("SaLaPaExpression.Evaluate()", "SALAPA: {0} result:false by non fitting product type [original rule: {1}]", saLaPaById.Name, value);
                return false;
            }

            bool flag;
            if (vec.FA == null || vec.VehicleIdentLevel == IdentificationLevel.BasicFeatures || vec.VehicleIdentLevel == IdentificationLevel.VINOnly || vec.VehicleIdentLevel == IdentificationLevel.VINBasedFeatures)
            {
                flag = true;
                logger.Info("SaLaPaExpression.Evaluate()", "SALAPA: {0} result:{1} by missing context [original rule: {2}]", saLaPaById.Name, flag, value);
            }
            else
            {
                flag = VehicleHelper.HasSA(vec, saLaPaById.Name);
                logger.Debug("SaLaPaExpression.Evaluate()", "SALAPA: {0} result:{1} [original rule: {2}]", saLaPaById.Name, flag, value);
            }

            return flag;
        }

        [PreserveSource(Hint = "Added")]
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
    }
}