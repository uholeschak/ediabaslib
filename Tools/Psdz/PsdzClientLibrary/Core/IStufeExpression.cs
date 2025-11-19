using PsdzClientLibrary;
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
        private string iStufe;
        [PreserveSource(Hint = "IDataProviderRuleEvaluation", Placeholder = true)]
        private readonly PlaceholderType dataProvider;

        [PreserveSource(Hint = "Modified")]
        public IStufeExpression()
        {
        }

        [PreserveSource(Hint = "Modified")]
        public IStufeExpression(long iStufeId)
        {
            this.value = iStufeId;
        }

        [PreserveSource(Hint = "Modified")]
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

        [PreserveSource(Hint = "Modified")]
        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationServices, ValidationRuleInternalResults internalResult)
        {
            if (vec == null)
            {
                return false;
            }

            this.vecInfo = vec; // [UH] [IGNORE] added
            bool flag;
            if (string.IsNullOrEmpty(vec.ILevel) || vec.ILevel == "0")
            {
                flag = true;
                ruleEvaluationServices.Logger.Info("IStufeExpression.Evaluate()", "IStufe: {0} result: {1} by missing context [original rule: {2}]", IStufe, flag, value);
            }
            else
            {
                flag = vec.ILevel == IStufe;
                ruleEvaluationServices.Logger.Debug("IStufeExpression.Evaluate()", "IStufe: {0} result: {1} [original rule: {2}]", IStufe, flag, value);
            }
            return flag;
        }

        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            if (ecus.IStufe == value || ecus.IStufe == 0)
            {
                return EEvaluationResult.VALID;
            }
            return EEvaluationResult.INVALID;
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.WriteByte(6);
            base.Serialize(ms);
        }

        [PreserveSource(Hint = "Added")]
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
            return "I-Stufe=" + IStufe + " [" + value + "]";
        }
    }
}
