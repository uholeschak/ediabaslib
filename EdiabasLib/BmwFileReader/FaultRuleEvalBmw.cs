using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using EdiabasLib;
using Mono.CSharp;

namespace BmwFileReader
{
    public class FaultRuleEvalBmw
    {
        public object RuleObject { get; private set; }
        private Dictionary<string, string> _propertiesDict = new Dictionary<string, string>();
        private HashSet<string> _unknownNamesHash = new HashSet<string>();

        public FaultRuleEvalBmw()
        {
            RuleObject = null;
        }

        public bool CreateRuleEvaluators(List<VehicleStructsBmw.FaultRuleInfo> faultRuleInfoList, out string errorMessage)
        {
            RuleObject = null;

            errorMessage = string.Empty;
            StringWriter reportWriter = new StringWriter();
            try
            {
                Evaluator evaluator = new Evaluator(new CompilerContext(new CompilerSettings(), new ConsoleReportPrinter(reportWriter)));
                evaluator.ReferenceAssembly(Assembly.GetExecutingAssembly());
                StringBuilder sb = new StringBuilder();
                sb.Append(
@"using BmwFileReader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

public class RuleEval
{
    public FaultRuleEvalBmw FaultRuleEvalClass { get; set; }

    public RuleEval()
    {
    }

    public bool IsRuleValid(string id)
    {
        switch (id.Trim())
        {
");
                foreach (VehicleStructsBmw.FaultRuleInfo faultRuleInfo in faultRuleInfoList)
                {
                    sb.Append(
$@"         case ""{faultRuleInfo.Id.Trim()}"":
                return {faultRuleInfo.RuleFormula};
"
                    );
                }
                sb.Append(
@"
        }

        return true;
    }

    private string RuleString(string name)
    {
        if (FaultRuleEvalClass != null)
        {
            return FaultRuleEvalClass.RuleString(name);
        }
        return string.Empty;
    }

    private long RuleNum(string name)
    {
        if (FaultRuleEvalClass != null)
        {
            return FaultRuleEvalClass.RuleNum(name);
        }
        return -1;
    }

    private bool IsValidRuleString(string name, string value)
    {
        if (FaultRuleEvalClass != null)
        {
            return FaultRuleEvalClass.IsValidRuleString(name, value);
        }
        return false;
    }

    private bool IsValidRuleNum(string name, long value)
    {
        if (FaultRuleEvalClass != null)
        {
            return FaultRuleEvalClass.IsValidRuleNum(name, value);
        }
        return false;
    }
}
");
                evaluator.Compile(sb.ToString());
                object ruleObject = evaluator.Evaluate("new RuleEval()");
                if (ruleObject == null)
                {
                    return false;
                }

                Type ruleType = ruleObject.GetType();
                PropertyInfo propertyFaultRuleEvalClass = ruleType.GetProperty("FaultRuleEvalClass");
                if (propertyFaultRuleEvalClass != null)
                {
                    propertyFaultRuleEvalClass.SetValue(ruleObject, this);
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

        public bool ExecuteRuleEvaluator(string id)
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

                _unknownNamesHash.Clear();
                // ReSharper disable once UsePatternMatching
                object[] args = { id };
                bool? valid = methodIsRuleValid.Invoke(RuleObject, args) as bool?;

                if (_unknownNamesHash.Count > 0)
                {
                    return true;
                }

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

        public void SetEvalProperties(string brand, string vehicleSeries, string iStep)
        {
            _propertiesDict.Clear();
            if (!string.IsNullOrEmpty(brand))
            {
                _propertiesDict.Add("MARKE", brand);
            }
            if (!string.IsNullOrEmpty(vehicleSeries))
            {
                _propertiesDict.Add("E-BEZEICHNUNG", vehicleSeries);
            }
            if (!string.IsNullOrEmpty(iStep))
            {
                _propertiesDict.Add("ISTUFE", iStep);
            }
        }

        public string RuleString(string name)
        {
            string propertyString = GetPropertyString(name);
            if (string.IsNullOrWhiteSpace(propertyString))
            {
                return string.Empty;
            }
            return propertyString;
        }

        public long RuleNum(string name)
        {
            long? propertyValue = GetPropertyNum(name);
            if (!propertyValue.HasValue)
            {
                return -1;
            }

            return propertyValue.Value;
        }

        public bool IsValidRuleString(string name, string value)
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

        public bool IsValidRuleNum(string name, long value)
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

            _unknownNamesHash.Add(key);
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
