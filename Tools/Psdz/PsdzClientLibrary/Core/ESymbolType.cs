namespace PsdzClient.Core
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