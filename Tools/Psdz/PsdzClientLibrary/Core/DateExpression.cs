using PsdzClientLibrary;
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
        private readonly ECompareOperator compareOperator;
        private readonly long datevalue;
        public DateExpression(ECompareOperator compareOperator, long datevalue)
        {
            this.datevalue = datevalue;
            this.compareOperator = compareOperator;
        }

        [PreserveSource(Hint = "Modified")]
        public static DateExpression Deserialize(Stream ms, Vehicle vec)
        {
            byte b = (byte)ms.ReadByte();
            ECompareOperator eCompareOperator = (ECompareOperator)b;
            byte[] array = new byte[8];
            ms.Read(array, 0, 8);
            long num = BitConverter.ToInt64(array, 0);
            return new DateExpression(eCompareOperator, num);
        }

        [PreserveSource(Hint = "Modified")]
        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationUtils, ValidationRuleInternalResults internalResult)
        {
            if (vec == null)
            {
                return false;
            }

            long num;
            try
            {
                num = Convert.ToInt32(vec.Modelljahr, CultureInfo.InvariantCulture) * 100 + Convert.ToInt32(vec.Modellmonat, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                ruleEvaluationUtils.Logger.WarningException("DateExpression.Evaluate()", exception);
                return false;
            }

            bool flag;
            switch (compareOperator)
            {
                case ECompareOperator.EQUAL:
                    flag = num == datevalue;
                    break;
                case ECompareOperator.NOT_EQUAL:
                    flag = num != datevalue;
                    break;
                case ECompareOperator.GREATER:
                    flag = num > datevalue;
                    break;
                case ECompareOperator.GREATER_EQUAL:
                    flag = num >= datevalue;
                    break;
                case ECompareOperator.LESS:
                    flag = num < datevalue;
                    break;
                case ECompareOperator.LESS_EQUAL:
                    flag = num <= datevalue;
                    break;
                default:
                    ruleEvaluationUtils.Logger.Warning("DateExpression.Evaluate", "unknown logical operator: {0}", compareOperator);
                    flag = false;
                    break;
            }

            ruleEvaluationUtils.Logger.Debug("DateExpression.Evaluate()", "rule: ConstructionDate {0} {1} result: {2}", compareOperator, datevalue, flag);
            return flag;
        }

        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            if (baseConfiguration.ProdDates.Count == 0)
            {
                return EEvaluationResult.MISSING_CHARACTERISTIC;
            }

            int num = baseConfiguration.ProdDates.BinarySearch(datevalue);
            switch (compareOperator)
            {
                case ECompareOperator.EQUAL:
                    return (num < 0) ? EEvaluationResult.INVALID : EEvaluationResult.VALID;
                case ECompareOperator.NOT_EQUAL:
                    return (num >= 0) ? EEvaluationResult.INVALID : EEvaluationResult.VALID;
                case ECompareOperator.GREATER:
                    if (num >= 0 && num < baseConfiguration.ProdDates.Count - 1)
                    {
                        return EEvaluationResult.VALID;
                    }

                    if (baseConfiguration.ProdDates.Count > 0 && baseConfiguration.ProdDates[0] > datevalue)
                    {
                        return EEvaluationResult.VALID;
                    }

                    return EEvaluationResult.INVALID;
                case ECompareOperator.GREATER_EQUAL:
                    if (num >= 0)
                    {
                        return EEvaluationResult.VALID;
                    }

                    if (baseConfiguration.ProdDates.Count > 0 && baseConfiguration.ProdDates[0] > datevalue)
                    {
                        return EEvaluationResult.VALID;
                    }

                    return EEvaluationResult.INVALID;
                case ECompareOperator.LESS:
                    if (num > 0)
                    {
                        return EEvaluationResult.VALID;
                    }

                    if (baseConfiguration.ProdDates.Count > 0 && baseConfiguration.ProdDates[baseConfiguration.ProdDates.Count - 1] < datevalue)
                    {
                        return EEvaluationResult.VALID;
                    }

                    return EEvaluationResult.INVALID;
                case ECompareOperator.LESS_EQUAL:
                    if (num >= 0)
                    {
                        return EEvaluationResult.VALID;
                    }

                    if (baseConfiguration.ProdDates.Count > 0 && baseConfiguration.ProdDates[baseConfiguration.ProdDates.Count - 1] < datevalue)
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
            ms.WriteByte((byte)compareOperator);
            ms.Write(BitConverter.GetBytes(datevalue), 0, 8);
        }

        public override string ToString()
        {
            return "Baustand " + GetOperator() + " " + datevalue.ToString(CultureInfo.InvariantCulture);
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

        [PreserveSource(Hint = "Added")]
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

        [PreserveSource(Hint = "Added")]
        private string GetFormulaOperator()
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
    }
}