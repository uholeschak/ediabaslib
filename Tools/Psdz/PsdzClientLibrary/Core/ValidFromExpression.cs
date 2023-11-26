using PsdzClientLibrary.Core;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

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
            bool flag = true;
            flag = ((DateTime.Now >= DateTime.FromBinary(value)) ? true : false);
            if (CoreFramework.DebugLevel > 1)
            {
                Trace.TraceInformation("{0} ValidFromExpression.Evaluate() - ValidFrom: {1} (original value: {2}) result: {3}", DateTime.Now, DateTime.FromBinary(value), value, flag);
            }
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
