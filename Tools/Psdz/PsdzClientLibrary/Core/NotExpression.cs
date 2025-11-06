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
		public NotExpression(RuleExpression operand)
		{
			this.operand = operand;
		}

		public RuleExpression Operand
		{
			get
			{
				return this.operand;
			}
			set
			{
				this.operand = value;
			}
		}

		public new static NotExpression Deserialize(Stream ms, ILogger logger, Vehicle vec)
		{
			return new NotExpression(RuleExpression.Deserialize(ms, logger, vec));
		}

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
			EEvaluationResult eevaluationResult = this.operand.EvaluateEmpiricalRule(premises);
			if (eevaluationResult == EEvaluationResult.VALID)
			{
				return EEvaluationResult.INVALID;
			}
			if (eevaluationResult == EEvaluationResult.INVALID)
			{
				return EEvaluationResult.VALID;
			}
			return eevaluationResult;
		}

		public override EEvaluationResult EvaluateFaultClassRule(Dictionary<string, List<double>> variables)
		{
			EEvaluationResult eevaluationResult = this.operand.EvaluateFaultClassRule(variables);
			if (eevaluationResult == EEvaluationResult.VALID)
			{
				return EEvaluationResult.INVALID;
			}
			if (eevaluationResult == EEvaluationResult.INVALID)
			{
				return EEvaluationResult.VALID;
			}
			return eevaluationResult;
		}

		public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
		{
			EEvaluationResult eevaluationResult = this.operand.EvaluateVariantRule(client, baseConfiguration, ecus);
			if (eevaluationResult == EEvaluationResult.VALID)
			{
				return EEvaluationResult.INVALID;
			}
			if (eevaluationResult == EEvaluationResult.INVALID)
			{
				return EEvaluationResult.VALID;
			}
			return eevaluationResult;
		}

		public override long GetExpressionCount()
		{
			return 1L + this.operand.GetExpressionCount();
		}

		public override long GetMemorySize()
		{
			return 16L + this.operand.GetMemorySize();
		}

		public override IList<long> GetUnknownCharacteristics(CharacteristicSet baseConfiguration)
		{
			return this.operand.GetUnknownCharacteristics(baseConfiguration);
		}

		public override IList<long> GetUnknownVariantIds(EcuConfiguration ecus)
		{
			return this.operand.GetUnknownVariantIds(ecus);
		}

		public override void Optimize()
		{
			if (this.operand != null)
			{
				this.operand.Optimize();
			}
		}

		public override void Serialize(MemoryStream ms)
		{
			ms.WriteByte(3);
			this.operand.Serialize(ms);
		}

        // [UH] added
        public override string ToFormula(FormulaConfig formulaConfig)
        {
            return "!(" + this.operand.ToFormula(formulaConfig) + ")";
        }

		public override string ToString()
		{
			return "NOT " + this.operand;
		}

        private RuleExpression operand;
	}
}
