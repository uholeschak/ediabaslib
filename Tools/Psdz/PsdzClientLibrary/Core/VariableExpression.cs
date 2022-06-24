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
		public VariableExpression(string variableName, CompareExpression.ECompareOperator compareOperator, double variableValue)
		{
			this.variableName = variableName;
			this.variableValue = variableValue;
			this.compareOperator = compareOperator;
		}

		public override EEvaluationResult EvaluateFaultClassRule(Dictionary<string, List<double>> variables)
		{
			List<double> list;
			if (variables.TryGetValue(this.variableName, out list))
			{
				using (List<double>.Enumerator enumerator = list.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						double num = enumerator.Current;
						switch (this.compareOperator)
						{
							case CompareExpression.ECompareOperator.EQUAL:
								if (num == this.variableValue)
								{
									return EEvaluationResult.VALID;
								}
								break;
							case CompareExpression.ECompareOperator.NOT_EQUAL:
								if (num != this.variableValue)
								{
									return EEvaluationResult.VALID;
								}
								break;
							case CompareExpression.ECompareOperator.GREATER:
								if (num > this.variableValue)
								{
									return EEvaluationResult.VALID;
								}
								break;
							case CompareExpression.ECompareOperator.GREATER_EQUAL:
								if (num >= this.variableValue)
								{
									return EEvaluationResult.VALID;
								}
								break;
							case CompareExpression.ECompareOperator.LESS:
								if (num < this.variableValue)
								{
									return EEvaluationResult.VALID;
								}
								break;
							case CompareExpression.ECompareOperator.LESS_EQUAL:
								if (num <= this.variableValue)
								{
									return EEvaluationResult.VALID;
								}
								break;
							default:
								throw new Exception("Unknown compare operator");
						}
					}
					return EEvaluationResult.INVALID;
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
			return string.Concat(new string[]
			{
				this.variableName.ToString(CultureInfo.InvariantCulture),
				" ",
				this.GetOperator(),
				" ",
				this.variableValue.ToString(CultureInfo.InvariantCulture)
			});
		}

		private string GetOperator()
		{
			switch (this.compareOperator)
			{
				case CompareExpression.ECompareOperator.EQUAL:
					return "=";
				case CompareExpression.ECompareOperator.NOT_EQUAL:
					return "!=";
				case CompareExpression.ECompareOperator.GREATER:
					return ">";
				case CompareExpression.ECompareOperator.GREATER_EQUAL:
					return ">=";
				case CompareExpression.ECompareOperator.LESS:
					return "<";
				case CompareExpression.ECompareOperator.LESS_EQUAL:
					return "<=";
				default:
					throw new Exception("Unknown operator");
			}
		}

		private readonly CompareExpression.ECompareOperator compareOperator;

		private readonly string variableName;

		private readonly double variableValue;
	}
}
