using System;
using System.Collections.Generic;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;

namespace BmwDeepObd
{
    public class MtcServiceConnection : Java.Lang.Object, IServiceConnection
    {
#if DEBUG
        private static readonly string Tag = typeof(MtcServiceConnection).FullName;
#endif
        public const string InterfaceToken = @"android.microntek.mtcser.BTServiceInf";
        public const string ServicePkg = @"android.microntek.mtcser";
        public const string ServiceClsV1 = @"android.microntek.mtcser.BTSerialService";
        public const string ServiceClsV2 = @"android.microntek.mtcser.BlueToothService";
        public delegate void ServiceConnectedDelegate(bool connected);
        private static bool? _isHct3;
        // ReSharper disable once NotAccessedField.Local
        private readonly Context _context;
        private readonly ServiceConnectedDelegate _connectedHandler;
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
                    if (IsHct3())
                    {
                        ApiVersion = 3;
                    }
                }

                SyncMatchList();
                _bound = true;
            }
#pragma warning disable 168
            catch (Exception ex)
#pragma warning restore 168
            {
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("MTC init exception: {0}", ex.Message));
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

        private bool IsHct3()
        {
            if (_isHct3.HasValue)
            {
                return _isHct3.Value;
            }

            _isHct3 = false;

            try
            {
                IList<PackageInfo> installedPackages = _context?.PackageManager.GetInstalledPackages(PackageInfoFlags.MatchSystemOnly);
                if (installedPackages == null)
                {
                    return _isHct3.Value;
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
                            if (!string.IsNullOrEmpty(fileName) && fileName.Contains("HCT3", StringComparison.OrdinalIgnoreCase))
                            {
                                _isHct3 = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return _isHct3.Value;
        }

        public static string CarManagerGetParameters(string args)
        {
            try
            {
                Java.Lang.Class carManager = Java.Lang.Class.ForName(@"android.microntek.CarManager");
                Java.Lang.Object carManagerInst = carManager.NewInstance();
                Java.Lang.Reflect.Method[] methods = carManager.GetDeclaredMethods();
                Java.Lang.Reflect.Method methodGetParameters = null;
                foreach (Java.Lang.Reflect.Method method in methods)
                {
                    if (string.Compare(method.Name, "getParameters", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        methodGetParameters = method;
                        break;
                    }
                }

                if (methodGetParameters == null)
                {
                    return null;
                }

                Java.Lang.Object[] pars = { new Java.Lang.String(args) };
                Java.Lang.Object paramResult = methodGetParameters.Invoke(carManagerInst, pars);
                Java.Lang.String paramString = paramResult.JavaCast<Java.Lang.String>();

                return paramString.ToString();
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

        public void SetAutoConnect(bool auto)
        {
            CommandSetInt(ApiVersion > 2 ? 31 : 22, auto ? 1 : 0);
        }

        public bool GetAutoConnect()
        {
            return CommandGetInt(ApiVersion > 2 ? 32 : 23) != 0;
        }

        public void SetAutoAnswer(bool auto)
        {
            CommandSetInt(ApiVersion > 2 ? 33 : 24, auto ? 1 : 0);
        }

        public bool GetAutoAnswer()
        {
            return CommandGetInt(ApiVersion > 2 ? 34 : 25) != 0;
        }

        public void ConnectBt(string mac)
        {
            CommandMac(ApiVersion > 2 ? 35 : 26, mac);
        }

        public void DisconnectBt(string mac)
        {
            CommandMac(ApiVersion > 2 ? 36 : 27, mac);
        }

        public void ConnectObd(string mac)
        {
            CommandMac(ApiVersion > 2 ? 37 : 28, mac);
        }

        public void DisconnectObd(string mac)
        {
            CommandMac(ApiVersion > 2 ? 38 : 29, mac);
        }

        public void DeleteObd(string mac)
        {
            CommandMac(ApiVersion > 2 ? 39 : 30, mac);
        }

        public void DeleteBt(string mac)
        {
            CommandMac(ApiVersion > 2 ? 40 : 31, mac);
        }

        public void SyncMatchList()
        {
            CommandVoid(ApiVersion > 2 ? 41 : 32);
        }

        public IList<string> GetMatchList()
        {
            return CommandGetList(ApiVersion > 2 ? 42 : 33);
        }

        public IList<string> GetDeviceList()
        {
            return CommandGetList(ApiVersion > 2 ? 43 : 34);
        }

        public void ScanStart()
        {
            CommandVoid(ApiVersion > 2 ? 51 : 42);
        }

        public void ScanStop()
        {
            CommandVoid(ApiVersion > 2 ? 52 : 43);
        }

        public int GetObdState()
        {
            return CommandGetInt(ApiVersion > 2 ? 57 : 48);
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

        private void CommandMac(int code, string mac)
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
                Android.Util.Log.Info(Tag, string.Format("SetMac({0}, {1})", code, mac));
#endif
                data.WriteInterfaceToken(InterfaceToken);
                data.WriteString(mac);
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

        // ReSharper disable once UnusedMember.Local
        private int CommandGetDataSize(int code)
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
                int dataSize = reply.DataSize();
#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("DataSize({0}): 0x{1:X02}", code, dataSize));
#endif
                return dataSize;
            }
            finally
            {
                data.Recycle();
                reply.Recycle();
            }
        }

    }
}
