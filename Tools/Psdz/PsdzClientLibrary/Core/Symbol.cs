namespace BMW.ISPI.TRIC.ISTA.RuleEvaluation.RuleVariantHandling
{
    internal class Symbol
    {
        private ESymbolType type;
        private object value;
        public ESymbolType Type
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
            type = ESymbolType.Unknown;
        }

        public Symbol(ESymbolType type)
        {
            this.type = type;
        }
    }
}