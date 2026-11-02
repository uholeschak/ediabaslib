using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using EdiabasLib;
using UdsFileReader;

namespace BmwDeepObd
{
    public static class EdiabasUtils
    {
        public static bool SetEdiabasUdsCanId(EdiabasNet ediabas, VehicleInfoVag.EcuAddressEntry ecuAddressEntry = null)
        {
            if (ediabas.EdInterfaceClass is EdInterfaceObd edInterfaceObd)
            {
                int oldEcuId = edInterfaceObd.UdsEcuCanIdOverride;
                int oldTesterId = edInterfaceObd.UdsTesterCanIdOverride;

                if (ecuAddressEntry != null)
                {
                    edInterfaceObd.UdsEcuCanIdOverride = (int)ecuAddressEntry.IsoTpEcuCanId;
                    edInterfaceObd.UdsTesterCanIdOverride = (int)ecuAddressEntry.IsoTpTesterCanId;
                }
                else
                {
                    edInterfaceObd.UdsEcuCanIdOverride = -1;
                    edInterfaceObd.UdsTesterCanIdOverride = -1;
                }

                if (oldEcuId != edInterfaceObd.UdsEcuCanIdOverride ||
                    oldTesterId != edInterfaceObd.UdsTesterCanIdOverride)
                {
                    // force reload
                    ediabas.CloseSgbd();
                }

                return true;
            }

            return false;
        }

        public static bool SetEdiabasConfigProperties(EdiabasNet ediabas, string tracePath, string simulationPath, bool appendTrace = false)
        {
            if (ediabas == null)
            {
                return false;
            }

            try
            {
                if (!string.IsNullOrEmpty(tracePath))
                {
                    ediabas.SetConfigProperty("TracePath", tracePath);
                    ediabas.SetConfigProperty("IfhTrace", string.Format("{0}", (int)EdiabasNet.EdLogLevel.Error));
                    ediabas.SetConfigProperty("AppendTrace", appendTrace ? "1" : "0");
                    ediabas.SetConfigProperty("ActivityCommon.IfhTraceBuffering", ActivityCommon.IfhTraceBuffering ? "1" : "0");
                    ediabas.SetConfigProperty("ActivityCommon.CompressTrace", ActivityCommon.CompressTrace ? "1" : "0");
                }
                else
                {
                    ediabas.SetConfigProperty("IfhTrace", "0");
                }

                if (!string.IsNullOrEmpty(simulationPath))
                {
                    ediabas.SetConfigProperty("Simulation", "1");
                    ediabas.SetConfigProperty("SimulationPath", simulationPath);
                    ediabas.SetConfigProperty("SimulationInterfaces", "OBD,ENET,EDIC,OBD_*,ENET_*,EDIC_*");
                }
                else
                {
                    ediabas.SetConfigProperty("Simulation", "0");
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static void ResolveSgbdFile(EdiabasNet ediabas, string fileName)
        {
            if (ediabas == null)
            {
                return;
            }

            bool mapped = false;
            string baseFileName = Path.GetFileName(fileName);
            string sgbdName = baseFileName;
            if (ActivityCommon.SelectedManufacturer != ActivityCommon.ManufacturerType.Bmw)
            {
                if (baseFileName != null && baseFileName.StartsWith(XmlToolActivity.VagUdsCommonSgbd + "#", true, CultureInfo.InvariantCulture))
                {
                    string[] nameArray = baseFileName.Split('#');
                    if (nameArray.Length == 2)
                    {
                        sgbdName = nameArray[0];
                        object addressObj = new UInt32Converter().ConvertFromInvariantString(nameArray[1]);
                        if (!(addressObj is UInt32 address))
                        {
                            ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ResolveSgbdFile invalid number for: {0} ", baseFileName);
                            // ReSharper disable once NotResolvedInText
                            throw new ArgumentOutOfRangeException("ResolveSgbdFile", "Parsing address failed");
                        }
                        VehicleInfoVag.EcuAddressEntry ecuAddressEntry = VehicleInfoVag.GetAddressEntry(address);
                        if (ecuAddressEntry == null)
                        {
                            ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ResolveSgbdFile no address entry for: {0} ", baseFileName);
                            // ReSharper disable once NotResolvedInText
                            throw new ArgumentOutOfRangeException("ResolveSgbdFile", "No address entry found");
                        }
                        ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Mapped address {0:X02} to UDS CAN IDs: {1:X03}, {2:X03}",
                            address, ecuAddressEntry.IsoTpEcuCanId, ecuAddressEntry.IsoTpTesterCanId);

                        SetEdiabasUdsCanId(ediabas, ecuAddressEntry);
                        mapped = true;
                    }
                }
            }

            if (!mapped)
            {
                SetEdiabasUdsCanId(ediabas);
            }

            ediabas.ResolveSgbdFile(sgbdName);
        }

        public static String FormatResultEcuFunction(JobReader.PageInfo pageInfo, JobReader.DisplayInfo displayInfo, MultiMap<string, EdiabasNet.ResultData> resultDict,
            out double? dataValue)
        {
            dataValue = null;
            if (string.IsNullOrWhiteSpace(displayInfo.EcuJobId) || string.IsNullOrWhiteSpace(displayInfo.EcuJobResultId))
            {
                return string.Empty;
            }

            IList<EdiabasNet.ResultData> resultDataList;
            if (resultDict != null && resultDict.TryGetValue(displayInfo.Result.ToUpperInvariant(), out resultDataList))
            {
                foreach (EdiabasNet.ResultData resultData in resultDataList)
                {
                    if (resultData is EdiabasThread.EcuFunctionResult ecuFunctionResult)
                    {
                        if (ecuFunctionResult.EcuJob != null && ecuFunctionResult.EcuJob.IdPresent(displayInfo.EcuJobId, pageInfo.UseCompatIds) &&
                            ecuFunctionResult.EcuJobResult != null && ecuFunctionResult.EcuJobResult.IdPresent(displayInfo.EcuJobResultId, pageInfo.UseCompatIds))
                        {
                            dataValue = ecuFunctionResult.ResultValue;
                            return ecuFunctionResult.ResultString;
                        }
                    }
                }
            }

            return string.Empty;
        }

        public static String FormatResultVagUds(string udsFileName, JobReader.PageInfo pageInfo, JobReader.DisplayInfo displayInfo, MultiMap<string, EdiabasNet.ResultData> resultDict,
            out double? dataValue)
        {
            dataValue = null;
            if (!ActivityCommon.VagUdsActive)
            {
                return string.Empty;
            }

            string[] displayParts = displayInfo.Name.Split('#');
            if (displayParts.Length != 3)
            {
                return string.Empty;
            }

            string uniqueIdString = displayParts[displayParts.Length - 1];
            UdsReader udsReader = ActivityCommon.GetUdsReader(udsFileName);
            UdsReader.ParseInfoMwb parseInfoMwb = udsReader?.GetMwbParseInfo(udsFileName, uniqueIdString);
            if (parseInfoMwb == null)
            {
                return string.Empty;
            }

            IList<EdiabasNet.ResultData> resultDataList;
            if (resultDict != null && resultDict.TryGetValue(displayInfo.Result.ToUpperInvariant(), out resultDataList))
            {
                foreach (EdiabasNet.ResultData resultData in resultDataList)
                {
                    if (resultData.OpData.GetType() == typeof(byte[]))
                    {
                        string resultText = parseInfoMwb.DataTypeEntry.ToString(CultureInfo.InvariantCulture, (byte[])resultData.OpData, out double? stringDataValue);
                        if (stringDataValue.HasValue && !string.IsNullOrEmpty(displayInfo.Format))
                        {
                            dataValue = stringDataValue;
                            resultText = EdiabasNet.FormatResult(new EdiabasNet.ResultData(EdiabasNet.ResultType.TypeR, displayInfo.Result, stringDataValue.Value), displayInfo.Format) ?? string.Empty;
                        }
                        return resultText;
                    }
                }
            }

            return string.Empty;
        }

        public static bool SetDoIpSslProperties(EdInterfaceEnet edInterfaceEnet, string appDataDir)
        {
            if (!string.IsNullOrEmpty(appDataDir))
            {
                string securityDir = Path.Combine(appDataDir, ActivityCommon.SecuritySubDir);
                string caCertsDir = Path.Combine(securityDir, ActivityCommon.CaCertsSubDir);
                string certsDir = Path.Combine(securityDir, ActivityCommon.CertsSubDir);
                edInterfaceEnet.DoIpSslSecurityPath = caCertsDir;
                edInterfaceEnet.DoIpS29Path = certsDir;
                edInterfaceEnet.NetworkProtocol = EdInterfaceEnet.NetworkProtocolSsl;
                edInterfaceEnet.Authentication = EdInterfaceEnet.AuthenticationS29;
                return true;
            }

            edInterfaceEnet.NetworkProtocol = EdInterfaceEnet.NetworkProtocolTcp;
            edInterfaceEnet.Authentication = EdInterfaceEnet.AuthenticationNone;
            return false;
        }

        public static bool SetConnectParamBmwFast(EdiabasNet ediabasNet)
        {
            if (ediabasNet?.EdInterfaceClass == null)
            {
                return false;
            }

            ediabasNet.EdInterfaceClass.CommParameter = EdInterfaceBase.CommParameterBmwFast;
            ediabasNet.EdInterfaceClass.CommAnswerLen = EdInterfaceBase.CommAnswerLenBmwFast;
            return true;
        }

        public static String FormatResultEdiabas(MultiMap<string, EdiabasNet.ResultData> resultDict, string dataName, string format)
        {
            StringBuilder sbResult = new StringBuilder();
            IList<EdiabasNet.ResultData> resultDataList;
            if (resultDict != null && resultDict.TryGetValue(dataName.ToUpperInvariant(), out resultDataList))
            {
                foreach (EdiabasNet.ResultData resultData in resultDataList)
                {
                    string result;
                    if (resultData.OpData.GetType() == typeof(byte[]))
                    {
                        StringBuilder sb = new StringBuilder();
                        byte[] data = (byte[])resultData.OpData;
                        foreach (byte value in data)
                        {
                            sb.Append(string.Format(CultureInfo.InvariantCulture, "{0:X02} ", value));
                        }
                        result = sb.ToString();
                    }
                    else
                    {
                        result = EdiabasNet.FormatResult(resultData, format) ?? "?";
                    }
                    if (sbResult.Length > 0)
                    {
                        sbResult.Append("\r\n");
                    }
                    sbResult.Append(result);
                }
            }
            return sbResult.ToString();
        }

        public static bool IsValidSimDir(string simulationDir)
        {
            try
            {
                if (string.IsNullOrEmpty(simulationDir))
                {
                    return false;
                }

                if (!Directory.Exists(simulationDir))
                {
                    return false;
                }

                string[] simFiles = Directory.GetFiles(simulationDir, "*.sim", SearchOption.TopDirectoryOnly);
                if (simFiles.Length == 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool ReadBatteryVoltage(EdiabasNet ediabas, out double? batteryVoltage, out byte[] adapterSerial)
        {
            batteryVoltage = null;
            adapterSerial = null;
            try
            {
                if (ediabas == null)
                {
                    return false;
                }
                if (ediabas.EdInterfaceClass is EdInterfaceObd edInterfaceObd)
                {
                    if (!edInterfaceObd.Connected)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                // this only reads cached values
                batteryVoltage = ediabas.EdInterfaceClass.AdapterVoltage;
                adapterSerial = ediabas.EdInterfaceClass.AdapterSerial;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsCommunicationError(string exceptionText)
        {
            if (string.IsNullOrEmpty(exceptionText))
            {
                return false;
            }
            if (exceptionText.Contains("executeJob aborted"))
            {
                return false;
            }
            return true;
        }
    }
}
