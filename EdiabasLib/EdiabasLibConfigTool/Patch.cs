using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
                string dllFile = Path.Combine(dirName, ApiDllName);
                string dllFileBackup = Path.Combine(dirName, ApiDllBackupName);
                if (!File.Exists(dllFileBackup))
                {
                    sr.Append("\r\n");
                    sr.Append(Strings.PatchCreateBackupFile);
                    File.Copy(dllFile, dllFileBackup, false);
                }
                else
                {
                    sr.Append("\r\n");
                    sr.Append(Strings.PatchBackupFileExisting);
                }
                string sourceDir = AssemblyDirectory;
                string sourceDll = Path.Combine(sourceDir, ApiDllName);
                if (!File.Exists(sourceDll))
                {
                    sr.Append("\r\n");
                    sr.Append(Strings.PatchApi32Missing);
                    return false;
                }
                File.Copy(sourceDll, dllFile, true);
                string sourceConfig = Path.Combine(sourceDir, ConfigFileName);
                if (!File.Exists(sourceConfig))
                {
                    sr.Append("\r\n");
                    sr.Append(Strings.PatchConfigMissing);
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
                    sr.Append(Strings.PatchConfigExisting);
                }
            }
            catch (Exception)
            {
                sr.Append("\r\n");
                sr.Append(Strings.PatchCopyFailed);
                return false;
            }
            return true;
        }

        public static bool PatchEdiabas(StringBuilder sr, string dirName, BluetoothDeviceInfo devInfo, WlanInterface wlanIface, string pin)
        {
            try
            {
                sr.AppendFormat(Strings.PatchDirectory, dirName);
                if (!PatchFiles(sr, dirName))
                {
                    return false;
                }
                string configFile = Path.Combine(dirName, ConfigFileName);
                if (!UpdateConfigFile(configFile, devInfo, wlanIface, pin))
                {
                    sr.Append("\r\n");
                    sr.Append(Strings.PatchConfigUpdateFailed);
                    return false;
                }
                sr.Append("\r\n");
                sr.Append(Strings.PatchConfigUpdateOk);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
