using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
	[Serializable]
	public abstract class RuleExpression : IRuleExpression
	{
        public enum EExpressionType
        {
            COMP,
            AND,
            OR,
            NOT,
            DATE,
            VALUE,
            ISTUFE,
            VALID_FROM,
            VALID_TO,
            COUNTRY,
            ECUGROUP,
            ECUVARIANT,
            ECUCLIQUE,
            EQUIPMENT,
            SALAPA,
            SIFA,
            VARIABLE,
            CHARACTERISTIC,
            ECUREPRESENTATIVE,
            MANUFACTORINGDATE,
            ISTUFEX,
            ECUPROGRAMMINGVARIANT = 22
        }

        public enum EEvaluationResult
        {
            VALID,
            INVALID,
            MISSING_CHARACTERISTIC,
            MISSING_VARIANT
        }

        public enum ESymbolType
        {
            Unknown,
            Value,
            Operator,
            TerminalAnd,
            TerminalOr,
            TerminalNot,
            TerminalLPar,
            TerminalRPar,
            TerminalProduktionsdatum,
            DateExpression,
            CompareExpression,
            NotExpression,
            OrExpression,
            AndExpression,
            Expression,
            VariableExpression
        }

		public static RuleExpression Deserialize(Stream ms)
		{
			EExpressionType type = (EExpressionType)((byte)ms.ReadByte());
			switch (type)
			{
				case EExpressionType.COMP:
					return CompareExpression.Deserialize(ms);
				case EExpressionType.AND:
					return AndExpression.Deserialize(ms);
				case EExpressionType.OR:
					return OrExpression.Deserialize(ms);
				case EExpressionType.NOT:
					return NotExpression.Deserialize(ms);
				case EExpressionType.DATE:
					return DateExpression.Deserialize(ms);
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
					return SingleAssignmentExpression.Deserialize(ms, type);
				case EExpressionType.CHARACTERISTIC:
					return CharacteristicExpression.Deserialize(ms);
				case EExpressionType.MANUFACTORINGDATE:
					return ManufactoringDateExpression.Deserialize(ms);
				case EExpressionType.ISTUFEX:
					return IStufeXExpression.Deserialize(ms);
			}
			throw new Exception("Unknown Expression-Type");
		}

		public static bool Evaluate(Vehicle vec, RuleExpression exp, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult = null)
		{
			if (internalResult == null)
			{
				internalResult = new ValidationRuleInternalResults();
			}
			if (exp is AndExpression)
			{
				return exp.Evaluate(vec, ffmResolver, internalResult);
			}
			if (exp is OrExpression)
			{
				return exp.Evaluate(vec, ffmResolver, internalResult);
			}
			if (exp is CharacteristicExpression)
			{
				return exp.Evaluate(vec, ffmResolver, internalResult);
			}
			if (exp is DateExpression)
			{
				return exp.Evaluate(vec, ffmResolver, internalResult);
			}
			if (exp is EcuCliqueExpression)
			{
				return exp.Evaluate(vec, ffmResolver, internalResult);
			}
			if (exp is NotExpression)
			{
				return exp.Evaluate(vec, ffmResolver, internalResult);
			}
			if (exp is SaLaPaExpression)
			{
				return exp.Evaluate(vec, ffmResolver, internalResult);
			}
			if (exp is CountryExpression)
			{
				return exp.Evaluate(vec, ffmResolver, internalResult);
			}
			if (exp is IStufeExpression)
			{
				return exp.Evaluate(vec, ffmResolver, internalResult);
			}
			if (exp is IStufeXExpression)
			{
				return exp.Evaluate(vec, ffmResolver, internalResult);
			}
			if (exp is EquipmentExpression)
			{
				return exp.Evaluate(vec, ffmResolver, internalResult);
			}
			if (exp is ValidFromExpression)
			{
				return exp.Evaluate(vec, ffmResolver, internalResult);
			}
			if (exp is ValidToExpression)
			{
				return exp.Evaluate(vec, ffmResolver, internalResult);
			}
			if (exp is SiFaExpression)
			{
				return exp.Evaluate(vec, ffmResolver, internalResult);
			}
			if (exp is EcuRepresentativeExpression)
			{
				return exp.Evaluate(vec, ffmResolver, internalResult);
			}
			if (exp is ManufactoringDateExpression)
			{
				return exp.Evaluate(vec, ffmResolver, internalResult);
			}
			if (exp is EcuVariantExpression)
			{
				return exp.Evaluate(vec, ffmResolver, internalResult);
			}
			if (exp is EcuProgrammingVariantExpression)
			{
				return exp.Evaluate(vec, ffmResolver, internalResult);
			}
			return false;
		}

		public static string ParseAndSerializeVariantRule(string rule)
		{
			return RuleExpression.SerializeToString(VariantRuleParser.Parse(rule));
		}

		public static RuleExpression ParseEmpiricalRule(string rule)
		{
			return EmpiricalRuleParser.Parse(rule);
		}

		public static RuleExpression ParseFaultClassRule(string rule)
		{
			return FaultClassRuleParser.Parse(rule);
		}

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

		public virtual bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
		{
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

		public static IList<string> RuleEvaluationProtocol;

		public const long MISSING_DATE_EXPRESSION = -1L;

		protected const long MEMORYSIZE_OBJECT = 8L;

		protected const long MEMORYSIZE_REFERENCE = 8L;
	}
}
