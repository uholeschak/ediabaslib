using PsdzClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

#pragma warning disable CA2022
namespace PsdzClient.Core
{
    [Serializable]
    public class OrExpression : RuleExpression
    {
        private readonly List<long> missingCharacteristics = new List<long>();
        private readonly List<long> missingVariants = new List<long>();
        private RuleExpression[] operands;
        public int Length => operands.Length;

        public RuleExpression this[int index]
        {
            get
            {
                return operands[index];
            }

            set
            {
                operands[index] = value;
            }
        }

        public OrExpression()
        {
            operands = new RuleExpression[0];
        }

        public OrExpression(RuleExpression firstOperand, RuleExpression secondOperand)
        {
            operands = new RuleExpression[2];
            operands[0] = firstOperand;
            operands[1] = secondOperand;
        }

        [PreserveSource(Hint = "dataProvider replaced by vec", OriginalHash = "4D1363B62AD03D057C5D3689F76E3908")]
        public new static OrExpression Deserialize(Stream ms, ILogger logger, Vehicle vec)
        {
            int value = 0;
            byte[] bytes = BitConverter.GetBytes(value);
            ms.Read(bytes, 0, bytes.Length);
            value = BitConverter.ToInt32(bytes, 0);
            OrExpression orExpression = new OrExpression();
            for (int i = 0; i < value; i++)
            {
                orExpression.AddOperand(RuleExpression.Deserialize(ms, logger, vec));
            }
            return orExpression;
        }

        public void AddOperand(RuleExpression operand)
        {
            RuleExpression[] array = new RuleExpression[operands.Length + 1];
            Array.Copy(operands, array, operands.Length);
            array[array.Length - 1] = operand;
            operands = array;
        }

        [PreserveSource(Hint = "dataProvider replaced by vec", OriginalHash = "9895C1C44368BC677DEFE5779D24E509")]
        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationUtils, ValidationRuleInternalResults internalResult)
        {
            bool flag = false;
            internalResult.RuleExpression = this;
            ILogger logger = ruleEvaluationUtils.Logger;
            RuleExpression[] array = operands;
            foreach (RuleExpression ruleExpression in array)
            {
                logger.Debug("OrExpression.Evaluate()", "operand: {0}", ruleExpression);
                flag |= RuleExpression.Evaluate(vec, ruleExpression, ffmResolver, ruleEvaluationUtils, internalResult);
            }

            logger.Debug("OrExpression.Evaluate()", "validity: {0}", flag);
            return flag;
        }

        public override EEvaluationResult EvaluateEmpiricalRule(long[] premises)
        {
            RuleExpression[] array = operands;
            foreach (RuleExpression ruleExpression in array)
            {
                EEvaluationResult eEvaluationResult = ruleExpression.EvaluateEmpiricalRule(premises);
                if (eEvaluationResult != EEvaluationResult.INVALID)
                {
                    return eEvaluationResult;
                }
            }

            return EEvaluationResult.INVALID;
        }

        public override EEvaluationResult EvaluateFaultClassRule(Dictionary<string, List<double>> variables)
        {
            RuleExpression[] array = operands;
            foreach (RuleExpression ruleExpression in array)
            {
                EEvaluationResult eEvaluationResult = ruleExpression.EvaluateFaultClassRule(variables);
                if (eEvaluationResult != EEvaluationResult.INVALID)
                {
                    return eEvaluationResult;
                }
            }

            return EEvaluationResult.INVALID;
        }

        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            bool flag = false;
            missingCharacteristics.Clear();
            missingVariants.Clear();
            EEvaluationResult result = EEvaluationResult.INVALID;
            RuleExpression[] array = operands;
            foreach (RuleExpression ruleExpression in array)
            {
                EEvaluationResult eEvaluationResult = ruleExpression.EvaluateVariantRule(client, baseConfiguration, ecus);
                switch (eEvaluationResult)
                {
                    case EEvaluationResult.VALID:
                        missingCharacteristics.Clear();
                        missingVariants.Clear();
                        return eEvaluationResult;
                    case EEvaluationResult.MISSING_CHARACTERISTIC:
                        missingCharacteristics.AddRange(ruleExpression.GetUnknownCharacteristics(baseConfiguration));
                        if (!flag)
                        {
                            flag = true;
                            result = eEvaluationResult;
                        }

                        break;
                    case EEvaluationResult.MISSING_VARIANT:
                        missingVariants.AddRange(ruleExpression.GetUnknownVariantIds(ecus));
                        if (!flag)
                        {
                            flag = true;
                            result = eEvaluationResult;
                        }

                        break;
                    default:
                        throw new Exception("Unknown result");
                    case EEvaluationResult.INVALID:
                        break;
                }
            }

            return result;
        }

        public override long GetExpressionCount()
        {
            long num = 1L;
            RuleExpression[] array = operands;
            foreach (RuleExpression ruleExpression in array)
            {
                num += ruleExpression.GetExpressionCount();
            }

            return num;
        }

        public override long GetMemorySize()
        {
            long num = (long)operands.Length * 8L + 8;
            RuleExpression[] array = operands;
            foreach (RuleExpression ruleExpression in array)
            {
                num += ruleExpression.GetMemorySize();
            }

            return num;
        }

        public override IList<long> GetUnknownCharacteristics(CharacteristicSet baseConfiguration)
        {
            return missingCharacteristics;
        }

        public override IList<long> GetUnknownVariantIds(EcuConfiguration ecus)
        {
            return missingVariants;
        }

        public override void Optimize()
        {
            List<RuleExpression> list = new List<RuleExpression>();
            RuleExpression[] array = operands;
            foreach (RuleExpression ruleExpression in array)
            {
                ruleExpression.Optimize();
                if (ruleExpression is AndExpression)
                {
                    AndExpression andExpression = (AndExpression)ruleExpression;
                    if (andExpression.Length == 1)
                    {
                        list.Add(andExpression[0]);
                    }
                    else
                    {
                        list.Add(andExpression);
                    }
                }
                else if (ruleExpression is OrExpression)
                {
                    OrExpression orExpression = (OrExpression)ruleExpression;
                    if (orExpression.operands.Length != 0)
                    {
                        list.AddRange(orExpression.operands);
                    }
                }
                else
                {
                    list.Add(ruleExpression);
                }
            }

            operands = list.ToArray();
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.WriteByte(2);
            byte[] bytes = BitConverter.GetBytes(operands.Length);
            ms.Write(bytes, 0, bytes.Length);
            RuleExpression[] array = operands;
            foreach (RuleExpression ruleExpression in array)
            {
                ruleExpression.Serialize(ms);
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("(");
            for (int i = 0; i < operands.Length; i++)
            {
                if (i > 0)
                {
                    stringBuilder.Append(" OR ");
                }

                stringBuilder.Append(operands[i]);
            }

            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }

        [PreserveSource(Hint = "Added")]
        public override string ToFormula(FormulaConfig formulaConfig)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("(");
            for (int i = 0; i < this.operands.Length; i++)
            {
                if (i > 0)
                {
                    stringBuilder.Append(" || ");
                }

                stringBuilder.Append(this.operands[i].ToFormula(formulaConfig));
            }

            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }
    }
}