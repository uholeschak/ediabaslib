using PsdzClient;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace PsdzClient.Core
{
    [Serializable]
    public class ValidFromExpression : SingleAssignmentExpression
    {
        public ValidFromExpression()
        {
        }

        public ValidFromExpression(DateTime date)
        {
            value = date.ToBinary();
        }

        [PreserveSource(Hint = "dataProvider removed, vec added", SignatureModified = true)]
        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationServices, ValidationRuleInternalResults internalResult)
        {
            bool flag = true;
            flag = ((DateTime.Now >= DateTime.FromBinary(value)) ? true : false);
            ruleEvaluationServices.Logger.Debug(ruleEvaluationServices.Logger.CurrentMethod(), "{0} ValidFromExpression.Evaluate() - ValidFrom: {1} (original value: {2}) result: {3}", DateTime.Now, DateTime.FromBinary(value), value, flag);
            return flag;
        }

        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            if (client.ClientDate >= DateTime.FromBinary(value))
            {
                return EEvaluationResult.VALID;
            }

            return EEvaluationResult.INVALID;
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.WriteByte(7);
            base.Serialize(ms);
        }

        public override string ToString()
        {
            return "ValidFrom=" + DateTime.FromBinary(value).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        }

        [PreserveSource(Added = true)]
        public override string ToFormula(FormulaConfig formulaConfig)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            stringBuilder.Append(formulaConfig.CheckLongFunc);
            stringBuilder.Append("(\"ValidFrom\", ");
            stringBuilder.Append("\"");
            stringBuilder.Append(this.value);
            stringBuilder.Append("\")");
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            return stringBuilder.ToString();
        }
    }
}