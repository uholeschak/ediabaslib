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
					while (i < rule.Length && char.IsDigit(rule[i]))
					{
						i++;
					}
					symbol = new Symbol();
					symbol.Type = RuleExpression.ESymbolType.CompareExpression;
					symbol.Value = Convert.ToInt64(rule.Substring(num, i - num), CultureInfo.InvariantCulture);
				}
				else
				{
					if (char.IsLetter(c))
					{
						string text = c.ToString(CultureInfo.InvariantCulture);
						while (i < rule.Length && char.IsLetter(rule[i]))
						{
							text += rule[i++].ToString();
						}
						if (text != null)
						{
							if (text == "AND")
							{
								symbol = new Symbol(RuleExpression.ESymbolType.TerminalAnd);
								goto IL_177;
							}
							if (text == "OR")
							{
								symbol = new Symbol(RuleExpression.ESymbolType.TerminalOr);
								goto IL_177;
							}
							if (text == "NOT")
							{
								symbol = new Symbol(RuleExpression.ESymbolType.TerminalNot);
								goto IL_177;
							}
						}
						throw new Exception(string.Concat(new object[]
						{
							"Unknown terminal symbol '",
							text,
							"' at position ",
							i
						}));
					}
					if (c == '(')
					{
						symbol = new Symbol(RuleExpression.ESymbolType.TerminalLPar);
					}
					else if (c == ')')
					{
						symbol = new Symbol(RuleExpression.ESymbolType.TerminalRPar);
					}
					else if (c == '=')
					{
						symbol = new Symbol();
						symbol.Type = RuleExpression.ESymbolType.Operator;
						symbol.Value = CompareExpression.ECompareOperator.EQUAL;
					}
					else
					{
						if (!char.IsWhiteSpace(c))
						{
							throw new Exception("Unknown character at position " + i);
						}
						symbol = null;
					}
				}
				IL_177:
				if (symbol != null)
				{
					stack.Push(symbol);
					bool flag = true;
					while (flag)
					{
						Symbol symbol2 = stack.Pop();
						Symbol symbol3;
						if (stack.Count > 0)
						{
							symbol3 = stack.Pop();
						}
						else
						{
							symbol3 = new Symbol(RuleExpression.ESymbolType.Unknown);
						}
						Symbol symbol4;
						if (stack.Count > 0)
						{
							symbol4 = stack.Pop();
						}
						else
						{
							symbol4 = new Symbol(RuleExpression.ESymbolType.Unknown);
						}
						bool flag2 = false;
						if (symbol4.Type == RuleExpression.ESymbolType.CompareExpression && symbol3.Type == RuleExpression.ESymbolType.Operator && symbol2.Type == RuleExpression.ESymbolType.CompareExpression)
						{
							stack.Push(new Symbol(RuleExpression.ESymbolType.Value)
							{
								Value = new ValueExpression((long)symbol2.Value)
							});
							flag2 = true;
						}
						else if (EmpiricalRuleParser.IsExpression(symbol4) && symbol3.Type == RuleExpression.ESymbolType.TerminalAnd && EmpiricalRuleParser.IsExpression(symbol2))
						{
							stack.Push(new Symbol(RuleExpression.ESymbolType.AndExpression)
							{
								Value = new AndExpression((RuleExpression)symbol4.Value, (RuleExpression)symbol2.Value)
							});
							flag2 = true;
						}
						else if (EmpiricalRuleParser.IsExpression(symbol4) && symbol3.Type == RuleExpression.ESymbolType.TerminalOr && EmpiricalRuleParser.IsExpression(symbol2))
						{
							stack.Push(new Symbol(RuleExpression.ESymbolType.OrExpression)
							{
								Value = new OrExpression((RuleExpression)symbol4.Value, (RuleExpression)symbol2.Value)
							});
							flag2 = true;
						}
						else if (symbol3.Type == RuleExpression.ESymbolType.TerminalNot && EmpiricalRuleParser.IsExpression(symbol2))
						{
							Symbol symbol5 = new Symbol(RuleExpression.ESymbolType.NotExpression);
							symbol5.Value = new NotExpression((RuleExpression)symbol2.Value);
							if (symbol4.Type != RuleExpression.ESymbolType.Unknown)
							{
								stack.Push(symbol4);
							}
							stack.Push(symbol5);
							flag2 = true;
						}
						else if (symbol4.Type == RuleExpression.ESymbolType.TerminalLPar && EmpiricalRuleParser.IsExpression(symbol3) && symbol2.Type == RuleExpression.ESymbolType.TerminalRPar)
						{
							stack.Push(symbol3);
							flag2 = true;
						}
						if (!flag2)
						{
							if (symbol4.Type != RuleExpression.ESymbolType.Unknown)
							{
								stack.Push(symbol4);
							}
							if (symbol3.Type != RuleExpression.ESymbolType.Unknown)
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
			}
			if (stack.Count == 0)
			{
				return null;
			}
			if (stack.Count != 1)
			{
				throw new Exception("Could not completely reduce tokens");
			}
			Symbol symbol6 = stack.Pop();
			if (!EmpiricalRuleParser.IsExpression(symbol6))
			{
				throw new Exception("Illegal last token");
			}
			return (RuleExpression)symbol6.Value;
		}

		private static bool IsExpression(Symbol op)
		{
			return op.Type == RuleExpression.ESymbolType.Value || op.Type == RuleExpression.ESymbolType.AndExpression || op.Type == RuleExpression.ESymbolType.OrExpression || op.Type == RuleExpression.ESymbolType.NotExpression;
		}
	}
}
