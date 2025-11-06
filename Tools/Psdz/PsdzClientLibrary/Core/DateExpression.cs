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
	public class DateExpression : RuleExpression
	{
		public DateExpression(CompareExpression.ECompareOperator compareOperator, long datevalue)
		{
			this.datevalue = datevalue;
			this.compareOperator = compareOperator;
		}

		public static DateExpression Deserialize(Stream ms, Vehicle vec)
		{
			CompareExpression.ECompareOperator ecompareOperator = (CompareExpression.ECompareOperator)((byte)ms.ReadByte());
			byte[] array = new byte[8];
			ms.Read(array, 0, 8);
			long num = BitConverter.ToInt64(array, 0);
			return new DateExpression(ecompareOperator, num);
		}

		public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationUtils, ValidationRuleInternalResults internalResult)
		{
			if (vec == null)
			{
				return false;
			}
			bool result;
			try
			{
				long num = (long)(Convert.ToInt32(vec.Modelljahr, CultureInfo.InvariantCulture) * 100 + Convert.ToInt32(vec.Modellmonat, CultureInfo.InvariantCulture));
                switch (this.compareOperator)
                {
                    case CompareExpression.ECompareOperator.EQUAL:
                    {
                        result = (num == this.datevalue);
                        break;
                    }
                    case CompareExpression.ECompareOperator.NOT_EQUAL:
                    {
                        result = (num != this.datevalue);
                        break;
                    }
                    case CompareExpression.ECompareOperator.GREATER:
                    {
                        result = (num > this.datevalue);
                        break;
                    }
                    case CompareExpression.ECompareOperator.GREATER_EQUAL:
                    {
                        result = (num >= this.datevalue);
                        break;
                    }
                    case CompareExpression.ECompareOperator.LESS:
                    {
                        result = (num < this.datevalue);
                        break;
                    }
                    case CompareExpression.ECompareOperator.LESS_EQUAL:
                    {
                        result = (num <= this.datevalue);
                        break;
                    }
                    default:
                        result = false;
                        break;
                }
			}
			catch (Exception exception)
			{
				Log.WarningException("DateExpression.Evaluate()", exception);
				result = false;
			}
			return result;
		}

		public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
		{
			if (baseConfiguration.ProdDates.Count == 0)
			{
				return EEvaluationResult.MISSING_CHARACTERISTIC;
			}
			int num = baseConfiguration.ProdDates.BinarySearch(this.datevalue);
			switch (this.compareOperator)
			{
				case CompareExpression.ECompareOperator.EQUAL:
					if (num < 0)
					{
						return EEvaluationResult.INVALID;
					}
					return EEvaluationResult.VALID;
				case CompareExpression.ECompareOperator.NOT_EQUAL:
					if (num >= 0)
					{
						return EEvaluationResult.INVALID;
					}
					return EEvaluationResult.VALID;
				case CompareExpression.ECompareOperator.GREATER:
					if (num >= 0 && num < baseConfiguration.ProdDates.Count - 1)
					{
						return EEvaluationResult.VALID;
					}
					if (baseConfiguration.ProdDates.Count > 0 && baseConfiguration.ProdDates[0] > this.datevalue)
					{
						return EEvaluationResult.VALID;
					}
					return EEvaluationResult.INVALID;
				case CompareExpression.ECompareOperator.GREATER_EQUAL:
					if (num >= 0)
					{
						return EEvaluationResult.VALID;
					}
					if (baseConfiguration.ProdDates.Count > 0 && baseConfiguration.ProdDates[0] > this.datevalue)
					{
						return EEvaluationResult.VALID;
					}
					return EEvaluationResult.INVALID;
				case CompareExpression.ECompareOperator.LESS:
					if (num > 0)
					{
						return EEvaluationResult.VALID;
					}
					if (baseConfiguration.ProdDates.Count > 0 && baseConfiguration.ProdDates[baseConfiguration.ProdDates.Count - 1] < this.datevalue)
					{
						return EEvaluationResult.VALID;
					}
					return EEvaluationResult.INVALID;
				case CompareExpression.ECompareOperator.LESS_EQUAL:
					if (num >= 0)
					{
						return EEvaluationResult.VALID;
					}
					if (baseConfiguration.ProdDates.Count > 0 && baseConfiguration.ProdDates[baseConfiguration.ProdDates.Count - 1] < this.datevalue)
					{
						return EEvaluationResult.VALID;
					}
					return EEvaluationResult.INVALID;
				default:
					throw new Exception("Unknown logical operator");
			}
		}

		public override long GetExpressionCount()
		{
			return 1L;
		}

		public override long GetMemorySize()
		{
			return 20L;
		}

		public override IList<long> GetUnknownCharacteristics(CharacteristicSet baseConfiguration)
		{
			List<long> list = new List<long>();
			if (baseConfiguration.ProdDates == null || baseConfiguration.ProdDates.Count == 0)
			{
				list.Add(-1L);
			}
			return list;
		}

		public override void Serialize(MemoryStream ms)
		{
			ms.WriteByte(4);
			ms.WriteByte((byte)this.compareOperator);
			ms.Write(BitConverter.GetBytes(this.datevalue), 0, 8);
		}

        // [UH] added
        public override string ToFormula(FormulaConfig formulaConfig)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            stringBuilder.Append("(");
            stringBuilder.Append(formulaConfig.GetLongFunc);
            stringBuilder.Append("(\"Baustand\") ");
            stringBuilder.Append(this.GetFormulaOperator());
            stringBuilder.Append(" ");
            stringBuilder.Append(this.datevalue.ToString(CultureInfo.InvariantCulture));
            stringBuilder.Append(")");
            stringBuilder.Append(FormulaSeparator(formulaConfig));

            return stringBuilder.ToString();
        }

		public override string ToString()
		{
			return "Baustand " + this.GetOperator() + " " + this.datevalue.ToString(CultureInfo.InvariantCulture);
		}

        private string GetFormulaOperator()
        {
            switch (this.compareOperator)
            {
                case CompareExpression.ECompareOperator.EQUAL:
                    return "==";
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

		private readonly long datevalue;
	}
}
