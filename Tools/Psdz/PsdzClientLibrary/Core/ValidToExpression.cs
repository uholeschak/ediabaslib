using PsdzClientLibrary.Core;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace PsdzClient.Core
{
    [Serializable]
    public class ValidToExpression : SingleAssignmentExpression
    {
        public ValidToExpression()
        {
        }

        public ValidToExpression(DateTime date)
        {
            this.value = date.ToBinary();
        }

        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
        {
            bool flag = ((DateTime.Now <= DateTime.FromBinary(value)) ? true : false);
            if (CoreFramework.DebugLevel > 1)
            {
                Trace.TraceInformation("{0} ValidToExpression.Evaluate() - ValidTo: {1} (original value: {2}) result: {3}", DateTime.Now, DateTime.FromBinary(value), value, flag);
            }
            return flag;
        }
        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            if (client.ClientDate <= DateTime.FromBinary(this.value))
            {
                return EEvaluationResult.VALID;
            }
            return EEvaluationResult.INVALID;
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.WriteByte(8);
            base.Serialize(ms);
        }

        public override string ToFormula(FormulaConfig formulaConfig)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            stringBuilder.Append(formulaConfig.CheckLongFunc);
            stringBuilder.Append("(\"ValidTo\", ");
            stringBuilder.Append("\"");
            stringBuilder.Append(this.value);
            stringBuilder.Append("\")");
            stringBuilder.Append(FormulaSeparator(formulaConfig));

            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            return "ValidTo=" + DateTime.FromBinary(this.value).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        }
    }
}
