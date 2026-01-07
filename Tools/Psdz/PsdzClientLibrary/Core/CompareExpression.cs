using PsdzClient;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace PsdzClient.Core
{
    [Serializable]
    public class CompareExpression : RuleExpression
    {
        private ECompareOperator compareOperator;
        private long dataclassId;
        private long datavalueId;
        public long DataclassId
        {
            get
            {
                return dataclassId;
            }

            set
            {
                dataclassId = value;
            }
        }

        public long DatavalueId
        {
            get
            {
                return datavalueId;
            }

            set
            {
                datavalueId = value;
            }
        }

        public CompareExpression(long dataclassId, ECompareOperator compareOperator, long datavalueId)
        {
            this.dataclassId = dataclassId;
            this.datavalueId = datavalueId;
            this.compareOperator = compareOperator;
        }

        [PreserveSource(Hint = "vec added", SignatureModified = true)]
        public static CompareExpression Deserialize(Stream ms, Vehicle vec)
        {
            throw new Exception("Class CompExpression is only an intermediate class. Use special classes instead before serializing the rule!");
        }

        public override long GetExpressionCount()
        {
            return 1L;
        }

        public override long GetMemorySize()
        {
            return 28L;
        }

        public override void Serialize(MemoryStream ms)
        {
            throw new Exception("Class CompExpression is only an intermediate class. Use special classes instead before serializing the rule!");
        }

        public override string ToString()
        {
            return dataclassId.ToString(CultureInfo.InvariantCulture) + " " + getOperator() + " " + datavalueId.ToString(CultureInfo.InvariantCulture);
        }

        private string getOperator()
        {
            switch (compareOperator)
            {
                case ECompareOperator.EQUAL:
                    return "=";
                case ECompareOperator.GREATER:
                    return ">";
                case ECompareOperator.GREATER_EQUAL:
                    return ">=";
                case ECompareOperator.LESS:
                    return "<";
                case ECompareOperator.LESS_EQUAL:
                    return "<=";
                case ECompareOperator.NOT_EQUAL:
                    return "!=";
                default:
                    throw new Exception("Unknown operator");
            }
        }

        [PreserveSource(Hint = "Added")]
        public string GetFormulaOperator()
        {
            switch (this.compareOperator)
            {
                case ECompareOperator.EQUAL:
                    return "==";
                case ECompareOperator.NOT_EQUAL:
                    return "!=";
                case ECompareOperator.GREATER:
                    return ">";
                case ECompareOperator.GREATER_EQUAL:
                    return ">=";
                case ECompareOperator.LESS:
                    return "<";
                case ECompareOperator.LESS_EQUAL:
                    return "<=";
                default:
                    throw new Exception("Unknown operator");
            }
        }

        [PreserveSource(Hint = "Added")]
        public override string ToFormula(FormulaConfig formulaConfig)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(" ");
            stringBuilder.Append(this.GetFormulaOperator());
            stringBuilder.Append(" ");
            return stringBuilder.ToString();
        }
    }
}