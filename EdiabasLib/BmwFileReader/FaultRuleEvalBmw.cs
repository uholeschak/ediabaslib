using System;
using System.IO;
using System.Reflection;
using System.Text;
using Android.Views;
using EdiabasLib;
using Mono.CSharp;

namespace BmwFileReader
{
    public class FaultRuleEvalBmw
    {
        public FaultRuleEvalBmw()
        {
        }

        public object CreateRuleEvaluator(VehicleStructsBmw.FaultRuleInfo faultRuleInfo, out string errorMessage)
        {
            errorMessage = string.Empty;
            StringWriter reportWriter = new StringWriter();
            try
            {
                Evaluator evaluator = new Evaluator(new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter(reportWriter)));
                evaluator.ReferenceAssembly(Assembly.GetExecutingAssembly());
                evaluator.ReferenceAssembly(typeof(EdiabasNet).Assembly);
                string evalCode =
$@"using EdiabasLib;
using BmwDeepObd;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

public class RuleEval
{{
    public RuleEval()
    {{
    }}

    public bool IsRuleValid()
    {{
        return {faultRuleInfo.RuleFormula};
    }}

    private string RuleString(string name)
    {{
        return string.Empty;
    }}

    private long RuleNum(string name)
    {{
        return -1;
    }}

    private bool IsValidRuleString(string name, string value)
    {{
        return false;
    }}

    private bool IsValidRuleNum(string name, long value)
    {{
        return false;
    }}
}}
";
                evaluator.Compile(evalCode);
                object ruleObject = evaluator.Evaluate("new RuleEval()");

                return ruleObject;
            }
            catch (Exception ex)
            {
                errorMessage = reportWriter.ToString();
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = EdiabasNet.GetExceptionText(ex);
                }
                return null;
            }
        }
    }
}
