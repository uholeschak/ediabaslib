using log4net.Repository.Hierarchy;
using PsdzClient.Core;
using System;
using System.Management;
using static EdiabasLib.EdiabasNet;

namespace PsdzClient.Utility
{
    internal static class WMIInfo
    {
        public static bool ExceptionAtStartup = true;

        public static string GetWMIInfo(string segment, string info, string argument = null)
        {
            SelectQuery query = CreateQueryFor(segment, info, argument);
            ManagementObjectSearcher managementObjectSearcher = null;
            try
            {
                managementObjectSearcher = new ManagementObjectSearcher(query);
                return GetSearchResult(info, managementObjectSearcher);
            }
            finally
            {
                managementObjectSearcher?.Dispose();
            }
        }

        public static string GetWMIInfoITools(string segment, string info, string argument = null)
        {
            ExceptionAtStartup = false;
            ManagementObjectSearcher managementObjectSearcher = null;
            try
            {
                SelectQuery query = CreateQueryFor(segment, info, argument);
                managementObjectSearcher = new ManagementObjectSearcher(new ManagementScope("root\\ITOOLS"), query);
                return GetSearchResult(info, managementObjectSearcher);
            }
            catch (ManagementException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0001, "WMIInfo.GetWMIInfoITools", "Could not retrieve WMIInfoITools", EventKind.Technical, LogLevel.Warning, exception);
                return string.Empty;
            }
            finally
            {
                managementObjectSearcher?.Dispose();
            }
        }

        private static string GetSearchResult(string info, ManagementObjectSearcher searcher)
        {
            try
            {
                string text = string.Empty;
                foreach (ManagementBaseObject item in searcher.Get())
                {
                    text = GetObjectValue(info, text, item);
                    if (!string.IsNullOrEmpty(text))
                    {
                        break;
                    }
                }
                searcher.Dispose();
                ExceptionAtStartup = false;
                return text;
            }
            catch (Exception)
            {
                return string.Empty;
            }
            finally
            {
                searcher?.Dispose();
            }
        }

        private static string GetObjectValue(string info, string result, ManagementBaseObject obj)
        {
            try
            {
                result = obj.GetPropertyValue(info).ToString().Trim();
                obj.Dispose();
                return result;
            }
            catch (NullReferenceException)
            {
                //Logger.Instance()?.Log(ICSEventId.ICS0123, "WMIInfo.GetObjectValue", ex.Message, EventKind.Technical, LogLevel.Warning, ex);
                return string.Empty;
            }
        }

        private static SelectQuery CreateQueryFor(string segment, string info, string argument)
        {
            SelectQuery selectQuery = new SelectQuery(segment);
            selectQuery.SelectedProperties.Add(info);
            if (!string.IsNullOrEmpty(argument))
            {
                selectQuery.Condition = argument;
            }
            return selectQuery;
        }
    }
}