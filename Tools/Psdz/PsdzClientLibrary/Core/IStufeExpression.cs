using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    public class IStufeExpression : SingleAssignmentExpression
    {
        public IStufeExpression()
        {
        }

        public IStufeExpression(long iStufeId)
        {
            this.value = iStufeId;
        }

        private string IStufe
        {
            get
            {
                if (string.IsNullOrEmpty(this.iStufe))
                {
                    this.iStufe = ClientContext.GetDatabase(this.vecInfo)?.GetIStufeById(this.value.ToString(CultureInfo.InvariantCulture));
                    return this.iStufe;
                }
                return this.iStufe;
            }
        }

        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationUtils, ValidationRuleInternalResults internalResult)
        {
            if (vec == null)
            {
                return false;
            }

            this.vecInfo = vec;
            bool flag;
            if (!string.IsNullOrEmpty(vec.ILevel) && !(vec.ILevel == "0"))
            {
                flag = (vec.ILevel == this.IStufe);
            }
            else
            {
                flag = true;
            }
            return flag;
        }

        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            if (ecus.IStufe != this.value && ecus.IStufe != 0L)
            {
                return EEvaluationResult.INVALID;
            }
            return EEvaluationResult.VALID;
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.WriteByte(6);
            base.Serialize(ms);
        }

        public override string ToFormula(FormulaConfig formulaConfig)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            stringBuilder.Append(formulaConfig.CheckStringFunc);
            stringBuilder.Append("(\"IStufe\", ");
            stringBuilder.Append("\"");
            stringBuilder.Append(this.IStufe);
            stringBuilder.Append("\")");
            stringBuilder.Append(FormulaSeparator(formulaConfig));

            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            return string.Concat(new object[]
            {
                "I-Stufe=",
                this.IStufe,
                " [",
                this.value,
                "]"
            });
        }

        private string iStufe;
    }
}
