using System;
using Android.Content;
using Android.OS;
using Android.Util;

namespace BmwDeepObd
{
    public class MtcServiceConnection : Java.Lang.Object, IServiceConnection
    {
#if DEBUG
        static readonly string Tag = typeof(MtcServiceConnection).FullName;
#endif
        public const string InterfaceToken = @"android.microntek.mtcser.BTServiceInf";
        public delegate void ServiceConnectedDelegate(bool connected);
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

        public MtcServiceConnection(Context context, ServiceConnectedDelegate connectedHandler = null)
        {
            _context = context;
            _connectedHandler = connectedHandler;
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            _binder = service;
            if (_binder == null)
            {
#if DEBUG
                Log.Info(Tag, "MTC Service no binder");
#endif
                _bound = false;
                return;
            }
            try
            {
                Init();
                _bound = true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Log.Info(Tag, string.Format("MTC init exception: {0}", ex.Message));
#endif
                _bound = false;
            }
            _connectedHandler?.Invoke(Bound);
#if DEBUG
            Log.Info(Tag, string.Format("MTC Service connected: {0}", Bound));
#endif
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            _binder = null;
            Bound = false;
            _connectedHandler?.Invoke(Bound);
#if DEBUG
            Log.Info(Tag, "MTC Service disconnected");
#endif
        }

        public void Init()
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
                _binder.Transact(1, data, reply, 0);
                reply.ReadException();
            }
            finally
            {
                data.Recycle();
                reply.Recycle();
            }
        }

        public sbyte GetBtState()
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
                _binder.Transact(2, data, reply, 0);
                reply.ReadException();
                sbyte result = reply.ReadByte();
#if DEBUG
                Log.Info(Tag, string.Format("MTC Bt state: {0}", result));
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
