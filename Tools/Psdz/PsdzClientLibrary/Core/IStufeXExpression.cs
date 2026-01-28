using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Utility;
using PsdzClient;

#pragma warning disable CS0169, CA2022
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

        [PreserveSource(Hint = "Database replaced", SuppressWarning = true)]
        private readonly PsdzDatabase dataProvider;
        private readonly ECompareOperator compareOperator;
        private readonly long iLevelId;
        private readonly ILevelyType iLevelType;
        [PreserveSource(Hint = "dataProvider removed, vec added", SignatureModified = true)]
        public IStufeXExpression(ECompareOperator compareOperator, long ilevelid, IStufeXExpression.ILevelyType iLevelType, Vehicle vec)
        {
            iLevelId = ilevelid;
            this.iLevelType = iLevelType;
            this.compareOperator = compareOperator;
            //[-] this.dataProvider = dataProvider;
            //[+] vecInfo = vec;
            vecInfo = vec;
        }

        [PreserveSource(Hint = "dataProvider removed, vec added", SignatureModified = true)]
        public static IStufeXExpression Deserialize(Stream ms, Vehicle vec)
        {
            byte b = (byte)ms.ReadByte();
            ECompareOperator eCompareOperator = (ECompareOperator)b;
            byte b2 = (byte)ms.ReadByte();
            ILevelyType levelyType = (ILevelyType)b2;
            byte[] array = new byte[8];
            ms.Read(array, 0, 8);
            long ilevelid = BitConverter.ToInt64(array, 0);
            //[-] return new IStufeXExpression(eCompareOperator, ilevelid, levelyType, dataProvider);
            //[+] return new IStufeXExpression(eCompareOperator, ilevelid, levelyType, vec);
            return new IStufeXExpression(eCompareOperator, ilevelid, levelyType, vec);
        }

        [PreserveSource(Hint = "dataProvider removed", SignatureModified = true)]
        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationUtils, ValidationRuleInternalResults internalResult)
        {
            if (vec == null)
            {
                return false;
            }

            //[+] vecInfo = vec;
            vecInfo = vec;
            bool flag = false;
            string iLevelOperand = GetILevelOperand(vec);
            //[-] string iStufeById = dataProvider.GetIStufeById(iLevelId);
            //[+] string iStufeById = ClientContext.GetDatabase(vecInfo)?.GetIStufeById(iLevelId.ToString(CultureInfo.InvariantCulture));
            string iStufeById = ClientContext.GetDatabase(vecInfo)?.GetIStufeById(iLevelId.ToString(CultureInfo.InvariantCulture));
            if (string.IsNullOrEmpty(iStufeById))
            {
                ruleEvaluationUtils.Logger.Warning("IStufeXExpression.Evaluate()", "IStufe id: {0} not found in database", iLevelId);
                return false;
            }
            if (string.IsNullOrEmpty(iLevelOperand) || "0".Equals(iLevelOperand))
            {
                return true;
            }
            string[] iLevelParts = GetILevelParts(iStufeById);
            if (iLevelParts.Length > 1 && string.Compare(iLevelParts[0], 0, iLevelOperand, 0, iLevelParts[0].Length, StringComparison.OrdinalIgnoreCase) != 0)
            {
                flag = false;
                ruleEvaluationUtils.Logger.Info("IStufeXExpression.Evaluate()", "IStufe : {0} operator: {1} type: {2} result: {3} by not fitting product line", iStufeById, compareOperator, GetILevelTypeDescription(), flag);
                return flag;
            }
            switch (compareOperator)
            {
                case ECompareOperator.EQUAL:
                    flag = ExtractNumericalILevel(iLevelOperand, ruleEvaluationUtils) == ExtractNumericalILevel(iStufeById, ruleEvaluationUtils);
                    break;
                case ECompareOperator.NOT_EQUAL:
                    flag = ExtractNumericalILevel(iLevelOperand, ruleEvaluationUtils) != ExtractNumericalILevel(iStufeById, ruleEvaluationUtils);
                    break;
                case ECompareOperator.GREATER:
                    flag = ExtractNumericalILevel(iLevelOperand, ruleEvaluationUtils) > ExtractNumericalILevel(iStufeById, ruleEvaluationUtils);
                    break;
                case ECompareOperator.GREATER_EQUAL:
                    flag = ExtractNumericalILevel(iLevelOperand, ruleEvaluationUtils) >= ExtractNumericalILevel(iStufeById, ruleEvaluationUtils);
                    break;
                case ECompareOperator.LESS:
                    flag = ExtractNumericalILevel(iLevelOperand, ruleEvaluationUtils) < ExtractNumericalILevel(iStufeById, ruleEvaluationUtils);
                    break;
                case ECompareOperator.LESS_EQUAL:
                    flag = ExtractNumericalILevel(iLevelOperand, ruleEvaluationUtils) <= ExtractNumericalILevel(iStufeById, ruleEvaluationUtils);
                    break;
                default:
                    ruleEvaluationUtils.Logger.Warning("IStufeXExpression.Evaluate", "unknown logical operator: {0}", compareOperator);
                    flag = false;
                    break;
            }
            ruleEvaluationUtils.Logger.Debug("IStufeXExpression.Evaluate()", "IStufe : {0} operator: {1} type: {2} result: {3} vehicle: {4}", iStufeById, compareOperator, GetILevelTypeDescription(), flag, iLevelOperand);
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

        public override string ToString()
        {
            //[-] string iStufeById = dataProvider.GetIStufeById(iLevelId);
            //[+] string iStufeById = ClientContext.GetDatabase(vecInfo)?.GetIStufeById(iLevelId.ToString(CultureInfo.InvariantCulture));
            string iStufeById = ClientContext.GetDatabase(vecInfo)?.GetIStufeById(iLevelId.ToString(CultureInfo.InvariantCulture));
            string iLevelTypeDescription = GetILevelTypeDescription();
            return string.Format(CultureInfo.InvariantCulture, "IStufeX: {0}-I-Stufe {1} '{2}' [{3}]", iLevelTypeDescription, compareOperator, iStufeById, iLevelId);
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

        [PreserveSource(Added = true)]
        private Vehicle vecInfo;
        [PreserveSource(Added = true)]
        public override string ToFormula(FormulaConfig formulaConfig)
        {
            string istufeById = ClientContext.GetDatabase(vecInfo)?.GetIStufeById(this.iLevelId.ToString(CultureInfo.InvariantCulture));
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