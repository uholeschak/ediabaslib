using PsdzClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CA2022
namespace PsdzClient.Core
{
    [Serializable]
    public abstract class SingleAssignmentExpression : RuleExpression
    {
        protected long value;
        public long Value => value;

        [PreserveSource(Hint = "dataProvider replaced by vec", OriginalHash = "9B80D176470C78725F5F89E46718BD08")]
        public static RuleExpression Deserialize(Stream ms, EExpressionType type, ILogger logger, Vehicle vec)
        {
            byte[] buffer = new byte[8];
            ms.Read(buffer, 0, 8);
            long num = BitConverter.ToInt64(buffer, 0);
            SingleAssignmentExpression singleAssignmentExpression;
            switch (type)
            {
                case EExpressionType.COUNTRY:
                    // [UH] [IGNORE] dataProvider removed
                    singleAssignmentExpression = new CountryExpression();
                    break;
                case EExpressionType.ECUCLIQUE:
                    singleAssignmentExpression = new EcuCliqueExpression();
                    break;
                case EExpressionType.ECUREPRESENTATIVE:
                    singleAssignmentExpression = new EcuRepresentativeExpression();
                    break;
                case EExpressionType.ECUGROUP:
                    singleAssignmentExpression = new EcuGroupExpression();
                    break;
                case EExpressionType.ECUVARIANT:
                    singleAssignmentExpression = new EcuVariantExpression();
                    break;
                case EExpressionType.ECUPROGRAMMINGVARIANT:
                    singleAssignmentExpression = new EcuProgrammingVariantExpression();
                    break;
                case EExpressionType.EQUIPMENT:
                    singleAssignmentExpression = new EquipmentExpression();
                    break;
                case EExpressionType.ISTUFE:
                    // [UH] [IGNORE] dataProvider removed
                    singleAssignmentExpression = new IStufeExpression();
                    break;
                case EExpressionType.SALAPA:
                    singleAssignmentExpression = new SaLaPaExpression();
                    break;
                case EExpressionType.SIFA:
                    singleAssignmentExpression = new SiFaExpression();
                    break;
                case EExpressionType.VALID_FROM:
                    singleAssignmentExpression = new ValidFromExpression();
                    break;
                case EExpressionType.VALID_TO:
                    singleAssignmentExpression = new ValidToExpression();
                    break;
                default:
                    logger.Warning("SingleAssignmentExpression.Deserialize()", "unhandled SingleAssignmentExpression found: {0}", type.ToString());
                    throw new Exception("Unknown expression type");
            }

            singleAssignmentExpression.value = num;
            // [UH] [IGNORE] added
            singleAssignmentExpression.vecInfo = vec;
            return singleAssignmentExpression;
        }

        [PreserveSource(Hint = "dataProvider replaced by vec", OriginalHash = "896E969176A3874B85317D15815F618F", SignatureModified = true)]
        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, IRuleEvaluationServices ruleEvaluationUtils, ValidationRuleInternalResults internalResult)
        {
            return false;
        }

        public override long GetExpressionCount()
        {
            return 1L;
        }

        public override long GetMemorySize()
        {
            return 16L;
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.Write(BitConverter.GetBytes(value), 0, 8);
        }

        [PreserveSource(Hint = "Added")]
        protected Vehicle vecInfo;
    }
}