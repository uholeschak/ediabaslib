using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.IO;

namespace PsdzClient.Core
{
    [Serializable]
    public abstract class RuleExpression : IRuleExpression
    {
        public static IList<string> RuleEvaluationProtocol;
        public const long MISSING_DATE_EXPRESSION = -1L;
        protected const long MEMORYSIZE_OBJECT = 8L;
        protected const long MEMORYSIZE_REFERENCE = 8L;
        [PreserveSource(Hint = "dataProvider replaced by vec", OriginalHash = "F80012312B5848E95CB166A6C0080344")]
        public static RuleExpression Deserialize(Stream ms, ILogger logger, Vehicle vec)
        {
            byte b = (byte)ms.ReadByte();
            EExpressionType eExpressionType = (EExpressionType)b;
            switch (eExpressionType)
            {
                case EExpressionType.COMP:
                    return CompareExpression.Deserialize(ms, vec);
                case EExpressionType.AND:
                    return AndExpression.Deserialize(ms, logger, vec);
                case EExpressionType.OR:
                    return OrExpression.Deserialize(ms, logger, vec);
                case EExpressionType.NOT:
                    return NotExpression.Deserialize(ms, logger, vec);
                case EExpressionType.DATE:
                    return DateExpression.Deserialize(ms, vec);
                case EExpressionType.CHARACTERISTIC:
                    return CharacteristicExpression.Deserialize(ms, vec);
                case EExpressionType.MANUFACTORINGDATE:
                    return ManufactoringDateExpression.Deserialize(ms, logger, vec);
                case EExpressionType.ISTUFEX:
                    return IStufeXExpression.Deserialize(ms, vec);
                case EExpressionType.ISTUFE:
                case EExpressionType.VALID_FROM:
                case EExpressionType.VALID_TO:
                case EExpressionType.COUNTRY:
                case EExpressionType.ECUGROUP:
                case EExpressionType.ECUVARIANT:
                case EExpressionType.ECUCLIQUE:
                case EExpressionType.EQUIPMENT:
                case EExpressionType.SALAPA:
                case EExpressionType.SIFA:
                case EExpressionType.ECUREPRESENTATIVE:
                case EExpressionType.ECUPROGRAMMINGVARIANT:
                    return SingleAssignmentExpression.Deserialize(ms, eExpressionType, logger, vec);
                default:
                    logger.Error("RuleExpression.Deserialize()", "Unknown Expression-Type");
                    throw new Exception("Unknown Expression-Type");
            }
        }

        [PreserveSource(Hint = "dataProvider removed", SignatureModified = true)]
        public static bool Evaluate(Vehicle vec, IRuleExpression exp, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationUtils = null, ValidationRuleInternalResults internalResult = null)
        {
            //[+] if (ruleEvaluationUtils == null)
            if (ruleEvaluationUtils == null)
                //[+] {
            {
                //[+] ruleEvaluationUtils = new RuleEvaluationServices(vec);
                ruleEvaluationUtils = new RuleEvaluationServices(vec);
                //[+] }
            }

            if (internalResult == null)
            {
                internalResult = new ValidationRuleInternalResults();
            }

            if (exp is AndExpression || exp is OrExpression || exp is CharacteristicExpression || exp is DateExpression || exp is EcuCliqueExpression || exp is NotExpression || exp is SaLaPaExpression || exp is CountryExpression || exp is IStufeExpression || exp is IStufeXExpression || exp is EquipmentExpression || exp is ValidFromExpression || exp is ValidToExpression || exp is SiFaExpression || exp is EcuRepresentativeExpression || exp is ManufactoringDateExpression || exp is EcuVariantExpression || exp is EcuProgrammingVariantExpression)
            {
                //[-] return exp.Evaluate(vec, dealer, ffmResolver, dataProvider, ruleEvaluationUtils, internalResult);
                //[+] return exp.Evaluate(vec, ffmResolver, ruleEvaluationUtils, internalResult);
                return exp.Evaluate(vec, ffmResolver, ruleEvaluationUtils, internalResult);
            }

            ruleEvaluationUtils.Logger.Error("RuleExpression.Evaluate(Vehicle vec, RuleExpression exp)", "RuleExpression {0} not implemented.", exp.ToString());
            return false;
        }

        [PreserveSource(Hint = "dataProvider removed", OriginalHash = "0A7E6E6BAD3A02FF6CACDE588B995D83")]
        public static string ParseAndSerializeVariantRule(string rule)
        {
            RuleExpression expression = VariantRuleParser.Parse(rule);
            return SerializeToString(expression);
        }

        [PreserveSource(Hint = "dataProvider removed", OriginalHash = "5AD08763624343397340C66D6B47418E")]
        public static RuleExpression ParseEmpiricalRule(string rule)
        {
            return EmpiricalRuleParser.Parse(rule);
        }

        [PreserveSource(Hint = "dataProvider removed", OriginalHash = "3DFE88DD28B543110D4709F2CA0F4A27")]
        public static RuleExpression ParseFaultClassRule(string rule)
        {
            return FaultClassRuleParser.Parse(rule);
        }

        [PreserveSource(Hint = "dataProvider removed", OriginalHash = "DC2E5A85B2E67280D99422031BB62095")]
        public static RuleExpression ParseVariantRule(string rule)
        {
            return VariantRuleParser.Parse(rule);
        }

        public static byte[] SerializeToByteArray(RuleExpression expression)
        {
            MemoryStream memoryStream = new MemoryStream();
            expression.Serialize(memoryStream);
            return memoryStream.ToArray();
        }

        public static string SerializeToString(RuleExpression expression)
        {
            MemoryStream memoryStream = new MemoryStream();
            expression.Serialize(memoryStream);
            return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        }

        [PreserveSource(Hint = "removed dealer and dataProvider", OriginalHash = "16F5F46F99A94A893752F2D82191CD5D")]
        public virtual bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationUtils, ValidationRuleInternalResults internalResult)
        {
            ruleEvaluationUtils.Logger.Error("RuleExpression.Evaluate(Vehicle vec)", "method Evaluate(Vehicle vec) is missing.");
            return false;
        }

        public virtual EEvaluationResult EvaluateEmpiricalRule(long[] premises)
        {
            return EEvaluationResult.INVALID;
        }

        public virtual EEvaluationResult EvaluateFaultClassRule(Dictionary<string, List<double>> variables)
        {
            return EEvaluationResult.INVALID;
        }

        public virtual EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            return EEvaluationResult.INVALID;
        }

        public abstract long GetExpressionCount();
        public abstract long GetMemorySize();
        public virtual IList<long> GetUnknownCharacteristics(CharacteristicSet baseConfiguration)
        {
            return new List<long>();
        }

        public virtual IList<long> GetUnknownVariantIds(EcuConfiguration ecus)
        {
            return new List<long>();
        }

        public virtual void Optimize()
        {
        }

        public abstract void Serialize(MemoryStream ms);
        [PreserveSource(Hint = "Added")]
        public class FormulaConfig
        {
            public FormulaConfig(string getStringFunc, string getLongFunc, string checkStringFunc, string checkLongFunc, string ruleValidFunc, bool isRuleValidNumFunc = false, List<string> subRuleIds = null, string operatorSeparator = null)
            {
                GetStringFunc = getStringFunc;
                GetLongFunc = getLongFunc;
                CheckStringFunc = checkStringFunc;
                CheckLongFunc = checkLongFunc;
                RuleValidFunc = ruleValidFunc;
                IsRuleValidNumFunc = isRuleValidNumFunc;
                SubRuleIds = subRuleIds;
                OperatorSeparator = operatorSeparator;
            }

            public string GetStringFunc { get; private set; }
            public string GetLongFunc { get; private set; }
            public string CheckStringFunc { get; private set; }
            public string CheckLongFunc { get; private set; }
            public string RuleValidFunc { get; private set; }
            public bool IsRuleValidNumFunc { get; private set; }
            public List<string> SubRuleIds { get; private set; }
            public string OperatorSeparator { get; private set; }
        }

        [PreserveSource(Hint = "Added")]
        public virtual string ToFormula(FormulaConfig formulaConfig)
        {
            throw new Exception("ToFormula() missing for class: \"" + this.GetType().Name + "\"");
        }

        [PreserveSource(Hint = "Added")]
        public virtual string FormulaSeparator(FormulaConfig formulaConfig)
        {
            if (!string.IsNullOrEmpty(formulaConfig.OperatorSeparator))
            {
                return formulaConfig.OperatorSeparator;
            }

            return string.Empty;
        }
    }
}