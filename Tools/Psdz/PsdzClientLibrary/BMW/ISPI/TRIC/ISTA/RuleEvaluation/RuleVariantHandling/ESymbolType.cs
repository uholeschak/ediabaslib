namespace BMW.ISPI.TRIC.ISTA.RuleEvaluation.RuleVariantHandling
{
    internal enum ESymbolType
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
}