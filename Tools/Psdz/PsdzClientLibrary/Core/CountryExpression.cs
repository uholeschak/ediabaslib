using System;
using System.Globalization;
using System.IO;
using System.Text;

#pragma warning disable CS0169
namespace PsdzClient.Core
{
    [Serializable]
    public class CountryExpression : SingleAssignmentExpression
    {
        private string countryCode;
        [PreserveSource(Hint = "IDataProviderRuleEvaluation", Placeholder = true)]
        private readonly PlaceholderType dataProvider;

        private string CountryCode
        {
            get
            {
                if (string.IsNullOrEmpty(countryCode))
                {
                    //[-] countryCode = dataProvider.GetCountryById(value);
                    //[+] countryCode = ClientContext.GetDatabase(vecInfo)?.GetCountryById(this.value.ToString(CultureInfo.InvariantCulture));
                    countryCode = ClientContext.GetDatabase(vecInfo)?.GetCountryById(this.value.ToString(CultureInfo.InvariantCulture));
                    return countryCode;
                }
                return countryCode;
            }
        }

        [PreserveSource(Hint = "dataProvider removed", OriginalHash = "F47C25B9514B07B236F1A56D11D577F3")]
        public CountryExpression()
        {
        }

        [PreserveSource(Hint = "dataProvider removed", OriginalHash = "0ACDF08839B829F7C53F3944F492ED20")]
        public CountryExpression(long countryId)
        {
            this.value = countryId;
        }

        [PreserveSource(Hint = "dataProvider removed", OriginalHash = "5C241DE3DACE1C95CDA99CE2ACB0A19A")]
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