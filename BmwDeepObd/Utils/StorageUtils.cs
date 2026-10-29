using Android.OS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ReSharper disable StringLiteralTypo

namespace BmwDeepObd
{
    public static class StorageUtils
    {
#if DEBUG
        private static readonly string Tag = typeof(StorageUtils).FullName;
#endif

        public class FileSystemBlockInfo
        {
            /// <summary>
            /// The path you asked to check file allocation blocks for
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// The file system block size, in bytes, for the given path
            /// </summary>
            public double BlockSizeBytes { get; set; }

            /// <summary>
            /// Total size of the file system at the given path
            /// </summary>
            public double TotalSizeBytes { get; set; }

            /// <summary>
            /// Available size of the file system at the given path
            /// </summary>
            public double AvailableSizeBytes { get; set; }

            /// <summary>
            /// Total free size of the file system at the given path
            /// </summary>
            public double FreeSizeBytes { get; set; }
        }

        private static string _externalPath;
        private static string _externalWritePath;

        public static string ExternalPath => _externalPath;

        public static string ExternalWritePath => _externalWritePath;

        public static void SetStoragePath()
        {
            _externalPath = string.Empty;
            _externalWritePath = string.Empty;
            if (Build.VERSION.SdkInt < BuildVersionCodes.Q)
            {
#pragma warning disable 618
                Java.IO.File extDir = Android.OS.Environment.ExternalStorageDirectory;
#pragma warning restore 618
                string extState = Android.OS.Environment.ExternalStorageState;
                if (extDir != null && extState != null && extDir.IsDirectory && extState.Equals(Android.OS.Environment.MediaMounted))
                {
                    _externalPath = extDir.AbsolutePath;
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("SetStoragePath: ExternalStorageDirectory: {0}", _externalPath));
#endif
                }
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {   // writing to external disk is only allowed in special directories.
                Java.IO.File[] externalFilesDirs = Android.App.Application.Context.GetExternalFilesDirs(null);
                if (externalFilesDirs?.Length > 0)
                {
                    // index 0 is the internal disk
                    if (externalFilesDirs.Length > 1)
                    {
                        Java.IO.File extDir = externalFilesDirs[1];
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (extDir != null)
                        {
                            string extState = Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ?
                                Android.OS.Environment.GetExternalStorageState(extDir) : Android.OS.Environment.ExternalStorageState;
                            if (extState != null && extDir.IsDirectory && extState.Equals(Android.OS.Environment.MediaMounted) && IsWritable(extDir.AbsolutePath))
                            {
                                _externalWritePath = extDir.AbsolutePath;
#if DEBUG
                                Android.Util.Log.Info(Tag, string.Format("SetStoragePath: GetExternalFilesDirs[1]: {0}", _externalWritePath));
#endif
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(_externalWritePath))
                    {
                        Java.IO.File extDir = externalFilesDirs[0];
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (extDir != null)
                        {
                            string extState = Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ?
                                Android.OS.Environment.GetExternalStorageState(extDir) : Android.OS.Environment.ExternalStorageState;
                            if (extState != null && extDir.IsDirectory && extState.Equals(Android.OS.Environment.MediaMounted) && IsWritable(extDir.AbsolutePath))
                            {
                                _externalWritePath = extDir.AbsolutePath;
#if DEBUG
                                Android.Util.Log.Info(Tag, string.Format("SetStoragePath: GetExternalFilesDirs[0]: {0}", _externalWritePath));
#endif
                            }
                        }
                    }
                }
            }
            else
            {
                string procMounts = ReadProcMounts();
                string sdCardEntry = ParseProcMounts(procMounts, _externalPath);
                if (!string.IsNullOrEmpty(sdCardEntry))
                {
                    _externalPath = sdCardEntry;
                }
            }
        }

        public static string ReadProcMounts()
        {
            try
            {
                string contents = File.ReadAllText("/proc/mounts");
                return contents;
            }
            catch (Exception)
            {
                // ignored
            }
            return string.Empty;
        }

        public static bool IsWritable(string pathToTest)
        {
            bool result = false;

            if (!string.IsNullOrWhiteSpace(pathToTest))
            {
                const string testText = "test text";
                string testFile = Guid.NewGuid() + ".txt";
                string testPath = Path.Combine(pathToTest, testFile);
                try
                {
                    File.WriteAllText(testPath, testText);
                    // case sensitive file systems are supported now
                    if (File.Exists(testPath))
                    {
                        result = true;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
                finally
                {
                    try
                    {
                        if (File.Exists(testPath))
                        {
                            File.Delete(testPath);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            return result;
        }

        private static string ParseProcMounts(string procMounts, string externalPath)
        {
            string sdCardEntry = string.Empty;
            if (!string.IsNullOrWhiteSpace(procMounts))
            {
                List<string> procMountEntries = procMounts.Split('\n', '\r').ToList();
                foreach (string entry in procMountEntries)
                {
                    string[] sdCardEntries = entry.Split(' ');
                    if (sdCardEntries.Length > 2)
                    {
                        string path = sdCardEntries[1];
                        if (path.StartsWith(externalPath, StringComparison.OrdinalIgnoreCase) &&
                            string.Compare(path, externalPath, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            if (IsWritable(path))
                            {
                                sdCardEntry = path;
                                break;
                            }
                        }
                    }
                }
            }
            return sdCardEntry;
        }

        public static List<string> ParseStorageMedia(string procMounts)
        {
            List<string> sdCardList = new List<string>();
            if (!string.IsNullOrWhiteSpace(procMounts))
            {
                List<string> procMountEntries = procMounts.Split('\n', '\r').ToList();
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (string entry in procMountEntries)
                {
                    string[] sdCardEntries = entry.Split(' ');
                    if (sdCardEntries.Length > 2)
                    {
                        string path = sdCardEntries[1];
                        if (IsWritable(path))
                        {
                            sdCardList.Add(path);
                        }
                    }
                }
            }
            return sdCardList;
        }

        public static FileSystemBlockInfo GetFileSystemBlockInfo(string path)
        {
            var statFs = new StatFs(path);
            var fsbi = new FileSystemBlockInfo();
            if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr2)
            {
                fsbi.Path = path;
                fsbi.BlockSizeBytes = statFs.BlockSizeLong;
                fsbi.TotalSizeBytes = statFs.BlockCountLong * statFs.BlockSizeLong;
                fsbi.AvailableSizeBytes = statFs.AvailableBlocksLong * statFs.BlockSizeLong;
                fsbi.FreeSizeBytes = statFs.FreeBlocksLong * statFs.BlockSizeLong;
            }
            else // this was deprecated in API level 18 (Android 4.3), so if your device is below level 18, this is what will be used instead.
            {
                fsbi.Path = path;
                // you may want to disable warning about obsoletes, earlier versions of Android are using the deprecated versions
#pragma warning disable 618
                fsbi.BlockSizeBytes = statFs.BlockSize;
                fsbi.TotalSizeBytes = statFs.BlockCount * (long)statFs.BlockSize;
                fsbi.AvailableSizeBytes = statFs.AvailableBlocks * (long)statFs.BlockSize;
                fsbi.FreeSizeBytes = statFs.FreeBlocks * (long)statFs.BlockSize;
#pragma warning restore 618
            }
            return fsbi;
        }
    }
}
