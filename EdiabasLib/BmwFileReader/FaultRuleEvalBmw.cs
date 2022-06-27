using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using EdiabasLib;
using Mono.CSharp;

namespace BmwFileReader
{
    public class FaultRuleEvalBmw
    {
        public delegate string RuleStringDelegate(string name);
        public delegate long RuleNumDelegate(string name);
        public delegate bool IsValidRuleStringDelegate(string name, string value);
        public delegate bool IsValidRuleNumDelegate(string name, long value);

        public object RuleObject { get; private set; }
        private Dictionary<string, string> _propertiesDict;

        public FaultRuleEvalBmw()
        {
            RuleObject = null;
        }

        public bool CreateRuleEvaluator(VehicleStructsBmw.FaultRuleInfo faultRuleInfo, out string errorMessage)
        {
            RuleObject = null;

            errorMessage = string.Empty;
            StringWriter reportWriter = new StringWriter();
            try
            {
                Evaluator evaluator = new Evaluator(new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter(reportWriter)));
                string evalCode =
$@"using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

public class RuleEval
{{
    public delegate string RuleStringDelegate(string name);
    public delegate long RuleNumDelegate(string name);
    public delegate bool IsValidRuleStringDelegate(string name, string value);
    public delegate bool IsValidRuleNumDelegate(string name, long value);

    public RuleStringDelegate RuleStringFunc {{ get; set; }}
    public RuleNumDelegate RuleNumFunc {{ get; set; }}
    public IsValidRuleStringDelegate IsValidRuleStringFunc {{ get; set; }}
    public IsValidRuleNumDelegate IsValidRuleNumFunc {{ get; set; }}

    public RuleEval()
    {{
    }}

    public bool IsRuleValid()
    {{
        return {faultRuleInfo.RuleFormula};
    }}

    private string RuleString(string name)
    {{
        if (RuleStringFunc != null)
        {{
            return RuleStringFunc(name);
        }}
        return string.Empty;
    }}

    private long RuleNum(string name)
    {{
        if (RuleNumFunc != null)
        {{
            return RuleNumFunc(name);
        }}
        return -1;
    }}

    private bool IsValidRuleString(string name, string value)
    {{
        if (IsValidRuleStringFunc != null)
        {{
            return IsValidRuleStringFunc(name, value);
        }}
        return false;
    }}

    private bool IsValidRuleNum(string name, long value)
    {{
        if (IsValidRuleNumFunc != null)
        {{
            return IsValidRuleNumFunc(name, value);
        }}
        return false;
    }}
}}
";
                evaluator.Compile(evalCode);
                object ruleObject = evaluator.Evaluate("new RuleEval()");
                if (ruleObject == null)
                {
                    return false;
                }

                Type ruleType = ruleObject.GetType();
                PropertyInfo propertyRuleString = ruleType.GetProperty("RuleStringFunc");
                if (propertyRuleString != null)
                {
                    propertyRuleString.SetValue(ruleObject, new RuleStringDelegate(RuleString));
                }

                PropertyInfo propertyRuleNum = ruleType.GetProperty("RuleNumFunc");
                if (propertyRuleNum != null)
                {
                    propertyRuleNum.SetValue(ruleObject, new RuleNumDelegate(RuleNum));
                }

                PropertyInfo propertyIsValidRuleString = ruleType.GetProperty("IsValidRuleStringFunc");
                if (propertyIsValidRuleString != null)
                {
                    propertyIsValidRuleString.SetValue(ruleObject, new IsValidRuleStringDelegate(IsValidRuleString));
                }

                PropertyInfo propertyIsValidRuleNum = ruleType.GetProperty("IsValidRuleNumFunc");
                if (propertyIsValidRuleNum != null)
                {
                    propertyIsValidRuleNum.SetValue(ruleObject, new IsValidRuleNumDelegate(IsValidRuleNum));
                }

                RuleObject = ruleObject;

                return true;
            }
            catch (Exception ex)
            {
                RuleObject = null;
                errorMessage = reportWriter.ToString();
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = EdiabasNet.GetExceptionText(ex);
                }
                return false;
            }
        }

        public bool ExecuteRuleEvaluator(Dictionary<string, string> propertiesDict)
        {
            if (RuleObject == null)
            {
                return false;
            }

            try
            {
                Type ruleType = RuleObject.GetType();
                MethodInfo methodIsRuleValid = ruleType.GetMethod("IsRuleValid");
                if (methodIsRuleValid == null)
                {
                    return false;
                }

                _propertiesDict = propertiesDict;
                // ReSharper disable once UsePatternMatching
                bool? valid = methodIsRuleValid.Invoke(RuleObject, null) as bool?;
                _propertiesDict = null;
                if (!valid.HasValue)
                {
                    return false;
                }

                return valid.Value;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string RuleString(string name)
        {
            string propertyString = GetPropertyString(name);
            if (string.IsNullOrWhiteSpace(propertyString))
            {
                return string.Empty;
            }
            return propertyString;
        }

        private long RuleNum(string name)
        {
            long? propertyValue = GetPropertyNum(name);
            if (!propertyValue.HasValue)
            {
                return -1;
            }

            return propertyValue.Value;
        }

        private bool IsValidRuleString(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string propertyString = GetPropertyString(name);
            if (string.IsNullOrWhiteSpace(propertyString))
            {
                return false;
            }

            if (string.Compare(propertyString, value.Trim(), StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }

            return false;
        }

        private bool IsValidRuleNum(string name, long value)
        {
            long? propertyValue = GetPropertyNum(name);
            if (!propertyValue.HasValue)
            {
                return false;
            }

            if (propertyValue.Value == value)
            {
                return true;
            }

            return false;
        }

        private string GetPropertyString(string name)
        {
            if (_propertiesDict == null)
            {
                return string.Empty;
            }

            string key = name.Trim().ToUpperInvariant();
            if (_propertiesDict.TryGetValue(key, out string value))
            {
                return value;
            }
            return string.Empty;
        }

        private long? GetPropertyNum(string name)
        {
            string valueString = GetPropertyString(name);
            if (string.IsNullOrWhiteSpace(valueString))
            {
                return null;
            }

            if (long.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out long result))
            {
                return result;
            }

            return null;
        }
    }
}
