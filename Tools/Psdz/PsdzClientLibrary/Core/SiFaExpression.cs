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
            this.value = -1L;
        }

        public SiFaExpression(long accessSiFa)
        {
            this.value = accessSiFa;
        }

        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
        {
            if (vec == null)
            {
                return false;
            }
#if false
            Dealer instance = Dealer.Instance;
            if (instance != null && vec.BrandName != null)
            {
                bool flag = instance.HasProtectionVehicleService(vec.BrandName.Value);
                return flag;
            }
#endif
            return false;
        }

        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            if (client.AccessSiFa)
            {
                if (this.value == 0L)
                {
                    return EEvaluationResult.INVALID;
                }
                return EEvaluationResult.VALID;
            }
            else
            {
                if (this.value == 0L)
                {
                    return EEvaluationResult.VALID;
                }
                return EEvaluationResult.INVALID;
            }
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.WriteByte(15);
            base.Serialize(ms);
        }

        public override string ToFormula(FormulaConfig formulaConfig)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            stringBuilder.Append(formulaConfig.CheckLongFunc);
            stringBuilder.Append("(\"SafetyVehicle\", ");
            int compareValue = this.value != 0L ? 1 : 0;
            stringBuilder.Append(compareValue.ToString(CultureInfo.InvariantCulture));
            stringBuilder.Append(")");
            stringBuilder.Append(FormulaSeparator(formulaConfig));

            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            string str = "SiFa=";
            bool value = this.value != 0L;
            return str + value.ToString();
        }
    }
}
