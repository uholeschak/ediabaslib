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
        private Context _context;
        public Messenger Messenger { get; private set; }
        public bool Bound { get; set; }

        public MtcServiceConnection(Context context)
        {
            _context = context;
        }
        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Messenger = new Messenger(service);
            Bound = Messenger != null;
#if DEBUG
            Log.Info(Tag, string.Format("MTC Service connected: {0}", Bound));
#endif
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            Messenger.Dispose();
            Messenger = null;
            Bound = false;
#if DEBUG
            Log.Info(Tag, "MTC Service disconnected");
#endif
        }
    }
}
