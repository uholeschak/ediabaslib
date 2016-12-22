using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using InTheHand.Net.Sockets;
using SimpleWifi.Win32;

namespace EdiabasLibConfigTool
{
    static public class Patch
    {
        private const string ApiDllName = @"api32.dll";
        private const string ApiDllBackupName = @"api32.backup.dll";
        private const string ConfigFileName = @"EdiabasLib.config";

        static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibrary(string dllToLoad);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

            [DllImport("kernel32.dll")]
            public static extern bool FreeLibrary(IntPtr hModule);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate int ApiCheckVersion(int versionCompatibility, sbyte[] versionInfo);

        public enum PatchType
        {
            Ediabas,
            VasPc,
            Istad,
        }

        static public string AssemblyDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        static public bool IsOriginalDll(string fileName)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    return false;
                }
                FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(fileName);
                if (string.IsNullOrEmpty(fileVersion.LegalCopyright))
                {
                    return false;
                }
                if (!fileVersion.LegalCopyright.ToLowerInvariant().Contains("softing"))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        static public void UpdateConfigNode(XElement settingsNode, string key, string value, bool onlyExisting = false)
        {
            XElement node = (from addNode in settingsNode.Elements("add")
                             let keyAttrib = addNode.Attribute("key")
                             where keyAttrib != null
                             where string.Compare(keyAttrib.Value, key, StringComparison.OrdinalIgnoreCase) == 0
                             select addNode).FirstOrDefault();
            if (node == null)
            {
                if (onlyExisting)
                {
                    return;
                }
                node = new XElement("add");
                node.Add(new XAttribute("key", key));
                settingsNode.AddFirst(node);
            }
            XAttribute valueAttrib = node.Attribute("value");
            if (valueAttrib == null)
            {
                valueAttrib = new XAttribute("value", value);
                node.Add(valueAttrib);
            }
            else
            {
                valueAttrib.Value = value;
            }
        }

        static public bool UpdateConfigFile(string fileName, BluetoothDeviceInfo devInfo, WlanInterface wlanIface, string pin)
        {
            try
            {
                XDocument xDocument = XDocument.Load(fileName);
                XElement settingsNode = xDocument.Root?.Element("appSettings");
                if (settingsNode == null)
                {
                    return false;
                }
                if (wlanIface != null)
                {
                    UpdateConfigNode(settingsNode, @"EnetRemoteHost", @"auto:all");
                    UpdateConfigNode(settingsNode, @"Interface", @"ENET");
                }
                else if (devInfo != null)
                {
                    string interfaceValue = @"STD:OBD";
                    if (fileName.ToLowerInvariant().Contains(@"\SIDIS\home\DBaseSys2\".ToLowerInvariant()))
                    {   // VAS-PC instalation
                        interfaceValue = @"EDIC";
                    }
                    string portValue = string.Format("BLUETOOTH:{0}#{1}", devInfo.DeviceAddress, pin);

                    UpdateConfigNode(settingsNode, @"ObdComPort", portValue);
                    UpdateConfigNode(settingsNode, @"Interface", interfaceValue);
                }
                else
                {
                    return false;
                }
                xDocument.Save(fileName);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool PatchFiles(StringBuilder sr, string dirName)
        {
            try
            {
                string sourceDir = AssemblyDirectory;
                string sourceDll = Path.Combine(sourceDir, ApiDllName);
                if (!File.Exists(sourceDll))
                {
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.PatchApi32Missing);
                    return false;
                }
                IntPtr hDll = NativeMethods.LoadLibrary(sourceDll);
                try
                {
                    if (hDll == IntPtr.Zero)
                    {
                        sr.Append("\r\n");
                        sr.Append(Resources.Strings.PatchLoadApi32Failed);
                        return false;
                    }
                    IntPtr pApiCheckVersion = NativeMethods.GetProcAddress(hDll, "__apiCheckVersion");
                    if (pApiCheckVersion == IntPtr.Zero)
                    {
                        sr.Append("\r\n");
                        sr.Append(Resources.Strings.PatchLoadApi32Failed);
                        return false;
                    }
                    ApiCheckVersion apiCheckVersion = (ApiCheckVersion) Marshal.GetDelegateForFunctionPointer(pApiCheckVersion, typeof (ApiCheckVersion));
                    sbyte[] versionInfo = new sbyte[0x100];
                    if (apiCheckVersion(0x700, versionInfo) == 0)
                    {
                        sr.Append("\r\n");
                        sr.Append(Resources.Strings.PatchLoadApi32Failed);
                        return false;
                    }
                    string version = Encoding.ASCII.GetString(versionInfo.TakeWhile(value => value != 0).Select(value => (byte) value).ToArray());
                    sr.Append("\r\n");
                    sr.Append(string.Format(Resources.Strings.PatchApiVersion, version));
                }
                catch (Exception)
                {
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.PatchLoadApi32Failed);
                    return false;
                }
                finally
                {
                    if (hDll != IntPtr.Zero)
                    {
                        NativeMethods.FreeLibrary(hDll);
                    }
                }

                string dllFile = Path.Combine(dirName, ApiDllName);
                string dllFileBackup = Path.Combine(dirName, ApiDllBackupName);
                if (!File.Exists(dllFileBackup) && IsOriginalDll(dllFile))
                {
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.PatchCreateBackupFile);
                    File.Copy(dllFile, dllFileBackup, false);
                }
                else
                {
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.PatchBackupFileExisting);
                }

                if (!IsOriginalDll(dllFileBackup))
                {
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.PatchNoValidBackupFile);
                    return false;
                }
                File.Copy(sourceDll, dllFile, true);
                string sourceConfig = Path.Combine(sourceDir, ConfigFileName);
                if (!File.Exists(sourceConfig))
                {
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.PatchConfigMissing);
                    return false;
                }
                string configFile = Path.Combine(dirName, ConfigFileName);
                if (!File.Exists(configFile))
                {
                    File.Copy(sourceConfig, configFile, false);
                }
                else
                {
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.PatchConfigExisting);
                }
            }
            catch (Exception)
            {
                sr.Append("\r\n");
                sr.Append(Resources.Strings.PatchCopyFailed);
                return false;
            }
            return true;
        }

        public static bool RestoreFiles(StringBuilder sr, string dirName)
        {
            try
            {
                string dllFile = Path.Combine(dirName, ApiDllName);
                string dllFileBackup = Path.Combine(dirName, ApiDllBackupName);
                if (!File.Exists(dllFileBackup))
                {
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.RestoreNoBackupFile);
                }
                else
                {
                    File.Copy(dllFileBackup, dllFile, true);
                    File.Delete(dllFileBackup);
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.RestoredApi32);
                }
            }
            catch (Exception)
            {
                sr.Append("\r\n");
                sr.Append(Resources.Strings.RestoreApi32Failed);
                return false;
            }
            return true;
        }

        public static bool IsValid(string dirName)
        {
            try
            {
                if (string.IsNullOrEmpty(dirName))
                {
                    return false;
                }
                string dllFile = Path.Combine(dirName, ApiDllName);
                if (!File.Exists(dllFile))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool IsPatched(string dirName)
        {
            try
            {
                if (string.IsNullOrEmpty(dirName))
                {
                    return false;
                }
                string dllFileBackup = Path.Combine(dirName, ApiDllBackupName);
                if (!File.Exists(dllFileBackup))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool PatchEdiabas(StringBuilder sr, PatchType patchType, string dirName, BluetoothDeviceInfo devInfo, WlanInterface wlanIface, string pin)
        {
            try
            {
                sr.AppendFormat(Resources.Strings.PatchDirectory, dirName);
                if (!PatchFiles(sr, dirName))
                {
                    return false;
                }
                string configFile = Path.Combine(dirName, ConfigFileName);
                if (!UpdateConfigFile(configFile, devInfo, wlanIface, pin))
                {
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.PatchConfigUpdateFailed);
                    return false;
                }
                sr.Append("\r\n");
                sr.Append(Resources.Strings.PatchConfigUpdateOk);
                switch (patchType)
                {
                    case PatchType.Istad:
                        sr.Append("\r\n");
                        sr.Append(Resources.Strings.PatchInstadInfo);
                        break;

                    case PatchType.VasPc:
                        sr.Append("\r\n");
                        sr.Append(Resources.Strings.PatchVaspcInfo);
                        break;
                }
            }
            catch (Exception)
            {
                sr.Append("\r\n");
                sr.Append(Resources.Strings.PatchConfigUpdateFailed);
                return false;
            }
            return true;
        }

        public static bool RestoreEdiabas(StringBuilder sr, string dirName)
        {
            sr.AppendFormat(Resources.Strings.RestoreDirectory, dirName);
            if (!RestoreFiles(sr, dirName))
            {
                return false;
            }
            return true;
        }
    }
}
