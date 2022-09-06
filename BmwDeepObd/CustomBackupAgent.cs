using Android.App.Backup;
using Android.OS;
using Java.IO;

namespace BmwDeepObd;

public class CustomBackupAgent : BackupAgent
{
#if DEBUG
    private static readonly string Tag = typeof(CustomBackupAgent).FullName;
#endif

    public override void OnCreate()
    {
#if DEBUG
        Android.Util.Log.Info(Tag, "OnCreate");
#endif
        base.OnCreate();
    }

    public override void OnDestroy()
    {
#if DEBUG
        Android.Util.Log.Info(Tag, "OnDestroy");
#endif
        base.OnDestroy();
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

    public override void OnRestore(BackupDataInput data, long appVersionCode, ParcelFileDescriptor newState)
    {
#if DEBUG
        Android.Util.Log.Info(Tag, string.Format("OnRestore: Version={0}", appVersionCode));
#endif
        base.OnRestore(data, appVersionCode, newState);
    }

    public override void OnFullBackup(FullBackupDataOutput data)
    {
#if DEBUG
        Android.Util.Log.Info(Tag, string.Format("OnFullBackup: Quota={0}", data?.Quota));
#endif
        base.OnFullBackup(data);
    }

    public override void OnRestoreFile(ParcelFileDescriptor data, long size, File destination, BackupFileType type, long mode, long mtime)
    {
#if DEBUG
        Android.Util.Log.Info(Tag, string.Format("OnRestoreFile: Name={0}, Size={1}", destination?.AbsoluteFile.Name, size));
#endif
        base.OnRestoreFile(data, size, destination, type, mode, mtime);
    }

    public override void OnRestoreFinished()
    {
#if DEBUG
        Android.Util.Log.Info(Tag, "OnRestoreFinished");
#endif
        base.OnRestoreFinished();
    }

    public override void OnQuotaExceeded(long backupDataBytes, long quotaBytes)
    {
#if DEBUG
        Android.Util.Log.Info(Tag, string.Format("OnQuotaExceeded: Backup={0}, Quota={1}", backupDataBytes, quotaBytes));
#endif
        base.OnQuotaExceeded(backupDataBytes, quotaBytes);
    }
}
