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
	public class ManufactoringDateExpression : RuleExpression
	{
		public ManufactoringDateExpression(CompareExpression.ECompareOperator compareOperator, long datevalue)
		{
			this.compareOperator = compareOperator;
			DateTime dateTime = new DateTime(datevalue);
			this.dateArgument = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
			this.datevalue = this.dateArgument.Ticks;
		}

		public new static ManufactoringDateExpression Deserialize(Stream ms, Vehicle vec)
		{
			CompareExpression.ECompareOperator ecompareOperator = (CompareExpression.ECompareOperator)((byte)ms.ReadByte());
			byte[] array = new byte[8];
			ms.Read(array, 0, 8);
			long ticks = BitConverter.ToInt64(array, 0);
			return new ManufactoringDateExpression(ecompareOperator, ticks);
		}

		public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
		{
			if (vec == null)
			{
				return false;
			}
			long ticks;
			try
			{
				if (!DateTime.MinValue.Ticks.Equals(vec.ProductionDate.Ticks))
				{
					ticks = vec.ProductionDate.Ticks;
				}
				else
				{
					if (string.IsNullOrEmpty(vec.Modelljahr) || string.IsNullOrEmpty(vec.Modellmonat))
					{
						return false;
					}
					ticks = new DateTime(Convert.ToInt32(vec.Modelljahr, CultureInfo.InvariantCulture), Convert.ToInt32(vec.Modellmonat, CultureInfo.InvariantCulture), 1).Ticks;
				}
			}
			catch (Exception exception)
			{
				Log.WarningException("ManufactoringDateExpression.Evaluate()", exception);
				return false;
			}
			bool flag;
			switch (this.compareOperator)
			{
				case CompareExpression.ECompareOperator.EQUAL:
					flag = (ticks == this.datevalue);
					break;
				case CompareExpression.ECompareOperator.NOT_EQUAL:
					flag = (ticks != this.datevalue);
					break;
				case CompareExpression.ECompareOperator.GREATER:
					flag = (ticks > this.datevalue);
					break;
				case CompareExpression.ECompareOperator.GREATER_EQUAL:
					flag = (ticks >= this.datevalue);
					break;
				case CompareExpression.ECompareOperator.LESS:
					flag = (ticks < this.datevalue);
					break;
				case CompareExpression.ECompareOperator.LESS_EQUAL:
					flag = (ticks <= this.datevalue);
					break;
				default:
					flag = false;
					break;
			}
			return flag;
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

        public override string ToFormula(FormulaConfig formulaConfig)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            stringBuilder.Append("(");
            stringBuilder.Append(formulaConfig.GetLongFunc);
            stringBuilder.Append("(\"Produktionsdatum\")");

            stringBuilder.Append(" ");
            stringBuilder.Append(this.GetFormulaOperator());
            stringBuilder.Append(" ");

            DateTime date = new DateTime(datevalue);
            stringBuilder.Append(date.ToString("yyyyMM", CultureInfo.InvariantCulture));
            stringBuilder.Append(")");
            stringBuilder.Append(FormulaSeparator(formulaConfig));

            return stringBuilder.ToString();
        }


		public override string ToString()
		{
			return "Produktionsdatum " + this.GetOperator() + " " + this.datevalue.ToString(CultureInfo.InvariantCulture);
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

		private readonly DateTime dateArgument;

		private readonly long datevalue;
	}
}
