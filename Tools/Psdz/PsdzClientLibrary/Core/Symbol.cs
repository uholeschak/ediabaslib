using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    public class Symbol
    {
        public Symbol()
        {
            this.type = RuleExpression.ESymbolType.Unknown;
        }

        public Symbol(RuleExpression.ESymbolType type)
        {
            this.type = type;
        }

        public RuleExpression.ESymbolType Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        public object Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }

        private RuleExpression.ESymbolType type;

        private object value;
    }
}
