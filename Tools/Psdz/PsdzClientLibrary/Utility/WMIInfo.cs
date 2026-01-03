using System;
using System.Management;
using System.Windows;

#pragma warning disable CS0168
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
            catch (ManagementException exception)
            {
                Logger.Instance()?.Log(ICSEventId.ICS0001, "WMIInfo.GetWMIInfoITools", "Could not retrieve WMIInfoITools", EventKind.Technical, LogLevel.Warning, exception);
                return string.Empty;
            }
            finally
            {
                managementObjectSearcher?.Dispose();
            }
        }

        [PreserveSource(Hint = "MessageBox disabled", OriginalHash = "E315C7CE7D5B280E5354CDC03BB03EC8")]
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
            catch (Exception exception)
            {
                if (ExceptionAtStartup)
                {
                    Logger.Instance()?.Log(ICSEventId.ICS0006, "WMIInfo.GetSearchResult", "Could not retrieve WMIInfo, possible WMI repository corruption", EventKind.Technical, LogLevel.Error, exception);
                    // [UH] [IGNORE] string caption = "ISPI AdminClient error ICS0006";
                    // [UH] [IGNORE] MessageBox.Show("Kritischer Fehler beim Lesen der lokalen WMI-Datenbank: Repository  ist möglicherweise beschädigt oder Windows Management Instrumentation Service muss neu gestartet werden. Bitte öffnen Sie ein Ticket für den technischen Support und geben Sie an, dass auf diesem Gerät der Fehler ICS0006 aufgetreten ist.\n\nCritical error in reading local WMI database: repository is corrupted or Windows Management Instrumentation service needs to be restarted. Please open a ticket to technical support mentioning error ICS0006 has occurred on this device.", caption, MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    throw new ManagementException("Critical error in reading WMI database: repository may be corrupted or Windows Management Instrumentation service needs to be restarted.");
                }
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
            catch (NullReferenceException ex)
            {
                Logger.Instance()?.Log(ICSEventId.ICS0123, "WMIInfo.GetObjectValue", ex.Message, EventKind.Technical, LogLevel.Warning, ex);
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