using Android.Nfc;
using EdiabasLib;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Text;

namespace BmwDeepObd;

public class AdapterTypeDetect
{
    public enum AdapterType
    {
        ConnectionFailed,   // connection to adapter failed
        Unknown,            // unknown adapter
        Elm327,             // ELM327
        Elm327Custom,       // ELM327 with custom firmware
        Elm327Invalid,      // ELM327 invalid type
        Elm327Fake,         // ELM327 fake version
        Elm327FakeOpt,      // ELM327 fake version optional command
        Elm327Limited,      // ELM327 limited support
        StnFwUpdate,        // STN fimrware update required
        Elm327NoCan,        // ELM327 no CAN support
        Custom,             // custom adapter
        CustomNoEscape,     // custom adapter with no escape support
        CustomUpdate,       // custom adapter with firmware update
        EchoOnly,           // only echo response
    }

#if DEBUG
    private static readonly string Tag = typeof(AdapterTypeDetect).FullName;
#endif
    public const int ResponseTimeout = 1000;

    private readonly StringBuilder _sbLog = new StringBuilder();
    private readonly ActivityCommon _activityCommon;

    public StringBuilder SbLog
    {
        get
        {
            return _sbLog;
        }
    }

    public int ElmVerH { get; private set; }
    public int ElmVerL { get; private set; }

    public AdapterTypeDetect(ActivityCommon activityCommon)
    {
        _activityCommon = activityCommon;
        ElmVerH = -1;
        ElmVerL = -1;
    }

    /// <summary>
    /// Detects the CAN adapter type
    /// </summary>
    /// <param name="adapterInStream">Adapter input stream</param>
    /// <param name="adapterOutStream">Adapter output stream</param>
    /// <returns>Adapter type</returns>
    public AdapterType AdapterTypeDetection(Stream adapterInStream, Stream adapterOutStream)
    {
        AdapterType adapterType = AdapterType.Unknown;
        ElmVerH = -1;
        ElmVerL = -1;

        try
        {
            const int minIgnitionRespLen = 6;
            byte[] customData = { 0x82, 0xF1, 0xF1, 0xFE, 0xFE, 0x00 }; // ignition state
            customData[^1] = EdCustomAdapterCommon.CalcChecksumBmwFast(customData, 0, customData.Length - 1);
            // custom adapter
            adapterInStream.Flush();
            while (adapterInStream.HasData())
            {
                adapterInStream.ReadByteAsync();
            }
#if DEBUG
            Android.Util.Log.Info(Tag, string.Format("Send: {0}", BitConverter.ToString(customData).Replace("-", " ")));
#endif
            LogData(customData, 0, customData.Length, "Send");
            adapterOutStream.Write(customData, 0, customData.Length);

            LogData(null, 0, 0, "Resp");
            List<byte> responseList = new List<byte>();
            long startTime = Stopwatch.GetTimestamp();
            for (; ; )
            {
                while (adapterInStream.HasData())
                {
                    int data = adapterInStream.ReadByteAsync();
                    if (data >= 0)
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, string.Format("Rec: {0:X02}", data));
#endif
                        LogByte((byte)data);
                        responseList.Add((byte)data);
                        startTime = Stopwatch.GetTimestamp();
                    }
                }

                if (responseList.Count >= customData.Length + minIgnitionRespLen &&
                    responseList.Count >= customData.Length + (responseList[customData.Length] & 0x3F) + 3)
                {
                    LogString("Custom adapter length");
                    bool validEcho = !customData.Where((t, i) => responseList[i] != t).Any();
                    if (!validEcho)
                    {
                        LogString("*** Echo incorrect");
                        break;
                    }

                    if (responseList.Count > customData.Length)
                    {
                        byte[] addResponse = responseList.GetRange(customData.Length, responseList.Count - customData.Length).ToArray();
                        if (EdCustomAdapterCommon.CalcChecksumBmwFast(addResponse, 0, addResponse.Length - 1) != addResponse[^1])
                        {
                            LogString("*** Checksum incorrect");
                            break;
                        }
                    }

                    LogString("Ignition response ok");
                    LogString("Escape mode: " + (_activityCommon.MtcBtEscapeMode ? "1" : "0"));
                    if (!string.IsNullOrEmpty(_activityCommon.MtcBtModuleName))
                    {
                        LogString("Bt module: " + _activityCommon.MtcBtModuleName);
                    }

                    bool escapeMode = _activityCommon.MtcBtEscapeMode;
                    BtEscapeStreamReader inStream = new BtEscapeStreamReader(adapterInStream);
                    BtEscapeStreamWriter outStream = new BtEscapeStreamWriter(adapterOutStream);
                    if (!SetCustomEscapeMode(inStream, outStream, ref escapeMode, out bool noEscapeSupport))
                    {
                        LogString("*** Set escape mode failed");
                    }

                    inStream.SetEscapeMode(escapeMode);
                    outStream.SetEscapeMode(escapeMode);

                    if (!ReadCustomFwVersion(inStream, outStream, out int adapterTypeId, out int fwVersion))
                    {
                        LogString("*** Read firmware version failed");
                        if (noEscapeSupport && _activityCommon.MtcBtEscapeMode)
                        {
                            LogString("Custom adapter with no escape mode support");
                            return AdapterType.CustomNoEscape;
                        }
                        break;
                    }
                    LogString(string.Format("AdapterType: {0}", adapterTypeId));
                    LogString(string.Format("AdapterVersion: {0}.{1}", fwVersion >> 8, fwVersion & 0xFF));

                    if (adapterTypeId >= 0x0002)
                    {
                        if (ReadCustomSerial(inStream, outStream, out byte[] adapterSerial))
                        {
                            LogString("AdapterSerial: " + BitConverter.ToString(adapterSerial).Replace("-", ""));
                        }
                    }

                    int fwUpdateVersion = PicBootloader.GetFirmwareVersion((uint)adapterTypeId);
                    if (fwUpdateVersion >= 0 && fwUpdateVersion > fwVersion)
                    {
                        LogString("Custom adapter with old firmware detected");
                        return AdapterType.CustomUpdate;
                    }
                    LogString("Custom adapter detected");

                    return AdapterType.Custom;
                }
                if (Stopwatch.GetTimestamp() - startTime > ResponseTimeout * ActivityCommon.TickResolMs)
                {
                    if (responseList.Count >= customData.Length)
                    {
                        bool validEcho = !customData.Where((t, i) => responseList[i] != t).Any();
                        if (validEcho)
                        {
                            if (responseList.Count > customData.Length)
                            {
                                byte[] addResponse = responseList.GetRange(customData.Length, responseList.Count - customData.Length).ToArray();
                                if (EdCustomAdapterCommon.CalcChecksumBmwFast(addResponse, 0, addResponse.Length - 1) != addResponse[addResponse.Length - 1])
                                {
                                    LogString("*** Additional response checksum incorrect");
                                    break;
                                }
                            }

                            LogString("Valid echo detected");
                            adapterType = AdapterType.EchoOnly;
                        }
                    }
                    break;
                }
            }
            LogString("No custom adapter found");

            // ELM327
            bool elmReports2X = false;
            Regex elmVerRegEx = new Regex(@"ELM327\s+v(\d+)\.(\d+)", RegexOptions.IgnoreCase);
            for (int retries = 0; retries < 2; retries++)
            {
                adapterInStream.Flush();
                while (adapterInStream.HasData())
                {
                    adapterInStream.ReadByteAsync();
                }

                string command = "ATI\r";
                byte[] sendData = Encoding.UTF8.GetBytes(command);
                LogData(sendData, 0, sendData.Length, "Send");
                adapterOutStream.Write(sendData, 0, sendData.Length);
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("ELM CMD send: {0}", command));
#endif
                LogString("ELM CMD send: " + command);

                string response = GetElm327Reponse(adapterInStream);
                if (response != null)
                {
                    MatchCollection matchesVer = elmVerRegEx.Matches(response);
                    if ((matchesVer.Count == 1) && (matchesVer[0].Groups.Count == 3))
                    {
                        if (!Int32.TryParse(matchesVer[0].Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int elmVerH))
                        {
                            ElmVerH = -1;
                        }
                        else
                        {
                            ElmVerH = elmVerH;
                        }

                        if (!Int32.TryParse(matchesVer[0].Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int elmVerL))
                        {
                            ElmVerL = -1;
                        }
                        else
                        {
                            ElmVerL = elmVerL;
                        }
                    }

                    if (ElmVerH >= 0 && ElmVerL >= 0)
                    {
                        LogString(string.Format("ELM327 version detected: {0}.{1}", ElmVerH, ElmVerL));
                        if (ElmVerH >= 2)
                        {
                            LogString("Version >= 2.x detected");
                            elmReports2X = true;
                        }
                        adapterType = AdapterType.Elm327;
                        break;
                    }
                }
            }
            if (adapterType == AdapterType.Elm327)
            {
                foreach (EdElmInterface.ElmInitEntry elmInitEntry in EdBluetoothInterface.Elm327InitCommands)
                {
                    adapterInStream.Flush();
                    while (adapterInStream.HasData())
                    {
                        adapterInStream.ReadByteAsync();
                    }
                    if (!Elm327SendCommand(adapterInStream, adapterOutStream, elmInitEntry.Command, false))
                    {
                        adapterType = AdapterType.Elm327Invalid;
                        break;
                    }

                    string response = GetElm327Reponse(adapterInStream);
                    if (response == null)
                    {
                        LogString("*** No ELM response");
                        adapterType = AdapterType.Elm327Invalid;
                        break;
                    }

                    if (elmInitEntry.OkResponse)
                    {
                        if (!response.Contains("OK\r"))
                        {
                            LogString("*** No ELM OK found");
                            bool optional = elmInitEntry.Version >= 0;
                            if (!optional)
                            {
                                adapterType = AdapterType.Elm327Invalid;
                                break;
                            }
                            if (elmReports2X && elmInitEntry.Version >= 200)
                            {
                                LogString("*** ELM command optional, fake 2.X");
                                adapterType = AdapterType.Elm327FakeOpt;
                            }
                            else
                            {
                                LogString("*** ELM command optional, fake");
                            }
                        }
                    }
                }

                switch (adapterType)
                {
                    case AdapterType.Elm327Invalid:
                        if (elmReports2X)
                        {
                            adapterType = AdapterType.Elm327Fake;
                        }
                        break;

                    case AdapterType.Elm327:
                    case AdapterType.Elm327FakeOpt:
                        {
                            if (!Elm327CheckCustomFirmware(adapterInStream, adapterOutStream, out bool customFirmware))
                            {
                                LogString("*** ELM firmware detection failed");
                            }
                            if (customFirmware)
                            {
                                adapterType = AdapterType.Elm327Custom;
                                break;
                            }

                            if (!Elm327CheckCompatibility(adapterInStream, adapterOutStream, out bool restricted, out bool fwUpdate))
                            {
                                LogString("*** ELM not compatible");
                                adapterType = AdapterType.Elm327Fake;
                                break;
                            }

                            if (restricted)
                            {
                                adapterType = AdapterType.Elm327Limited;
                            }
                            else if (fwUpdate)
                            {
                                adapterType = AdapterType.StnFwUpdate;
                            }

                            if (!Elm327CheckCan(adapterInStream, adapterOutStream, out bool canSupport))
                            {
                                LogString("*** ELM CAN detection failed");
                                adapterType = AdapterType.Elm327Invalid;
                                break;
                            }

                            if (!canSupport)
                            {
                                LogString("*** ELM no vehicle CAN support");
                                adapterType = AdapterType.Elm327NoCan;
                            }
                            break;
                        }
                }
            }
        }
        catch (Exception ex)
        {
            LogString("*** Exception: " + EdiabasNet.GetExceptionText(ex));
            return AdapterType.ConnectionFailed;
        }
        LogString("Adapter type: " + adapterType);
        return adapterType;
    }

    private bool SetCustomEscapeMode(BtEscapeStreamReader inStream, BtEscapeStreamWriter outStream, ref bool escapeMode, out bool noEscapeSupport)
    {
        const int escapeRespLen = 8;
        byte escapeModeValue = (byte)((escapeMode ? 0x03 : 0x00) ^ EdCustomAdapterCommon.EscapeXor);
        byte[] escapeData = { 0x84, 0xF1, 0xF1, 0x06, escapeModeValue,
                EdCustomAdapterCommon.EscapeCodeDefault ^ EdCustomAdapterCommon.EscapeXor, EdCustomAdapterCommon.EscapeMaskDefault ^ EdCustomAdapterCommon.EscapeXor, 0x00 };
        escapeData[^1] = EdCustomAdapterCommon.CalcChecksumBmwFast(escapeData, 0, escapeData.Length - 1);

        LogString(string.Format("Set escape mode: {0}", escapeMode));

        noEscapeSupport = false;

        LogData(escapeData, 0, escapeData.Length, "Send");
        outStream.Write(escapeData, 0, escapeData.Length);

        LogData(null, 0, 0, "Resp");
        List<byte> responseList = new List<byte>();
        long startTime = Stopwatch.GetTimestamp();
        for (; ; )
        {
            while (inStream.HasData())
            {
                int data = inStream.ReadByte();
                if (data >= 0)
                {
                    LogByte((byte)data);
                    responseList.Add((byte)data);
                    startTime = Stopwatch.GetTimestamp();
                }
            }

            if (responseList.Count >= escapeData.Length + escapeRespLen)
            {
                LogString("Escape mode length");
                bool validEcho = !escapeData.Where((t, i) => responseList[i] != t).Any();
                if (!validEcho)
                {
                    LogString("*** Echo incorrect");
                    break;
                }

                byte[] addResponse = responseList.GetRange(escapeData.Length, responseList.Count - escapeData.Length).ToArray();
                if (EdCustomAdapterCommon.CalcChecksumBmwFast(addResponse, 0, addResponse.Length - 1) != addResponse[^1])
                {
                    LogString("*** Checksum incorrect");
                    escapeMode = false;
                    return false;
                }

                if (responseList[escapeData.Length + 4] != escapeModeValue)
                {
                    LogString("*** Escape mode incorrect");
                    escapeMode = false;
                    return false;
                }

                if (escapeMode)
                {
                    if (responseList[escapeData.Length + 5] != (EdCustomAdapterCommon.EscapeCodeDefault ^ EdCustomAdapterCommon.EscapeXor))
                    {
                        LogString("*** Escape code incorrect");
                        escapeMode = false;
                        return false;
                    }

                    if (responseList[escapeData.Length + 6] != (EdCustomAdapterCommon.EscapeMaskDefault ^ EdCustomAdapterCommon.EscapeXor))
                    {
                        LogString("*** Escape mask incorrect");
                        escapeMode = false;
                        return false;
                    }
                }

                break;
            }
            if (Stopwatch.GetTimestamp() - startTime > ResponseTimeout * ActivityCommon.TickResolMs)
            {
                if (responseList.Count == escapeData.Length)
                {
                    bool validEcho = !escapeData.Where((t, i) => responseList[i] != t).Any();
                    if (validEcho)
                    {
                        LogString("Escape mode echo correct");
                        escapeMode = false;
                        noEscapeSupport = true;
                        break;
                    }
                }

                LogString("*** Escape mode timeout");
                escapeMode = false;
                return false;
            }
        }
        return true;
    }

    private bool ReadCustomFwVersion(BtEscapeStreamReader inStream, BtEscapeStreamWriter outStream, out int adapterTypeId, out int fwVersion)
    {
        adapterTypeId = -1;
        fwVersion = -1;
        const int fwRespLen = 9;
        byte[] fwData = { 0x82, 0xF1, 0xF1, 0xFD, 0xFD, 0x00 };
        fwData[^1] = EdCustomAdapterCommon.CalcChecksumBmwFast(fwData, 0, fwData.Length - 1);

        LogString("Reading firmware version");

        LogData(fwData, 0, fwData.Length, "Send");
        outStream.Write(fwData, 0, fwData.Length);

        LogData(null, 0, 0, "Resp");
        List<byte> responseList = new List<byte>();
        long startTime = Stopwatch.GetTimestamp();
        for (; ; )
        {
            while (inStream.HasData())
            {
                int data = inStream.ReadByte();
                if (data >= 0)
                {
                    LogByte((byte)data);
                    responseList.Add((byte)data);
                    startTime = Stopwatch.GetTimestamp();
                }
            }

            if (responseList.Count >= fwData.Length + fwRespLen)
            {
                LogString("FW data length");
                bool validEcho = !fwData.Where((t, i) => responseList[i] != t).Any();
                if (!validEcho)
                {
                    LogString("*** Echo incorrect");
                    break;
                }

                byte[] addResponse = responseList.GetRange(fwData.Length, responseList.Count - fwData.Length).ToArray();
                if (EdCustomAdapterCommon.CalcChecksumBmwFast(addResponse, 0, addResponse.Length - 1) != addResponse[^1])
                {
                    LogString("*** Checksum incorrect");
                    return false;
                }

                adapterTypeId = responseList[fwData.Length + 5] + (responseList[fwData.Length + 4] << 8);
                fwVersion = responseList[fwData.Length + 7] + (responseList[fwData.Length + 6] << 8);
                break;
            }
            if (Stopwatch.GetTimestamp() - startTime > ResponseTimeout * ActivityCommon.TickResolMs)
            {
                LogString("*** FW data timeout");
                return false;
            }
        }
        return true;
    }

    private bool ReadCustomSerial(BtEscapeStreamReader inStream, BtEscapeStreamWriter outStream, out byte[] adapterSerial)
    {
        adapterSerial = null;
        const int idRespLen = 13;
        byte[] idData = { 0x82, 0xF1, 0xF1, 0xFB, 0xFB, 0x00 };
        idData[^1] = EdCustomAdapterCommon.CalcChecksumBmwFast(idData, 0, idData.Length - 1);

        LogString("Reading id data");

        LogData(idData, 0, idData.Length, "Send");
        outStream.Write(idData, 0, idData.Length);

        LogData(null, 0, 0, "Resp");
        List<byte> responseList = new List<byte>();
        long startTime = Stopwatch.GetTimestamp();
        for (; ; )
        {
            while (inStream.HasData())
            {
                int data = inStream.ReadByte();
                if (data >= 0)
                {
                    LogByte((byte)data);
                    responseList.Add((byte)data);
                    startTime = Stopwatch.GetTimestamp();
                }
            }

            if (responseList.Count >= idData.Length + idRespLen)
            {
                LogString("Id data length");
                bool validEcho = !idData.Where((t, i) => responseList[i] != t).Any();
                if (!validEcho)
                {
                    LogString("*** Echo incorrect");
                    break;
                }

                byte[] addResponse = responseList.GetRange(idData.Length, responseList.Count - idData.Length).ToArray();
                if (EdCustomAdapterCommon.CalcChecksumBmwFast(addResponse, 0, addResponse.Length - 1) != addResponse[^1])
                {
                    LogString("*** Checksum incorrect");
                    return false;
                }

                adapterSerial = responseList.GetRange(idData.Length + 4, 8).ToArray();
                break;
            }
            if (Stopwatch.GetTimestamp() - startTime > ResponseTimeout * ActivityCommon.TickResolMs)
            {
                LogString("*** Id data timeout");
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Check for vehicle can support
    /// </summary>
    /// <param name="adapterInStream"></param>
    /// <param name="adapterOutStream"></param>
    /// <param name="canSupport">True: CAN supported</param>
    /// <returns></returns>
    private bool Elm327CheckCan(Stream adapterInStream, Stream adapterOutStream, out bool canSupport)
    {
        canSupport = true;
        Elm327SendCommand(adapterInStream, adapterOutStream, "ATCTM1");     // standard multiplier
        int timeout = 1000 / 4; // 1sec
        if (!Elm327SendCommand(adapterInStream, adapterOutStream, string.Format("ATST{0:X02}", timeout)))
        {
            LogString("*** ELM setting timeout failed");
            return false;
        }

        if (!Elm327SendCommand(adapterInStream, adapterOutStream, "0000000000000000", false)) // dummy data
        {
            LogString("*** ELM sending data failed");
            return false;
        }
        string answer = GetElm327Reponse(adapterInStream);
        if (answer != null)
        {
            if (answer.Contains("CAN ERROR\r"))
            {
#if DEBUG
                Android.Util.Log.Info(Tag, "ELM CAN error");
#endif
                LogString("*** ELM CAN error");
                canSupport = false;
            }
        }

        if (canSupport)
        {
            // fake adapters are not able to send short telegrams
            if (!Elm327SendCommand(adapterInStream, adapterOutStream, "00", false))
            {
                LogString("*** ELM sending data failed");
                return false;
            }
            answer = GetElm327Reponse(adapterInStream);
            if (answer != null)
            {
                if (answer.Contains("CAN ERROR\r"))
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "ELM CAN error, fake adapter");
#endif
                    LogString("*** ELM CAN error, fake adapter");
                    return false;
                }
            }
        }

        return true;
    }

    private bool Elm327CheckCustomFirmware(Stream adapterInStream, Stream adapterOutStream, out bool customFirmware)
    {
        customFirmware = false;
        adapterInStream.Flush();
        while (adapterInStream.HasData())
        {
            adapterInStream.ReadByteAsync();
        }

        if (!Elm327SendCommand(adapterInStream, adapterOutStream, @"AT@2", false))
        {
            LogString("*** ELM read device identifier failed");
            return false;
        }

        string answer = GetElm327Reponse(adapterInStream);
        if (answer != null)
        {
            if (answer.StartsWith("DEEPOBD"))
            {
                customFirmware = true;
            }
        }

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (customFirmware)
        {
            LogString("Custom ELM firmware");
        }
        else
        {
            LogString("No custom ELM firmware");
        }

        return true;
    }

    private bool Elm327CheckCompatibility(Stream adapterInStream, Stream adapterOutStream, out bool restricted, out bool fwUpdate)
    {
        restricted = false;
        fwUpdate = false;
        adapterInStream.Flush();
        while (adapterInStream.HasData())
        {
            adapterInStream.ReadByteAsync();
        }

        if (!Elm327SendCommand(adapterInStream, adapterOutStream, @"AT@1", false))
        {
            LogString("*** ELM read device description failed");
            return false;
        }

        string elmDevDesc = GetElm327Reponse(adapterInStream);
        if (elmDevDesc != null)
        {
            LogString(string.Format("ELM ID: {0}", elmDevDesc));
            if (elmDevDesc.ToUpperInvariant().Contains(EdElmInterface.Elm327CarlyIdentifier))
            {
                restricted = true;
            }
        }

        if (!Elm327SendCommand(adapterInStream, adapterOutStream, @"AT#1", false))
        {
            LogString("*** ELM read manufacturer failed");
            return false;
        }

        string elmManufact = GetElm327Reponse(adapterInStream);
        if (elmManufact != null)
        {
            LogString(string.Format("ELM Manufacturer: {0}", elmManufact));
            if (elmManufact.ToUpperInvariant().Contains(EdElmInterface.Elm327WgSoftIdentifier))
            {
                if (elmDevDesc != null)
                {
                    string verString = elmDevDesc.Trim('\r', '\n', '>', ' ');
                    if (double.TryParse(verString, NumberStyles.Float, CultureInfo.InvariantCulture, out double version))
                    {
                        if (version < EdElmInterface.Elm327WgSoftMinVer)
                        {
                            restricted = true;
                        }
                    }
                }
            }
        }

        if (!Elm327SendCommand(adapterInStream, adapterOutStream, @"STI", false))
        {
            LogString("*** STN read firmware version failed");
            return false;
        }

        string stnVers = GetElm327Reponse(adapterInStream, true);
        string stnVersExt = null;
        if (stnVers != null)
        {
            LogString(string.Format("STN Version: {0}", stnVers));
            if (!Elm327SendCommand(adapterInStream, adapterOutStream, @"STIX", false))
            {
                LogString("*** STN read ext firmware version failed");
                return false;
            }

            stnVersExt = GetElm327Reponse(adapterInStream, true);
            if (stnVersExt != null)
            {
                LogString(string.Format("STN Ext Version: {0}", stnVersExt));
            }
        }

        if (stnVers != null && stnVersExt != null)
        {
            //stnVers = "STN2255 v5.7.0";
            Regex stnVerRegEx = new Regex(@"STN(\d+)\s+v(\d+)\.(\d+)\.(\d+)", RegexOptions.IgnoreCase);
            MatchCollection matchesVer = stnVerRegEx.Matches(stnVers);
            if ((matchesVer.Count == 1) && (matchesVer[0].Groups.Count == 5))
            {
                if (!Int32.TryParse(matchesVer[0].Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int stnType))
                {
                    stnType = -1;
                }
                if (!Int32.TryParse(matchesVer[0].Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int stnVerH))
                {
                    stnVerH = -1;
                }
                if (!Int32.TryParse(matchesVer[0].Groups[3].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int stnVerM))
                {
                    stnVerM = -1;
                }
                if (!Int32.TryParse(matchesVer[0].Groups[4].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int stnVerL))
                {
                    stnVerL = -1;
                }

                if (stnType >= 0 && stnVerL >= 0 && stnVerM >= 0 && stnVerH >= 0)
                {
                    LogString(string.Format("STN{0:0000} version detected: {1}.{2}.{3}", stnType, stnVerH, stnVerM, stnVerL));
                    int stnVer = stnVerH * 10000 + stnVerM * 100 + stnVerL;
                    int minVer = 0;
                    // ToDo: Update versions from: https://www.scantool.net/scantool/downloads/updates/
                    switch (stnType)
                    {
                        case 1000:  // OBDLink CI
                            minVer = 20200;
                            break;

                        case 1100:  // OBDLink
                        case 1101:  // OBDLink S
                            minVer = 40200;
                            break;

                        case 1120:  // microOBD 200
                            minVer = 40201;
                            break;

                        case 1150:  // OBDLink MX Bluetooth
                        case 1151:  // OBDLink MX Bluetooth 2
                        case 1155:  // OBDLink LX Bluetooth
                            minVer = 50619;
                            break;

                        case 1110:
                        case 1130:  // OBDLink SX
                        case 1170:
                        case 2100:
                        case 2120:
                        case 2230:  // OBDLink EX
                        case 2255:  // OBDLink MX+
                            minVer = 50701;
                            break;
                    }

                    if (stnVer < minVer)
                    {
                        fwUpdate = true;
                    }
                }
            }
        }

        if (!restricted)
        {
            if (!Elm327SendCommand(adapterInStream, adapterOutStream, @"ATPP2COFF"))
            {
                LogString("*** ELM ATPP2COFF failed, fake device");
                return false;
            }
        }

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (restricted)
        {
#if DEBUG
            Android.Util.Log.Info(Tag, "Restricted ELM firmware");
#endif
            LogString("Restricted ELM firmware");
        }
        else
        {
            LogString("Standard ELM firmware");
        }

        if (fwUpdate)
        {
#if DEBUG
            Android.Util.Log.Info(Tag, "STN firmware update required");
#endif
            LogString("STN firmware update required");
        }

        return true;
    }

    /// <summary>
    /// Send command to EL327
    /// </summary>
    /// <param name="adapterInStream">Adapter input stream</param>
    /// <param name="adapterOutStream">Adapter output stream</param>
    /// <param name="command">Command to send</param>
    /// <param name="readAnswer">True: Check for valid answer</param>
    /// <returns>True: command ok</returns>
    private bool Elm327SendCommand(Stream adapterInStream, Stream adapterOutStream, string command, bool readAnswer = true)
    {
        byte[] sendData = Encoding.UTF8.GetBytes(command + "\r");
        LogData(sendData, 0, sendData.Length, "Send");
        adapterOutStream.Write(sendData, 0, sendData.Length);
#if DEBUG
        Android.Util.Log.Info(Tag, string.Format("ELM CMD send: {0}", command));
#endif
        LogString("ELM CMD send: " + command);

        if (readAnswer)
        {
            string answer = GetElm327Reponse(adapterInStream);
            if (answer == null)
            {
                LogString("*** No ELM response");
                return false;
            }
            // check for OK
            if (!answer.Contains("OK\r"))
            {
                LogString("*** ELM invalid response");
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Get response from EL327
    /// </summary>
    /// <param name="adapterInStream">Adapter input stream</param>
    /// <param name="checkValid">Check if resposne is valid</param>
    /// <returns>Response string, null for no reponse</returns>
    private string GetElm327Reponse(Stream adapterInStream, bool checkValid = false)
    {
        LogData(null, 0, 0, "Resp");
        string response = null;
        StringBuilder stringBuilder = new StringBuilder();
        long startTime = Stopwatch.GetTimestamp();
        bool lengthMessage = false;
        for (; ; )
        {
            while (adapterInStream.HasData())
            {
                int data = adapterInStream.ReadByteAsync();
                if (data >= 0 && data != 0x00)
                {
                    // remove 0x00
                    LogByte((byte)data);
                    stringBuilder.Append(EdElmInterface.ConvertToChar(data));
                    startTime = Stopwatch.GetTimestamp();
                }
                if (data == 0x3E)
                {
                    // prompt
                    response = stringBuilder.ToString();
                    break;
                }
                if (stringBuilder.Length > 500)
                {
                    if (!lengthMessage)
                    {
                        lengthMessage = true;
                        LogString("*** ELM response too long");
                    }
                    break;
                }
            }
            if (response != null)
            {
                break;
            }
            if (Stopwatch.GetTimestamp() - startTime > ResponseTimeout * ActivityCommon.TickResolMs)
            {
                LogString("*** ELM response timeout");
                break;
            }
        }
        if (response == null)
        {
            LogString("*** No ELM prompt");
        }
        else
        {
            string bareResponse = response.Replace("\r", "").Replace(">", "");
#if DEBUG
            Android.Util.Log.Info(Tag, string.Format("ELM CMD rec: {0}", bareResponse));
#endif
            LogString("ELM CMD rec: " + bareResponse);
            if (checkValid)
            {
                if (bareResponse.Trim().StartsWith("?"))
                {
                    LogString("*** ELM response not valid");
                    response = null;
                }
            }
        }
        return response;
    }

    public void LogData(byte[] data, int offset, int length, string info = null)
    {
        if (!string.IsNullOrEmpty(info))
        {
            if (_sbLog.Length > 0)
            {
                _sbLog.Append("\n");
            }
            _sbLog.Append(" (");
            _sbLog.Append(info);
            _sbLog.Append("): ");
        }
        if (data != null)
        {
            for (int i = 0; i < length; i++)
            {
                _sbLog.Append(string.Format(ActivityMain.Culture, "{0:X02} ", data[offset + i]));
            }
        }
    }

    public void LogString(string info)
    {
        if (_sbLog.Length > 0)
        {
            _sbLog.Append("\n");
        }
        _sbLog.Append(info);
    }

    public void LogByte(byte data)
    {
        _sbLog.Append(string.Format(ActivityMain.Culture, "{0:X02} ", data));
    }
}
