using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security;

namespace PsdzClient.Utility
{
    internal static class MachineIdentifier
    {
        public static string GetVolumeSerialNumber()
        {
            string pathRoot = Path.GetPathRoot(System.Environment.SystemDirectory);
            pathRoot = pathRoot.Replace("\\", string.Empty);
            string argument = "DeviceID =\"" + pathRoot + "\"";
            string text = WMIInfo.GetWMIInfo("Win32_LogicalDisk", "VolumeSerialNumber", argument);
            if (string.IsNullOrEmpty(text))
            {
                text = "a";
            }

            text = text.Replace("-", string.Empty);
            int num = 1;
            while (text.Length < 8)
            {
                text += num.ToString(CultureInfo.InvariantCulture);
                num++;
            }

            Logger.Instance()?.Log(ICSEventId.ICSNone, "MachineIdentifier.GetVolumeSerialNumber", "Volume serial number: " + text, EventKind.Technical, LogLevel.Info);
            return text.Substring(0, 8);
        }

        public static string GetMachineGuid()
        {
            string text = string.Empty;
            RegistryKey registryKey = null;
            RegistryKey registryKey2 = null;
            try
            {
                registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                registryKey2 = registryKey.OpenSubKey("SOFTWARE\\Microsoft\\Cryptography\\");
                if (registryKey2 == null || registryKey2.GetValue("MachineGuid") == null)
                {
                    return text;
                }

                text = registryKey2.GetValue("MachineGuid").ToString();
                if (string.IsNullOrEmpty(text) || text.Length < 16)
                {
                    return text;
                }

                return string.IsNullOrEmpty(text) ? string.Empty : text.ToUpper().Replace("-", string.Empty);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Instance()?.Log(ICSEventId.ICS0005, "MachineIdentifier.GetMachineGuid", $"The user does not have the necessary registry rights: Error  {ex}", EventKind.Technical, LogLevel.Error, ex);
            }
            catch (SecurityException ex2)
            {
                Logger.Instance()?.Log(ICSEventId.ICS0131, "MachineIdentifier.GetMachineGuid", $"The user does not have the permissions required to perform this action.: Error  {ex2}", EventKind.Technical, LogLevel.Error, ex2);
            }
            catch (IOException ex3)
            {
                Logger.Instance()?.Log(ICSEventId.ICS0117, "MachineIdentifier.GetMachineGuid", $"The RegistryKey that contains the specified value has been marked for deletion.: Error  {ex3}", EventKind.Technical, LogLevel.Error, ex3);
            }
            finally
            {
                registryKey?.Dispose();
                registryKey2?.Dispose();
            }

            return text;
        }

        public static string GetUuid()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("cmd", "/c wmic csproduct get UUID")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            char[] trimChars = new char[4]
            {
                ' ',
                '\n',
                '\r',
                '\t'
            };
            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            string text = process.StandardOutput.ReadToEnd().Replace("UUID", string.Empty).Replace("-", string.Empty).Trim(trimChars).ToUpper();
            Logger.Instance()?.Log(ICSEventId.ICS0117, "MachineIdentifier.GetUuid", "id: " + text, EventKind.Technical, LogLevel.Info);
            process.Close();
            return text;
        }
    }
}