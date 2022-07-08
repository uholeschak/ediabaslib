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
        private readonly Dictionary<string, List<string>> _propertiesDict = new Dictionary<string, List<string>>();
        private readonly HashSet<string> _unknownNamesHash = new HashSet<string>();
        private string _unknownId;

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

        RuleNotFound(id.Trim());
        return true;
    }

    private void RuleNotFound(string id)
    {
        if (FaultRuleEvalClass != null)
        {
            FaultRuleEvalClass.RuleNotFound(id);
        }
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
                _unknownId = null;
                // ReSharper disable once UsePatternMatching
                object[] args = { id };
                bool? valid = methodIsRuleValid.Invoke(RuleObject, args) as bool?;

                if (_unknownId != null)
                {
                    return true;
                }

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

        public bool SetEvalProperties(DetectVehicleBmw detectVehicleBmw, EcuFunctionStructs.EcuVariant ecuVariant)
        {
            try
            {
                _propertiesDict.Clear();

                if (detectVehicleBmw == null || !detectVehicleBmw.Valid)
                {
                    return false;
                }

                if (detectVehicleBmw.BrandList != null && detectVehicleBmw.BrandList.Count > 0)
                {
                    _propertiesDict.Add("Marke".ToUpperInvariant(), detectVehicleBmw.BrandList);
                }

                if (!string.IsNullOrWhiteSpace(detectVehicleBmw.TypeKey))
                {
                    _propertiesDict.Add("Typschl\u00FCssel".ToUpperInvariant(), new List<string> { detectVehicleBmw.TypeKey.Trim() });
                }

                if (!string.IsNullOrWhiteSpace(detectVehicleBmw.Series))
                {
                    _propertiesDict.Add("E-Bezeichnung".ToUpperInvariant(), new List<string> { detectVehicleBmw.Series.Trim() });
                }

                if (!string.IsNullOrWhiteSpace(detectVehicleBmw.ConstructYear))
                {
                    string constructDate = detectVehicleBmw.ConstructYear;
                    if (!string.IsNullOrWhiteSpace(detectVehicleBmw.ConstructMonth))
                    {
                        constructDate += detectVehicleBmw.ConstructMonth;
                    }
                    else
                    {
                        constructDate += "01";
                    }
                    _propertiesDict.Add("Baustand".ToUpperInvariant(), new List<string> { constructDate });

                    string productionDate = constructDate;
                    if (!string.IsNullOrWhiteSpace(detectVehicleBmw.ILevelShip))
                    {
                        string iLevelTrim = detectVehicleBmw.ILevelShip.Trim();
                        string[] levelParts = iLevelTrim.Split("-");
                        if (levelParts.Length == 4)
                        {
                            if (Int32.TryParse(levelParts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int iLevelYear) &&
                                Int32.TryParse(levelParts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int iLevelMonth))
                            {
                                int dateValue = (iLevelYear + 2000) * 100 + iLevelMonth;
                                productionDate = dateValue.ToString(CultureInfo.InvariantCulture);
                            }
                        }
                    }

                    _propertiesDict.Add("Produktionsdatum".ToUpperInvariant(), new List<string> { productionDate });
                }

                if (!string.IsNullOrWhiteSpace(detectVehicleBmw.ILevelCurrent))
                {
                    string iLevelTrim = detectVehicleBmw.ILevelCurrent.Trim();
                    string[] levelParts = iLevelTrim.Split("-");
                    _propertiesDict.Add("IStufe".ToUpperInvariant(), new List<string> { iLevelTrim.Trim() });
                    if (levelParts.Length == 4 && iLevelTrim.Length == 14)
                    {
                        string iLevelNum = levelParts[1] + levelParts[2] + levelParts[3];
                        if (Int32.TryParse(iLevelNum, NumberStyles.Integer, CultureInfo.InvariantCulture, out int iLevelValue))
                        {
                            _propertiesDict.Add("IStufeX".ToUpperInvariant(), new List<string> { iLevelValue.ToString(CultureInfo.InvariantCulture) });
                        }

                        _propertiesDict.Add("Baureihenverbund".ToUpperInvariant(), new List<string> { levelParts[0] });
                    }
                }

                if (ecuVariant != null)
                {
                    string repsName = ecuVariant.EcuClique?.EcuRepsName;
                    if (!string.IsNullOrEmpty(repsName))
                    {
                        _propertiesDict.Add("EcuRepresentative".ToUpperInvariant(), new List<string> { repsName });
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void RuleNotFound(string id)
        {
            _unknownId = id;
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
            long? propertyValue = GetPropertyValue(name);
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

            List<string> propertyStrings = GetPropertyStrings(name);
            if (propertyStrings == null)
            {
                return false;
            }

            foreach (string propertyString in propertyStrings)
            {
                if (string.Compare(propertyString.Trim(), value.Trim(), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsValidRuleNum(string name, long value)
        {
            List<long> propertyValues = GetPropertyValues(name);
            if (propertyValues == null)
            {
                return false;
            }

            foreach (long propertyValue in propertyValues)
            {
                if (propertyValue == value)
                {
                    return true;
                }
            }

            return false;
        }

        private string GetPropertyString(string name)
        {
            List<string> stringList = GetPropertyStrings(name);
            if (stringList != null && stringList.Count > 0)
            {
                return stringList[0];
            }

            return string.Empty;
        }

        private List<string> GetPropertyStrings(string name)
        {
            if (_propertiesDict == null)
            {
                return null;
            }

            string key = name.Trim().ToUpperInvariant();
            if (_propertiesDict.TryGetValue(key, out List<string> valueList))
            {
                return valueList;
            }

            _unknownNamesHash.Add(key);
            return null;
        }

        private long? GetPropertyValue(string name)
        {
            List<long> propertyValues = GetPropertyValues(name);
            if (propertyValues != null && propertyValues.Count > 0)
            {
                return propertyValues[0];
            }

            return null;
        }

        private List<long> GetPropertyValues(string name)
        {
            List<string> propertyStrings = GetPropertyStrings(name);
            if (propertyStrings == null)
            {
                return null;
            }

            List<long> valueList = new List<long>();
            foreach (string propertyString in propertyStrings)
            {
                if (long.TryParse(propertyString, NumberStyles.Integer, CultureInfo.InvariantCulture, out long result))
                {
                    if (!valueList.Contains(result))
                    {
                        valueList.Add(result);
                    }
                }
            }

            return valueList;
        }
    }
}
