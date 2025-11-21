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
    [Serializable]
    public class SiFaExpression : SingleAssignmentExpression
    {
        public SiFaExpression()
        {
            value = -1L;
        }

        public SiFaExpression(long accessSiFa)
        {
            value = accessSiFa;
        }

        [PreserveSource(Hint = "Added")]
        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationServices, ValidationRuleInternalResults internalResult)
        {
            if (vec == null)
            {
                return false;
            }

            bool flag = ClientContext.GetProtectionVehicleService(this.vecInfo);
            ruleEvaluationServices.Logger.Debug("SiFaExpression.Evaluate()", "SiFa: {0}", flag);
            return flag;
        }

        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            if (client.AccessSiFa)
            {
                if (value == 0)
                {
                    return EEvaluationResult.INVALID;
                }

                return EEvaluationResult.VALID;
            }

            if (value == 0)
            {
                return EEvaluationResult.VALID;
            }

            return EEvaluationResult.INVALID;
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.WriteByte(15);
            base.Serialize(ms);
        }

        public override string ToString()
        {
            return "SiFa=" + ((value != 0L) ? true : false);
        }

        [PreserveSource(Hint = "Added")]
        public override string ToFormula(FormulaConfig formulaConfig)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            stringBuilder.Append("!");
            stringBuilder.Append(formulaConfig.CheckLongFunc);
            stringBuilder.Append("(\"ProtectionVehicleService\", 0)");
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            return stringBuilder.ToString();
        }
    }
}