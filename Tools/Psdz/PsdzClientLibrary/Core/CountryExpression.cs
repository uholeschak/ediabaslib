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
    public class CountryExpression : SingleAssignmentExpression
    {
        private string countryCode;

        public CountryExpression()
        {
        }

        [PreserveSource(Hint = "dataProvider removed", OriginalHash = "")]
        public CountryExpression(long countryId)
        {
            this.value = countryId;
        }

        private string CountryCode
        {
            get
            {
                if (string.IsNullOrEmpty(this.countryCode))
                {
                    this.countryCode = ClientContext.GetDatabase(this.vecInfo)?.GetCountryById(this.value.ToString(CultureInfo.InvariantCulture));
                    return this.countryCode;
                }

                return this.countryCode;
            }
        }

        [PreserveSource(Hint = "dataProvider removed", OriginalHash = "")]
        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationServices, ValidationRuleInternalResults internalResult)
        {
            this.vecInfo = vec;
            bool flag = false;
            try
            {
                string outletCountry = ClientContext.GetCountry(this.vecInfo);
                flag = outletCountry == CountryCode;
                ruleEvaluationServices.Logger.Debug("CountryExpression.Evaluate()", "Country: {0} result: {1} (session context: {2}) [original rule: {3}])", CountryCode, flag, outletCountry, value);
            }
            catch (Exception exception)
            {
                ruleEvaluationServices.Logger.WarningException("CountryExpression.Evaluate()", exception);
            }

            return flag;
        }

        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            if (client.CountryId == value || client.CountryId == 0)
            {
                return EEvaluationResult.VALID;
            }

            return EEvaluationResult.INVALID;
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.WriteByte(9);
            base.Serialize(ms);
        }

        public override string ToString()
        {
            return "Country=" + CountryCode + " [" + value.ToString(CultureInfo.InvariantCulture) + "]";
        }

        [PreserveSource(Hint = "Added")]
        public override string ToFormula(FormulaConfig formulaConfig)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(formulaConfig.CheckStringFunc);
            stringBuilder.Append("(\"Country\", ");
            stringBuilder.Append("\"");
            stringBuilder.Append(this.CountryCode);
            stringBuilder.Append("\")");
            return stringBuilder.ToString();
        }
    }
}