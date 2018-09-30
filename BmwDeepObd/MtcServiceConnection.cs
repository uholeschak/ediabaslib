using System;
using System.Collections.Generic;
using Android.Content;
using Android.OS;

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
            }
            if (name?.ClassName != null && String.Compare(name.ClassName, ServiceClsV1, StringComparison.OrdinalIgnoreCase) == 0)
            {
                ApiVersion = 1;
            }
            else
            {
                ApiVersion = 2;
            }
            _connectedHandler?.Invoke(Bound);
#if DEBUG
            Android.Util.Log.Info(Tag, string.Format("MTC Service connected: {0}, Version: {1}", Bound, ApiVersion));
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
            return CommandGetLong(7);
        }

        public string GetNowDevName()
        {
            return CommandGetString(8);
        }

        public void SetAutoConnect(bool auto)
        {
            CommandSetInt(22, auto ? 1 : 0);
        }

        public bool GetAutoConnect()
        {
            return CommandGetInt(23) != 0;
        }

        public void SetAutoAnswer(bool auto)
        {
            CommandSetInt(24, auto ? 1 : 0);
        }

        public bool GetAutoAnswer()
        {
            return CommandGetInt(25) != 0;
        }

        public void ConnectBt(string mac)
        {
            CommandMac(26, mac);
        }

        public void DisconnectBt(string mac)
        {
            CommandMac(27, mac);
        }

        public void ConnectObd(string mac)
        {
            CommandMac(28, mac);
        }

        public void DisconnectObd(string mac)
        {
            CommandMac(29, mac);
        }

        public void DeleteObd(string mac)
        {
            CommandMac(30, mac);
        }

        public void DeleteBt(string mac)
        {
            CommandMac(31, mac);
        }

        public IList<string> GetMatchList()
        {
            return CommandGetList(33);
        }

        public IList<string> GetDeviceList()
        {
            return CommandGetList(34);
        }

        public void ScanStart()
        {
            CommandVoid(42);
        }

        public void ScanStop()
        {
            CommandVoid(43);
        }

        public int GetObdState()
        {
            return CommandGetInt(48);
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
                sb.Append(string.Format("GetList({0}): ", code));
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

    }
}
