using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace BmwDeepObd
{
    public static class SerialInfoUtils
    {
        private static readonly object SerialInfoLockObject = new object();
        private static readonly List<ActivityCommon.SerialInfoEntry> _serialInfoList = new List<ActivityCommon.SerialInfoEntry>();

        public static bool GetSerialInfo(string responseXml, out string serial, out string oem, out bool disabled)
        {
            serial = null;
            oem = null;
            disabled = false;
            try
            {
                if (string.IsNullOrEmpty(responseXml))
                {
                    return false;
                }
                XDocument xmlDoc = XDocument.Parse(responseXml);
                XElement serialInfoNode = xmlDoc.Root?.Element("serial_info");
                if (serialInfoNode != null)
                {
                    XAttribute serialAttr = serialInfoNode.Attribute("serial");
                    if (serialAttr != null)
                    {
                        serial = serialAttr.Value;
                    }

                    XAttribute oemAttr = serialInfoNode.Attribute("oem");
                    if (oemAttr != null)
                    {
                        oem = oemAttr.Value;
                    }

                    XAttribute disabledAttr = serialInfoNode.Attribute("disabled");
                    if (disabledAttr != null)
                    {
                        try
                        {
                            disabled = XmlConvert.ToBoolean(disabledAttr.Value);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool UpdateSerialInfo(string responseXml)
        {
            ActivityCommon.SerialInfoEntry serialInfo = null;
            if (GetSerialInfo(responseXml, out string serial, out string oem, out bool disabled))
            {
                serialInfo = new ActivityCommon.SerialInfoEntry(serial, oem, disabled, true);
            }
            else
            {
                if (!string.IsNullOrEmpty(ActivityCommon.LastAdapterSerial))
                {
                    serialInfo = new ActivityCommon.SerialInfoEntry(ActivityCommon.LastAdapterSerial, string.Empty, false, false);
                }
            }

            if (serialInfo != null)
            {
                return SerialInfoListAdd(serialInfo);
            }

            return false;
        }

        public static List<ActivityCommon.SerialInfoEntry> GetSerialInfoList()
        {
            lock (SerialInfoLockObject)
            {
                return new List<ActivityCommon.SerialInfoEntry>(_serialInfoList);
            }
        }

        public static void SetSerialInfoList(List<ActivityCommon.SerialInfoEntry> serialList)
        {
            lock (SerialInfoLockObject)
            {
                _serialInfoList.Clear();
                if (serialList != null)
                {
                    foreach (ActivityCommon.SerialInfoEntry serialInfo in serialList)
                    {
                        if (string.IsNullOrEmpty(serialInfo.Serial))
                        {
                            continue;
                        }

                        if (_serialInfoList.Contains(serialInfo))
                        {
                            continue;
                        }

                        _serialInfoList.Add(serialInfo);
                    }
                }
            }
        }

        public static void SerialInfoListClear()
        {
            lock (SerialInfoLockObject)
            {
                _serialInfoList.Clear();
            }
        }

        public static bool SerialInfoListAdd(ActivityCommon.SerialInfoEntry serialInfo)
        {
            try
            {
                if (string.IsNullOrEmpty(serialInfo.Serial))
                {
                    return false;
                }

                lock (SerialInfoLockObject)
                {
                    if (_serialInfoList.Contains(serialInfo))
                    {
                        _serialInfoList.Remove(serialInfo);
                    }

                    _serialInfoList.Insert(0, serialInfo);
                    while (_serialInfoList.Count > 10)
                    {
                        _serialInfoList.RemoveAt(_serialInfoList.Count - 1);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsSerialNumberCheckRequired()
        {
            string serialNumber = ActivityCommon.LastAdapterSerial;
            if (string.IsNullOrEmpty(serialNumber))
            {
                return false;
            }

            ActivityCommon.SerialInfoEntry serialInfo = new ActivityCommon.SerialInfoEntry(serialNumber, string.Empty, false, true);
            lock (SerialInfoLockObject)
            {
                return !_serialInfoList.Contains(serialInfo);
            }
        }
    }
}
