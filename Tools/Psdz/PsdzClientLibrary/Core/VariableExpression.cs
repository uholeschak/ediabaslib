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
    public class VariableExpression : RuleExpression
    {
        private readonly CompareExpression.ECompareOperator compareOperator;

        private readonly string variableName;

        private readonly double variableValue;

        public VariableExpression(string variableName, CompareExpression.ECompareOperator compareOperator, double variableValue)
        {
            this.variableName = variableName;
            this.variableValue = variableValue;
            this.compareOperator = compareOperator;
        }

        public override EEvaluationResult EvaluateFaultClassRule(Dictionary<string, List<double>> variables)
        {
            if (variables.TryGetValue(variableName, out var value))
            {
                foreach (double item in value)
                {
                    switch (compareOperator)
                    {
                        case CompareExpression.ECompareOperator.EQUAL:
                            if (item == variableValue)
                            {
                                return EEvaluationResult.VALID;
                            }
                            break;
                        case CompareExpression.ECompareOperator.GREATER:
                            if (item > variableValue)
                            {
                                return EEvaluationResult.VALID;
                            }
                            break;
                        case CompareExpression.ECompareOperator.GREATER_EQUAL:
                            if (item >= variableValue)
                            {
                                return EEvaluationResult.VALID;
                            }
                            break;
                        case CompareExpression.ECompareOperator.LESS:
                            if (item < variableValue)
                            {
                                return EEvaluationResult.VALID;
                            }
                            break;
                        case CompareExpression.ECompareOperator.LESS_EQUAL:
                            if (item <= variableValue)
                            {
                                return EEvaluationResult.VALID;
                            }
                            break;
                        case CompareExpression.ECompareOperator.NOT_EQUAL:
                            if (item != variableValue)
                            {
                                return EEvaluationResult.VALID;
                            }
                            break;
                        default:
                            throw new Exception("Unknown compare operator");
                    }
                }
            }
            return EEvaluationResult.INVALID;
        }

        public override long GetExpressionCount()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override long GetMemorySize()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void Serialize(MemoryStream ms)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        // [UH] added
        public override string ToFormula(FormulaConfig formulaConfig)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            stringBuilder.Append(this.variableName.ToString(CultureInfo.InvariantCulture));
            stringBuilder.Append(" ");
            stringBuilder.Append(this.GetOperator());
            stringBuilder.Append(" ");
            stringBuilder.Append(this.variableValue.ToString(CultureInfo.InvariantCulture));
            stringBuilder.Append(FormulaSeparator(formulaConfig));

            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            string[] obj = new string[5]
            {
                variableName.ToString(CultureInfo.InvariantCulture),
                " ",
                GetOperator(),
                " ",
                null
            };
            double num = variableValue;
            obj[4] = num.ToString(CultureInfo.InvariantCulture);
            return string.Concat(obj);
        }

        private string GetOperator()
        {
            switch (compareOperator)
            {
                case CompareExpression.ECompareOperator.EQUAL:
                    return "=";
                case CompareExpression.ECompareOperator.GREATER:
                    return ">";
                case CompareExpression.ECompareOperator.GREATER_EQUAL:
                    return ">=";
                case CompareExpression.ECompareOperator.LESS:
                    return "<";
                case CompareExpression.ECompareOperator.LESS_EQUAL:
                    return "<=";
                case CompareExpression.ECompareOperator.NOT_EQUAL:
                    return "!=";
                default:
                    throw new Exception("Unknown operator");
            }
        }
    }
}
