using PsdzClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS0169
namespace PsdzClient.Core
{
    public class IStufeExpression : SingleAssignmentExpression
    {
        private string iStufe;
        [PreserveSource(Hint = "IDataProviderRuleEvaluation", Placeholder = true)]
        private readonly PlaceholderType dataProvider;
        private string IStufe
        {
            get
            {
                if (string.IsNullOrEmpty(iStufe))
                {
                    //[-] iStufe = dataProvider.GetIStufeById(value);
                    //[+] iStufe = ClientContext.GetDatabase(this.vecInfo)?.GetIStufeById(this.value.ToString(CultureInfo.InvariantCulture));
                    iStufe = ClientContext.GetDatabase(this.vecInfo)?.GetIStufeById(this.value.ToString(CultureInfo.InvariantCulture));
                    return iStufe;
                }

                return iStufe;
            }
        }

        [PreserveSource(Hint = "dataProvider removed", SignatureModified = true)]
        public IStufeExpression()
        {
            //[-] this.dataProvider = dataProvider;
        }

        [PreserveSource(Hint = "dataProvider removed", SignatureModified = true)]
        public IStufeExpression(long iStufeId)
        {
            value = iStufeId;
        //[-] this.dataProvider = dataProvider;
        }

        [PreserveSource(Hint = "dataProvider removed", SignatureModified = true)]
        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationServices, ValidationRuleInternalResults internalResult)
        {
            if (vec == null)
            {
                return false;
            }

            //[+] vecInfo = vec;
            vecInfo = vec;
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

        public override string ToString()
        {
            return "I-Stufe=" + IStufe + " [" + value + "]";
        }

        [PreserveSource(Added = true)]
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
    }
}