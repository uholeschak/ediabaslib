using PsdzClientLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    [Serializable]
    public class NotExpression : RuleExpression
    {
        private RuleExpression operand;
        public RuleExpression Operand
        {
            get
            {
                return operand;
            }

            set
            {
                operand = value;
            }
        }

        public NotExpression(RuleExpression operand)
        {
            this.operand = operand;
        }

        [PreserveSource(Hint = "Modified")]
        public new static NotExpression Deserialize(Stream ms, ILogger logger, Vehicle vec)
        {
            return new NotExpression(RuleExpression.Deserialize(ms, logger, vec));
        }

        [PreserveSource(Hint = "Modified")]
        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationUtils, ValidationRuleInternalResults internalResult)
        {
            if (vec == null)
            {
                return false;
            }

            internalResult.RuleExpression = this;
            return !RuleExpression.Evaluate(vec, this.operand, ffmResolver, ruleEvaluationUtils, internalResult);
        }

        public override EEvaluationResult EvaluateEmpiricalRule(long[] premises)
        {
            EEvaluationResult eEvaluationResult = operand.EvaluateEmpiricalRule(premises);
            switch (eEvaluationResult)
            {
                case EEvaluationResult.INVALID:
                    return EEvaluationResult.VALID;
                case EEvaluationResult.VALID:
                    return EEvaluationResult.INVALID;
                default:
                    return eEvaluationResult;
            }
        }

        public override EEvaluationResult EvaluateFaultClassRule(Dictionary<string, List<double>> variables)
        {
            EEvaluationResult eEvaluationResult = operand.EvaluateFaultClassRule(variables);
            switch (eEvaluationResult)
            {
                case EEvaluationResult.INVALID:
                    return EEvaluationResult.VALID;
                case EEvaluationResult.VALID:
                    return EEvaluationResult.INVALID;
                default:
                    return eEvaluationResult;
            }
        }

        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            EEvaluationResult eEvaluationResult = operand.EvaluateVariantRule(client, baseConfiguration, ecus);
            switch (eEvaluationResult)
            {
                case EEvaluationResult.INVALID:
                    return EEvaluationResult.VALID;
                case EEvaluationResult.VALID:
                    return EEvaluationResult.INVALID;
                default:
                    return eEvaluationResult;
            }
        }

        public override long GetExpressionCount()
        {
            return 1 + operand.GetExpressionCount();
        }

        public override long GetMemorySize()
        {
            return 16 + operand.GetMemorySize();
        }

        public override IList<long> GetUnknownCharacteristics(CharacteristicSet baseConfiguration)
        {
            return operand.GetUnknownCharacteristics(baseConfiguration);
        }

        public override IList<long> GetUnknownVariantIds(EcuConfiguration ecus)
        {
            return operand.GetUnknownVariantIds(ecus);
        }

        public override void Optimize()
        {
            if (operand != null)
            {
                operand.Optimize();
            }
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.WriteByte(3);
            operand.Serialize(ms);
        }

        public override string ToString()
        {
            return "NOT " + operand;
        }

        [PreserveSource(Hint = "Added")]
        public override string ToFormula(FormulaConfig formulaConfig)
        {
            return "!(" + this.operand.ToFormula(formulaConfig) + ")";
        }
    }
}