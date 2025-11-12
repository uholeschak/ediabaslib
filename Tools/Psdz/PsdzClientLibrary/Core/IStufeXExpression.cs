using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Utility;
using PsdzClientLibrary;

#pragma warning disable CS0169
namespace PsdzClient.Core
{
    [Serializable]
    public class IStufeXExpression : RuleExpression
    {
        public enum ILevelyType : byte
        {
            HO,
            Factory,
            Ziel
        }

        [PreserveSource(Hint = "Modified")]
        private readonly PsdzDatabase dataProvider;
        private readonly ECompareOperator compareOperator;
        private readonly long iLevelId;
        private readonly ILevelyType iLevelType;
        [PreserveSource(Hint = "Modified")]
        public IStufeXExpression(ECompareOperator compareOperator, long ilevelid, IStufeXExpression.ILevelyType iLevelType, Vehicle vec)
        {
            this.iLevelId = ilevelid;
            this.iLevelType = iLevelType;
            this.compareOperator = compareOperator;
            this.vecInfo = vec;
        }

        [PreserveSource(Hint = "Modified")]
        public static IStufeXExpression Deserialize(Stream ms, Vehicle vec)
        {
            ECompareOperator ecompareOperator = (ECompareOperator)((byte)ms.ReadByte());
            IStufeXExpression.ILevelyType levelyType = (IStufeXExpression.ILevelyType)ms.ReadByte();
            byte[] array = new byte[8];
            ms.Read(array, 0, 8);
            long ilevelid = BitConverter.ToInt64(array, 0);
            return new IStufeXExpression(ecompareOperator, ilevelid, levelyType, vec);
        }

        [PreserveSource(Hint = "Modified")]
        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationUtils, ValidationRuleInternalResults internalResult)
        {
            if (vec == null)
            {
                return false;
            }

            this.vecInfo = vec;
            string ilevelOperand = this.GetILevelOperand(vec);
            string istufeById = ClientContext.GetDatabase(this.vecInfo)?.GetIStufeById(this.iLevelId.ToString(CultureInfo.InvariantCulture));
            if (string.IsNullOrEmpty(istufeById))
            {
                return false;
            }

            if (string.IsNullOrEmpty(ilevelOperand) || "0".Equals(ilevelOperand))
            {
                return true;
            }

            string[] ilevelParts = this.GetILevelParts(istufeById);
            if (ilevelParts.Length > 1 && string.Compare(ilevelParts[0], 0, ilevelOperand, 0, ilevelParts[0].Length, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }

            bool flag;
            switch (this.compareOperator)
            {
                case ECompareOperator.EQUAL:
                {
                    int? num = FormatConverter.ExtractNumericalILevel(ilevelOperand);
                    int? num2 = FormatConverter.ExtractNumericalILevel(istufeById);
                    flag = (num.GetValueOrDefault() == num2.GetValueOrDefault() & num != null == (num2 != null));
                    break;
                }

                case ECompareOperator.NOT_EQUAL:
                {
                    int? num2 = FormatConverter.ExtractNumericalILevel(ilevelOperand);
                    int? num = FormatConverter.ExtractNumericalILevel(istufeById);
                    flag = !(num2.GetValueOrDefault() == num.GetValueOrDefault() & num2 != null == (num != null));
                    break;
                }

                case ECompareOperator.GREATER:
                {
                    int? num = FormatConverter.ExtractNumericalILevel(ilevelOperand);
                    int? num2 = FormatConverter.ExtractNumericalILevel(istufeById);
                    flag = (num.GetValueOrDefault() > num2.GetValueOrDefault() & (num != null & num2 != null));
                    break;
                }

                case ECompareOperator.GREATER_EQUAL:
                {
                    int? num2 = FormatConverter.ExtractNumericalILevel(ilevelOperand);
                    int? num = FormatConverter.ExtractNumericalILevel(istufeById);
                    flag = (num2.GetValueOrDefault() >= num.GetValueOrDefault() & (num2 != null & num != null));
                    break;
                }

                case ECompareOperator.LESS:
                {
                    int? num = FormatConverter.ExtractNumericalILevel(ilevelOperand);
                    int? num2 = FormatConverter.ExtractNumericalILevel(istufeById);
                    flag = (num.GetValueOrDefault() < num2.GetValueOrDefault() & (num != null & num2 != null));
                    break;
                }

                case ECompareOperator.LESS_EQUAL:
                {
                    int? num2 = FormatConverter.ExtractNumericalILevel(ilevelOperand);
                    int? num = FormatConverter.ExtractNumericalILevel(istufeById);
                    flag = (num2.GetValueOrDefault() <= num.GetValueOrDefault() & (num2 != null & num != null));
                    break;
                }

                default:
                    flag = false;
                    break;
            }

            return flag;
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
            return 21L;
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.WriteByte(20);
            ms.WriteByte((byte)compareOperator);
            ms.WriteByte((byte)iLevelType);
            ms.Write(BitConverter.GetBytes(iLevelId), 0, 8);
        }

        [PreserveSource(Hint = "Modified")]
        public override string ToString()
        {
            string istufeById = ClientContext.GetDatabase(this.vecInfo)?.GetIStufeById(this.iLevelId.ToString(CultureInfo.InvariantCulture));
            string ilevelTypeDescription = this.GetILevelTypeDescription();
            return string.Format(CultureInfo.InvariantCulture, "IStufeX: {0}-I-Stufe {1} '{2}' [{3}]", new object[] { ilevelTypeDescription, this.compareOperator, istufeById, this.iLevelId });
        }

        private int? ExtractNumericalILevel(string iLevel, IRuleEvaluationServices ruleEvaluationServices)
        {
            if (string.IsNullOrEmpty(iLevel) || iLevel.Length != 14)
            {
                ruleEvaluationServices.Logger.Warning("FormatConverter.ExtractNumericalILevel()", "iLevel format was not correct: '{0}'", iLevel);
                return null;
            }

            try
            {
                string text = iLevel.Replace("-", string.Empty);
                return Convert.ToInt32(text.Substring(4), CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                ruleEvaluationServices.Logger.WarningException("FormatConverter.ExtractNumericalILevel()", exception);
            }

            return null;
        }

        private string GetILevelOperand(IVehicleRuleEvaluation vehicle)
        {
            switch (iLevelType)
            {
                case ILevelyType.Factory:
                    return vehicle.ILevelWerk;
                case ILevelyType.HO:
                    return vehicle.ILevel;
                case ILevelyType.Ziel:
                    return vehicle.TargetILevel;
                default:
                    throw new Exception($"{iLevelType} is not a valid I-Level type.");
            }
        }

        private string[] GetILevelParts(string iLevel)
        {
            string[] array = new string[0];
            string text = iLevel;
            if (iLevel.Contains("|"))
            {
                text = iLevel.Split('|')[1];
            }

            return text.Split('-');
        }

        private string GetILevelTypeDescription()
        {
            switch (iLevelType)
            {
                case ILevelyType.Factory:
                    return "Werk";
                case ILevelyType.HO:
                    return "HO";
                case ILevelyType.Ziel:
                    return "Ziel";
                default:
                    return "Unknown";
            }
        }

        [PreserveSource(Hint = "Added")]
        private Vehicle vecInfo;
        [PreserveSource(Hint = "Added")]
        public override string ToFormula(FormulaConfig formulaConfig)
        {
            string istufeById = ClientContext.GetDatabase(this.vecInfo)?.GetIStufeById(this.iLevelId.ToString(CultureInfo.InvariantCulture));
            int? iLevelValue = null;
            if (!string.IsNullOrEmpty(istufeById))
            {
                iLevelValue = FormatConverter.ExtractNumericalILevel(istufeById);
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            stringBuilder.Append("(");
            stringBuilder.Append(formulaConfig.GetLongFunc);
            stringBuilder.Append("(\"IStufeX\") ");
            if (iLevelValue != null)
            {
                CompareExpression compareExpression = new CompareExpression(-1, compareOperator, -1);
                stringBuilder.Append(compareExpression.GetFormulaOperator());
                stringBuilder.Append(" ");
                stringBuilder.Append(iLevelValue.Value.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                stringBuilder.Append(" == -1");
            }

            stringBuilder.Append(")");
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            return stringBuilder.ToString();
        }
    }
}