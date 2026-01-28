using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using PsdzClient.Utility;

namespace PsdzClient.Core.Container
{
    public class ECUJob : ECUJobAbstract, IEcuJob, INotifyPropertyChanged
    {
        private bool fastaRelevant;
        private readonly decimal hashValue;
        private readonly IProtocolBasic fastaservice;
        private bool ana09Protocolled;
        public bool FASTARelevant
        {
            get
            {
                return fastaRelevant;
            }

            set
            {
                if (value != fastaRelevant)
                {
                    fastaRelevant = value;
                    RaisePropertyChanged("FASTARelevant");
                }
            }
        }

        [XmlIgnore]
        public virtual int NrForJobResultSets => base.JobResultSets;

        public ECUJob()
        {
            hashValue = Environment.TickCount;
        }

        public ECUJob(IProtocolBasic fastaprotocoller)
        {
            hashValue = Environment.TickCount;
            fastaservice = fastaprotocoller;
        }

        public bool IsDone()
        {
            if (base.JobErrorCode != 0)
            {
                return false;
            }

            if (base.JobResult == null)
            {
                return false;
            }

            if (base.JobResult.Count > 0)
            {
                return true;
            }

            return false;
        }

        public bool IsFASTARelevant(ushort set, string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            if (base.JobResult != null)
            {
                IEcuResult ecuResult = base.JobResult.FirstOrDefault((IEcuResult item) => item.Set == set && string.Equals(item.Name, resultName, StringComparison.OrdinalIgnoreCase));
                if (ecuResult != null)
                {
                    return ecuResult.FASTARelevant;
                }
            }

            return false;
        }

        public bool IsJobState(ushort set, string state)
        {
            if (IsDone())
            {
                return getStringResult(set, "JOB_STATUS") == state;
            }

            return false;
        }

        public bool IsJobState(string state)
        {
            if (IsDone())
            {
                return getResultsAs<string>("JOB_STATUS") == state;
            }

            return false;
        }

        public bool IsNullOrEmpty(ushort set, string resultName)
        {
            try
            {
                object result = getResult(set, resultName);
                if (result == null)
                {
                    return true;
                }

                if (result is string value)
                {
                    return string.IsNullOrEmpty(value);
                }

                if (result is byte[] array)
                {
                    return array.Length < 0;
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUJob.IsNullOrEmpty(ushort,string)", exception);
            }

            return false;
        }

        public bool IsNullOrEmpty(string resultName)
        {
            try
            {
                object result = getResult(resultName);
                if (result == null)
                {
                    return true;
                }

                if (result is string value)
                {
                    return string.IsNullOrEmpty(value);
                }

                if (result is byte[] array)
                {
                    return array.Length < 0;
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUJob.IsNullOrEmpty(string)", exception);
                return false;
            }

            return false;
        }

        public virtual bool IsOkay()
        {
            if (IsDone())
            {
                return getResultsAs<string>("JOB_STATUS") == "OKAY";
            }

            return false;
        }

        public bool IsOkay(ushort set)
        {
            if (IsDone())
            {
                return getResultsAs<string>("JOB_STATUS", null, set) == "OKAY";
            }

            return false;
        }

        public override string ToString()
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (string.IsNullOrEmpty(base.EcuName))
                {
                    return null;
                }

                if (string.IsNullOrEmpty(base.JobName))
                {
                    return null;
                }

                if (base.JobResult != null)
                {
                    foreach (IEcuResult item in base.JobResult)
                    {
                        string arg = string.Empty;
                        if (item.Format != 7)
                        {
                            arg = getResult(item.Set, item.Name).ToString();
                        }
                        else if (getResult(item.Set, item.Name)is byte[] param)
                        {
                            arg = FormatConverter.ByteArray2String(param, item.Length);
                        }

                        stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "Set:{0} {1}={2}\n", item.Set, item.Name, arg));
                    }

                    return stringBuilder.ToString();
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUJob.Dump()", exception);
            }

            return null;
        }

        public byte[] getByteArrayResult(ushort set, string resultName, out uint len)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                if (base.JobResult != null)
                {
                    byte[] resultsAs = getResultsAs<byte[]>(resultName, null, set);
                    if (resultsAs != null)
                    {
                        len = (uint)resultsAs.Length;
                    }

                    len = 0u;
                    return resultsAs;
                }

                Log.Warning("ECUJob.getByteArrayResult()", "(set={0},resultName={1}) - JobResult was null.", set, resultName);
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getByteArrayResult()", "({0},{1}) - failed with exception {2}", set, resultName, ex.ToString());
            }

            len = 0u;
            return null;
        }

        public byte[] getByteArrayResult(string resultName, out uint len)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                if (base.JobResult != null)
                {
                    byte[] resultsAs = getResultsAs<byte[]>(resultName);
                    if (resultsAs == null)
                    {
                        len = 0u;
                        return null;
                    }

                    len = (uint)resultsAs.Length;
                    return resultsAs;
                }

                Log.Warning("ECUJob.getByteArrayResult()", "JobResult was null for resultName: {0} ", resultName);
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getByteArrayResult()", "failed for result: {0} with exception {1}", resultName, ex.ToString());
            }

            len = 0u;
            return null;
        }

        public byte? getByteResult(ushort set, string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                if (base.JobResult != null)
                {
                    foreach (IEcuResult item in base.JobResult.Where((IEcuResult result) => result.Set == set && string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (item.Format == 1)
                        {
                            return (byte)item.Value;
                        }

                        Log.Warning("ECUJob.getByteResult()", "(set={0},resultName={1}) has different format type!!! You selected Byte but should be:{2}", set, resultName, ECUKom.APIFormatName(item.Format));
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getByteResult()", "(set={0},resultName={1}) - JobResult was null.", set, resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getByteResult()", "({0},{1}) - failed with exception {2}", set, resultName, ex.ToString());
            }

            return null;
        }

        public byte? getByteResult(string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                if (base.JobResult != null)
                {
                    foreach (IEcuResult item in base.JobResult.Where((IEcuResult result) => string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (item.Format == 1)
                        {
                            return (byte)item.Value;
                        }

                        Log.Warning("ECUJob.getByteResult()", "(resultName={0}) has different format type!!! You selected Byte but should be:{1}", resultName, ECUKom.APIFormatName(item.Format));
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getByteResult()", "(resultName={0}) - JobResult was null.", resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getByteResult()", "({0}) - failed with exception {1}", resultName, ex.ToString());
            }

            return null;
        }

        public object getISTAResult(string resultName)
        {
            try
            {
                if (!(resultName == "/Result/Status/JOB_STATUS"))
                {
                    if (resultName == "/Result/Rows/$Count")
                    {
                        if (base.JobResultSets > 0)
                        {
                            return base.JobResultSets;
                        }

                        return 0;
                    }

                    if (resultName.Contains('['))
                    {
                        Match match = Regex.Match(resultName, "\\[\\d+\\]");
                        if (match.Success)
                        {
                            int num = Convert.ToInt32(match.Value.Replace("[", string.Empty).Replace("]", string.Empty));
                            string[] array = Regex.Split(resultName, string.Format(CultureInfo.InvariantCulture, "/Row\\[{0}\\]/", num));
                            if (array.Length == 2)
                            {
                                return getResult((ushort)(num + 1), array[1]);
                            }

                            Log.Warning("ECUJob.getISTAResult()", "failed to separate result name from result path: {0}", resultName);
                            return null;
                        }

                        Log.Warning("ECUJob.getISTAResult()", "failed to evaluate query: {0}", resultName);
                    }
                    else if (resultName.StartsWith("/Result/Status/", StringComparison.Ordinal))
                    {
                        string text = resultName.Substring(15);
                        if (!string.IsNullOrEmpty(text))
                        {
                            object result = getResult(text);
                            if (result == null)
                            {
                                Log.Warning("ECUJob.getISTAResult()", "obj was null when query for {0}; guess your testmodule will die... cross your fingers", resultName);
                            }
                            else
                            {
                                Log.Info("ECUJob.getISTAResult()", "obj type is {0}", result.GetType().ToString());
                            }

                            return result;
                        }
                    }
                    else
                    {
                        Log.Warning("ECUJob.getISTAResult()", "failed to evaluate query: {0}", resultName);
                    }

                    return getResult(resultName);
                }

                return getResult("JOB_STATUS");
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUJob.getISTAResult()", exception);
            }

            return null;
        }

        public T getISTAResultAs<T>(string resultName)
        {
            try
            {
                Type typeFromHandle = typeof(T);
                if (!string.IsNullOrEmpty(resultName))
                {
                    object iSTAResult = getISTAResult(resultName);
                    if (iSTAResult == null)
                    {
                        Log.Error("ECUJob.getISTAResultAs(string resultName)", "obj was null when query for {0}; guess your testmodule will die... cross your fingers", resultName);
                        return default(T);
                    }

                    Log.Info("ECUJob.getISTAResultAs(string resultName)", "obj type is {0} targetType is: {1}", iSTAResult.GetType().ToString(), typeFromHandle.ToString());
                    try
                    {
                        if (iSTAResult.GetType() != typeFromHandle)
                        {
                            return (T)Convert.ChangeType(iSTAResult, typeFromHandle, CultureInfo.InvariantCulture);
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.WarningException("ECUJob.getISTAResultAs()", exception);
                        string text = iSTAResult.ToString();
                        if (!string.IsNullOrEmpty(text))
                        {
                            string[] array = text.Split(' ');
                            if (array.Length != 0)
                            {
                                if (!(typeFromHandle == typeof(bool)))
                                {
                                    return (T)Convert.ChangeType(array[0], typeFromHandle, CultureInfo.InvariantCulture);
                                }

                                if ("wahr".Equals(array[0], StringComparison.OrdinalIgnoreCase) || "true".Equals(array[0], StringComparison.OrdinalIgnoreCase) || "1".Equals(array[0], StringComparison.OrdinalIgnoreCase))
                                {
                                    return (T)(object)true;
                                }

                                if ("unwahr".Equals(array[0], StringComparison.OrdinalIgnoreCase) || "false".Equals(array[0], StringComparison.OrdinalIgnoreCase) || "falsch".Equals(array[0], StringComparison.OrdinalIgnoreCase) || "0".Equals(array[0], StringComparison.OrdinalIgnoreCase))
                                {
                                    return (T)(object)false;
                                }
                            }
                        }
                    }

                    return (T)iSTAResult;
                }

                return default(T);
            }
            catch (Exception exception2)
            {
                Log.WarningException("ECUJob.getISTAResultAs(string resultName)", exception2);
            }

            return default(T);
        }

        public object getISTAResultAsType(string resultName, Type targetType)
        {
            try
            {
                if (!(resultName == "/Result/Status/JOB_STATUS"))
                {
                    if (resultName == "/Result/Rows/$Count")
                    {
                        if (base.JobResultSets > 0)
                        {
                            return base.JobResultSets;
                        }

                        return 0;
                    }

                    if (resultName.Contains('['))
                    {
                        Match match = Regex.Match(resultName, "\\[\\d+\\]");
                        if (match.Success)
                        {
                            int num = Convert.ToInt32(match.Value.Replace("[", string.Empty).Replace("]", string.Empty));
                            string[] array = Regex.Split(resultName, $"/Row\\[{num}\\]/");
                            if (array.Length == 2)
                            {
                                object result = getResult((ushort)(num + 1), array[1]);
                                if (result == null)
                                {
                                    Log.Warning("ECUJob.getISTAResultAsType()", "obj was null when query for {0}; guess your testmodule will die... cross your fingers", resultName);
                                    return null;
                                }

                                if (result.GetType() != targetType)
                                {
                                    string text = result.GetType().ToString();
                                    string text2 = targetType.ToString();
                                    Log.Info("ECUJob.getISTAResultAsType(string, Type)", "result: {0} obj type is {1} targetType is: {2}", resultName, text, text2);
                                }

                                try
                                {
                                    if (result.GetType() != targetType)
                                    {
                                        return Convert.ChangeType(result, targetType, CultureInfo.InvariantCulture);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Log.WarningException("ECUJob.getISTAResultAsType()", exception);
                                    string text3 = result.ToString();
                                    if (!string.IsNullOrEmpty(text3))
                                    {
                                        string[] array2 = text3.Split(' ');
                                        if (array2.Length != 0)
                                        {
                                            if (!(targetType == typeof(bool)))
                                            {
                                                Log.Info("ECUJob.getISTAResultAsType()", "found multipart string; trying conversion with the first part");
                                                return Convert.ChangeType(array2[0], targetType, CultureInfo.InvariantCulture);
                                            }

                                            if ("wahr".Equals(array2[0], StringComparison.OrdinalIgnoreCase) || "true".Equals(array2[0], StringComparison.OrdinalIgnoreCase) || "1".Equals(array2[0], StringComparison.OrdinalIgnoreCase))
                                            {
                                                return true;
                                            }

                                            if ("unwahr".Equals(array2[0], StringComparison.OrdinalIgnoreCase) || "false".Equals(array2[0], StringComparison.OrdinalIgnoreCase) || "falsch".Equals(array2[0], StringComparison.OrdinalIgnoreCase) || "0".Equals(array2[0], StringComparison.OrdinalIgnoreCase))
                                            {
                                                return false;
                                            }
                                        }
                                    }
                                }

                                if (result is string && Regex.Match(resultName, "STAT.*TEXT").Success)
                                {
                                    Log.Warning("ECUJob.getISTAResultAsType()", "found STAT_*_TEXT string construct for result: {0}; force character conversion to enable bogus testmodules", resultName);
                                    return FormatConverter.Ascii2UTF8(result);
                                }

                                return result;
                            }

                            Log.Warning("ECUJob.getISTAResultAsType()", "failed to separate result name from result path: {0}", resultName);
                            return null;
                        }

                        Log.Warning("ECUJob.getISTAResultAsType()", "failed to evaluate query: {0}", resultName);
                    }
                    else if (resultName.StartsWith("/Result/Status/", StringComparison.Ordinal))
                    {
                        string text4 = resultName.Substring(15);
                        if (!string.IsNullOrEmpty(text4))
                        {
                            object result2 = getResult(text4);
                            if (result2 == null)
                            {
                                Log.Warning("ECUJob.getISTAResultAsType()", "obj was null when query for {0}; guess your testmodule will die... cross your fingers", resultName);
                                return null;
                            }

                            if (result2.GetType() != targetType)
                            {
                                Log.Info("ECUJob.getISTAResultAsType()", "result: {0} obj type is {1} targetType is: {2}", resultName, result2.GetType().ToString(), targetType.ToString());
                            }

                            if (result2.GetType() != targetType)
                            {
                                return Convert.ChangeType(result2, targetType, CultureInfo.InvariantCulture);
                            }

                            return result2;
                        }
                    }
                    else
                    {
                        Log.Warning("ECUJob.getISTAResultAsType()", "failed to evaluate query: {0}", resultName);
                    }

                    return getResult(resultName);
                }

                return getResult("JOB_STATUS");
            }
            catch (Exception exception2)
            {
                Log.WarningException("ECUJob.getISTAResultAsType()", exception2);
            }

            return null;
        }

        public object getResult(ushort set, string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                if (base.JobResult != null)
                {
                    IEcuResult ecuResult = base.JobResult.FirstOrDefault((IEcuResult item) => item.Set == set && string.Equals(item.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    if (ecuResult != null)
                    {
                        return ecuResult.Value;
                    }

                    Log.Warning("ECUJob.getResult()", "({0},{1}) - result not found in JobResult list.", set, resultName);
                }
                else
                {
                    Log.Warning("ECUJob.getResult()", "({0},{1}) - JobResult was null.", set, resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getResult()", "({0},{1}) - failed with exception {2}", set, resultName, ex.ToString());
            }

            return null;
        }

        public object getResult(string resultName, bool getLast = false)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                if (base.JobResult != null)
                {
                    IEcuResult ecuResult = (getLast ? base.JobResult.LastOrDefault((IEcuResult item) => string.Equals(item.Name, resultName, StringComparison.OrdinalIgnoreCase)) : base.JobResult.FirstOrDefault((IEcuResult item) => string.Equals(item.Name, resultName, StringComparison.OrdinalIgnoreCase)));
                    if (ecuResult != null)
                    {
                        return ecuResult.Value;
                    }

                    Log.Warning("ECUJob.getResult()", "no matching result found for result name: {0}", resultName);
                }
                else
                {
                    Log.Warning("ECUJob.getResult()", "JobResult was null for result name: {0}", resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getResult()", "failed for result name: {0} with exception {1}", resultName, ex.ToString());
            }

            return null;
        }

        public T getResultAs<T>(ushort set, string resultName, T defaultRes = default(T))
        {
            try
            {
                Type typeFromHandle = typeof(T);
                if (!string.IsNullOrEmpty(resultName))
                {
                    object result = getResult(set, resultName);
                    if (result == null)
                    {
                        Log.Warning("ECUJob.getResultAs(ushort set, string resultName)", "obj was null when query for {0}; guess your testmodule will die... cross your fingers", resultName);
                        return defaultRes;
                    }

                    Log.Info("ECUJob.getResultAs(ushort set, string resultName)", "obj type is {0} targetType is: {1}", result.GetType().ToString(), typeFromHandle.ToString());
                    if (result.GetType() != typeFromHandle)
                    {
                        return (T)Convert.ChangeType(result, typeFromHandle, CultureInfo.InvariantCulture);
                    }

                    return (T)result;
                }

                return defaultRes;
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUJob.getISTAResult(ushort set, string resultName)", exception);
                return defaultRes;
            }
        }

        public T getResultAs<T>(string resultName, T defaultRes = default(T), bool getLast = false)
        {
            try
            {
                Type typeFromHandle = typeof(T);
                if (!string.IsNullOrEmpty(resultName))
                {
                    object result = getResult(resultName, getLast);
                    if (result == null)
                    {
                        Log.Error("ECUJob.getResultAs(string resultName)", "obj was null when query for {0}; guess your testmodule will die... cross your fingers", resultName);
                        return defaultRes;
                    }

                    if (result.GetType() != typeFromHandle)
                    {
                        Log.Info("ECUJob.getISTAResultAs(string resultName)", "result: {0} obj type is {1} targetType is: {2}", resultName, result.GetType().ToString(), typeFromHandle.ToString());
                    }

                    if (result.GetType() != typeFromHandle)
                    {
                        return (T)Convert.ChangeType(result, typeFromHandle, CultureInfo.InvariantCulture);
                    }

                    return (T)result;
                }

                return defaultRes;
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUJob.getISTAResultAs(string resultName)", exception);
                return defaultRes;
            }
        }

        public T getResultsAs<T>(string resultName, T defaultRes = default(T), int set = -1)
        {
            if (base.JobResult == null || base.JobResult.Count == 0)
            {
                return defaultRes;
            }

            if (!typeof(T).IsArray)
            {
                return GetResultsAsClassic(resultName, defaultRes, set);
            }

            object obj = null;
            try
            {
                if (string.IsNullOrEmpty(resultName))
                {
                    Log.Warning("ECUJob.getResultsAs(string resultName, T defaultRes, int set)", "resultName is null! returning default value.");
                    return defaultRes;
                }

                bool flag = set <= -2;
                bool flag2 = set == -1;
                int relevantSet = set;
                int num = resultName.Length - resultName.Replace("[]", "-").Length;
                bool flag3 = num > 0;
                bool flag4 = false;
                Log.Debug("ECUJob.getResultsAs(string resultName, T defaultRes, int set)", "ECUJob.getResultsAs(string resultName, T defaultRes, int set) -> result: {0}, set: {1}, fields: {2}", resultName, set, num);
                Dictionary<IEcuResult, List<int>> dictionary = new Dictionary<IEcuResult, List<int>>();
                List<int> list = new List<int>();
                if (flag)
                {
                    list.Add(base.JobResultSets);
                }

                Regex regex = new Regex("^" + resultName.Replace("[].", "[(\\d+)]\\.").Replace("[", "\\[").Replace("]", "\\]") + "$", RegexOptions.IgnoreCase);
                foreach (IEcuResult item in base.JobResult)
                {
                    Match match = regex.Match(item.Name);
                    if (!match.Success)
                    {
                        continue;
                    }

                    if (flag2)
                    {
                        if (item.Set < relevantSet)
                        {
                            continue;
                        }

                        relevantSet = item.Set;
                    }
                    else if (!flag && item.Set != set)
                    {
                        continue;
                    }

                    bool flag5 = false;
                    List<int> list2 = new List<int>();
                    if (flag)
                    {
                        int num2 = item.Set - 1;
                        if (num2 < 0)
                        {
                            flag5 = true;
                        }

                        list2.Add(num2);
                        num2++;
                    }

                    if (flag3)
                    {
                        for (int i = 1; i < match.Groups.Count; i++)
                        {
                            string value = match.Groups[i].Value;
                            if (IsDigitsOnly(value))
                            {
                                int num3 = Convert.ToInt32(value);
                                list2.Add(num3);
                                num3++;
                                if (list.Count < list2.Count)
                                {
                                    list.Add(num3);
                                }
                                else if (list[list2.Count - 1] < num3)
                                {
                                    list[list2.Count - 1] = num3;
                                }

                                continue;
                            }

                            flag5 = true;
                            Log.Error("ECUJob.getResultsAs(string resultName, T defaultRes, int set)", "Error during processing jobresult: {0}", item.Name);
                            break;
                        }
                    }

                    if (flag5)
                    {
                        continue;
                    }

                    if (item.Value.GetType() == typeof(byte[]))
                    {
                        flag4 = true;
                        list2.Add(0);
                        int length = (item.Value as Array).Length;
                        if (list.Count < list2.Count)
                        {
                            list.Add(length);
                        }
                        else if (list[list2.Count - 1] < length)
                        {
                            list[list2.Count - 1] = length;
                        }
                    }

                    dictionary[item] = list2;
                }

                if (dictionary.Count == 0)
                {
                    Log.Warning("ECUJob.getResultsAs(string resultName, T defaultRes, int set)", $"No job results with name {resultName} found for set {set}. Returning default result.");
                    return defaultRes;
                }

                Type typeFromHandle = typeof(T);
                Type type = null;
                bool flag6 = false;
                if (typeFromHandle.BaseType == typeof(Array))
                {
                    List<int> list3 = new List<int>();
                    type = typeFromHandle.GetElementType();
                    int arrayRank = typeFromHandle.GetArrayRank();
                    if (defaultRes != null)
                    {
                        Array array = defaultRes as Array;
                        for (int j = 0; j < arrayRank; j++)
                        {
                            list3.Add(array.GetLength(j));
                        }

                        if (list3.Count == list.Count)
                        {
                            for (int k = 0; k < arrayRank; k++)
                            {
                                if (list3[k] < list[k])
                                {
                                    flag6 = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            flag6 = true;
                        }
                    }
                    else
                    {
                        flag6 = true;
                    }
                }
                else
                {
                    type = typeFromHandle;
                }

                if (!flag && !flag3)
                {
                    obj = null;
                    IEcuResult ecuResult = dictionary.Keys.FirstOrDefault((IEcuResult item) => item.Set == relevantSet);
                    if (ecuResult != null)
                    {
                        obj = ecuResult.Value;
                    }

                    if (obj == null)
                    {
                        Log.Error("ECUJob.getResultsAs(string resultName, T defaultRes, int set)", "obj was null when query for {0}; guess your testmodule will die... cross your fingers", resultName);
                        return defaultRes;
                    }

                    if (obj.GetType() != typeFromHandle)
                    {
                        if (Nullable.GetUnderlyingType(typeFromHandle) != null)
                        {
                            return (T)Convert.ChangeType(obj, Nullable.GetUnderlyingType(typeFromHandle), CultureInfo.InvariantCulture);
                        }

                        Log.Debug("ECUJob.getResultsAs(string resultName, T defaultRes, int set)", "result: {0} obj type is {1} targetType is: {2}", resultName, obj.GetType().ToString(), typeFromHandle.ToString());
                        return (T)Convert.ChangeType(obj, typeFromHandle, CultureInfo.InvariantCulture);
                    }

                    return (T)obj;
                }

                Array array2 = null;
                if (flag6)
                {
                    Dictionary<Type, object> dictionary2 = new Dictionary<Type, object>
                    {
                        {
                            typeof(bool),
                            false
                        },
                        {
                            typeof(sbyte),
                            sbyte.MaxValue
                        },
                        {
                            typeof(byte),
                            byte.MaxValue
                        },
                        {
                            typeof(short),
                            short.MaxValue
                        },
                        {
                            typeof(ushort),
                            ushort.MaxValue
                        },
                        {
                            typeof(int),
                            int.MaxValue
                        },
                        {
                            typeof(uint),
                            uint.MaxValue
                        },
                        {
                            typeof(long),
                            long.MaxValue
                        },
                        {
                            typeof(ulong),
                            ulong.MaxValue
                        },
                        {
                            typeof(string),
                            null
                        }
                    };
                    array2 = ((!dictionary2.ContainsKey(type)) ? __initArray<T>(list.ToArray(), default(T)) : __initArray<T>(list.ToArray(), dictionary2[type]));
                }

                array2 = array2 ?? (defaultRes as Array);
                foreach (IEcuResult key in dictionary.Keys)
                {
                    if (!flag && relevantSet != key.Set)
                    {
                        continue;
                    }

                    obj = key.Value;
                    if (!flag4)
                    {
                        obj = Convert.ChangeType(obj, type, CultureInfo.InvariantCulture);
                        int[] indices = dictionary[key].ToArray();
                        array2.SetValue(obj, indices);
                        continue;
                    }

                    obj = Convert.ChangeType(obj, typeof(byte[]), CultureInfo.InvariantCulture);
                    int[] array3 = dictionary[key].ToArray();
                    byte[] array4 = obj as byte[];
                    foreach (byte b in array4)
                    {
                        array2.SetValue(b, array3);
                        array3[array3.Length - 1]++;
                    }
                }

                return (T)Convert.ChangeType(array2, typeFromHandle, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Log.WarningException("ECUJob.getResultsAs(string resultName, T defaultRes, int set)", ex);
                string name = typeof(T).Name;
                string actualType = obj?.GetType()?.Name;
                ProtocolGetResultError(ex, name, actualType, resultName);
                return defaultRes;
            }
        }

        private T GetResultsAsClassic<T>(string resultName, T defaultRes = default(T), int set = -1)
        {
            object obj = null;
            try
            {
                if (!typeof(T).IsArray)
                {
                    Type typeFromHandle = typeof(T);
                    Log.Debug("GetResultsAsClassic", "result: {0} requested type is {1} - processing as a classic (simple type) result", resultName, typeFromHandle.ToString());
                    bool isSetRelevant = set > -1;
                    if (set != -1)
                    {
                        _ = set;
                    }
                    else
                    {
                        base.JobResult.Max((IEcuResult res) => res.Set);
                    }

                    IEcuResult ecuResult = base.JobResult.FirstOrDefault((IEcuResult x) => string.Equals(x.Name, resultName, StringComparison.InvariantCultureIgnoreCase) && (!isSetRelevant || x.Set == set));
                    if (ecuResult == null)
                    {
                        return defaultRes;
                    }

                    obj = ecuResult.Value;
                    if (obj.GetType() != typeFromHandle)
                    {
                        if (Nullable.GetUnderlyingType(typeFromHandle) != null)
                        {
                            return (T)Convert.ChangeType(obj, Nullable.GetUnderlyingType(typeFromHandle), CultureInfo.InvariantCulture);
                        }

                        Log.Debug("GetResultsAsClassic", "result: {0} obj type is {1} targetType is: {2}", resultName, obj.GetType().ToString(), typeFromHandle.ToString());
                        return (T)Convert.ChangeType(obj, typeFromHandle, CultureInfo.InvariantCulture);
                    }

                    return (T)obj;
                }
            }
            catch (Exception ex)
            {
                string fullName = typeof(T).FullName;
                string text = obj?.GetType()?.FullName;
                Log.Warning("GetResultsAsClassic", "Fetching of the job result: " + resultName + " failed for the requested type: " + fullName + ". The discovered type is: " + text + ". Error: " + ex.Message);
                ProtocolGetResultError(ex, fullName, text, resultName);
            }

            return defaultRes;
        }

        private void ProtocolGetResultError(Exception ex, string requestedType, string actualType, string resultName)
        {
            if (!ana09Protocolled)
            {
                try
                {
                    if (ServiceLocator.Current.TryGetService<IFasta2Service>(out var service))
                    {
                        string currentlyRunningModuleName = TimeMetricsUtility.Instance.GetCurrentlyRunningModuleName();
                        string value = "Source: " + currentlyRunningModuleName + ", ecu: " + base.EcuName + ", job: " + base.JobName + ", args: " + base.JobParam + ", result name: " + resultName + ", requested type: " + requestedType + ", actual type: " + actualType + ". Error: " + ex.Message;
                        service.AddServiceCode(ServiceCodes.ANA09_ResultSetFailed_nu_LF, value, LayoutGroup.D, allowMultipleEntries: true);
                        ana09Protocolled = true;
                    }
                }
                catch (Exception ex2)
                {
                    Log.Error(Log.CurrentMethod(), "Cannot protocol " + ServiceCodes.ANA09_ResultSetFailed_nu_LF + ", ex: " + ex2.Message);
                }
            }

            if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Diagnostics.ThrowOnJobRetrievalError", defaultValue: false))
            {
                throw ex;
            }
        }

        internal bool IsDigitsOnly(string str)
        {
            return str.All(char.IsDigit);
        }

        internal Array __initArray<T>(int[] sizes, object initValue)
        {
            Array array = null;
            try
            {
                Type type = typeof(T);
                if (type.BaseType == typeof(Array))
                {
                    type = type.GetElementType();
                }

                array = Array.CreateInstance(type, sizes);
                object value = default(T);
                if (initValue != null && type.IsAssignableFrom(initValue.GetType()))
                {
                    value = Convert.ChangeType(initValue, type, CultureInfo.InvariantCulture);
                }

                int num = sizes[0];
                for (int i = 1; i < sizes.Length; i++)
                {
                    num *= sizes[i];
                }

                int[] array2 = new int[sizes.Length];
                for (int j = 0; j < num; j++)
                {
                    array.SetValue(value, array2);
                    array2[sizes.Length - 1]++;
                    for (int num2 = sizes.Length - 1; num2 >= 0; num2--)
                    {
                        if (array2[num2] == sizes[num2])
                        {
                            array2[num2] = 0;
                            if (num2 > 0)
                            {
                                array2[num2 - 1]++;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("ISTAModule.__initArray<T>(int[], object)", exception);
            }

            return array;
        }

        public int getResultFormat(ushort set, string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                if (base.JobResult != null)
                {
                    using (IEnumerator<IEcuResult> enumerator = base.JobResult.Where((IEcuResult result) => result.Set == set && string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase)).GetEnumerator())
                    {
                        if (enumerator.MoveNext())
                        {
                            return enumerator.Current.Format;
                        }
                    }

                    Log.Warning("ECUJob.getResultFormat()", "(resultName={0}) - no matching result found in JobResult.", resultName);
                }
                else
                {
                    Log.Warning("ECUJob.getResultFormat()", "(resultName={0}) - JobResult was null.", resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getResultFormat()", "({0},{1}) - failed with exception {2}", set, resultName, ex.ToString());
            }

            Log.Warning("ECUJob.getResultFormat()", "({0},{1}) - no valid format in selected result set found.", set, resultName);
            return -1;
        }

        public int getResultFormat(string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                if (base.JobResult != null)
                {
                    return getResultsAs(resultName, 0);
                }

                Log.Warning("ECUJob.getResultFormat()", "({0}) - JobResult was null.", resultName);
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getResultFormat()", "({0}) - failed with exception {1}", resultName, ex.ToString());
            }

            return -1;
        }

        public IList<IEcuResult> getResultSet(ushort set)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            IList<IEcuResult> list = new List<IEcuResult>();
            try
            {
                if (base.JobResult != null)
                {
                    foreach (IEcuResult item in base.JobResult.Where((IEcuResult result) => result.Set == set))
                    {
                        list.Add(item);
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getResults()", "({0}) - JobResult was null.", set);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getResults()", "({0}) - failed with exception {1}", set, ex.ToString());
            }

            return list;
        }

        public string getResultString(ushort set, string resultName, string format)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            string text = getResult(set, resultName) as string;
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
                    StringBuilder stringBuilder = new StringBuilder();
                    byte[] bytes = aSCIIEncoding.GetBytes(text);
                    foreach (byte b in bytes)
                    {
                        stringBuilder.AppendFormat(format, b);
                    }

                    return stringBuilder.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getResult()", "({0},{1},{2}) - failed with exception {3}", set, resultName, format, ex.ToString());
            }

            return null;
        }

        public IList<string> getResultStringList(ushort startSet, ushort stopSet, string resultName, string format)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            List<string> list = new List<string>();
            try
            {
                if (base.JobResult != null)
                {
                    foreach (IEcuResult item in base.JobResult)
                    {
                        if (item.Set < startSet || item.Set > stopSet)
                        {
                            continue;
                        }

                        if (item.Name == resultName)
                        {
                            list.Add(item.Value.ToString());
                        }
                        else
                        {
                            if (!resultName.Contains("*"))
                            {
                                continue;
                            }

                            if (resultName.EndsWith("*", StringComparison.Ordinal))
                            {
                                if (item.Name.StartsWith(resultName.TrimEnd('*'), StringComparison.Ordinal))
                                {
                                    list.Add(item.Value.ToString());
                                }

                                continue;
                            }

                            string[] array = resultName.Split('*');
                            if (array.Length == 2 && item.Name.StartsWith(array[0], StringComparison.Ordinal) && item.Name.EndsWith(array[1], StringComparison.Ordinal))
                            {
                                list.Add(item.Value.ToString());
                            }
                        }
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getResultStringList()", "({0},{1},{2}) - JobResult was null.", startSet, stopSet, resultName);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUJob.getResultStringList()", exception);
            }

            return list;
        }

        public string getStringResult(ushort set, string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                if (base.JobResult != null)
                {
                    string resultsAs = getResultsAs<string>(resultName, null, set);
                    if (resultsAs == null)
                    {
                        return null;
                    }

                    return resultsAs;
                }

                Log.Warning("ECUJob.getStringResult()", "(set={0},resultName={1}) - JobResult was null.", set, resultName);
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getStringResult()", "({0},{1}) - failed with exception {2}", set, resultName, ex.ToString());
            }

            return null;
        }

        public string getStringResult(string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                if (base.JobResult != null)
                {
                    string resultsAs = getResultsAs<string>(resultName);
                    if (resultsAs == null)
                    {
                        return null;
                    }

                    return resultsAs;
                }

                Log.Warning("ECUJob.getStringResult()", "(resultName={0}) - JobResult was null.", resultName);
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getStringResult()", "({0}) - failed with exception {1}", resultName, ex.ToString());
            }

            return null;
        }

        public char? getcharResult(ushort set, string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                if (base.JobResult != null)
                {
                    foreach (IEcuResult item in base.JobResult.Where((IEcuResult result) => result.Set == set && string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (item.Format == 0)
                        {
                            return (char)item.Value;
                        }

                        Log.Warning("ECUJob.getcharResult()", "(set={0},resultName={1}) has different format type!!! You selected char but should be:{2}", set, resultName, ECUKom.APIFormatName(item.Format));
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getcharResult()", "(set={0},resultName={1}) - JobResult was null.", set, resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getcharResult()", "({0},{1}) - failed with exception {2}", set, resultName, ex.ToString());
            }

            return null;
        }

        public char? getcharResult(string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                if (base.JobResult != null)
                {
                    foreach (IEcuResult item in base.JobResult.Where((IEcuResult result) => string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (item.Format == 0)
                        {
                            return (char)item.Value;
                        }

                        Log.Warning("ECUJob.getcharResult()", "(resultName={0}) has different format type!!! You selected char but should be:{1}", resultName, ECUKom.APIFormatName(item.Format));
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getcharResult()", "(resultName={0}) - JobResult was null.", resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getcharResult()", "({0}) - failed with exception {1}", resultName, ex.ToString());
            }

            return null;
        }

        public double? getdoubleResult(ushort set, string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                if (base.JobResult != null)
                {
                    return getResultsAs<double?>(resultName, null, set);
                }

                Log.Warning("ECUJob.getdoubleResult()", "(set={0},resultName={1}) - JobResult was null.", set, resultName);
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getdoubleResult()", "({0},{1}) - failed with exception {2}", set, resultName, ex.ToString());
            }

            return null;
        }

        public double? getdoubleResult(string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                if (base.JobResult != null)
                {
                    foreach (IEcuResult item in base.JobResult.Where((IEcuResult result) => string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (item.Format == 8)
                        {
                            return (double)item.Value;
                        }

                        Log.Warning("ECUJob.getdoubleResult()", "(resultName={0}) has different format type!!! You selected double but should be:{1}", resultName, ECUKom.APIFormatName(item.Format));
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getdoubleResult()", "(resultName={0}) - JobResult was null.", resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getdoubleResult()", "({0}) - failed with exception {1}", resultName, ex.ToString());
            }

            return null;
        }

        public virtual long? getlongResult(ushort set, string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            if (string.IsNullOrEmpty(resultName))
            {
                Log.Warning("ECUKom.getintResult(ushort set, string resultName)", "failed due to resultName was empty or null.");
                return null;
            }

            try
            {
                if (base.JobResult != null)
                {
                    return getResultsAs<long?>(resultName, null, set);
                }

                Log.Warning("ECUJob.getintResult()", "(set={0},resultName={1}) - JobResult was null.", set, resultName);
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getintResult()", "({0},{1}) - failed with exception {2}", set, resultName, ex.ToString());
            }

            return null;
        }

        public virtual long? getlongResult(string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            if (string.IsNullOrEmpty(resultName))
            {
                Log.Warning("ECUKom.getintResult(string resultName)", "failed due to resultName was empty or null.");
                return null;
            }

            try
            {
                if (base.JobResult != null)
                {
                    return getResultsAs<long?>(resultName);
                }

                Log.Warning("ECUJob.getintResult()", "resultName={0} - JobResult was null.", resultName);
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUJob.getintResult()", exception);
            }

            return null;
        }

        public virtual int? getintResult(ushort set, string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            if (string.IsNullOrEmpty(resultName))
            {
                Log.Warning("ECUKom.getintResult(ushort set, string resultName)", "failed due to resultName was empty or null.");
                return null;
            }

            try
            {
                if (base.JobResult != null)
                {
                    return getResultsAs<int?>(resultName, null, set);
                }

                Log.Warning("ECUJob.getintResult()", "(set={0},resultName={1}) - JobResult was null.", set, resultName);
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getintResult()", "({0},{1}) - failed with exception {2}", set, resultName, ex.ToString());
            }

            return null;
        }

        public virtual int? getintResult(string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            if (string.IsNullOrEmpty(resultName))
            {
                Log.Warning("ECUKom.getintResult(string resultName)", "failed due to resultName was empty or null.");
                return null;
            }

            try
            {
                if (base.JobResult != null)
                {
                    return getResultsAs<int?>(resultName);
                }

                Log.Warning("ECUJob.getintResult()", "resultName={0} - JobResult was null.", resultName);
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUJob.getintResult()", exception);
            }

            return null;
        }

        public short? getshortResult(ushort set, string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                if (base.JobResult != null)
                {
                    return getResultsAs<short?>(resultName, null, set);
                }

                Log.Warning("ECUJob.getshortResult()", "(set={0},resultName={1}) - JobResult was null.", set, resultName);
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getshortResult()", "({0},{1}) - failed with exception {2}", set, resultName, ex.ToString());
            }

            return null;
        }

        public short? getshortResult(string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                if (base.JobResult != null)
                {
                    return getResultsAs<short?>(resultName);
                }

                Log.Warning("ECUJob.getshortResult()", "(resultName={0}) - JobResult was null.", resultName);
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getshortResult()", "({0}) - failed with exception {1}", resultName, ex.ToString());
            }

            return null;
        }

        public uint? getuintResult(ushort set, string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            if (string.IsNullOrEmpty(resultName))
            {
                Log.Warning("ECUKom.getuintResult(ushort set, string resultName)", "failed due to resultName was empty or null.");
                return null;
            }

            try
            {
                if (base.JobResult != null)
                {
                    return getResultsAs<uint?>(resultName, null, set);
                }

                Log.Warning("ECUJob.getuintResult()", "(set={0},resultName={1}) - JobResult was null.", set, resultName);
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getuintResult()", "({0},{1}) - failed with exception {2}", set, resultName, ex.ToString());
            }

            return null;
        }

        public uint? getuintResult(string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            if (string.IsNullOrEmpty(resultName))
            {
                Log.Warning("ECUKom.getuintResult(ushort set, string resultName)", "failed due to resultName was empty or null.");
                return null;
            }

            try
            {
                if (base.JobResult != null)
                {
                    foreach (IEcuResult item in base.JobResult.Where((IEcuResult result) => string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (item.Format == 3)
                        {
                            return (ushort)item.Value;
                        }

                        if (item.Format == 5)
                        {
                            return (uint)item.Value;
                        }

                        if (item.Format == 2)
                        {
                            return (uint)item.Value;
                        }

                        if (item.Format == 4)
                        {
                            Log.Debug(VehicleCommunication.DebugLevel, 2, "ECUJob.getuintResult()", "(resultName={0}) signed/unsigned mismatch", resultName);
                            return (uint)(int)item.Value;
                        }

                        if (item.Format == 6)
                        {
                            Log.Warning("ECUJob.getuintResult()", "resultName='{0}', resultFormat='{1}', resultValue='{2}')", resultName, ECUKom.APIFormatName(item.Format), item.Value as string);
                        }

                        Log.Warning("ECUJob.getuintResult()", "(resultName={0}) has different format type!!! You selected uint but should be:{1}", resultName, ECUKom.APIFormatName(item.Format));
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getuintResult()", "(resultName={0}) - JobResult was null.", resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getuintResult()", "(resultName={0}) - failed with exception {1}", resultName, ex.ToString());
            }

            return null;
        }

        public ushort? getushortResult(ushort set, string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            if (string.IsNullOrEmpty(resultName))
            {
                Log.Warning("ECUKom.getushortResult()", "(ushort set, string resultName) - failed due to resultName was empty or null.");
                return null;
            }

            try
            {
                if (base.JobResult != null)
                {
                    return getResultsAs<ushort?>(resultName, null, set);
                }

                Log.Warning("ECUJob.getushortResult()", "(set={0},resultName={1})", "JobResult was null.", set, resultName);
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getushortResult()", "({0},{1}) - failed with exception {2}", set, resultName, ex.ToString());
            }

            return null;
        }

        public ushort? getushortResult(string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            if (string.IsNullOrEmpty(resultName))
            {
                Log.Warning("ECUKom.getushortResult()", "failed due to resultName was empty or null.");
                return null;
            }

            try
            {
                if (base.JobResult != null)
                {
                    return getResultsAs<ushort?>(resultName);
                }

                Log.Warning("ECUJob.getushortResult()", "(resultName={0}) - JobResult was null.", resultName);
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getushortResult()", "({0}) - failed with exception {1}", resultName, ex.ToString());
            }

            return null;
        }

        public static void Dump(IEcuJob dJob)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                if (dJob == null)
                {
                    Log.Warning("ECUJob.Dump()", "failed due job was not initialized.");
                    return;
                }

                if (string.IsNullOrEmpty(dJob.EcuName))
                {
                    Log.Warning("ECUJob.Dump()", "failed EcuName is null or empty.");
                    return;
                }

                if (string.IsNullOrEmpty(dJob.JobName))
                {
                    Log.Warning("ECUJob.Dump()", "failed for EcuName: {0}. JobName is null or empty.", dJob.EcuName);
                    return;
                }

                string empty = string.Empty;
                string empty2 = string.Empty;
                string empty3 = string.Empty;
                if (!string.IsNullOrEmpty(dJob.JobErrorText))
                {
                    empty = dJob.JobErrorText;
                }

                if (!string.IsNullOrEmpty(dJob.JobParam))
                {
                    empty2 = dJob.JobParam;
                }

                if (!string.IsNullOrEmpty(dJob.JobResultFilter))
                {
                    empty3 = dJob.JobResultFilter;
                }

                Log.Info("ECUJob.Dump()", "ECUJob({0},{1},{2},{3}). Job returned: {4} {5} RSets: {6}", dJob.EcuName, dJob.JobName, empty2, empty3, dJob.JobErrorCode, empty, dJob.JobResultSets);
                if (dJob.JobResult == null)
                {
                    return;
                }

                foreach (IEcuResult item in dJob.JobResult)
                {
                    string text = string.Empty;
                    if (item.Format != 7)
                    {
                        object result = dJob.getResult(item.Set, item.Name);
                        if (result != null)
                        {
                            text = result.ToString();
                        }
                    }
                    else if (dJob.getResult(item.Set, item.Name)is byte[] param)
                    {
                        text = FormatConverter.ByteArray2String(param, item.Length);
                    }

                    Log.Info("ECUJob.Dump()", "Set:{0} Format:({1}/{2}) Name:{3} Value:{4} FASTA:{5}", item.Set, item.Format, ECUKom.APIFormatName(item.Format), item.Name, text, item.FASTARelevant);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUJob.Dump()", exception);
            }
        }

        public static void DumpList(IList<IEcuJob> jobList)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                if (jobList == null)
                {
                    Log.Warning("ECUJob.Dump(List<ECUJob>)", "failed due job was not initialized.");
                    return;
                }

                foreach (IEcuJob job in jobList)
                {
                    string text = ((!string.IsNullOrEmpty(job.JobResultFilter)) ? job.JobResultFilter : string.Empty);
                    string text2 = ((!string.IsNullOrEmpty(job.JobParam)) ? job.JobParam : string.Empty);
                    Log.Info("ECUJob.DumpList()", "Dump of (List<ECUJob>). Job: ({0},{1},{2},{3}) - retcode: {4}", job.EcuName, job.JobName, text2, text, job.JobErrorCode);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUJob.Dump()", exception);
            }
        }

        public static IList<string> scanECUfromFunctionalResponse(IEcuJob job)
        {
            try
            {
                if (!VehicleCommunication.validLicense)
                {
                    throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
                }

                List<string> list = new List<string>();
                foreach (IEcuResult item in job.JobResult)
                {
                    if (item.Name == "ECU_GROBNAME")
                    {
                        list.Add((string)item.Value);
                    }
                }

                return list;
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUJob.scanECUfromFunctionalResponse()", exception);
            }

            return null;
        }

        public override int GetHashCode()
        {
            return hashValue.GetHashCode();
        }

        public List<object> getResults(string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            if (string.IsNullOrEmpty(resultName))
            {
                Log.Warning("ECUKom.getResults()", "failed due to resultName was empty or null.");
                return null;
            }

            List<object> list = new List<object>();
            try
            {
                if (base.JobResult != null)
                {
                    foreach (IEcuResult item in base.JobResult.Where((IEcuResult result) => string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase)))
                    {
                        list.Add(item.Value);
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getResults()", "({0}) - JobResult was null.", resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getResults()", "({0}) - failed with exception {1}", resultName, ex.ToString());
            }

            return null;
        }

        public void maskResultFASTARelevant(bool defRelevant)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                FASTARelevant = defRelevant;
                if (base.JobResult != null)
                {
                    foreach (IEcuResult item in base.JobResult)
                    {
                        item.FASTARelevant = defRelevant;
                    }

                    return;
                }

                Log.Warning("ECUJob.maskResultFASTARelevant()", "JobResult was null.");
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUJob.maskResultFASTARelevant()", exception);
            }
        }

        public void maskResultFASTARelevant(ushort startSet, int stopSet, string resultName)
        {
            if (!VehicleCommunication.validLicense)
            {
                throw new Exception("This copy of VehicleCommunication.dll is not licensed !!!");
            }

            try
            {
                FASTARelevant = true;
                ushort num = startSet;
                Regex regex = new Regex("^" + resultName.Replace("[].", "[(\\d+)]\\.").Replace("[", "\\[").Replace("]", "\\]") + "$");
                if (base.JobResult != null)
                {
                    num = ((stopSet < 0) ? ((ushort)(base.JobResultSets + stopSet + 1)) : ((ushort)stopSet));
                    {
                        foreach (IEcuResult item in base.JobResult)
                        {
                            if (item.FASTARelevant || item.Set < startSet || item.Set > num)
                            {
                                continue;
                            }

                            if (regex.Match(item.Name).Success)
                            {
                                item.FASTARelevant = true;
                            }
                            else if (item.Name.Equals(resultName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                item.FASTARelevant = true;
                            }
                            else
                            {
                                if (!resultName.Contains("*"))
                                {
                                    continue;
                                }

                                if (resultName == "*")
                                {
                                    item.FASTARelevant = true;
                                    continue;
                                }

                                if (resultName.EndsWith("*", StringComparison.Ordinal))
                                {
                                    if (item.Name.StartsWith(resultName.TrimEnd('*'), StringComparison.Ordinal))
                                    {
                                        item.FASTARelevant = true;
                                    }

                                    continue;
                                }

                                string[] array = resultName.Split('*');
                                if (array.Length == 2)
                                {
                                    if (item.Name.StartsWith(array[0], StringComparison.Ordinal) && item.Name.EndsWith(array[1], StringComparison.Ordinal))
                                    {
                                        item.FASTARelevant = true;
                                    }
                                }
                                else
                                {
                                    item.FASTARelevant = true;
                                }
                            }
                        }

                        return;
                    }
                }

                Log.Warning("ECUJob.maskResultFASTARelevant()", "({0},{1},{2}) - JobResult was null.", startSet, stopSet, resultName);
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUJob.maskResultFASTARelevant()", exception);
            }
        }
    }
}