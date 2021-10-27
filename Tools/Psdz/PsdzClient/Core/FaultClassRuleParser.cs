using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
	public class FaultClassRuleParser
	{
		private FaultClassRuleParser()
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
					symbol.Type = RuleExpression.ESymbolType.Value;
					symbol.Value = Convert.ToInt64(rule.Substring(num, i - num), CultureInfo.InvariantCulture);
				}
				else
				{
					if (!char.IsLetter(c))
					{
						if (c != '_')
						{
							if (c == '(')
							{
								symbol = new Symbol();
								symbol.Type = RuleExpression.ESymbolType.TerminalLPar;
								goto IL_257;
							}
							if (c == ')')
							{
								symbol = new Symbol();
								symbol.Type = RuleExpression.ESymbolType.TerminalRPar;
								goto IL_257;
							}
							if (c == '=')
							{
								symbol = new Symbol();
								symbol.Type = RuleExpression.ESymbolType.Operator;
								symbol.Value = CompareExpression.ECompareOperator.EQUAL;
								goto IL_257;
							}
							if (c == '<')
							{
								symbol = new Symbol();
								symbol.Type = RuleExpression.ESymbolType.Operator;
								if (i < rule.Length && rule[i] == '=')
								{
									i++;
									symbol.Value = CompareExpression.ECompareOperator.LESS_EQUAL;
									goto IL_257;
								}
								symbol.Value = CompareExpression.ECompareOperator.LESS;
								goto IL_257;
							}
							else if (c == '>')
							{
								symbol = new Symbol();
								symbol.Type = RuleExpression.ESymbolType.Operator;
								if (i < rule.Length && rule[i] == '=')
								{
									i++;
									symbol.Value = CompareExpression.ECompareOperator.GREATER_EQUAL;
									goto IL_257;
								}
								symbol.Value = CompareExpression.ECompareOperator.GREATER;
								goto IL_257;
							}
							else
							{
								if (char.IsWhiteSpace(c))
								{
									symbol = null;
									goto IL_257;
								}
								throw new Exception("Unknown character at position " + i);
							}
						}
					}
					string text = c.ToString(CultureInfo.InvariantCulture);
					while (i < rule.Length)
					{
						if (!char.IsLetter(rule[i]) && rule[i] != '_')
						{
							break;
						}
						text += rule[i++].ToString();
					}
					if (text != null)
					{
						if (text == "AND")
						{
							symbol = new Symbol(RuleExpression.ESymbolType.TerminalAnd);
							goto IL_257;
						}
						if (text == "OR")
						{
							symbol = new Symbol(RuleExpression.ESymbolType.TerminalOr);
							goto IL_257;
						}
						if (text == "NOT")
						{
							symbol = new Symbol(RuleExpression.ESymbolType.TerminalNot);
							goto IL_257;
						}
					}
					symbol = new Symbol(RuleExpression.ESymbolType.Value);
					symbol.Value = text;
				}
				IL_257:
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
						if (symbol4.Type == RuleExpression.ESymbolType.Value && symbol3.Type == RuleExpression.ESymbolType.Operator && symbol2.Type == RuleExpression.ESymbolType.Value)
						{
							stack.Push(new Symbol(RuleExpression.ESymbolType.VariableExpression)
							{
								Value = new VariableExpression((string)symbol4.Value, (CompareExpression.ECompareOperator)symbol3.Value, (double)((long)symbol2.Value))
							});
							flag2 = true;
						}
						else if (FaultClassRuleParser.IsExpression(symbol4) && symbol3.Type == RuleExpression.ESymbolType.TerminalAnd && FaultClassRuleParser.IsExpression(symbol2))
						{
							stack.Push(new Symbol(RuleExpression.ESymbolType.AndExpression)
							{
								Value = new AndExpression((RuleExpression)symbol4.Value, (RuleExpression)symbol2.Value)
							});
							flag2 = true;
						}
						else if (FaultClassRuleParser.IsExpression(symbol4) && symbol3.Type == RuleExpression.ESymbolType.TerminalOr && FaultClassRuleParser.IsExpression(symbol2))
						{
							stack.Push(new Symbol(RuleExpression.ESymbolType.OrExpression)
							{
								Value = new OrExpression((RuleExpression)symbol4.Value, (RuleExpression)symbol2.Value)
							});
							flag2 = true;
						}
						else if (symbol3.Type == RuleExpression.ESymbolType.TerminalNot && FaultClassRuleParser.IsExpression(symbol2))
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
						else if (symbol4.Type == RuleExpression.ESymbolType.TerminalLPar && FaultClassRuleParser.IsExpression(symbol3) && symbol2.Type == RuleExpression.ESymbolType.TerminalRPar)
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
			int count = stack.Count;
			if (count == 0)
			{
				return null;
			}
			if (count != 1)
			{
				throw new Exception("Could not completely reduce tokens");
			}
			Symbol symbol6 = stack.Pop();
			if (FaultClassRuleParser.IsExpression(symbol6))
			{
				return (RuleExpression)symbol6.Value;
			}
			throw new Exception("Illegal last token");
		}

		private static bool IsExpression(Symbol op)
		{
			return op.Type == RuleExpression.ESymbolType.VariableExpression || op.Type == RuleExpression.ESymbolType.AndExpression || op.Type == RuleExpression.ESymbolType.OrExpression || op.Type == RuleExpression.ESymbolType.NotExpression;
		}
	}
}
