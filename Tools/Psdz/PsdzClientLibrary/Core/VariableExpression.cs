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
    public class VariableExpression : RuleExpression
    {
        private readonly ECompareOperator compareOperator;
        private readonly string variableName;
        private readonly double variableValue;
        public VariableExpression(string variableName, ECompareOperator compareOperator, double variableValue)
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
                        case ECompareOperator.EQUAL:
                            if (item == variableValue)
                            {
                                return EEvaluationResult.VALID;
                            }

                            break;
                        case ECompareOperator.GREATER:
                            if (item > variableValue)
                            {
                                return EEvaluationResult.VALID;
                            }

                            break;
                        case ECompareOperator.GREATER_EQUAL:
                            if (item >= variableValue)
                            {
                                return EEvaluationResult.VALID;
                            }

                            break;
                        case ECompareOperator.LESS:
                            if (item < variableValue)
                            {
                                return EEvaluationResult.VALID;
                            }

                            break;
                        case ECompareOperator.LESS_EQUAL:
                            if (item <= variableValue)
                            {
                                return EEvaluationResult.VALID;
                            }

                            break;
                        case ECompareOperator.NOT_EQUAL:
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

        public override string ToString()
        {
            return variableName.ToString(CultureInfo.InvariantCulture) + " " + GetOperator() + " " + variableValue.ToString(CultureInfo.InvariantCulture);
        }

        private string GetOperator()
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

        [PreserveSource(Added = true)]
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
    }
}