using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace BmwDeepObd
{
    public class MtcServiceConnection : Java.Lang.Object, IServiceConnection
    {
        /*
        Cmd | API4 | API3 | API2 | API1
        init | 1 | 1 | 1 | 1
        getBTState | 2 | 2 | 2 | 2
        getAVState | 3 | 3 | 3 | 3
        getDialOutNum | 4 | 4 | 4 | 4
        getCallInNum | 5 | 5 | 5 | 5
        getPhoneNum | 6 | 6 | 6 | 6
        IsThreeWayCalling | 7 | 7 | - | -
        IsConferenceCalling | 8 | 8 | - | -
        getThreeWayCallNum | 9 | 9 | - | -
        getCallingNumberList | 10 | 10 | - | -
        getNowDevAddr | 11 | 11 | 7 | 7
        getNowDevName | 12 | 12 | 8 | 8
        getNowDevUuids | 13 | 13 | 9 | -
        avPlay | 14 | - | - | -
        avPlayPause | 15 | 14 | 9 | 9
        avPlayStop | 16 | 15 | 10 | 10
        avPlayPrev | 17 | 16 | 11 | 11
        avPlayNext | 18 | 17 | 12 | 12
        answerCall | 19 | 18 | 13 | 13
        hangupCall | 20 | 19 | 14 | 14
        rejectCall | 21 | 20 | 15 | 15
        addCall | 22 | 21 | - | -
        swichCall | 23 | 22 | - | -
        mergeCall | 24 | 23 | - | -
        voiceControl | 25 | 24 | - | -
        switchVoice | 26 | 25 | 16 | 16
        syncPhonebook | 27 | 26 | 17 | 17
        getModuleName | 28 | 27 | 18 | 18
        getModulePassword | 29 | 28 | 19 | 19
        setModuleName | 30 | 29 | 20 | 20
        setModulePassword | 31 | 30 | 21 | 21
        setAutoConnect | 32 | 31 | 22 | 22
        getAutoConnect | 33 | 32 | 23 | 23
        setAutoAnswer | 34 | 33 | 24 | 24
        getAutoAnswer | 35 | 34 | 25 | 25
        connectBT | 36 | 35 | 26 | 26
        disconnectBT | 37 | 36 | 27 | 27
        connectOBD | 38 | 37 | 28 | 28
        disconnectOBD | 39 | 38 | 29 | 29
        deleteOBD | 40 | 39 | 30 | 30
        deleteBT | 41 | 40 | 31 | 31
        syncMatchList | 42 | 41 | 32 | 32
        getMatchList | 43 | 42 | 33 | 33
        getDeviceList | 44 | 43 | 34 | 34
        getHistoryList | 45 | 44 | 35 | 35
        getPhoneBookList | 46 | 45 | 36 | -
        setPhoneBookList | 47 | 46 | 37 | -
        deleteHistory | 48 | 47 | 38 | 36
        deleteHistoryAll | 49 | 48 | 39 | 37
        musicMute | 50 | 49 | 40 | 38
        musicUnmute | 51 | 50 | 41 | 39
        scanStart | 52 | 51 | 42 | 40
        scanStop | 53 | 52 | 43 | 41
        dialOut | 54 | 53 | 44 | 42
        dialOutSub | 55 | 54 | 45 | 43
        reDial | 56 | 55 | 46 | -
        getMusicInfo | 57 | 56 | 47 | -
        getOBDstate | 58 | 57 | 48 | -
        requestBtInfo | 59 | 58 | 49 | -
        */
#if DEBUG
        private static readonly string Tag = typeof(MtcServiceConnection).FullName;
#endif
        public const string InterfaceToken = @"android.microntek.mtcser.BTServiceInf";
        public const string ServicePkg = @"android.microntek.mtcser";
        public const string ServiceClsV1 = @"android.microntek.mtcser.BTSerialService";
        public const string ServiceClsV2 = @"android.microntek.mtcser.BlueToothService";
        public static string[] BtModulesNames =
        {
            "NO",
            "MD725",
            "WQ_GT",
            "WQ_BC6",
            "WQ_BC8",
            "Parrot_FC6000T",
            "SD-968",
            "SD-BC6",
            "SD-GT936",
            "BARROT-i145",
            "SD-916",
            "WQ_RF210",
            "SD-816",
            "FSC-BW124",
            "SDIO-AUTO",
            "BARROT-i1107e",
        };

        public delegate void ServiceConnectedDelegate(bool connected);
        private static readonly Regex HctVerRegEx = new Regex(@"HCT(\d)+(\D)", RegexOptions.IgnoreCase);
        private static int? _hctApiVerDetected;
        // ReSharper disable once NotAccessedField.Local
        private readonly Context _context;
        private readonly ServiceConnectedDelegate _connectedHandler;
        private Java.Lang.Class _carManager;
        private Java.Lang.Object _carManagerInst;
        private IBinder _binder;
        private bool _bound;
        public bool Bound
        {
            get => _bound;
            set
            {
                if (!value)
                {
                    _bound = false;
                    _binder = null;
                }
            }
        }
        public bool Connected => _binder != null;

        public int ApiVersion { get; private set; }

        public int ApiOffset => ApiVersion > 3 ? 1 : 0;

        public MtcServiceConnection(Context context, ServiceConnectedDelegate connectedHandler = null)
        {
            _context = context;
            _connectedHandler = connectedHandler;
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
#if DEBUG
            Android.Util.Log.Info(Tag, string.Format("MTC Service connected: {0}", name));
#endif
            _binder = service;
            if (_binder == null)
            {
#if DEBUG
                Android.Util.Log.Info(Tag, "MTC Service no binder");
#endif
                _bound = false;
                return;
            }

            try
            {
                Init();
                if (name?.ClassName != null && String.Compare(name.ClassName, ServiceClsV1, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ApiVersion = 1;
                }
                else
                {
                    ApiVersion = 2;
                    int hctApiVerDetected = HctApiVerDetect();
                    if (hctApiVerDetected != 0)
                    {
                        ApiVersion = hctApiVerDetected;
                    }
                }

                //SyncMatchList();  // this command crashes on some devices!
                _bound = true;
            }
#pragma warning disable 168
            catch (Exception ex)
#pragma warning restore 168
            {
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("MTC init exception: {0}", EdiabasLib.EdiabasNet.GetExceptionText(ex)));
#endif
                _bound = false;
                ApiVersion = 0;
            }

            _connectedHandler?.Invoke(Bound);
#if DEBUG
            Android.Util.Log.Info(Tag, string.Format("MTC Service connected: {0}, ClassName: {1}, Version: {2}", Bound, name?.ClassName ?? string.Empty, ApiVersion));
#endif
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            _binder = null;
            Bound = false;
            ApiVersion = 0;
            _connectedHandler?.Invoke(Bound);
#if DEBUG
            Android.Util.Log.Info(Tag, "MTC Service disconnected");
#endif
        }

        private int HctApiVerDetect()
        {
            if (_hctApiVerDetected.HasValue)
            {
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("HctApiVerDetect cache: {0}", _hctApiVerDetected.Value));
#endif
                return _hctApiVerDetected.Value;
            }

            _hctApiVerDetected = 0;

            try
            {
                IList<PackageInfo> installedPackages = ActivityCommon.GetInstalledPackages(_context?.PackageManager, PackageInfoFlags.MatchSystemOnly);
                if (installedPackages == null)
                {
                    return _hctApiVerDetected.Value;
                }

                foreach (PackageInfo packageInfo in installedPackages)
                {
                    ApplicationInfo appInfo = packageInfo.ApplicationInfo;
                    if (appInfo != null)
                    {
                        string sourceDir = appInfo.PublicSourceDir;
                        if (!string.IsNullOrEmpty(sourceDir) &&
                            !string.IsNullOrEmpty(appInfo.PackageName) && appInfo.PackageName.Contains("microntek", StringComparison.OrdinalIgnoreCase))
                        {
                            string fileName = System.IO.Path.GetFileName(sourceDir);
#if DEBUG
                            Android.Util.Log.Info(Tag, string.Format("HctApiVerDetect: package name='{0}', file name='{1}'", appInfo.PackageName, fileName));
#endif
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                int? apiVerNew = null;

                                MatchCollection matchesVer = HctVerRegEx.Matches(fileName);
                                if ((matchesVer.Count == 1) && (matchesVer[0].Groups.Count == 3))
                                {
                                    if (!Int32.TryParse(matchesVer[0].Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out Int32 hctVer))
                                    {
                                        hctVer = -1;
                                    }

                                    string hctSuffix = matchesVer[0].Groups[2].Value;
                                    if (hctVer == 3)
                                    {
                                        apiVerNew = 3;
                                        if (hctSuffix.StartsWith("C", StringComparison.OrdinalIgnoreCase))
                                        {
                                            apiVerNew = 4;
                                        }
                                    }
                                    else if (hctVer >= 4)
                                    {
                                        apiVerNew = 4;
                                    }
                                }

                                if (apiVerNew.HasValue && apiVerNew.Value > _hctApiVerDetected.Value)
                                {
                                    _hctApiVerDetected = apiVerNew;
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
#if DEBUG
            Android.Util.Log.Info(Tag, string.Format("Hct3ApiVer: {0}", _hctApiVerDetected.Value));
#endif
            return _hctApiVerDetected.Value;
        }

        public string CarManagerGetParameters(string args)
        {
            try
            {
                if (_carManager == null)
                {
                    _carManager = Java.Lang.Class.ForName(@"android.microntek.CarManager");
                }
                if (_carManager == null)
                {
                    return null;
                }

                if (_carManagerInst == null)
                {
                    _carManagerInst = _carManager.GetConstructor().NewInstance();
                }
                if (_carManagerInst == null)
                {
                    return null;
                }

                Java.Lang.Reflect.Method methodGetParameters =
                    _carManager.GetDeclaredMethod(@"getParameters", Java.Lang.Class.FromType(typeof(Java.Lang.String)));
                if (methodGetParameters == null)
                {
                    return null;
                }

                Java.Lang.Object paramResult = methodGetParameters.Invoke(_carManagerInst, new Java.Lang.String(args));
                Java.Lang.String paramString = Android.Runtime.Extensions.JavaCast<Java.Lang.String>(paramResult);

                return paramString.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public byte[] CarManagerGetCfg(bool user)
        {
            try
            {
                string cfgString = CarManagerGetParameters(user ? @"cfg_user=" : @"cfg_factory=");
                string[] cfgArray = cfgString?.Split(',');
                return cfgArray?.Select(byte.Parse).ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string CarManagerGetBtModuleName()
        {
            try
            {
                byte[] cfgArray = CarManagerGetCfg(false);
                if (cfgArray == null || cfgArray.Length != 128)
                {
                    return null;
                }

                byte btModuleIdx = cfgArray[4 + 4 + 4 + 24 + 16 + 7];
                if (btModuleIdx == 0 || btModuleIdx >= BtModulesNames.Length)
                {
                    return null;
                }

                string result = BtModulesNames[btModuleIdx];
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("BT module name: {0}", result));
#endif
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string CarManagerGetMcuVersion()
        {
            try
            {
                string result = CarManagerGetParameters("sta_mcu_version=");
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("STA MCU version: {0}", result));
#endif
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string CarManagerGetMcuDate()
        {
            try
            {
                string result = CarManagerGetParameters("sta_mcu_date=");
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("STA MCU date: {0}", result));
#endif
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string CarManagerGetBtPin()
        {
            try
            {
                string result = CarManagerGetParameters("sta_bt_pin=");
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("STA Bt Pin: {0}", result));
#endif
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string CarManagerGetBtName()
        {
            try
            {
                string result = CarManagerGetParameters("sta_bt_name=");
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("STA Bt Name: {0}", result));
#endif
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public byte[] CommandTest(int code)
        {
            try
            {
                return CommandGetRawData(code);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Init()
        {
            CommandVoid(1);
        }

        public sbyte GetBtState()
        {
            sbyte result = CommandGetByte(2);
#if DEBUG
            Android.Util.Log.Info(Tag, string.Format("MTC Bt state: {0}", result));
#endif
            return result;
        }

        public long GetNowDevAddr()
        {
            return CommandGetLong(ApiVersion > 2 ? 11 : 7);
        }

        public string GetNowDevName()
        {
            return CommandGetString(ApiVersion > 2 ? 12 : 8);
        }

        public string GetModuleName()
        {
            return CommandGetString(ApiVersion > 2 ? 27 + ApiOffset : 18);
        }

        public string GetModulePassword()
        {
            return CommandGetString(ApiVersion > 2 ? 28 + ApiOffset : 19);
        }

        public void SetModuleName(string name)
        {
            CommandSetString(ApiVersion > 2 ? 29 + ApiOffset : 20, name);
        }

        public void SetModulePassword(string name)
        {
            CommandSetString(ApiVersion > 2 ? 30 + ApiOffset : 21, name);
        }

        public void SetAutoConnect(bool auto)
        {
            CommandSetInt(ApiVersion > 2 ? 31 + ApiOffset : 22, auto ? 1 : 0);
        }

        public bool GetAutoConnect()
        {
            return CommandGetInt(ApiVersion > 2 ? 32 + ApiOffset : 23) != 0;
        }

        public void SetAutoAnswer(bool auto)
        {
            CommandSetInt(ApiVersion > 2 ? 33 + ApiOffset : 24, auto ? 1 : 0);
        }

        public bool GetAutoAnswer()
        {
            return CommandGetInt(ApiVersion > 2 ? 34 + ApiOffset : 25) != 0;
        }

        public void ConnectBt(string mac)
        {
            CommandSetString(ApiVersion > 2 ? 35 + ApiOffset : 26, mac);
        }

        public void DisconnectBt(string mac)
        {
            CommandSetString(ApiVersion > 2 ? 36 + ApiOffset : 27, mac);
        }

        public void ConnectObd(string mac)
        {
            CommandSetString(ApiVersion > 2 ? 37 + ApiOffset : 28, mac);
        }

        public void DisconnectObd(string mac)
        {
            CommandSetString(ApiVersion > 2 ? 38 + ApiOffset : 29, mac);
        }

        public void DeleteObd(string mac)
        {
            CommandSetString(ApiVersion > 2 ? 39 + ApiOffset : 30, mac);
        }

        public void DeleteBt(string mac)
        {
            CommandSetString(ApiVersion > 2 ? 40 + ApiOffset : 31, mac);
        }

        public void SyncMatchList()
        {
            CommandVoid(ApiVersion > 2 ? 41 + ApiOffset : 32);
        }

        public IList<string> GetMatchList()
        {
            return CommandGetList(ApiVersion > 2 ? 42 + ApiOffset : 33);
        }

        public IList<string> GetDeviceList()
        {
            return CommandGetList(ApiVersion > 2 ? 43 + ApiOffset : 34);
        }

        public void ScanStart()
        {
            CommandVoid(ApiVersion > 2 ? 51 + ApiOffset : 42);
        }

        public void ScanStop()
        {
            CommandVoid(ApiVersion > 2 ? 52 + ApiOffset : 43);
        }

        public int GetObdState()
        {
            return CommandGetInt(ApiVersion > 2 ? 57 + ApiOffset : 48);
        }

        private void CommandVoid(int code)
        {
            if (_binder == null)
            {
                throw new RemoteException("Not bound");
            }
            Parcel data = Parcel.Obtain();
            Parcel reply = Parcel.Obtain();
            try
            {
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("Void({0})", code));
#endif
                data.WriteInterfaceToken(InterfaceToken);
                _binder.Transact(code, data, reply, 0);
                reply.ReadException();
            }
            finally
            {
                data.Recycle();
                reply.Recycle();
            }
        }

        private sbyte CommandGetByte(int code)
        {
            if (_binder == null)
            {
                throw new RemoteException("Not bound");
            }
            Parcel data = Parcel.Obtain();
            Parcel reply = Parcel.Obtain();
            try
            {
                data.WriteInterfaceToken(InterfaceToken);
                _binder.Transact(code, data, reply, 0);
                reply.ReadException();
                sbyte result = reply.ReadByte();
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("GetByte({0}): 0x{1:X02}", code, result));
#endif
                return result;
            }
            finally
            {
                data.Recycle();
                reply.Recycle();
            }
        }

        private int CommandGetInt(int code)
        {
            if (_binder == null)
            {
                throw new RemoteException("Not bound");
            }
            Parcel data = Parcel.Obtain();
            Parcel reply = Parcel.Obtain();
            try
            {
                data.WriteInterfaceToken(InterfaceToken);
                _binder.Transact(code, data, reply, 0);
                reply.ReadException();
                int result = reply.ReadInt();
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("GetInt({0}): 0x{1:X08}", code, result));
#endif
                return result;
            }
            finally
            {
                data.Recycle();
                reply.Recycle();
            }
        }

        private long CommandGetLong(int code)
        {
            if (_binder == null)
            {
                throw new RemoteException("Not bound");
            }
            Parcel data = Parcel.Obtain();
            Parcel reply = Parcel.Obtain();
            try
            {
                data.WriteInterfaceToken(InterfaceToken);
                _binder.Transact(code, data, reply, 0);
                reply.ReadException();
                long result = reply.ReadLong();
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("GetLong({0}): 0x{1:X016}", code, result));
#endif
                return result;
            }
            finally
            {
                data.Recycle();
                reply.Recycle();
            }
        }

        private string CommandGetString(int code)
        {
            if (_binder == null)
            {
                throw new RemoteException("Not bound");
            }
            Parcel data = Parcel.Obtain();
            Parcel reply = Parcel.Obtain();
            try
            {
                data.WriteInterfaceToken(InterfaceToken);
                _binder.Transact(code, data, reply, 0);
                reply.ReadException();
                string result = reply.ReadString();
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("GetString({0}): {1}", code, result));
#endif
                return result;
            }
            finally
            {
                data.Recycle();
                reply.Recycle();
            }
        }

        private void CommandSetInt(int code, int value)
        {
            if (_binder == null)
            {
                throw new RemoteException("Not bound");
            }
            Parcel data = Parcel.Obtain();
            Parcel reply = Parcel.Obtain();
            try
            {
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("SetInt({0}, {1})", code, value));
#endif
                data.WriteInterfaceToken(InterfaceToken);
                data.WriteInt(value);
                _binder.Transact(code, data, reply, 0);
                reply.ReadException();
            }
            finally
            {
                data.Recycle();
                reply.Recycle();
            }
        }

        private void CommandSetString(int code, string text)
        {
            if (_binder == null)
            {
                throw new RemoteException("Not bound");
            }
            Parcel data = Parcel.Obtain();
            Parcel reply = Parcel.Obtain();
            try
            {
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("SetMac({0}, {1})", code, text));
#endif
                data.WriteInterfaceToken(InterfaceToken);
                data.WriteString(text);
                _binder.Transact(code, data, reply, 0);
                reply.ReadException();
            }
            finally
            {
                data.Recycle();
                reply.Recycle();
            }
        }

        private IList<string> CommandGetList(int code)
        {
            if (_binder == null)
            {
                throw new RemoteException("Not bound");
            }
            Parcel data = Parcel.Obtain();
            Parcel reply = Parcel.Obtain();
            try
            {
                data.WriteInterfaceToken(InterfaceToken);
                _binder.Transact(code, data, reply, 0);
                reply.ReadException();
                IList<string> result = reply.CreateStringArrayList();
#if DEBUG
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append(string.Format("GetList({0}): {1}=", code, result.Count));
                foreach (string text in result)
                {
                    sb.Append("\"");
                    sb.Append(text);
                    sb.Append("\" ");
                }
                Android.Util.Log.Info(Tag, sb.ToString());
#endif
                return result;
            }
            finally
            {
                data.Recycle();
                reply.Recycle();
            }
        }

        private byte[] CommandGetRawData(int code)
        {
            if (_binder == null)
            {
                throw new RemoteException("Not bound");
            }
            Parcel data = Parcel.Obtain();
            Parcel reply = Parcel.Obtain();
            try
            {
                data.WriteInterfaceToken(InterfaceToken);
                _binder.Transact(code, data, reply, 0);
                reply.ReadException();
                byte[] dataArray = reply.Marshall();
#if DEBUG
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append(string.Format("DataArray({0}): {1}=", code, dataArray?.Length));
                if (dataArray != null)
                {
                    foreach (byte value in dataArray)
                    {
                        sb.Append(string.Format("{0:X02} ", value));
                    }
                }
                Android.Util.Log.Info(Tag, sb.ToString());
#endif
                return dataArray;
            }
            finally
            {
                data.Recycle();
                reply.Recycle();
            }
        }
    }
}
