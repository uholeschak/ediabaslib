using Android.Bluetooth;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using System;
using EdiabasLib;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace CarControlAndroid
{
    public class ActivityCommon
    {
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
        
        public enum InterfaceType
        {
            None,
            Bluetooth,
            Enet,
        }

        public delegate bool ProgressZipDelegate(int percent);
        public const string EmulatorEnetIp = "192.168.10.244";

        private readonly Android.App.Activity _activity;
        private readonly bool _emulator;
        private string _externalPath;
        private string _externalWritePath;
        private readonly BluetoothAdapter _btAdapter;
        private readonly WifiManager _maWifi;
        private readonly ConnectivityManager _maConnectivity;
        private InterfaceType _selectedInterface;
        private bool _activateRequest;

        public bool Emulator
        {
            get
            {
                return _emulator;
            }
        }

        public string ExternalPath
        {
            get
            {
                return _externalPath;
            }
        }

        public string ExternalWritePath
        {
            get
            {
                return _externalWritePath;
            }
        }

        public InterfaceType SelectedInterface
        {
            get
            {
                return _selectedInterface;
            }
            set
            {
                _selectedInterface = value;
            }
        }

        public BluetoothAdapter BtAdapter
        {
            get
            {
                return _btAdapter;
            }
        }

        public WifiManager MaWifi
        {
            get
            {
                return _maWifi;
            }
        }

        public ConnectivityManager MaConnectivity
        {
            get
            {
                return _maConnectivity;
            }
        }

        public ActivityCommon(Android.App.Activity activity)
        {
            _activity = activity;
            _emulator = IsEmulator();
            SetStoragePath();

            _btAdapter = BluetoothAdapter.DefaultAdapter;
            _maWifi = (WifiManager)activity.GetSystemService(Context.WifiService);
            _maConnectivity = (ConnectivityManager)activity.GetSystemService(Context.ConnectivityService);
            _selectedInterface = InterfaceType.None;
            _activateRequest = false;
        }

        public bool IsInterfaceEnabled()
        {
            switch (_selectedInterface)
            {
                case InterfaceType.Bluetooth:
                    if (_btAdapter == null)
                    {
                        return false;
                    }
                    return _btAdapter.IsEnabled;

                case InterfaceType.Enet:
                    if (_maWifi == null)
                    {
                        return false;
                    }
                    return _maWifi.IsWifiEnabled;
            }
            return false;
        }

        public bool IsInterfaceAvailable()
        {
            switch (_selectedInterface)
            {
                case InterfaceType.Bluetooth:
                    if (_btAdapter == null)
                    {
                        return false;
                    }
                    return _btAdapter.IsEnabled;

                case InterfaceType.Enet:
                    if (_maConnectivity == null)
                    {
                        return false;
                    }
                    NetworkInfo networkInfo = _maConnectivity.ActiveNetworkInfo;
                    if (networkInfo == null)
                    {
                        return false;
                    }
                    return networkInfo.IsConnected;
            }
            return false;
        }

        public void ShowAlert(string message)
        {
            new AlertDialog.Builder(_activity)
            .SetMessage(message)
            .SetNeutralButton(Resource.String.compile_ok_btn, (s, e) => { })
            .Show();
        }

        public void SelectInterface(EventHandler<DialogClickEventArgs> handler)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(_activity);
            builder.SetTitle(Resource.String.select_interface);
            ListView listView = new ListView(_activity);
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(_activity, Android.Resource.Layout.SimpleListItemSingleChoice,
                new[] {
                    _activity.GetString(Resource.String.select_interface_bt),
                    _activity.GetString(Resource.String.select_interface_enet)
                });
            listView.Adapter = adapter;
            listView.ChoiceMode = ChoiceMode.Single;
            switch (_selectedInterface)
            {
                case InterfaceType.Bluetooth:
                    listView.SetItemChecked(0, true);
                    break;

                case InterfaceType.Enet:
                    listView.SetItemChecked(1, true);
                    break;
            }
            builder.SetView(listView);
            builder.SetPositiveButton(Resource.String.button_ok, (sender, args) =>
                {
                    switch (listView.CheckedItemPosition)
                    {
                        case 0:
                            _selectedInterface = InterfaceType.Bluetooth;
                            handler(sender, args);
                            break;

                        case 1:
                            _selectedInterface = InterfaceType.Enet;
                            handler(sender, args);
                            break;
                    }
                });
            builder.SetNegativeButton(Resource.String.button_abort, (sender, args) =>
                {
                });
            builder.Show();
        }

        public void EnableInterface()
        {
            switch (_selectedInterface)
            {
                case InterfaceType.Bluetooth:
                    if (_btAdapter == null)
                    {
                        Toast.MakeText(_activity, Resource.String.bt_not_available, ToastLength.Long).Show();
                        break;
                    }
                    if (!_btAdapter.IsEnabled)
                    {
                        try
                        {
#pragma warning disable 0618
                            _btAdapter.Enable();
#pragma warning restore 0618
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    break;

                case InterfaceType.Enet:
                    if (_maWifi == null)
                    {
                        Toast.MakeText(_activity, Resource.String.wifi_not_available, ToastLength.Long).Show();
                        break;
                    }
                    if (!_maWifi.IsWifiEnabled)
                    {
                        try
                        {
                            _maWifi.SetWifiEnabled(true);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    break;
            }
        }

        public void RequestInterfaceEnable(EventHandler<DialogClickEventArgs> handler)
        {
            if (_activateRequest)
            {
                return;
            }
            if (IsInterfaceAvailable())
            {
                return;
            }
            if (IsInterfaceEnabled())
            {
                return;
            }
            int resourceId;
            switch (_selectedInterface)
            {
                case InterfaceType.Bluetooth:
                    resourceId = Resource.String.bt_enable;
                    break;

                case InterfaceType.Enet:
                    resourceId = Resource.String.wifi_enable;
                    break;

                default:
                    return;
            }
            _activateRequest = true;
            new AlertDialog.Builder(_activity)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    _activateRequest = false;
                    EnableInterface();
                    handler(sender, args);
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                    _activateRequest = false;
                })
                .SetCancelable(false)
                .SetMessage(resourceId)
                .SetTitle(Resource.String.interface_activate)
                .Show();
        }

        public bool RequestBluetoothDeviceSelect(int requestCode, EventHandler<DialogClickEventArgs> handler)
        {
            if (!IsInterfaceAvailable())
            {
                return true;
            }
            if (SelectedInterface != InterfaceType.Bluetooth)
            {
                return true;
            }
            new AlertDialog.Builder(_activity)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    if (SelectBluetoothDevice(requestCode))
                    {
                        handler(sender, args);
                    }
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                })
                .SetCancelable(false)
                .SetMessage(Resource.String.bt_device_select)
                .SetTitle(Resource.String.bt_device_select_title)
                .Show();
            return false;
        }

        public bool SelectBluetoothDevice(int requestCode)
        {
            if (!IsInterfaceAvailable())
            {
                return false;
            }
            if (SelectedInterface != InterfaceType.Bluetooth)
            {
                return false;
            }
            Intent serverIntent = new Intent(_activity, typeof(DeviceListActivity));
            _activity.StartActivityForResult(serverIntent, requestCode);
            return true;
        }

        public void SetEdiabasInterface(EdiabasNet ediabas, string btDeviceAddress)
        {
            // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
            if (ediabas.EdInterfaceClass is EdInterfaceObd)
            {
                ((EdInterfaceObd)ediabas.EdInterfaceClass).ComPort = "BLUETOOTH:" + btDeviceAddress;
            }
            else if (ediabas.EdInterfaceClass is EdInterfaceEnet)
            {
                string remoteHost = "auto";
                if (Emulator)
                {   // broadcast is not working with emulator
                    remoteHost = EmulatorEnetIp;
                }
                ((EdInterfaceEnet)ediabas.EdInterfaceClass).RemoteHost = remoteHost;
            }
        }

        public static string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(toPath))
            {
                return fromPath;
            }
            System.Uri fromUri = new System.Uri(fromPath);
            System.Uri toUri = new System.Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            System.Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = System.Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.ToUpperInvariant() == "FILE")
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static void ExtractZipFile(string archiveFilenameIn, string outFolder, ProgressZipDelegate progressHandler)
        {
            ZipFile zf = null;
            try
            {
                FileStream fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                long index = 0;
                foreach (ZipEntry zipEntry in zf)
                {
                    if (progressHandler != null)
                    {
                        if (progressHandler((int)(100 * index / zf.Count)))
                        {
                            return;
                        }
                    }
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = Path.Combine(outFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (!string.IsNullOrEmpty(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                    index++;
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }

        public static string CreateValidFileName(string s, char replaceChar = '_', char[] includeChars = null)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            if (includeChars != null) invalid = invalid.Union(includeChars).ToArray();
            return string.Join(string.Empty, s.ToCharArray().Select(o => invalid.Contains(o) ? replaceChar : o));
        }

        private static bool IsEmulator()
        {
            string fing = Build.Fingerprint;
            bool isEmulator = false;
            if (fing != null)
            {
                isEmulator = fing.Contains("vbox") || fing.Contains("generic");
            }
            return isEmulator;
        }

        private void SetStoragePath()
        {
            _externalPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            _externalWritePath = string.Empty;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {   // writing to external disk is only allowed in special directories.
                Java.IO.File[] externalFilesDirs = Android.App.Application.Context.GetExternalFilesDirs(null);
                if (externalFilesDirs.Length > 0)
                {
                    // index 0 is the internal disk
                    _externalWritePath = externalFilesDirs.Length > 1 ? externalFilesDirs[1].AbsolutePath : externalFilesDirs[0].AbsolutePath;
                }
            }
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
                fsbi.FreeSizeBytes = statFs.FreeBlocks * (long)statFs.BlockSize;
                fsbi.AvailableSizeBytes = statFs.AvailableBlocks * (long)statFs.BlockSize;
#pragma warning restore 618
            }
            return fsbi;
        }

        public static long GetDirectorySize(string path)
        {
            if (!Directory.Exists(path))
            {
                return 0;
            }
            // 1.
            // Get array of all file names.
            string[] a = Directory.GetFiles(path, "*.*");

            // 2.
            // Calculate total bytes of all files in a loop.
            return a.Select(name => new FileInfo(name)).Select(info => info.Length).Sum();
        }
    }
}
