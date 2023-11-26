// BMW.Rheingold.VehicleCommunication.ECUJob
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using PsdzClient.Utility;
using PsdzClientLibrary.Core;

namespace PsdzClient.Core.Container
{
    public class ECUJob : ECUJobAbstract, IEcuJob, INotifyPropertyChanged
    {
        private bool fastaRelevant = false;

        private readonly decimal hashValue;

        //private readonly IProtocolBasic fastaservice;

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
#if false
        public ECUJob(IProtocolBasic fastaprotocoller)
        {
            hashValue = Environment.TickCount;
            fastaservice = fastaprotocoller;
        }
#endif
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
            if (base.JobResult != null)
            {
                ECUResult eCUResult = base.JobResult.FirstOrDefault((ECUResult item) => item.Set == set && string.Equals(item.Name, resultName, StringComparison.OrdinalIgnoreCase));
                if (eCUResult != null)
                {
                    return eCUResult.FASTARelevant;
                }
            }
            return false;
        }

        public bool IsJobState(ushort set, string state)
        {
            if (IsDone())
            {
                if (getStringResult(set, "JOB_STATUS") == state)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        public bool IsJobState(string state)
        {
            if (IsDone())
            {
                if (getStringResult("JOB_STATUS") == state)
                {
                    return true;
                }
                return false;
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
                string stringResult = getStringResult((ushort)base.JobResultSets, "JOB_STATUS");
                if (string.IsNullOrEmpty(stringResult))
                {
                    stringResult = getStringResult("JOB_STATUS");
                }
                if (string.CompareOrdinal(stringResult, "OKAY") == 0)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        public bool IsOkay(ushort set)
        {
            if (IsDone())
            {
                if (getStringResult(set, "JOB_STATUS") == "OKAY")
                {
                    return true;
                }
                return false;
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
                    foreach (ECUResult item in base.JobResult)
                    {
                        string arg = string.Empty;
                        if (item.Format != 7)
                        {
                            arg = getResult(item.Set, item.Name).ToString();
                        }
                        else if (getResult(item.Set, item.Name) is byte[] param)
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
            try
            {
                if (base.JobResult != null)
                {
                    ECUResult eCUResult = base.JobResult.FirstOrDefault((ECUResult item) => item.Set == set && string.Equals(item.Name, resultName, StringComparison.OrdinalIgnoreCase) && item.Format == 7);
                    if (eCUResult != null)
                    {
                        eCUResult.FASTARelevant = true;
                        len = eCUResult.Length;
                        return (byte[])eCUResult.Value;
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getByteArrayResult()", "(set={0},resultName={1}) - JobResult was null.", set, resultName);
                }
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
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    foreach (ECUResult item in enumerable)
                    {
                        if (item.Format != 7)
                        {
                            Log.Warning("ECUJob.getByteArrayResult()", "resultName: {0} has different format type!!! You selected ByteArray but should be:{1}", resultName, ECUKom.APIFormatName(item.Format));
                            continue;
                        }
                        if (item.Value == null)
                        {
                            len = 0u;
                            return null;
                        }
                        byte[] array = (byte[])item.Value;
                        len = item.Length;
                        if (array != null && array.Length > len)
                        {
                            Array.Resize(ref array, (int)len);
                        }
                        return array;
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getByteArrayResult()", "JobResult was null for resultName: {0} ", resultName);
                }
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
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => result.Set == set && string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    foreach (ECUResult item in enumerable)
                    {
                        if (item.Format != 1)
                        {
                            Log.Warning("ECUJob.getByteResult()", "(set={0},resultName={1}) has different format type!!! You selected Byte but should be:{2}", set, resultName, ECUKom.APIFormatName(item.Format));
                            continue;
                        }
                        return (byte)item.Value;
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
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    foreach (ECUResult item in enumerable)
                    {
                        if (item.Format != 1)
                        {
                            Log.Warning("ECUJob.getByteResult()", "(resultName={0}) has different format type!!! You selected Byte but should be:{1}", resultName, ECUKom.APIFormatName(item.Format));
                            continue;
                        }
                        return (byte)item.Value;
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
                switch (resultName)
                {
                    case "/Result/Rows/$Count":
                        if (base.JobResultSets > 0)
                        {
                            return base.JobResultSets;
                        }
                        return 0;
                    case "/Result/Status/JOB_STATUS":
                        return getResult("JOB_STATUS");
                    default:
                        if (resultName.Contains('['))
                        {
                            Match match = Regex.Match(resultName, "\\[\\d+\\]");
                            if (match.Success)
                            {
                                string text = match.Value.Replace("[", string.Empty);
                                text = text.Replace("]", string.Empty);
                                int num2 = Convert.ToInt32(text);
                                string[] array = Regex.Split(resultName, string.Format(CultureInfo.InvariantCulture, "/Row\\[{0}\\]/", num2));
                                if (array.Length == 2)
                                {
                                    return getResult((ushort)(num2 + 1), array[1]);
                                }
                                Log.Warning("ECUJob.getISTAResult()", "failed to separate result name from result path: {0}", resultName);
                                return null;
                            }
                            Log.Warning("ECUJob.getISTAResult()", "failed to evaluate query: {0}", resultName);
                        }
                        else if (resultName.StartsWith("/Result/Status/", StringComparison.Ordinal))
                        {
                            string text2 = resultName.Substring(15);
                            if (!string.IsNullOrEmpty(text2))
                            {
                                object result = getResult(text2);
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
                            return (T)Convert.ChangeType(iSTAResult, typeFromHandle);
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
                                    return (T)Convert.ChangeType(array[0], typeFromHandle);
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
                switch (resultName)
                {
                    case "/Result/Rows/$Count":
                        if (base.JobResultSets > 0)
                        {
                            return base.JobResultSets;
                        }
                        return 0;
                    case "/Result/Status/JOB_STATUS":
                        return getResult("JOB_STATUS");
                    default:
                        if (resultName.Contains('['))
                        {
                            Match match = Regex.Match(resultName, "\\[\\d+\\]");
                            if (match.Success)
                            {
                                string text = match.Value.Replace("[", string.Empty);
                                text = text.Replace("]", string.Empty);
                                int num2 = Convert.ToInt32(text);
                                string[] array = Regex.Split(resultName, $"/Row\\[{num2}\\]/");
                                if (array.Length == 2)
                                {
                                    object result = getResult((ushort)(num2 + 1), array[1]);
                                    if (result == null)
                                    {
                                        Log.Warning("ECUJob.getISTAResultAsType()", "obj was null when query for {0}; guess your testmodule will die... cross your fingers", resultName);
                                        return null;
                                    }
                                    if (result.GetType() != targetType)
                                    {
                                        string text2 = result.GetType().ToString();
                                        string text3 = targetType.ToString();
                                        Log.Info("ECUJob.getISTAResultAsType(string, Type)", "result: {0} obj type is {1} targetType is: {2}", resultName, text2, text3);
                                    }
                                    try
                                    {
                                        if (result.GetType() != targetType)
                                        {
                                            return Convert.ChangeType(result, targetType);
                                        }
                                    }
                                    catch (Exception exception)
                                    {
                                        Log.WarningException("ECUJob.getISTAResultAsType()", exception);
                                        string text4 = result.ToString();
                                        if (!string.IsNullOrEmpty(text4))
                                        {
                                            string[] array2 = text4.Split(' ');
                                            if (array2.Length != 0)
                                            {
                                                if (!(targetType == typeof(bool)))
                                                {
                                                    Log.Info("ECUJob.getISTAResultAsType()", "found multipart string; trying conversion with the first part");
                                                    return Convert.ChangeType(array2[0], targetType);
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
                            string text5 = resultName.Substring(15);
                            if (!string.IsNullOrEmpty(text5))
                            {
                                object result2 = getResult(text5);
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
                                    return Convert.ChangeType(result2, targetType);
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
            }
            catch (Exception exception2)
            {
                Log.WarningException("ECUJob.getISTAResultAsType()", exception2);
            }
            return null;
        }

        public object getResult(ushort set, string resultName)
        {
            try
            {
                if (base.JobResult != null)
                {
                    ECUResult eCUResult = base.JobResult.FirstOrDefault((ECUResult item) => item.Set == set && string.Equals(item.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    if (eCUResult != null)
                    {
                        eCUResult.FASTARelevant = true;
                        return eCUResult.Value;
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
            try
            {
                if (base.JobResult != null)
                {
                    ECUResult eCUResult = (getLast ? base.JobResult.LastOrDefault((ECUResult item) => string.Equals(item.Name, resultName, StringComparison.OrdinalIgnoreCase)) : base.JobResult.FirstOrDefault((ECUResult item) => string.Equals(item.Name, resultName, StringComparison.OrdinalIgnoreCase)));
                    if (eCUResult != null)
                    {
                        eCUResult.FASTARelevant = true;
                        return eCUResult.Value;
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
                        return (T)Convert.ChangeType(result, typeFromHandle);
                    }
                    return (T)result;
                }
                return defaultRes;
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUJob.getISTAResult(ushort set, string resultName)", exception);
            }
            return defaultRes;
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
                        return (T)Convert.ChangeType(result, typeFromHandle);
                    }
                    return (T)result;
                }
                return defaultRes;
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUJob.getISTAResultAs(string resultName)", exception);
            }
            return defaultRes;
        }

        public int getResultFormat(ushort set, string resultName)
        {
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => result.Set == set && string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    using (IEnumerator<ECUResult> enumerator = enumerable.GetEnumerator())
                    {
                        if (enumerator.MoveNext())
                        {
                            ECUResult current = enumerator.Current;
                            return current.Format;
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
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    using (IEnumerator<ECUResult> enumerator = enumerable.GetEnumerator())
                    {
                        if (enumerator.MoveNext())
                        {
                            ECUResult current = enumerator.Current;
                            return current.Format;
                        }
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getResultFormat()", "({0}) - JobResult was null.", resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getResultFormat()", "({0}) - failed with exception {1}", resultName, ex.ToString());
            }
            return -1;
        }

        public IList<IEcuResult> getResultSet(ushort set)
        {
            IList<IEcuResult> list = new List<IEcuResult>();
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => result.Set == set);
                    foreach (ECUResult item in enumerable)
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
            string text = getResult(set, resultName) as string;
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
                    StringBuilder stringBuilder = new StringBuilder();
                    byte[] bytes = aSCIIEncoding.GetBytes(text);
                    byte[] array = bytes;
                    foreach (byte b in array)
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
            List<string> list = new List<string>();
            try
            {
                if (base.JobResult != null)
                {
                    foreach (ECUResult item in base.JobResult)
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
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => result.Set == set && string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    foreach (ECUResult item in enumerable)
                    {
                        if (item.Format != 6)
                        {
                            if (item.Format != 2)
                            {
                                Log.Warning("ECUJob.getStringResult()", "(set={0},resultName={1}) has different format type!!! You selected string but should be:{2}", set, resultName, ECUKom.APIFormatName(item.Format));
                                continue;
                            }
                            return ((short)item.Value).ToString(CultureInfo.InvariantCulture);
                        }
                        return (string)item.Value;
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getStringResult()", "(set={0},resultName={1}) - JobResult was null.", set, resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getStringResult()", "({0},{1}) - failed with exception {2}", set, resultName, ex.ToString());
            }
            return null;
        }

        public string getStringResult(string resultName)
        {
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    foreach (ECUResult item in enumerable)
                    {
                        if (item.Format != 6)
                        {
                            if (item.Format != 2)
                            {
                                if (item.Format != 4)
                                {
                                    Log.Warning("ECUJob.getStringResult()", "(resultName={0}) has different format type!!! You selected string but should be:{1}", resultName, ECUKom.APIFormatName(item.Format));
                                    continue;
                                }
                                return ((int)item.Value).ToString(CultureInfo.InvariantCulture);
                            }
                            return ((short)item.Value).ToString(CultureInfo.InvariantCulture);
                        }
                        return (string)item.Value;
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getStringResult()", "(resultName={0}) - JobResult was null.", resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getStringResult()", "({0}) - failed with exception {1}", resultName, ex.ToString());
            }
            return null;
        }

        public char? getcharResult(ushort set, string resultName)
        {
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => result.Set == set && string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    foreach (ECUResult item in enumerable)
                    {
                        if (item.Format != 0)
                        {
                            Log.Warning("ECUJob.getcharResult()", "(set={0},resultName={1}) has different format type!!! You selected char but should be:{2}", set, resultName, ECUKom.APIFormatName(item.Format));
                            continue;
                        }
                        return (char)item.Value;
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
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    foreach (ECUResult item in enumerable)
                    {
                        if (item.Format != 0)
                        {
                            Log.Warning("ECUJob.getcharResult()", "(resultName={0}) has different format type!!! You selected char but should be:{1}", resultName, ECUKom.APIFormatName(item.Format));
                            continue;
                        }
                        return (char)item.Value;
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
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => result.Set == set && string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    foreach (ECUResult item in enumerable)
                    {
                        if (item.Format != 8)
                        {
                            if (item.Format != 6)
                            {
                                Log.Warning("ECUJob.getdoubleResult()", "(set={0},resultName={1}) has different format type!!! You selected double but should be:{2}", set, resultName, ECUKom.APIFormatName(item.Format));
                                continue;
                            }
                            return double.Parse((string)item.Value, CultureInfo.InvariantCulture);
                        }
                        return (double)item.Value;
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getdoubleResult()", "(set={0},resultName={1}) - JobResult was null.", set, resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getdoubleResult()", "({0},{1}) - failed with exception {2}", set, resultName, ex.ToString());
            }
            return null;
        }

        public double? getdoubleResult(string resultName)
        {
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    foreach (ECUResult item in enumerable)
                    {
                        if (item.Format != 8)
                        {
                            Log.Warning("ECUJob.getdoubleResult()", "(resultName={0}) has different format type!!! You selected double but should be:{1}", resultName, ECUKom.APIFormatName(item.Format));
                            continue;
                        }
                        return (double)item.Value;
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

        public virtual int? getintResult(ushort set, string resultName)
        {
            if (string.IsNullOrEmpty(resultName))
            {
                Log.Warning("ECUKom.getintResult(ushort set, string resultName)", "failed due to resultName was empty or null.");
                return null;
            }
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => result.Set == set && string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    foreach (ECUResult item in enumerable)
                    {
                        if (item.Format != 4)
                        {
                            if (item.Format != 2)
                            {
                                if (item.Format != 0)
                                {
                                    if (item.Format != 1)
                                    {
                                        if (item.Format != 8)
                                        {
                                            Log.Warning("ECUJob.getintResult()", "(set={0},resultName={1}) has different format type!!! You selected int but should be:{2}", set, resultName, ECUKom.APIFormatName(item.Format));
                                            continue;
                                        }
                                        return (int)(double)item.Value;
                                    }
                                    return (byte)item.Value;
                                }
                                return (char)item.Value;
                            }
                            return (short)item.Value;
                        }
                        return (int)item.Value;
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getintResult()", "(set={0},resultName={1}) - JobResult was null.", set, resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getintResult()", "({0},{1}) - failed with exception {2}", set, resultName, ex.ToString());
            }
            return null;
        }

        public virtual int? getintResult(string resultName)
        {
            if (string.IsNullOrEmpty(resultName))
            {
                Log.Warning("ECUKom.getintResult(string resultName)", "failed due to resultName was empty or null.");
                return null;
            }
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    foreach (ECUResult item in enumerable)
                    {
                        if (item.Format != 4)
                        {
                            if (item.Format != 2)
                            {
                                if (item.Format != 0)
                                {
                                    if (item.Format != 1)
                                    {
                                        Log.Warning("ECUJob.getintResult()", "resultName={0} has different format type!!! You selected int but should be:{1}", resultName, ECUKom.APIFormatName(item.Format));
                                        continue;
                                    }
                                    return (byte)item.Value;
                                }
                                return (char)item.Value;
                            }
                            return (short)item.Value;
                        }
                        return (int)item.Value;
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getintResult()", "resultName={0} - JobResult was null.", resultName);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("ECUJob.getintResult()", exception);
            }
            return null;
        }

        public short? getshortResult(ushort set, string resultName)
        {
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => result.Set == set && string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    foreach (ECUResult item in enumerable)
                    {
                        if (item.Format != 2)
                        {
                            if (item.Format != 0)
                            {
                                if (item.Format != 1)
                                {
                                    if (item.Format != 6)
                                    {
                                        Log.Warning("ECUJob.getshortResult()", "(set={0},resultName={1}) has different format type!!! You selected short but should be:{2}", set, resultName, ECUKom.APIFormatName(item.Format));
                                        continue;
                                    }
                                    return short.Parse(item.Value.ToString());
                                }
                                return (byte)item.Value;
                            }
                            return (short)(char)item.Value;
                        }
                        return (short)item.Value;
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getshortResult()", "(set={0},resultName={1}) - JobResult was null.", set, resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getshortResult()", "({0},{1}) - failed with exception {2}", set, resultName, ex.ToString());
            }
            return null;
        }

        public short? getshortResult(string resultName)
        {
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    foreach (ECUResult item in enumerable)
                    {
                        if (item.Format != 2)
                        {
                            if (item.Format != 0)
                            {
                                if (item.Format != 1)
                                {
                                    if (item.Format != 6)
                                    {
                                        Log.Warning("ECUJob.getshortResult()", "(resultName={0}) has different format type!!! You selected short but should be:{1}", resultName, ECUKom.APIFormatName(item.Format));
                                        continue;
                                    }
                                    return short.Parse(item.Value.ToString());
                                }
                                return (byte)item.Value;
                            }
                            return (short)(char)item.Value;
                        }
                        return (short)item.Value;
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getshortResult()", "(resultName={0}) - JobResult was null.", resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getshortResult()", "({0}) - failed with exception {1}", resultName, ex.ToString());
            }
            return null;
        }

        public uint? getuintResult(ushort set, string resultName)
        {
            if (string.IsNullOrEmpty(resultName))
            {
                Log.Warning("ECUKom.getuintResult(ushort set, string resultName)", "failed due to resultName was empty or null.");
                return null;
            }
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => result.Set == set && string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    foreach (ECUResult item in enumerable)
                    {
                        if (item.Format != 3)
                        {
                            if (item.Format != 5)
                            {
                                if (item.Format != 2)
                                {
                                    if (item.Format != 4)
                                    {
                                        Log.Warning("ECUJob.getuintResult()", "(set={0},resultName={1}) has different format type!!! You selected uint but should be:{2}", set, resultName, ECUKom.APIFormatName(item.Format));
                                        continue;
                                    }
                                    Log.Debug(VehicleCommunication.DebugLevel, 2, "ECUJob.getuintResult()", "(set={0},resultName={1}) signed/unsigned mismatch", set, resultName);
                                    return (uint)(int)item.Value;
                                }
                                return (uint)item.Value;
                            }
                            return (uint)item.Value;
                        }
                        return (ushort)item.Value;
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getuintResult()", "(set={0},resultName={1}) - JobResult was null.", set, resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getuintResult()", "({0},{1}) - failed with exception {2}", set, resultName, ex.ToString());
            }
            return null;
        }

        public uint? getuintResult(string resultName)
        {
            if (string.IsNullOrEmpty(resultName))
            {
                Log.Warning("ECUKom.getuintResult(ushort set, string resultName)", "failed due to resultName was empty or null.");
                return null;
            }
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    foreach (ECUResult item in enumerable)
                    {
                        if (item.Format != 3)
                        {
                            if (item.Format != 5)
                            {
                                if (item.Format != 2)
                                {
                                    if (item.Format != 4)
                                    {
                                        if (item.Format == 6)
                                        {
                                            Log.Warning("ECUJob.getuintResult()", "resultName='{0}', resultFormat='{1}', resultValue='{2}')", resultName, ECUKom.APIFormatName(item.Format), item.Value as string);
                                        }
                                        Log.Warning("ECUJob.getuintResult()", "(resultName={0}) has different format type!!! You selected uint but should be:{1}", resultName, ECUKom.APIFormatName(item.Format));
                                        continue;
                                    }
                                    Log.Debug(VehicleCommunication.DebugLevel, 2, "ECUJob.getuintResult()", "(resultName={0}) signed/unsigned mismatch", resultName);
                                    return (uint)(int)item.Value;
                                }
                                return (uint)item.Value;
                            }
                            return (uint)item.Value;
                        }
                        return (ushort)item.Value;
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
            if (string.IsNullOrEmpty(resultName))
            {
                Log.Warning("ECUKom.getushortResult()", "(ushort set, string resultName) - failed due to resultName was empty or null.");
                return null;
            }
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => result.Set == set && string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    using (IEnumerator<ECUResult> enumerator = enumerable.GetEnumerator())
                    {
                        if (enumerator.MoveNext())
                        {
                            ECUResult current = enumerator.Current;
                            if (current.Format == 3)
                            {
                                return (ushort)current.Value;
                            }
                            Log.Warning("ECUJob.getushortResult()", "(set={0},resultName={1}) has different format type!!! You selected ushort but should be:{2}", set, resultName, ECUKom.APIFormatName(current.Format));
                            return Convert.ToUInt16(current.Value);
                        }
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getushortResult()", "(set={0},resultName={1})", "JobResult was null.", set, resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getushortResult()", "({0},{1}) - failed with exception {2}", set, resultName, ex.ToString());
            }
            return null;
        }

        public ushort? getushortResult(string resultName)
        {
            if (string.IsNullOrEmpty(resultName))
            {
                Log.Warning("ECUKom.getushortResult()", "failed due to resultName was empty or null.");
                return null;
            }
            try
            {
                if (base.JobResult != null)
                {
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    using (IEnumerator<ECUResult> enumerator = enumerable.GetEnumerator())
                    {
                        if (enumerator.MoveNext())
                        {
                            ECUResult current = enumerator.Current;
                            if (current.Format == 3)
                            {
                                return (ushort)current.Value;
                            }
                            Log.Warning("ECUJob.getushortResult()", "resultName={0}) has different format type!!! You selected ushort but should be:{1}", resultName, ECUKom.APIFormatName(current.Format));
                            return Convert.ToUInt16(current.Value);
                        }
                    }
                }
                else
                {
                    Log.Warning("ECUJob.getushortResult()", "(resultName={0}) - JobResult was null.", resultName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("ECUJob.getushortResult()", "({0}) - failed with exception {1}", resultName, ex.ToString());
            }
            return null;
        }

        public static void Dump(ECUJob dJob)
        {
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
                foreach (ECUResult item in dJob.JobResult)
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
                    else if (dJob.getResult(item.Set, item.Name) is byte[] param)
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

        public static void DumpList(IList<ECUJob> jobList)
        {
            try
            {
                if (jobList == null)
                {
                    Log.Warning("ECUJob.Dump(List<ECUJob>)", "failed due job was not initialized.");
                    return;
                }
                foreach (ECUJob job in jobList)
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

        public static IList<string> scanECUfromFunctionalResponse(ECUJob job)
        {
            try
            {
                List<string> list = new List<string>();
                foreach (ECUResult item in job.JobResult)
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
                    IEnumerable<ECUResult> enumerable = base.JobResult.Where((ECUResult result) => string.Equals(result.Name, resultName, StringComparison.OrdinalIgnoreCase));
                    foreach (ECUResult item in enumerable)
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
            try
            {
                FASTARelevant = defRelevant;
                if (base.JobResult != null)
                {
                    foreach (ECUResult item in base.JobResult)
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
            try
            {
                FASTARelevant = true;
                ushort num2 = startSet;
                if (base.JobResult != null)
                {
                    num2 = ((stopSet < 0) ? ((ushort)(base.JobResultSets + stopSet + 1)) : ((ushort)stopSet));
                    {
                        foreach (ECUResult item in base.JobResult)
                        {
                            if (item.FASTARelevant || item.Set < startSet || item.Set > num2)
                            {
                                continue;
                            }
                            if (item.Name.Equals(resultName, StringComparison.InvariantCultureIgnoreCase))
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
