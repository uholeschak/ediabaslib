using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using InTheHand.Net.Sockets;
using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;

namespace EdiabasLibConfigTool
{
    public static class Patch
    {
        public const string AdapterSsidEnet = @"Deep OBD BMW";
        public const string AdapterSsidElm = @"WiFi_OBDII";
        public const string AdapterSsidEspLink = @"DeepOBD";
        private const string ApiDirName = @"Api32";
        private const string ApiDllName = @"api32.dll";
        private const string ApiDllBackupName = @"api32.backup.dll";
        private const string ConfigFileName = @"EdiabasLib.config";
        private static readonly string[] RuntimeFiles = { "api-ms-win*.dll", "ucrtbase.dll", "msvcp140.dll", "vcruntime140.dll" };

        static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern IntPtr LoadLibrary(string dllToLoad);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

            [DllImport("kernel32.dll")]
            public static extern bool FreeLibrary(IntPtr hModule);

            [DllImport("kernel32.dll")]
            public static extern bool SetDllDirectory(string lpPathName);

            [DllImport("kernel32.dll")]
            public static extern ErrorModes SetErrorMode(ErrorModes uMode);

            [Flags]
            // ReSharper disable UnusedMember.Local
            // ReSharper disable InconsistentNaming
            public enum ErrorModes : uint
            {
                SYSTEM_DEFAULT = 0x0,
                SEM_FAILCRITICALERRORS = 0x0001,
                SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
                SEM_NOGPFAULTERRORBOX = 0x0002,
                SEM_NOOPENFILEERRORBOX = 0x8000
            }
            // ReSharper restore UnusedMember.Local
            // ReSharper restore InconsistentNaming
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private delegate int ApiCheckVersion(int versionCompatibility, sbyte[] versionInfo);

        public enum PatchType
        {
            Ediabas,
            VasPc,
            Istad,
        }

        public static bool CopyRuntimeRequired { get; private set; }

        public static string AssemblyDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static bool IsOriginalDll(string fileName)
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

        public static List<string> GetRuntimeFiles(string dirName)
        {
            List<string> fileList = new List<string>();
            foreach (string filePattern in RuntimeFiles)
            {
                try
                {
                    string[] fileArray = Directory.GetFiles(dirName, filePattern, SearchOption.TopDirectoryOnly);
                    fileList.AddRange(fileArray);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return fileList;
        }

        public static string EdiabasLibVersion(string fileName, bool setSearchDir)
        {
            try
            {
                NativeMethods.SetErrorMode(NativeMethods.ErrorModes.SEM_FAILCRITICALERRORS);
                string searchDir = null;
                if (setSearchDir)
                {
                    searchDir = Path.GetDirectoryName(fileName);
                }
                if (!NativeMethods.SetDllDirectory(searchDir))
                {
                    return null;
                }
            }
            catch (Exception)
            {
                // ignored
            }
            IntPtr hDll = NativeMethods.LoadLibrary(fileName);
            try
            {
                if (hDll == IntPtr.Zero)
                {
                    return null;
                }
                IntPtr pApiCheckVersion = NativeMethods.GetProcAddress(hDll, "__apiCheckVersion");
                if (pApiCheckVersion == IntPtr.Zero)
                {
                    return null;
                }
                ApiCheckVersion apiCheckVersion = (ApiCheckVersion)Marshal.GetDelegateForFunctionPointer(pApiCheckVersion, typeof(ApiCheckVersion));
                sbyte[] versionInfo = new sbyte[0x100];
                if (apiCheckVersion(0x700, versionInfo) == 0)
                {
                    return null;
                }
                string version = Encoding.ASCII.GetString(versionInfo.TakeWhile(value => value != 0).Select(value => (byte)value).ToArray());
                return version;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                NativeMethods.FreeLibrary(hDll);
            }
        }

        public static void UpdateConfigNode(XElement settingsNode, string key, string value, bool onlyExisting = false)
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

        public static bool UpdateConfigFile(string fileName, int adapterType, BluetoothDeviceInfo devInfo, WlanInterface wlanIface, string pin)
        {
            try
            {
                XDocument xDocument = XDocument.Load(fileName);
                XElement settingsNode = xDocument.Root?.Element("appSettings");
                if (settingsNode == null)
                {
                    return false;
                }
                string interfaceValue = @"STD:OBD";
                if (fileName.ToLowerInvariant().Contains(@"\SIDIS\home\DBaseSys2\".ToLowerInvariant()))
                {   // VAS-PC instalation
                    interfaceValue = @"EDIC";
                }

                if (wlanIface != null)
                {
                    WlanConnectionAttributes conn = wlanIface.CurrentConnection;
                    string ssidString = Encoding.ASCII.GetString(conn.wlanAssociationAttributes.dot11Ssid.SSID).TrimEnd('\0');
                    if (string.Compare(ssidString, AdapterSsidEnet, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        UpdateConfigNode(settingsNode, @"EnetRemoteHost", @"auto:all");
                        UpdateConfigNode(settingsNode, @"Interface", @"ENET");
                    }
                    else
                    {
                        UpdateConfigNode(settingsNode, @"ObdComPort", "DEEPOBDWIFI");
                        UpdateConfigNode(settingsNode, @"Interface", interfaceValue);
                    }
                    UpdateConfigNode(settingsNode, @"ObdKeepConnectionOpen", "0");
                }
                else if (devInfo != null)
                {
                    string portValue = string.Format("BLUETOOTH:{0}#{1}", devInfo.DeviceAddress, pin);

                    UpdateConfigNode(settingsNode, @"ObdComPort", portValue);
                    UpdateConfigNode(settingsNode, @"Interface", interfaceValue);

                    string keepConnectionValue;
                    switch (adapterType)
                    {
                        case 4: // HC04
                        case 5: // SPP-UART
                            keepConnectionValue = @"1";
                            break;

                        default:
                            keepConnectionValue = @"0";
                            break;
                    }
                    UpdateConfigNode(settingsNode, @"ObdKeepConnectionOpen", keepConnectionValue);
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
                string sourceDir = Path.Combine(AssemblyDirectory, ApiDirName);
                string sourceDll = Path.Combine(sourceDir, ApiDllName);
                if (!File.Exists(sourceDll))
                {
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.PatchApi32Missing);
                    return false;
                }
                string version = EdiabasLibVersion(sourceDll, false);
                if (string.IsNullOrEmpty(version))
                {
                    CopyRuntimeRequired = true;
                    version = EdiabasLibVersion(sourceDll, true);
                    if (string.IsNullOrEmpty(version))
                    {
                        sr.Append("\r\n");
                        sr.Append(Resources.Strings.PatchLoadApi32Failed);
                        return false;
                    }
                }
                sr.Append("\r\n");
                sr.Append(string.Format(Resources.Strings.PatchApiVersion, version));

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

                if (CopyRuntimeRequired)
                {
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.PatchCopyRuntime);

                    List<string> runtimeFiles = GetRuntimeFiles(sourceDir);
                    foreach (string file in runtimeFiles)
                    {
                        string baseFile = Path.GetFileName(file);
                        if (baseFile != null)
                        {
                            string destFile = Path.Combine(dirName, baseFile);
                            File.Copy(file, destFile, true);
                        }
                    }
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
                List<string> runtimeFiles = GetRuntimeFiles(dirName);
                foreach (string file in runtimeFiles)
                {
                    File.Delete(file);
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

        public static bool PatchEdiabas(StringBuilder sr, PatchType patchType, int adapterType, string dirName, BluetoothDeviceInfo devInfo, WlanInterface wlanIface, string pin)
        {
            try
            {
                sr.AppendFormat(Resources.Strings.PatchDirectory, dirName);
                if (!PatchFiles(sr, dirName))
                {
                    return false;
                }
                string configFile = Path.Combine(dirName, ConfigFileName);
                if (!UpdateConfigFile(configFile, adapterType, devInfo, wlanIface, pin))
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
