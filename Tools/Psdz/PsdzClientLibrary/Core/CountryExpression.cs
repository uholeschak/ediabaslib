using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClientLibrary.Core;

namespace PsdzClient.Core
{
    [Serializable]
    public class CountryExpression : SingleAssignmentExpression
    {
        public CountryExpression()
        {
        }

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

        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
        {
            this.vecInfo = vec;
            bool flag = false;
            try
            {
                string outletCountry = ClientContext.GetCountry(this.vecInfo);
                flag = (outletCountry == this.CountryCode);
            }
            catch (Exception exception)
            {
                Log.WarningException("CountryExpression.Evaluate()", exception);
            }
            return flag;
        }

        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            if (client.CountryId != this.value && client.CountryId != 0L)
            {
                return EEvaluationResult.INVALID;
            }
            return EEvaluationResult.VALID;
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.WriteByte(9);
            base.Serialize(ms);
        }

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

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "Country=",
                this.CountryCode,
                " [",
                this.value.ToString(CultureInfo.InvariantCulture),
                "]"
            });
        }

        private string countryCode;
    }
}
