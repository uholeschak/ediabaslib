using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    internal class Symbol
    {
        private RuleExpression.ESymbolType type;
        private object value;
        public RuleExpression.ESymbolType Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }

        public object Value
        {
            get
            {
                return value;
            }

            set
            {
                this.value = value;
            }
        }

        public Symbol()
        {
            type = RuleExpression.ESymbolType.Unknown;
        }

        public Symbol(RuleExpression.ESymbolType type)
        {
            this.type = type;
        }
    }
}