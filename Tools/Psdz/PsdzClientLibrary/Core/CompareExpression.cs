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
    public class CompareExpression : RuleExpression
    {
        public enum ECompareOperator
        {
            EQUAL,
            NOT_EQUAL,
            GREATER,
            GREATER_EQUAL,
            LESS,
            LESS_EQUAL
        }

        public CompareExpression(long dataclassId, ECompareOperator compareOperator, long datavalueId)
        {
            this.dataclassId = dataclassId;
            this.datavalueId = datavalueId;
            this.compareOperator = compareOperator;
        }

        public long DataclassId
        {
            get
            {
                return this.dataclassId;
            }
            set
            {
                this.dataclassId = value;
            }
        }

        public long DatavalueId
        {
            get
            {
                return this.datavalueId;
            }
            set
            {
                this.datavalueId = value;
            }
        }

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

        // [UH] added
        public override string ToFormula(FormulaConfig formulaConfig)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(" ");
            stringBuilder.Append(this.GetFormulaOperator());
            stringBuilder.Append(" ");

            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                this.dataclassId.ToString(CultureInfo.InvariantCulture),
                " ",
                this.getOperator(),
                " ",
                this.datavalueId.ToString(CultureInfo.InvariantCulture)
            });
        }

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

        private string getOperator()
        {
            switch (this.compareOperator)
            {
                case ECompareOperator.EQUAL:
                    return "=";
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

        private ECompareOperator compareOperator;

        private long dataclassId;

        private long datavalueId;
    }
}
