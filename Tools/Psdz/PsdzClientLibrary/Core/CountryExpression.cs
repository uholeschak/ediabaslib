using PsdzClient.Core.Container;
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

        [PreserveSource(Hint = "dataProvider removed", SignatureModified = true)]
        public CountryExpression()
        {
        //[-] this.dataProvider = dataProvider;
        }

        [PreserveSource(Hint = "dataProvider removed", SignatureModified = true)]
        public CountryExpression(long countryId)
        {
            value = countryId;
        //[-] this.dataProvider = dataProvider;
        }

        [PreserveSource(Hint = "dataProvider removed", SignatureModified = true)]
        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationServices, ValidationRuleInternalResults internalResult)
        {
            //[+] vecInfo = vec;
            vecInfo = vec;
            bool flag = false;
            try
            {
                //[-] string outletCountry = dealer.OutletCountry;
                //[+] string outletCountry = ClientContext.GetCountry(this.vecInfo);
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