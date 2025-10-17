using EdiabasLib;
using InTheHand.Net.Sockets;
using Microsoft.Win32;
using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Xml.Linq;
using static EdiabasLibConfigTool.FormMain;

namespace EdiabasLibConfigTool
{
    [SupportedOSPlatform("windows")]
    public static class Patch
    {
        public const string AdapterSsidEnet = @"Deep OBD BMW";
        public const string AdapterSsidElm1 = @"WiFi_OBDII";
        public const string AdapterSsidElm2 = @"WiFi-OBDII";
        public const string AdapterSsidEspLink = @"DeepOBD";
        public const string AdapterSsidEnetLink = @"ENET-LINK_";
        public const string PasswordWifiEnetLink = @"12345678";
        public const string AdapterSsidModBmw = @"modBMW ENET";
        public const string PasswordWifiModBmw = @"12345678";
        public const string AdapterSsidUniCar = @"UniCarScan";
        public const string PasswordWifiUniCar = @"12345678";
        public const string AdapterSsidMhd = @"MHD ENET";
        public const string PasswordWifiMhd = @"123456789";
        public const string EdiabasDirName = @"Ediabas";
        public const string EdiabasBinDirName = @"Bin";
        public const string ApiDirName = @"Api32";
        public const string Api32DllName = @"api32.dll";
        public const string Api64DllName = @"api64.dll";
        public const string Api32DllBackupName = @"api32.backup.dll";
        public const string Api64DllBackupName = @"api64.backup.dll";
        public const string ConfigFileName = @"EdiabasLib.config";
        public const string IniFileName = @"EDIABAS.INI";
        public const string RegKeyReingold = @"SOFTWARE\BMWGroup\ISPI\Rheingold";
        public const string RegKeyFtdiBus = @"SYSTEM\CurrentControlSet\Enum\FTDIBUS";
        public const int VcRuntimeMajor = 14;
        public const int VcRuntimeMinor = 42;
        public const string RegKeyVcRuntime14Base = @"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes";
        public const string RegKeyVcRuntime14X86 = RegKeyVcRuntime14Base + @"\x86";
        public const string RegKeyVcRuntime14X64 = RegKeyVcRuntime14Base + @"\x64";
        public const string RegValueFtdiLatencyTimer = @"LatencyTimer";
        public const string RegKeyIsta = @"SOFTWARE\BMWGroup\ISPI\ISTA";
        public const string RegValueIstaLocation = @"InstallLocation";
        public const string RegValueIstaMainProdVer = @"MainProductVersion";
        public const string RegKeyRheingoldNameStart = @"BMW.Rheingold.";
        public const string RegKeyIstaBinPath = @"BMW.Rheingold.ISTAGUI.BinPathModifications";
        public const string RegKeyIstaIdesBinPath = @"BMW.Rheingold.ISTAGUI.EdiabasIDESBinPathModifications";
        public const string RegKeyIstaBinFull = RegKeyReingold + @": " + RegKeyIstaBinPath;
        public const string RegKeyIstaIdesBinFull = RegKeyReingold + @": " + RegKeyIstaIdesBinPath;
        public const string RegKeyIstaOpMode = @"BMW.Rheingold.OperationalMode";
        public const string SectionConfig = @"Configuration";
        public const string KeyInterface = @"Interface";
        public const string VcRedistX32Link = @"https://aka.ms/vs/17/release/vc_redist.x86.exe";
        public const string VcRedistX64Link = @"https://aka.ms/vs/17/release/vc_redist.x64.exe";
        public const string DcanKlineLink = @"https://uholeschak.github.io/ediabaslib/docs/Build_Bluetooth_D-CAN_adapter.html";
        private static readonly string[] RuntimeFiles = { "api-ms-win*.dll", "ucrtbase.dll", "msvcp140.dll", "vcruntime140.dll" };

        static class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
            public static extern int LoadLibrary(string dllToLoad);

            [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
            public static extern IntPtr GetProcAddress(int hModule, string procedureName);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool FreeLibrary(int hModule);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern bool SetDllDirectory(string lpPathName);

            [DllImport("kernel32.dll")]
            public static extern ErrorModes SetErrorMode(ErrorModes uMode);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
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
            IstadExt,
        }

        [Flags]
        public enum PatchRegType
        {
            None = 0x00,
            Standard = 0x01,
            Ides = 0x02,
        }

        public class UsbInfo
        {
            public UsbInfo(uint locationId, int comPortNum, string comPortName, int latencyTimer, int maxRegLatencyTimer)
            {
                LocationId = locationId;
                ComPortNum = comPortNum;
                ComPortName = comPortName;
                LatencyTimer = latencyTimer;
                MaxRegLatencyTimer = maxRegLatencyTimer;
            }

            public uint LocationId { get; }
            public int ComPortNum { get; }
            public string ComPortName { get; }
            public int LatencyTimer { get; }
            public int MaxRegLatencyTimer { get; }
        }

        public static string AssemblyDirectory
        {
            get
            {
#if NET
                string location = Assembly.GetExecutingAssembly().Location;
                if (string.IsNullOrEmpty(location) || !File.Exists(location))
                {
                    return null;
                }
                return Path.GetDirectoryName(location);
#else
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    return null;
                }
                return Path.GetDirectoryName(path);
#endif
            }
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

        public static string EdiabasLibVersion(string fileName, bool setSearchDir, out int errorCode)
        {
            errorCode = 0;
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
                    errorCode = Marshal.GetLastWin32Error();
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
                    errorCode = Marshal.GetLastWin32Error();
                    return null;
                }

                IntPtr pApiCheckVersion = NativeMethods.GetProcAddress(hDll, "__apiCheckVersion");
                if (pApiCheckVersion == IntPtr.Zero)
                {
                    errorCode = Marshal.GetLastWin32Error();
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

        public static void UpdateConfigNode(XElement settingsNode, string key, string value = null, bool onlyExisting = false)
        {
            XElement node = (from addNode in settingsNode.Elements("add")
                             let keyAttrib = addNode.Attribute("key")
                             where keyAttrib != null
                             where string.Compare(keyAttrib.Value, key, StringComparison.OrdinalIgnoreCase) == 0
                             select addNode).FirstOrDefault();
            if (node == null)
            {
                if (value == null || onlyExisting)
                {
                    return;
                }
                node = new XElement("add");
                node.Add(new XAttribute("key", key));
                settingsNode.AddFirst(node);
            }
            else
            {
                if (value == null)
                {
                    node.Remove();
                    return;
                }
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

        public static bool UpdateConfigFile(PatchType patchType, string configFile, string ediabasDir, string iniFile, int adapterType,
            BluetoothItem devInfo, WlanInterface wlanIface, EdInterfaceEnet.EnetConnection enetConnection, UsbInfo usbInfo, string pin)
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

                bool istadMode;
                string enetVehicleProtocol = null;
                switch (patchType)
                {
                    case PatchType.Istad:
                    case PatchType.IstadExt:
                        istadMode = true;
                        break;

                    default:
                        istadMode = false;
                        enetVehicleProtocol = EdInterfaceEnet.ProtocolHsfz;
                        break;
                }

                bool keepConnectionConfigured = false;
                bool icomConfigured = false;
                bool portsConfigured = false;

                if (wlanIface != null)
                {
                    WlanConnectionAttributes conn = wlanIface.CurrentConnection;
                    string ssidString = Encoding.ASCII.GetString(conn.wlanAssociationAttributes.dot11Ssid.SSID).TrimEnd('\0');
                    if (string.Compare(ssidString, AdapterSsidEnet, StringComparison.OrdinalIgnoreCase) == 0 ||
                        ssidString.StartsWith(AdapterSsidEnetLink, StringComparison.OrdinalIgnoreCase) ||
                        ssidString.StartsWith(AdapterSsidModBmw, StringComparison.OrdinalIgnoreCase) ||
                        ssidString.StartsWith(AdapterSsidUniCar, StringComparison.OrdinalIgnoreCase) ||
                        ssidString.StartsWith(AdapterSsidMhd, StringComparison.OrdinalIgnoreCase))
                    {
                        UpdateConfigNode(settingsNode, @"EnetRemoteHost", EdInterfaceEnet.AutoIp + EdInterfaceEnet.AutoIpAll);
                        UpdateConfigNode(settingsNode, @"EnetVehicleProtocol", enetVehicleProtocol);
                        UpdateConfigNode(settingsNode, KeyInterface, @"ENET");
                        UpdateIniFile(iniFile, SectionConfig, KeyInterface, @"ENET", true);
                    }
                    else
                    {
                        UpdateConfigNode(settingsNode, @"ObdComPort", "DEEPOBDWIFI");
                        UpdateConfigNode(settingsNode, KeyInterface, interfaceValue);
                        UpdateIniFile(iniFile, SectionConfig, KeyInterface, interfaceValue, true);
                    }
                }
                else if (devInfo != null)
                {
                    string btParam = devInfo.Device != null ? EdBluetoothInterface.TypeBle : pin;
                    string portValue = string.Format("BLUETOOTH:{0}#{1}", devInfo.Address, btParam);

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
                    keepConnectionConfigured = true;
                }
                else if (enetConnection != null)
                {
                    UpdateConfigNode(settingsNode, @"EnetRemoteHost", EdInterfaceEnet.AutoIp + EdInterfaceEnet.AutoIpAll);
                    UpdateConfigNode(settingsNode, @"EnetVehicleProtocol", enetVehicleProtocol);
                    UpdateConfigNode(settingsNode, KeyInterface, @"ENET");
                    UpdateIniFile(iniFile, SectionConfig, KeyInterface, @"ENET", true);

                    if (enetConnection.ConnectionType == EdInterfaceEnet.EnetConnection.InterfaceType.Icom &&
                        enetConnection.DiagPort > 0)
                    {
                        UpdateConfigNode(settingsNode, @"IcomEnetRedirect_ICOM_P", "1");

                        if (!istadMode)
                        {
                            UpdateConfigNode(settingsNode, @"EnetDiagnosticPort", enetConnection.DiagPort.ToString(CultureInfo.InvariantCulture));
                            UpdateConfigNode(settingsNode, @"EnetControlPort", enetConnection.ControlPort.ToString(CultureInfo.InvariantCulture));
                            portsConfigured = true;

                            UpdateConfigNode(settingsNode, @"EnetIcomAllocate", "1");
                            icomConfigured = true;
                        }
                    }
                }
                else if (usbInfo != null)
                {
                    UpdateConfigNode(settingsNode, @"ObdComPort", usbInfo.ComPortName);
                    UpdateConfigNode(settingsNode, KeyInterface, interfaceValue);
                    UpdateIniFile(iniFile, SectionConfig, KeyInterface, interfaceValue, true);
                }
                else
                {
                    return false;
                }

                if (!keepConnectionConfigured)
                {
                    UpdateConfigNode(settingsNode, @"ObdKeepConnectionOpen");
                }

                if (!portsConfigured)
                {
                    UpdateConfigNode(settingsNode, @"EnetDiagnosticPort");
                    UpdateConfigNode(settingsNode, @"EnetControlPort");
                }

                if (!icomConfigured)
                {
                    UpdateConfigNode(settingsNode, @"EnetIcomAllocate");
                }

                if (!string.IsNullOrEmpty(ediabasDir) && Directory.Exists(ediabasDir))
                {
                    string ediabasBaseDir = Directory.GetParent(ediabasDir)?.FullName;
                    bool updateNodes = false;
                    if (!string.IsNullOrEmpty(ediabasBaseDir))
                    {
                        string securityPath = Path.Combine(ediabasBaseDir, EdInterfaceEnet.DoIpSecurityDir);
                        string sslSecurityPath = Path.Combine(securityPath, EdInterfaceEnet.DoIpSslTrustDir);
                        string s29BasePath = Path.Combine(securityPath, EdInterfaceEnet.DoIpS29Dir);
                        if (Directory.Exists(sslSecurityPath) && Directory.Exists(s29BasePath))
                        {
                            updateNodes = true;
                        }

                        if (updateNodes)
                        {
                            UpdateConfigNode(settingsNode, @"SslSecurityPath", sslSecurityPath);
                            // configured by SslSecurityPath
                            UpdateConfigNode(settingsNode, @"S29Path");
                            UpdateConfigNode(settingsNode, @"JSONRequestPath");
                            UpdateConfigNode(settingsNode, @"JSONResponsePath");
                        }
                    }
                }

                xDocument.Save(configFile);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool PatchFiles(StringBuilder sr, string dirName, bool copyOnly)
        {
            try
            {
                if (!IsVcRedistRegPresent(false))
                {
                    sr.Append("\r\n");
                    sr.Append(string.Format(Resources.Strings.PatchVCRuntimeInstalled, VcRedistX32Link));
                    return false;
                }

                if (!IsVcRedistRegPresent(true))
                {
                    sr.Append("\r\n");
                    sr.Append(string.Format(Resources.Strings.PatchVCRuntimeInstalled, VcRedistX64Link));
                    return false;
                }

                string sourceDir = Path.Combine(AssemblyDirectory, ApiDirName);
                // 32 bit
                string sourceDll32 = Path.Combine(sourceDir, Api32DllName);
                if (!File.Exists(sourceDll32))
                {
                    sr.Append("\r\n");
                    sr.Append(string.Format(Resources.Strings.PatchApiDllMissing, Api32DllName));
                    return false;
                }

                string version32 = EdiabasLibVersion(sourceDll32, false, out int _);
                if (string.IsNullOrEmpty(version32))
                {
                    version32 = EdiabasLibVersion(sourceDll32, true, out int _);
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
                    version64 = EdiabasLibVersion(sourceDll64, true);
                    if (string.IsNullOrEmpty(version64))
                    {
                        sr.Append("\r\n");
                        sr.Append(string.Format(Resources.Strings.PatchLoadApiDllFailed, Api64DllName, VcRedistLink));
                        return false;
                    }
                }
                sr.Append("\r\n");
                sr.Append(string.Format(Resources.Strings.PatchApiVersion, version64));
#endif
                string dllFile32 = Path.Combine(dirName, Api32DllName);
                string dllFile64 = Path.Combine(dirName, Api64DllName);
                if (copyOnly)
                {
                    File.Copy(sourceDll32, dllFile32, true);
                    File.Copy(sourceDll64, dllFile64, true);
                }
                else
                {
                    // 32 bit
                    bool dll32Exits = File.Exists(dllFile32);
                    if (!dll32Exits)
                    {
                        sr.Append("\r\n");
                        sr.Append(string.Format(Resources.Strings.PatchOriginalApiDllMissing, Api32DllName));
                        return false;
                    }

                    // 64 bit
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
                string dllFile32 = Path.Combine(dirName, Api32DllName);
                string dllFile32Backup = Path.Combine(dirName, Api32DllBackupName);
                if (!File.Exists(dllFile32Backup))
                {
                    if (!IsOriginalDll(dllFile32))
                    {
                        sr.Append("\r\n");
                        sr.Append(string.Format(Resources.Strings.RestoreNoBackupFile, Api32DllBackupName));
                    }
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
                    if (!IsOriginalDll(dllFile64))
                    {
                        sr.Append("\r\n");
                        sr.Append(string.Format(Resources.Strings.RestoreNoBackupFile, Api64DllBackupName));
                    }
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

            return true;
        }

        public static bool RemoveIstaReg(StringBuilder sr)
        {
            // ReSharper disable once ReplaceWithSingleAssignment.True
            bool result = true;
            if (!RemoveIstaReg(sr, RegistryView.Registry32))
            {
                result = false;
            }

            if (!RemoveIstaReg(sr, RegistryView.Registry64))
            {
                result = false;
            }

            return result;
        }

        public static bool RemoveIstaReg(StringBuilder sr, RegistryView? registryViewIsta)
        {
            if (registryViewIsta != null)
            {
                PatchRegType patchRegType = IsIstaRegPresent(registryViewIsta);
                if (patchRegType != PatchRegType.None)
                {
                    if ((patchRegType & PatchRegType.Standard) != PatchRegType.None)
                    {
                        sr.Append("\r\n");
                        sr.Append(string.Format(Resources.Strings.RemovingRegKey, RegKeyIstaBinFull));
                    }

                    if ((patchRegType & PatchRegType.Ides) != PatchRegType.None)
                    {
                        sr.Append("\r\n");
                        sr.Append(string.Format(Resources.Strings.RemovingRegKey, RegKeyIstaIdesBinFull));
                    }

                    if (!PatchIstaReg(registryViewIsta, null, patchRegType))
                    {
                        if ((patchRegType & PatchRegType.Standard) != PatchRegType.None)
                        {
                            sr.Append("\r\n");
                            sr.Append(string.Format(Resources.Strings.RemoveRegKeyFailed, RegKeyIstaBinFull));
                        }

                        if ((patchRegType & PatchRegType.Ides) != PatchRegType.None)
                        {
                            sr.Append("\r\n");
                            sr.Append(string.Format(Resources.Strings.RemoveRegKeyFailed, RegKeyIstaIdesBinFull));
                        }
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool DeleteEdiabasMachineCerts(string ediabasDir)
        {
            try
            {
                if (string.IsNullOrEmpty(ediabasDir))
                {
                    return false;
                }

                if (!Directory.Exists(ediabasDir))
                {
                    return false;
                }

                string ediabasBaseDir = Directory.GetParent(ediabasDir)?.FullName;
                if (string.IsNullOrEmpty(ediabasBaseDir))
                {
                    return true;
                }

                string s29BasePath = Path.Combine(ediabasBaseDir, EdInterfaceEnet.DoIpSecurityDir, EdInterfaceEnet.DoIpS29Dir);
                if (!Directory.Exists(s29BasePath))
                {
                    return true;
                }

                bool result = true;
                string certPath = Path.Combine(s29BasePath, EdInterfaceEnet.DoIpCertificatesDir);
                if (Directory.Exists(certPath))
                {
                    string machineName = EdSec4Diag.GetMachineName();
                    string machinePrivateFile = Path.Combine(certPath, machineName + ".p12");
                    string machinePublicFile = Path.Combine(certPath, machineName + EdSec4Diag.S29MachinePublicName);

                    if (File.Exists(machinePrivateFile))
                    {
                        try
                        {
                            File.Delete(machinePrivateFile);
                        }
                        catch (Exception)
                        {
                            result = false;
                        }
                    }

                    if (File.Exists(machinePublicFile))
                    {
                        try
                        {
                            File.Delete(machinePublicFile);
                        }
                        catch (Exception)
                        {
                            result = false;
                        }
                    }

                    IEnumerable<string> pemFiles = Directory.EnumerateFiles(certPath, "*.pem", SearchOption.AllDirectories);
                    foreach (string pemFile in pemFiles)
                    {
                        string baseFileName = Path.GetFileName(pemFile);

                        if (!baseFileName.StartsWith("certificates_", StringComparison.OrdinalIgnoreCase) &&
                            !baseFileName.StartsWith("S29-", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        try
                        {
                            File.Delete(pemFile);
                        }
                        catch (Exception)
                        {
                            result = false;
                        }
                    }
                }

                string jsonRequestPath = Path.Combine(s29BasePath, "JSONRequests");
                if (Directory.Exists(jsonRequestPath))
                {
                    IEnumerable<string> jsonFiles = Directory.EnumerateFiles(jsonRequestPath, "*.json", SearchOption.AllDirectories);
                    foreach (string jsonFile in jsonFiles)
                    {
                        string baseFileName = Path.GetFileName(jsonFile);
                        if (string.Compare(baseFileName, "template.json", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            continue;
                        }

                        try
                        {
                            File.Delete(jsonFile);
                        }
                        catch (Exception)
                        {
                            result = false;
                        }
                    }
                }

                string jsonResponsePath = Path.Combine(s29BasePath, "JSONResponses");
                if (Directory.Exists(jsonResponsePath))
                {
                    IEnumerable<string> jsonFiles = Directory.EnumerateFiles(jsonResponsePath, "*.json", SearchOption.AllDirectories);
                    foreach (string jsonFile in jsonFiles)
                    {
                        try
                        {
                            File.Delete(jsonFile);
                        }
                        catch (Exception)
                        {
                            result = false;
                        }
                    }
                }

                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static List<string> GetFtdiRegKeys(string comPort)
        {
            if (string.IsNullOrEmpty(comPort))
            {
                return null;
            }

            List<string> regKeys = new List<string>();
            try
            {
                using (RegistryKey ftdiBusKey = Registry.LocalMachine.OpenSubKey(RegKeyFtdiBus, false))
                {
                    if (ftdiBusKey != null)
                    {
                        foreach (string subKeyName in ftdiBusKey.GetSubKeyNames())
                        {
                            string paramKeyName = subKeyName + @"\0000\Device Parameters";
                            using (RegistryKey paramKey = ftdiBusKey.OpenSubKey(paramKeyName))
                            {
                                if (paramKey != null)
                                {
                                    string portName = paramKey.GetValue("PortName") as string;
                                    if (string.Compare(portName, comPort, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        regKeys.Add(RegKeyFtdiBus + @"\" + paramKeyName);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return regKeys;
        }

        public static List<int> GetFtdiLatencyTimer(string comPort)
        {
            List<string> regKeys = GetFtdiRegKeys(comPort);
            if (regKeys == null)
            {
                return null;
            }

            List<int> latencyTimers = new List<int>();
            foreach (string regKey in regKeys)
            {
                try
                {
                    using (RegistryKey ftdiKey = Registry.LocalMachine.OpenSubKey(regKey, false))
                    {
                        if (ftdiKey != null)
                        {
                            object latencyTimer = ftdiKey.GetValue(RegValueFtdiLatencyTimer);
                            if (latencyTimer is int latencyValue)
                            {
                                latencyTimers.Add(latencyValue);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return latencyTimers;
        }

        public static bool SetFtdiLatencyTimer(string comPort, int value)
        {
            List<string> regKeys = GetFtdiRegKeys(comPort);
            if (regKeys == null)
            {
                return false;
            }

            bool result = false;
            foreach (string regKey in regKeys)
            {
                try
                {
                    using (RegistryKey ftdiKey = Registry.LocalMachine.OpenSubKey(regKey, true))
                    {
                        if (ftdiKey != null)
                        {
                            ftdiKey.SetValue(RegValueFtdiLatencyTimer, value, RegistryValueKind.DWord);
                            result = true;
                        }
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return result;
        }

        public static bool ResetFtdiDevice(UsbInfo usbInfo)
        {
            if (usbInfo == null)
            {
                return false;
            }

            IntPtr handleFtdi = IntPtr.Zero;
            try
            {
                Ftd2Xx.FT_STATUS ftStatus = Ftd2Xx.FT_OpenEx((IntPtr)usbInfo.LocationId, Ftd2Xx.FT_OPEN_BY_LOCATION, out handleFtdi);
                if (ftStatus != Ftd2Xx.FT_STATUS.FT_OK)
                {
                    handleFtdi = IntPtr.Zero;
                    return false;
                }

                if (handleFtdi != IntPtr.Zero)
                {
                    ftStatus = Ftd2Xx.FT_CyclePort(handleFtdi);
                    if (ftStatus == Ftd2Xx.FT_STATUS.FT_OK)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (handleFtdi != IntPtr.Zero)
                {
                    Ftd2Xx.FT_Close(handleFtdi);
                }
            }
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

        public static bool IsPatched(string dirName, PatchType patchType)
        {
            try
            {
                switch (patchType)
                {
                    case PatchType.Istad:
                    case PatchType.IstadExt:
                        if (IsIstaRegPresent(RegistryView.Registry32) != PatchRegType.None)
                        {
                            return true;
                        }

                        if (IsIstaRegPresent(RegistryView.Registry64) != PatchRegType.None)
                        {
                            return true;
                        }
                        break;
                }
            }
            catch (Exception)
            {
                // ignored
            }

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
                // ignored
            }

            return false;
        }

        public static bool PatchEdiabas(StringBuilder sr, PatchType patchType, int adapterType, string dirName, string ediabasDir,
            BluetoothItem devInfo, WlanInterface wlanIface, EdInterfaceEnet.EnetConnection enetConnection, UsbInfo usbInfo, string pin)
        {
            if (string.IsNullOrEmpty(dirName))
            {
                sr.Append("\r\n");
                sr.Append(Resources.Strings.PatchConfigUpdateFailed);
                return false;
            }

            try
            {
                bool copyOnly = false;
                RegistryView? registryViewIsta = null;
                PatchRegType patchRegTypeSet = PatchRegType.None;

                switch (patchType)
                {
                    case PatchType.Istad:
                    {
                        registryViewIsta = GetIstaReg();
                        string dirNameParent1 = Directory.GetParent(dirName)?.FullName;
                        if (!string.IsNullOrEmpty(dirNameParent1))
                        {
                            string dirNameParent2 = Directory.GetParent(dirNameParent1)?.FullName;
                            if (!string.IsNullOrEmpty(dirNameParent2))
                            {
                                string api32Ides = Path.Combine(dirNameParent2, "EdiabasForIDES", "bin", Api32DllName);
                                if (File.Exists(api32Ides))
                                {
                                    patchRegTypeSet |= PatchRegType.Ides;
                                }
                            }
                        }
                        break;
                    }

                    case PatchType.IstadExt:
                        if (!Directory.Exists(dirName))
                        {
                            Directory.CreateDirectory(dirName);
                        }

                        copyOnly = true;
                        registryViewIsta = GetIstaReg();
                        patchRegTypeSet = PatchRegType.Standard | PatchRegType.Ides;
                        break;
                }

                sr.AppendFormat(Resources.Strings.PatchDirectory, dirName);
                if (registryViewIsta != null)
                {
                    RemoveIstaReg(sr);
                }

                if (!string.IsNullOrEmpty(ediabasDir))
                {
                    DeleteEdiabasMachineCerts(ediabasDir);
                }

                if (!PatchFiles(sr, dirName, copyOnly))
                {
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.PatchConfigUpdateFailed);
                    return false;
                }

                string configFile = Path.Combine(dirName, ConfigFileName);
                if (!UpdateConfigFile(patchType, configFile, ediabasDir, null, adapterType, devInfo, wlanIface, enetConnection, usbInfo, pin))
                {
                    sr.Append("\r\n");
                    sr.Append(Resources.Strings.PatchConfigUpdateFailed);
                    return false;
                }

                if (registryViewIsta != null)
                {
                    if ((patchRegTypeSet & PatchRegType.Standard) != PatchRegType.None)
                    {
                        sr.Append("\r\n");
                        sr.AppendFormat(Resources.Strings.PatchRegistry, RegKeyIstaBinFull);
                    }

                    if ((patchRegTypeSet & PatchRegType.Ides) != PatchRegType.None)
                    {
                        sr.Append("\r\n");
                        sr.AppendFormat(Resources.Strings.PatchRegistry, RegKeyIstaIdesBinFull);
                    }

                    string ediabasBinPath = Path.GetDirectoryName(configFile);
                    if (!PatchIstaReg(registryViewIsta, ediabasBinPath, patchRegTypeSet))
                    {
                        sr.Append("\r\n");
                        sr.Append(Resources.Strings.PatchConfigUpdateFailed);
                        return false;
                    }
                }

                sr.Append("\r\n");
                sr.Append(Resources.Strings.PatchConfigUpdateOk);
                switch (patchType)
                {
                    case PatchType.Istad:
                    case PatchType.IstadExt:
                        sr.Append("\r\n");
                        sr.Append(Resources.Strings.PatchIstadInfoHint);
                        if (enetConnection != null)
                        {
                            sr.Append("\r\n");
                            sr.Append(Resources.Strings.PatchIstadInfoEnet);
                            break;
                        }

                        sr.Append("\r\n");
                        sr.Append(Resources.Strings.PatchIstadInfoEdiabas);
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

        public static bool RestoreEdiabas(StringBuilder sr, PatchType patchType, string dirName, string ediabasDir)
        {
            sr.AppendFormat(Resources.Strings.RestoreDirectory, dirName);

            RegistryView? registryViewIsta = null;
            switch (patchType)
            {
                case PatchType.Istad:
                case PatchType.IstadExt:
                    registryViewIsta = GetIstaReg();
                    break;
            }

            // ReSharper disable once ReplaceWithSingleAssignment.True
            bool result = true;
            if (!RestoreFiles(sr, dirName))
            {
                result = false;
            }

            if (registryViewIsta != null)
            {
                if (!RemoveIstaReg(sr))
                {
                    result = false;
                }
            }

            if (!string.IsNullOrEmpty(ediabasDir))
            {
                if (!DeleteEdiabasMachineCerts(ediabasDir))
                {
                    result = false;
                }
            }

            return result;
        }

        public static RegistryView? GetIstaReg()
        {
            try
            {
                using (RegistryKey localMachine64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (RegistryKey key = localMachine64.OpenSubKey(RegKeyIsta, false))
                    {
                        if (key != null)
                        {
                            object istaMainProdVer = key.GetValue(RegValueIstaMainProdVer);
                            if (istaMainProdVer is string istaVerStr)
                            {
                                if (Version.TryParse(istaVerStr, out Version istaVer))
                                {
                                    Version istaVerReg64Bit = new Version("4.55");  // full 64 bit registry support from ISTA 4.55
                                    if (istaVer >= istaVerReg64Bit)
                                    {
                                        return RegistryView.Registry64;
                                    }
                                }

                                return RegistryView.Registry32;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                using (RegistryKey localMachine32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    using (RegistryKey key = localMachine32.OpenSubKey(RegKeyIsta, false))
                    {
                        if (key != null)
                        {
                            object istaMainProdVer = key.GetValue(RegValueIstaMainProdVer);
                            if (istaMainProdVer is string istaVerStr)
                            {
                                if (Version.TryParse(istaVerStr, out Version _))
                                {
                                    return RegistryView.Registry32;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        public static bool PatchIstaReg(RegistryView? registryViewIsta, string ediabasBinLocation = null, PatchRegType patchRegType = PatchRegType.Standard | PatchRegType.Ides)
        {
            if (registryViewIsta == null)
            {
                return false;
            }

            bool result = false;
            bool opModeSet = false;
            try
            {
                using (RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryViewIsta.Value))
                {
                    using (RegistryKey key = localMachine.OpenSubKey(RegKeyReingold, true))
                    {
                        if (key != null)
                        {
                            if (!string.IsNullOrEmpty(ediabasBinLocation))
                            {
                                if ((patchRegType & PatchRegType.Standard) != PatchRegType.None)
                                {
                                    key.SetValue(RegKeyIstaBinPath, ediabasBinLocation);
                                }

                                if ((patchRegType & PatchRegType.Ides) != PatchRegType.None)
                                {
                                    key.SetValue(RegKeyIstaIdesBinPath, ediabasBinLocation);
                                }

                                key.SetValue(RegKeyIstaOpMode, "ISTA_PLUS");     // show ediabas.ini option in ISTA
                                opModeSet = true;
                            }
                            else
                            {
                                if ((patchRegType & PatchRegType.Standard) != PatchRegType.None)
                                {
                                    key.DeleteValue(RegKeyIstaBinPath, false);
                                }

                                if ((patchRegType & PatchRegType.Ides) != PatchRegType.None)
                                {
                                    key.DeleteValue(RegKeyIstaIdesBinPath, false);
                                }
                            }

                            result = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            if (opModeSet && registryViewIsta.Value == RegistryView.Registry64)
            {   // remove 32 bit registry entry
                try
                {
                    using (RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                    {
                        using (RegistryKey key = localMachine.OpenSubKey(RegKeyReingold, true))
                        {
                            if (key != null)
                            {
                                key.DeleteValue(RegKeyIstaOpMode, false);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return result;
        }

        public static PatchRegType IsIstaRegPresent(RegistryView? registryViewIsta, bool idesBin = false)
        {
            if (registryViewIsta == null)
            {
                return PatchRegType.None;
            }

            PatchRegType patchRegType = PatchRegType.None;
            try
            {
                using (RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryViewIsta.Value))
                {
                    using (RegistryKey key = localMachine.OpenSubKey(RegKeyReingold, false))
                    {
                        if (key != null)
                        {
                            if (key.GetValue(RegKeyIstaBinPath) != null)
                            {
                                patchRegType |= PatchRegType.Standard;
                            }
                            if (key.GetValue(RegKeyIstaIdesBinPath) != null)
                            {
                                patchRegType |= PatchRegType.Ides;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return patchRegType;
        }

        public static bool IsVcRedistRegPresent(bool check64Bit)
        {
            try
            {
                using (RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    string subKeyName = check64Bit ? RegKeyVcRuntime14X64 : RegKeyVcRuntime14X86;
                    using (RegistryKey key = localMachine.OpenSubKey(subKeyName, false))
                    {
                        if (key != null)
                        {
                            bool majorValid = false;
                            object majorObject = key.GetValue("Major");
                            if (majorObject is int majorValue)
                            {
                                if (majorValue == VcRuntimeMajor)
                                {
                                    majorValid = true;
                                }
                            }

                            bool minorValid = false;
                            if (majorValid)
                            {
                                object minorObject = key.GetValue("Minor");
                                if (minorObject is int minorValue)
                                {
                                    if (minorValue >= VcRuntimeMinor)
                                    {
                                        minorValid = true;
                                    }
                                }
                            }

                            if (majorValid && minorValid)
                            {
                                return true;
                            }
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
