using Android.App.Backup;
using Android.OS;

namespace BmwDeepObd;

public class CustomBackupAgent : BackupAgent
{
#if DEBUG
    private static readonly string Tag = typeof(CustomBackupAgent).FullName;
#endif

    public override void OnCreate()
    {
        base.OnCreate();
#if DEBUG
        Android.Util.Log.Info(Tag, "OnCreate");
#endif
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
#if DEBUG
        Android.Util.Log.Info(Tag, "OnDestroy");
#endif
    }

    public override void OnBackup(ParcelFileDescriptor oldState, BackupDataOutput data, ParcelFileDescriptor newState)
    {
#if DEBUG
        Android.Util.Log.Info(Tag, string.Format("OnBackup: Flags={0}", data?.TransportFlags));
#endif
    }

    public override void OnRestore(BackupDataInput data, int appVersionCode, ParcelFileDescriptor newState)
    {
#if DEBUG
        Android.Util.Log.Info(Tag, string.Format("OnRestore: Version={0}", appVersionCode));
#endif
    }

    public override void OnQuotaExceeded(long backupDataBytes, long quotaBytes)
    {
        base.OnQuotaExceeded(backupDataBytes, quotaBytes);
#if DEBUG
        Android.Util.Log.Info(Tag, string.Format("OnQuotaExceeded: Backup={0}, Quota={1}", backupDataBytes, quotaBytes));
#endif
    }
}
