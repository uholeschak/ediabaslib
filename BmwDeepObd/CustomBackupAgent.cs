#if DEBUG
using Android.App.Backup;
using Android.OS;
using Java.IO;
using System.Runtime.Versioning;

namespace BmwDeepObd;

public class CustomBackupAgent : BackupAgent
{
    private static readonly string Tag = typeof(CustomBackupAgent).FullName;

    public override void OnCreate()
    {
        Android.Util.Log.Info(Tag, "OnCreate");
        base.OnCreate();
    }

    public override void OnDestroy()
    {
        Android.Util.Log.Info(Tag, "OnDestroy");
        base.OnDestroy();
    }

    [SupportedOSPlatform("android28.0")]
    public override void OnBackup(ParcelFileDescriptor oldState, BackupDataOutput data, ParcelFileDescriptor newState)
    {
        BackupTransportFlags transportFlags = 0;
        long quota = 0;
        if (data != null)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
            {
                transportFlags = data.TransportFlags;
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                quota = data.Quota;
            }
        }

        Android.Util.Log.Info(Tag, string.Format("OnBackup: Flags={0}, Quota={1}", transportFlags, quota));
    }

    public override void OnRestore(BackupDataInput data, int appVersionCode, ParcelFileDescriptor newState)
    {
        int dataSize = 0;
        if (data != null)
        {
            dataSize = data.DataSize;
        }

        Android.Util.Log.Info(Tag, string.Format("OnRestore: Version={0}, Size={1}", appVersionCode, dataSize));
    }

    [SupportedOSPlatform("android28.0")]
    public override void OnRestore(BackupDataInput data, long appVersionCode, ParcelFileDescriptor newState)
    {
        int dataSize = 0;
        if (data != null)
        {
            dataSize = data.DataSize;
        }

        Android.Util.Log.Info(Tag, string.Format("OnRestore: Version={0}, Size={1}", appVersionCode, dataSize));
        base.OnRestore(data, appVersionCode, newState);
    }

    [SupportedOSPlatform("android28.0")]
    public override void OnFullBackup(FullBackupDataOutput data)
    {
        BackupTransportFlags transportFlags = 0;
        long quota = 0;
        if (data != null)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
            {
                transportFlags = data.TransportFlags;
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                quota = data.Quota;
            }
        }

        Android.Util.Log.Info(Tag, string.Format("OnFullBackup: Flags={0}, Quota={1}", transportFlags, quota));
        base.OnFullBackup(data);
    }

    public override void OnRestoreFile(ParcelFileDescriptor data, long size, File destination, BackupFileType type, long mode, long mtime)
    {
        string fileName = string.Empty;
        if (destination != null)
        {
            fileName = destination.AbsoluteFile.Name;
        }

        Android.Util.Log.Info(Tag, string.Format("OnRestoreFile: Name={0}, Size={1}", fileName, size));
        base.OnRestoreFile(data, size, destination, type, mode, mtime);
    }

    public override void OnRestoreFinished()
    {
        Android.Util.Log.Info(Tag, "OnRestoreFinished");
        base.OnRestoreFinished();
    }

    [SupportedOSPlatform("android24.0")]
    public override void OnQuotaExceeded(long backupDataBytes, long quotaBytes)
    {
        Android.Util.Log.Info(Tag, string.Format("OnQuotaExceeded: Backup={0}, Quota={1}", backupDataBytes, quotaBytes));
        base.OnQuotaExceeded(backupDataBytes, quotaBytes);
    }
}
#endif
