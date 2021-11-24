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
    public class ValidFromExpression : SingleAssignmentExpression
    {
        public ValidFromExpression()
        {
        }

        public ValidFromExpression(DateTime date)
        {
            this.value = date.ToBinary();
        }

        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
        {
            bool flag = DateTime.Now >= DateTime.FromBinary(this.value);
            return flag;
        }

        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            if (client.ClientDate >= DateTime.FromBinary(this.value))
            {
                return EEvaluationResult.VALID;
            }
            return EEvaluationResult.INVALID;
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.WriteByte(7);
            base.Serialize(ms);
        }

        public override string ToString()
        {
            return "ValidFrom=" + DateTime.FromBinary(this.value).ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        }
    }
}
