using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
	internal sealed class EmpiricalRuleParser
	{
		private EmpiricalRuleParser()
		{
		}

		public static RuleExpression Parse(string rule)
		{
            Stack<Symbol> stack = new Stack<Symbol>();
            int i = 0;
            while (i < rule.Length)
            {
                char c = rule[i++];
                Symbol symbol;
                if (char.IsDigit(c))
                {
                    int num = i - 1;
                    for (; i < rule.Length && char.IsDigit(rule[i]); i++)
                    {
                    }
                    symbol = new Symbol();
                    symbol.Type = ESymbolType.CompareExpression;
                    symbol.Value = Convert.ToInt64(rule.Substring(num, i - num), CultureInfo.InvariantCulture);
                }
                else if (char.IsLetter(c))
                {
                    string text = c.ToString(CultureInfo.InvariantCulture);
                    while (i < rule.Length && char.IsLetter(rule[i]))
                    {
                        text += rule[i++];
                    }
                    switch (text)
                    {
                        case "AND":
                            symbol = new Symbol(ESymbolType.TerminalAnd);
                            break;
                        case "OR":
                            symbol = new Symbol(ESymbolType.TerminalOr);
                            break;
                        case "NOT":
                            symbol = new Symbol(ESymbolType.TerminalNot);
                            break;
                        default:
                            throw new Exception("Unknown terminal symbol '" + text + "' at position " + i);
                    }
                }
                else
                {
                    switch (c)
                    {
                        case '(':
                            symbol = new Symbol(ESymbolType.TerminalLPar);
                            break;
                        case ')':
                            symbol = new Symbol(ESymbolType.TerminalRPar);
                            break;
                        case '=':
                            symbol = new Symbol();
                            symbol.Type = ESymbolType.Operator;
                            symbol.Value = CompareExpression.ECompareOperator.EQUAL;
                            break;
                        default:
                            if (char.IsWhiteSpace(c))
                            {
                                symbol = null;
                                break;
                            }
                            throw new Exception("Unknown character at position " + i);
                    }
                }
                if (symbol == null)
                {
                    continue;
                }
                stack.Push(symbol);
                bool flag = true;
                while (flag)
                {
                    Symbol symbol2 = stack.Pop();
                    Symbol symbol3 = ((stack.Count <= 0) ? new Symbol(ESymbolType.Unknown) : stack.Pop());
                    Symbol symbol4 = ((stack.Count <= 0) ? new Symbol(ESymbolType.Unknown) : stack.Pop());
                    bool flag2 = false;
                    if (symbol4.Type == ESymbolType.CompareExpression && symbol3.Type == ESymbolType.Operator && symbol2.Type == ESymbolType.CompareExpression)
                    {
                        Symbol symbol5 = new Symbol(ESymbolType.Value);
                        symbol5.Value = new ValueExpression((long)symbol2.Value);
                        stack.Push(symbol5);
                        flag2 = true;
                    }
                    else if (IsExpression(symbol4) && symbol3.Type == ESymbolType.TerminalAnd && IsExpression(symbol2))
                    {
                        Symbol symbol6 = new Symbol(ESymbolType.AndExpression);
                        symbol6.Value = new AndExpression((RuleExpression)symbol4.Value, (RuleExpression)symbol2.Value);
                        stack.Push(symbol6);
                        flag2 = true;
                    }
                    else if (IsExpression(symbol4) && symbol3.Type == ESymbolType.TerminalOr && IsExpression(symbol2))
                    {
                        Symbol symbol7 = new Symbol(ESymbolType.OrExpression);
                        symbol7.Value = new OrExpression((RuleExpression)symbol4.Value, (RuleExpression)symbol2.Value);
                        stack.Push(symbol7);
                        flag2 = true;
                    }
                    else if (symbol3.Type == ESymbolType.TerminalNot && IsExpression(symbol2))
                    {
                        Symbol symbol8 = new Symbol(ESymbolType.NotExpression);
                        symbol8.Value = new NotExpression((RuleExpression)symbol2.Value);
                        if (symbol4.Type != 0)
                        {
                            stack.Push(symbol4);
                        }
                        stack.Push(symbol8);
                        flag2 = true;
                    }
                    else if (symbol4.Type == ESymbolType.TerminalLPar && IsExpression(symbol3) && symbol2.Type == ESymbolType.TerminalRPar)
                    {
                        stack.Push(symbol3);
                        flag2 = true;
                    }
                    if (!flag2)
                    {
                        if (symbol4.Type != 0)
                        {
                            stack.Push(symbol4);
                        }
                        if (symbol3.Type != 0)
                        {
                            stack.Push(symbol3);
                        }
                        stack.Push(symbol2);
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
            if (stack.Count == 0)
            {
                return null;
            }
            if (stack.Count == 1)
            {
                Symbol symbol9 = stack.Pop();
                if (IsExpression(symbol9))
                {
                    return (RuleExpression)symbol9.Value;
                }
                throw new Exception("Illegal last token");
            }
            throw new Exception("Could not completely reduce tokens");
        }

        private static bool IsExpression(Symbol op)
        {
            return op.Type == ESymbolType.Value || op.Type == ESymbolType.AndExpression || op.Type == ESymbolType.OrExpression || op.Type == ESymbolType.NotExpression;
        }
    }
}
