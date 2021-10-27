using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    public class IStufeExpression : SingleAssignmentExpression
    {
        public IStufeExpression()
        {
        }

        public IStufeExpression(long iStufeId)
        {
            this.value = iStufeId;
        }

        private string IStufe
        {
            get
            {
                if (string.IsNullOrEmpty(this.iStufe))
                {
                    this.iStufe = DatabaseProviderFactory.Instance.GetIStufeById(this.value);
                    return this.iStufe;
                }
                return this.iStufe;
            }
        }

        public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
        {
            if (vec == null)
            {
                return false;
            }
            bool flag;
            if (!string.IsNullOrEmpty(vec.ILevel) && !(vec.ILevel == "0"))
            {
                flag = (vec.ILevel == this.IStufe);
            }
            else
            {
                flag = true;
            }
            return flag;
        }

        public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
        {
            if (ecus.IStufe != this.value && ecus.IStufe != 0L)
            {
                return EEvaluationResult.INVALID;
            }
            return EEvaluationResult.VALID;
        }

        public override void Serialize(MemoryStream ms)
        {
            ms.WriteByte(6);
            base.Serialize(ms);
        }

        public override string ToString()
        {
            return string.Concat(new object[]
            {
                "I-Stufe=",
                this.IStufe,
                " [",
                this.value,
                "]"
            });
        }

        private string iStufe;
    }
}
