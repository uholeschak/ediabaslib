using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using EdiabasLib;
using InTheHand.Net.Sockets;
using Microsoft.Win32;
using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;

namespace EdiabasLibConfigTool
{
    public static class Patch
    {
        public const string AdapterSsidEnet = @"Deep OBD BMW";
        public const string AdapterSsidElm1 = @"WiFi_OBDII";
        public const string AdapterSsidElm2 = @"WiFi-OBDII";
        public const string AdapterSsidEspLink = @"DeepOBD";
        public const string AdapterSsidEnetLink = @"ENET-LINK_";
        public const string PassordWifiEnetLink = @"12345678";
        public const string AdapterSsidModBmw = @"modBMW ENET";
        public const string PassordWifiModBmw = @"12345678";
        public const string AdapterSsidUniCar = @"UniCarScan";
        public const string PassordWifiUniCar = @"12345678";
        public const string ApiDirName = @"Api32";
        public const string Api32DllName = @"api32.dll";
        public const string Api64DllName = @"api64.dll";
        public const string Api32DllBackupName = @"api32.backup.dll";
        public const string Api64DllBackupName = @"api64.backup.dll";
        public const string ConfigFileName = @"EdiabasLib.config";
        public const string IniFileName = @"EDIABAS.INI";
        public const string ReingoldRegKey = @"SOFTWARE\BMWGroup\ISPI\Rheingold";
        public const string IstaBinPath = @"BMW.Rheingold.ISTAGUI.BinPathModifications";
        public const string SectionConfig = @"Configuration";
        public const string KeyInterface = @"Interface";
        private static readonly string[] RuntimeFiles = { "api-ms-win*.dll", "ucrtbase.dll", "msvcp140.dll", "vcruntime140.dll" };

        static class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
            public static extern int LoadLibrary(string dllToLoad);

            [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
            public static extern IntPtr GetProcAddress(int hModule, string procedureName);

            [DllImport("kernel32.dll")]
            public static extern bool FreeLibrary(int hModule);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern bool SetDllDirectory(string lpPathName);

            [DllImport("kernel32.dll")]
            public static extern ErrorModes SetErrorMode(ErrorModes uMode);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

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

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static string NormalizePath(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    return null;
                }

                return Path.GetFullPath(new Uri(path).LocalPath)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .ToUpperInvariant();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool PathStartWith(string fullPath, string subPath)
        {
            try
            {
                string fullPathNorm = NormalizePath(fullPath);
                string subPathNorm = NormalizePath(subPath);
                if (string.IsNullOrEmpty(fullPathNorm) || string.IsNullOrEmpty(subPathNorm))
                {
                    return false;
                }

                if (fullPathNorm.IndexOf(subPathNorm, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }

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

        public static long ConvertApiVersion(string apiVersion)
        {
            try
            {
                if (string.IsNullOrEmpty(apiVersion))
                {
                    return 0;
                }

                long apiVerNum = 0;
                string[] versionParts = apiVersion.Split('.');
                if (versionParts.Length == 3)
                {
                    if (!long.TryParse(versionParts[0], out long verH))
                    {
                        return 0;
                    }

                    if (!long.TryParse(versionParts[1], out long verM))
                    {
                        return 0;
                    }

                    if (!long.TryParse(versionParts[2], out long verL))
                    {
                        return 0;
                    }

                    apiVerNum = ((verH & 0x0F) << 8) + ((verM & 0x0F) << 4) + (verL & 0x0F);
                }

                return apiVerNum;
            }
            catch (Exception)
            {
                return 0;
            }
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

            int hDll = NativeMethods.LoadLibrary(fileName);
            try
            {
                if (hDll == 0)
                {
                    return null;
                }

                IntPtr pApiCheckVersion = NativeMethods.GetProcAddress(hDll, "__apiCheckVersion");
                if (pApiCheckVersion == IntPtr.Zero)
                {
                    return null;
                }

                ApiCheckVersion apiCheckVersion = Marshal.GetDelegateForFunctionPointer(pApiCheckVersion, typeof(ApiCheckVersion)) as ApiCheckVersion;
                if (apiCheckVersion == null)
                {
                    return null;
                }

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

        public static bool UpdateIniFile(string fileName, string section, string key, string value, bool onlyExisting = false)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(section) || string.IsNullOrEmpty(key))
                {
                    return false;
                }

                if (!File.Exists(fileName))
                {
                    return false;
                }

                StringBuilder sb = new StringBuilder(1000);
                int result = NativeMethods.GetPrivateProfileString(section, key, null, sb, sb.Capacity, fileName);
                if (onlyExisting)
                {
                    if (result == 0)
                    {
                        result = NativeMethods.GetPrivateProfileString(section, key, "-", sb, sb.Capacity, fileName);
                        if (result == 1)
                        {
                            return false;
                        }
                    }
                }

                if (string.Compare(sb.ToString().Trim(), value.Trim(), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return false;
                }

                NativeMethods.WritePrivateProfileString(section, key, value, fileName);
                return true;
            }
            catch (Exception)
            {
                // ignored
            }
            return false;
        }

        public static bool UpdateConfigFile(string configFile, string iniFile, RegistryView? registryViewIsta, int adapterType, BluetoothDeviceInfo devInfo, WlanInterface wlanIface, EdInterfaceEnet.EnetConnection enetConnection, string pin)
        {
            try
            {
                XDocument xDocument = XDocument.Load(configFile);
                XElement settingsNode = xDocument.Root?.Element("appSettings");
                if (settingsNode == null)
                {
                    return false;
                }

                string interfaceValue = @"STD:OBD";
                if (configFile.ToLowerInvariant().Contains(@"\SIDIS\home\DBaseSys2\".ToLowerInvariant()))
                {   // VAS-PC instalation
                    interfaceValue = @"EDIC";
                }

                if (wlanIface != null)
                {
                    WlanConnectionAttributes conn = wlanIface.CurrentConnection;
                    string ssidString = Encoding.ASCII.GetString(conn.wlanAssociationAttributes.dot11Ssid.SSID).TrimEnd('\0');
                    if (string.Compare(ssidString, AdapterSsidEnet, StringComparison.OrdinalIgnoreCase) == 0 ||
                        ssidString.StartsWith(Patch.AdapterSsidEnetLink, StringComparison.OrdinalIgnoreCase) ||
                        ssidString.StartsWith(Patch.AdapterSsidModBmw, StringComparison.OrdinalIgnoreCase) ||
                        ssidString.StartsWith(Patch.AdapterSsidUniCar, StringComparison.OrdinalIgnoreCase))
                    {
                        UpdateConfigNode(settingsNode, @"EnetRemoteHost", EdInterfaceEnet.AutoIp + EdInterfaceEnet.AutoIpAll);
                        UpdateConfigNode(settingsNode, @"EnetVehicleProtocol", EdInterfaceEnet.ProtocolHsfz);
                        UpdateConfigNode(settingsNode, KeyInterface, @"ENET");
                        UpdateIniFile(iniFile, SectionConfig, KeyInterface, @"ENET", true);
                    }
                    else
                    {
                        UpdateConfigNode(settingsNode, @"ObdComPort", "DEEPOBDWIFI");
                        UpdateConfigNode(settingsNode, KeyInterface, interfaceValue);
                        UpdateIniFile(iniFile, SectionConfig, KeyInterface, interfaceValue, true);
                    }
                    UpdateConfigNode(settingsNode, @"ObdKeepConnectionOpen", "0");
                }
                else if (devInfo != null)
                {
                    string portValue = string.Format("BLUETOOTH:{0}#{1}", devInfo.DeviceAddress, pin);

                    UpdateConfigNode(settingsNode, @"ObdComPort", portValue);
                    UpdateConfigNode(settingsNode, KeyInterface, interfaceValue);
                    UpdateIniFile(iniFile, SectionConfig, KeyInterface, interfaceValue, true);

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
                else if (enetConnection != null)
                {
                    UpdateConfigNode(settingsNode, @"EnetRemoteHost", EdInterfaceEnet.AutoIp + EdInterfaceEnet.AutoIpAll);

                    string vehicleProtocol = enetConnection.ConnectionType == EdInterfaceEnet.EnetConnection.InterfaceType.DirectDoIp ?
                        EdInterfaceEnet.ProtocolDoIp : EdInterfaceEnet.ProtocolHsfz;
                    UpdateConfigNode(settingsNode, @"EnetVehicleProtocol", vehicleProtocol);
                    UpdateConfigNode(settingsNode, KeyInterface, @"ENET");
                    UpdateIniFile(iniFile, SectionConfig, KeyInterface, @"ENET", true);
                    UpdateConfigNode(settingsNode, @"ObdKeepConnectionOpen", "0");
                }
                else
                {
                    return false;
                }
                xDocument.Save(configFile);

                string ediabasBinPath = Path.GetDirectoryName(configFile);
                PatchIstaReg(registryViewIsta, ediabasBinPath);
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

                // 32 bit
                string sourceDll32 = Path.Combine(sourceDir, Api32DllName);
                if (!File.Exists(sourceDll32))
                {
                    sr.Append("\r\n");
                    sr.Append(string.Format(Resources.Strings.PatchApiDllMissing, Api32DllName));
                    return false;
                }
                string version32 = EdiabasLibVersion(sourceDll32, false);
                if (string.IsNullOrEmpty(version32))
                {
                    CopyRuntimeRequired = true;
                    version32 = EdiabasLibVersion(sourceDll32, true);
                    if (string.IsNullOrEmpty(version32))
                    {
                        sr.Append("\r\n");
                        sr.Append(string.Format(Resources.Strings.PatchLoadApiDllFailed, Api32DllName));
                        return false;
                    }
                }
                sr.Append("\r\n");
                sr.Append(string.Format(Resources.Strings.PatchApiVersion, version32));

                long apiVerNum32 = ConvertApiVersion(version32);
                if (apiVerNum32 <= 0 || apiVerNum32 > EdiabasNet.EdiabasVersion)
                {
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.PatchApiVersionUnknown);
                }

                // 64 bit
                string sourceDll64 = Path.Combine(sourceDir, Api64DllName);
                if (!File.Exists(sourceDll64))
                {
                    sr.Append("\r\n");
                    sr.Append(string.Format(Resources.Strings.PatchApiDllMissing, Api64DllName));
                    return false;
                }
#if false
                string version64 = EdiabasLibVersion(sourceDll64, false);
                if (string.IsNullOrEmpty(version64))
                {
                    CopyRuntimeRequired = true;
                    version64 = EdiabasLibVersion(sourceDll64, true);
                    if (string.IsNullOrEmpty(version64))
                    {
                        sr.Append("\r\n");
                        sr.Append(string.Format(Resources.Strings.PatchLoadApiDllFailed, Api64DllName));
                        return false;
                    }
                }
                sr.Append("\r\n");
                sr.Append(string.Format(Resources.Strings.PatchApiVersion, version64));
#endif
                // 32 bit
                string dllFile32 = Path.Combine(dirName, Api32DllName);
                bool dll32Exits = File.Exists(dllFile32);
                if (!dll32Exits)
                {
                    sr.Append("\r\n");
                    sr.Append(string.Format(Resources.Strings.PatchOriginalApiDllMissing, Api32DllName));
                    return false;
                }

                // 64 bit
                string dllFile64 = Path.Combine(dirName, Api64DllName);
                bool dll64Exits = File.Exists(dllFile64);
                if (!dll64Exits)
                {
                    sr.Append("\r\n");
                    sr.Append(string.Format(Resources.Strings.PatchOriginalApiDllMissing, Api64DllName));
                    // accept missing file
                    //return false;
                }

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (dll32Exits)
                {
                    string dllFile32Backup = Path.Combine(dirName, Api32DllBackupName);
                    if (!File.Exists(dllFile32Backup))
                    {
                        if (IsOriginalDll(dllFile32))
                        {
                            sr.Append("\r\n");
                            sr.Append(string.Format(Resources.Strings.PatchCreateBackupFile, Api32DllBackupName));
                            File.Copy(dllFile32, dllFile32Backup, false);
                        }
                    }
                    else
                    {
                        sr.Append("\r\n");
                        sr.Append(string.Format(Resources.Strings.PatchBackupFileExisting, Api32DllBackupName));
                    }

                    if (!IsOriginalDll(dllFile32Backup))
                    {
                        sr.Append("\r\n");
                        sr.Append(string.Format(Resources.Strings.PatchNoValidBackupFile, Api32DllName));
                        return false;
                    }

                    File.Copy(sourceDll32, dllFile32, true);
                }

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (dll64Exits)
                {
                    string dllFile64Backup = Path.Combine(dirName, Api64DllBackupName);
                    if (!File.Exists(dllFile64Backup))
                    {
                        if (IsOriginalDll(dllFile64))
                        {
                            sr.Append("\r\n");
                            sr.Append(string.Format(Resources.Strings.PatchCreateBackupFile, Api64DllBackupName));
                            File.Copy(dllFile64, dllFile64Backup, false);
                        }
                    }
                    else
                    {
                        sr.Append("\r\n");
                        sr.Append(string.Format(Resources.Strings.PatchBackupFileExisting, Api64DllBackupName));
                    }

                    if (!IsOriginalDll(dllFile64Backup))
                    {
                        sr.Append("\r\n");
                        sr.Append(string.Format(Resources.Strings.PatchNoValidBackupFile, Api64DllName));
                        return false;
                    }

                    File.Copy(sourceDll64, dllFile64, true);
                }

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

        public static bool RestoreFiles(StringBuilder sr, string dirName, RegistryView? registryViewIsta)
        {
            try
            {
                string dllFile32 = Path.Combine(dirName, Api32DllName);
                string dllFile32Backup = Path.Combine(dirName, Api32DllBackupName);
                if (!File.Exists(dllFile32Backup))
                {
                    sr.Append("\r\n");
                    sr.Append(string.Format(Resources.Strings.RestoreNoBackupFile, Api32DllBackupName));
                }
                else
                {
                    File.Copy(dllFile32Backup, dllFile32, true);
                    File.Delete(dllFile32Backup);
                    sr.Append("\r\n");
                    sr.Append(string.Format(Resources.Strings.RestoredApiDll, Api32DllName));
                }
            }
            catch (Exception)
            {
                sr.Append("\r\n");
                sr.Append(string.Format(Resources.Strings.RestoreApiDllFailed, Api32DllName));
                return false;
            }

            try
            {
                string dllFile64 = Path.Combine(dirName, Api64DllName);
                string dllFile64Backup = Path.Combine(dirName, Api64DllBackupName);
                if (!File.Exists(dllFile64Backup))
                {
                    sr.Append("\r\n");
                    sr.Append(string.Format(Resources.Strings.RestoreNoBackupFile, Api64DllBackupName));
                }
                else
                {
                    File.Copy(dllFile64Backup, dllFile64, true);
                    File.Delete(dllFile64Backup);
                    sr.Append("\r\n");
                    sr.Append(string.Format(Resources.Strings.RestoredApiDll, Api64DllName));
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
                sr.Append(string.Format(Resources.Strings.RestoreApiDllFailed, Api64DllName));
                return false;
            }

            PatchIstaReg(registryViewIsta);
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
                string dllFile = Path.Combine(dirName, Api32DllName);
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

                string dllFile32 = Path.Combine(dirName, Api32DllName);
                if (File.Exists(dllFile32))
                {
                    if (!IsOriginalDll(dllFile32))
                    {
                        return true;
                    }
                }

                string dllFile64 = Path.Combine(dirName, Api64DllName);
                if (File.Exists(dllFile64))
                {
                    if (!IsOriginalDll(dllFile64))
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        public static bool PatchEdiabas(StringBuilder sr, PatchType patchType, int adapterType, string dirName, BluetoothDeviceInfo devInfo, WlanInterface wlanIface, EdInterfaceEnet.EnetConnection enetConnection, string pin)
        {
            try
            {
                sr.AppendFormat(Resources.Strings.PatchDirectory, dirName);
                if (!PatchFiles(sr, dirName))
                {
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.PatchConfigUpdateFailed);
                    return false;
                }

                string configFile = Path.Combine(dirName, ConfigFileName);
                string iniFile = null;

                RegistryView? registryViewIsta = null;
                if (patchType == PatchType.Istad)
                {
                    iniFile = Path.Combine(dirName, IniFileName);

                    using (RegistryKey localMachine64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                    {
                        using (RegistryKey key = localMachine64.OpenSubKey(ReingoldRegKey, false))
                        {
                            if (key != null)
                            {
                                registryViewIsta = RegistryView.Registry64;
                            }
                        }
                    }

                    using (RegistryKey localMachine32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                    {
                        using (RegistryKey key = localMachine32.OpenSubKey(ReingoldRegKey, false))
                        {
                            if (key != null)
                            {
                                registryViewIsta = RegistryView.Registry32;
                            }
                        }
                    }
                }

                if (!UpdateConfigFile(configFile, iniFile, registryViewIsta, adapterType, devInfo, wlanIface, enetConnection, pin))
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
            if (!RestoreFiles(sr, dirName, null))
            {
                return false;
            }
            return true;
        }

        public static bool PatchIstaReg(RegistryView? registryViewIsta, string ediabasBinLocation = null)
        {
            if (registryViewIsta == null)
            {
                return false;
            }

            try
            {
                using (RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryViewIsta.Value))
                {
                    using (RegistryKey key = localMachine.OpenSubKey(ReingoldRegKey, true))
                    {
                        if (key != null)
                        {
                            if (!string.IsNullOrEmpty(ediabasBinLocation))
                            {
                                key.SetValue(IstaBinPath, ediabasBinLocation);
                            }
                            else
                            {
                                key.DeleteValue(IstaBinPath, false);
                            }
                            return true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }
    }
}
