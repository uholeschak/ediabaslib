using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
	[Serializable]
	public abstract class SingleAssignmentExpression : RuleExpression
	{
		public long Value
		{
			get
			{
				return this.value;
			}
		}

        // [UH] vec added
        // ToDo: Check on update
        public static RuleExpression Deserialize(Stream ms, EExpressionType type, Vehicle vec)
        {
            byte[] buffer = new byte[8];
            ms.Read(buffer, 0, 8);
            long num = BitConverter.ToInt64(buffer, 0);
            SingleAssignmentExpression singleAssignmentExpression;
            switch (type)
            {
                case EExpressionType.ISTUFE:
                    singleAssignmentExpression = new IStufeExpression();
                    break;
                case EExpressionType.VALID_FROM:
                    singleAssignmentExpression = new ValidFromExpression();
                    break;
                case EExpressionType.VALID_TO:
                    singleAssignmentExpression = new ValidToExpression();
                    break;
                case EExpressionType.COUNTRY:
                    singleAssignmentExpression = new CountryExpression();
                    break;
                case EExpressionType.ECUGROUP:
                    singleAssignmentExpression = new EcuGroupExpression();
                    break;
                case EExpressionType.ECUVARIANT:
                    singleAssignmentExpression = new EcuVariantExpression();
                    break;
                case EExpressionType.ECUCLIQUE:
                    singleAssignmentExpression = new EcuCliqueExpression();
                    break;
                case EExpressionType.EQUIPMENT:
                    singleAssignmentExpression = new EquipmentExpression();
                    break;
                case EExpressionType.SALAPA:
                    singleAssignmentExpression = new SaLaPaExpression();
                    break;
                case EExpressionType.SIFA:
                    singleAssignmentExpression = new SiFaExpression();
                    break;
                case EExpressionType.ECUREPRESENTATIVE:
                    singleAssignmentExpression = new EcuRepresentativeExpression();
                    break;
                case EExpressionType.ECUPROGRAMMINGVARIANT:
                    singleAssignmentExpression = new EcuProgrammingVariantExpression();
                    break;
                default:
                    //Log.Warning("SingleAssignmentExpression.Deserialize()", "unhandled SingleAssignmentExpression found: {0}", type.ToString());
                    throw new Exception("Unknown expression type");
            }
            singleAssignmentExpression.value = num;
            singleAssignmentExpression.vecInfo = vec;
            return singleAssignmentExpression;
        }

        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
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
			ms.Write(BitConverter.GetBytes(this.value), 0, 8);
		}

		protected long value;

        protected Vehicle vecInfo;
    }
}
