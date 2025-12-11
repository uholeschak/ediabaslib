using PsdzClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CA2022
namespace PsdzClient.Core
{
    [Serializable]
    public class ValueExpression : RuleExpression
    {
        private readonly long value;
        public long Value => value;

        public ValueExpression(long value)
        {
            this.value = value;
        }

        [PreserveSource(Hint = "Modified")]
        public static ValueExpression Deserialize(Stream ms, Vehicle vec)
        {
            ms.ReadByte();
            byte[] buffer = new byte[8];
            ms.Read(buffer, 0, 8);
            return new ValueExpression(BitConverter.ToInt64(buffer, 0));
        }

        public override EEvaluationResult EvaluateEmpiricalRule(long[] premises)
        {
            if (Array.BinarySearch(premises, value) >= 0)
            {
                return EEvaluationResult.VALID;
            }

            return EEvaluationResult.INVALID;
        }

        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            return EEvaluationResult.INVALID;
        }

        public override long GetExpressionCount()
        {
            return 1L;
        }

        public override long GetMemorySize()
        {
            return 16L;
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.WriteByte(5);
            ms.Write(BitConverter.GetBytes(value), 0, 8);
        }

        public override string ToString()
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        [PreserveSource(Hint = "Added")]
        public override string ToFormula(FormulaConfig formulaConfig)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(this.value.ToString(CultureInfo.InvariantCulture));
            return stringBuilder.ToString();
        }
    }
}