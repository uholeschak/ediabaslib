using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Android.Bluetooth;
using Android.Content;
using Android.Hardware.Usb;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using EdiabasLib;
using Hoho.Android.UsbSerial.Driver;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace BmwDiagnostics
{
    public class ActivityCommon : IDisposable
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
            Ftdi,
        }

        public delegate bool ProgressZipDelegate(int percent);
        public delegate void BcReceiverUpdateDisplayDelegate();
        public delegate void BcReceiverReceivedDelegate(Context context, Intent intent);
        public const string EmulatorEnetIp = "192.168.10.244";
        public const string ActionUsbPermission = "de.holeschak.bmw_deep_obd.USB_PERMISSION";

        private bool _disposed;
        private readonly Android.App.Activity _activity;
        private readonly BcReceiverUpdateDisplayDelegate _bcReceiverUpdateDisplayHandler;
        private readonly BcReceiverReceivedDelegate _bcReceiverReceivedHandler;
        private readonly bool _emulator;
        private string _externalPath;
        private string _externalWritePath;
        private readonly BluetoothAdapter _btAdapter;
        private readonly WifiManager _maWifi;
        private readonly ConnectivityManager _maConnectivity;
        private readonly UsbManager _usbManager;
        private Receiver _bcReceiver;
        private InterfaceType _selectedInterface;
        private bool _activateRequest;

        public bool Emulator
        {
            get
            {
                return _emulator;
            }
        }

        public bool UsbSupport
        {
            get
            {
                return !Emulator && (Build.VERSION.SdkInt >= BuildVersionCodes.HoneycombMr1);
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

        public UsbManager UsbManager
        {
            get { return _usbManager; }
        }

        public Receiver BcReceiver
        {
            get { return _bcReceiver; }
        }

        public ActivityCommon(Android.App.Activity activity, BcReceiverUpdateDisplayDelegate bcReceiverUpdateDisplayHandler = null, BcReceiverReceivedDelegate bcReceiverReceivedHandler = null)
        {
            _activity = activity;
            _bcReceiverUpdateDisplayHandler = bcReceiverUpdateDisplayHandler;
            _bcReceiverReceivedHandler = bcReceiverReceivedHandler;
            _emulator = IsEmulator();
            SetStoragePath();

            _btAdapter = BluetoothAdapter.DefaultAdapter;
            _maWifi = (WifiManager)activity.GetSystemService(Context.WifiService);
            _maConnectivity = (ConnectivityManager)activity.GetSystemService(Context.ConnectivityService);
            _usbManager = activity.GetSystemService(Context.UsbService) as UsbManager;
            _selectedInterface = InterfaceType.None;
            _activateRequest = false;

            if ((_bcReceiverUpdateDisplayHandler != null) || (_bcReceiverReceivedHandler != null))
            {
                _bcReceiver = new Receiver(this);
                activity.RegisterReceiver(_bcReceiver, new IntentFilter(BluetoothAdapter.ActionStateChanged));
                activity.RegisterReceiver(_bcReceiver, new IntentFilter(ConnectivityManager.ConnectivityAction));
                if (UsbSupport)
                {   // usb handling
                    activity.RegisterReceiver(_bcReceiver, new IntentFilter(ActionUsbPermission));
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    if (_activity != null && _bcReceiver != null)
                    {
                        _activity.UnregisterReceiver(_bcReceiver);
                        _bcReceiver = null;
                    }
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

        public string InterfaceName()
        {
            switch (_selectedInterface)
            {
                case InterfaceType.Bluetooth:
                    return _activity.GetString(Resource.String.select_interface_bt);

                case InterfaceType.Enet:
                    return _activity.GetString(Resource.String.select_interface_enet);

                case InterfaceType.Ftdi:
                    return _activity.GetString(Resource.String.select_interface_ftdi);
            }
            return string.Empty;
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

                case InterfaceType.Ftdi:
                    return true;
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

                case InterfaceType.Ftdi:
                {
                    List<IUsbSerialDriver> availableDrivers = EdFtdiInterface.GetDriverList(_usbManager);
                    if (availableDrivers.Count <= 0)
                    {
                        return false;
                    }
                    if (!_usbManager.HasPermission(availableDrivers[0].Device))
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        public bool AllowCanAdapterConfig(string deviceAddress)
        {
            switch (_selectedInterface)
            {
                case InterfaceType.Bluetooth:
                {
                    if (string.IsNullOrEmpty(deviceAddress))
                    {
                        return false;
                    }
                    string[] stringList = deviceAddress.Split(';');
                    if (stringList.Length > 1)
                    {
                        if (string.Compare(stringList[1], EdBluetoothInterface.Elm327Tag, StringComparison.OrdinalIgnoreCase) == 0)
                        {   // ELM device
                            return false;
                        }
                    }
                    return true;
                }

                case InterfaceType.Ftdi:
                    return true;
            }
            return false;
        }

        public void ShowAlert(string message)
        {
            new AlertDialog.Builder(_activity)
            .SetMessage(message)
            .SetNeutralButton(Resource.String.button_ok, (s, e) => { })
            .Show();
        }

        public void SelectInterface(EventHandler<DialogClickEventArgs> handler)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(_activity);
            builder.SetTitle(Resource.String.select_interface);
            ListView listView = new ListView(_activity);

            List<string> interfaceNames = new List<string>
            {
                _activity.GetString(Resource.String.select_interface_bt),
                _activity.GetString(Resource.String.select_interface_enet)
            };
            if (UsbSupport)
            {
                interfaceNames.Add(_activity.GetString(Resource.String.select_interface_ftdi));
            }
            ArrayAdapter<string> adapter = new ArrayAdapter<string>(_activity,
                Android.Resource.Layout.SimpleListItemSingleChoice, interfaceNames.ToArray());
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

                case InterfaceType.Ftdi:
                    listView.SetItemChecked(2, true);
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

                        case 2:
                            _selectedInterface = InterfaceType.Ftdi;
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

        public bool RequestInterfaceEnable(EventHandler handler)
        {
            if (_activateRequest)
            {
                return false;
            }
            if (IsInterfaceAvailable())
            {
                return false;
            }
            if (IsInterfaceEnabled())
            {
                return false;
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
                    return false;
            }
            _activateRequest = true;
            AlertDialog builder = new AlertDialog.Builder(_activity)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    EnableInterface();
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                })
                .SetCancelable(true)
                .SetMessage(resourceId)
                .SetTitle(Resource.String.interface_activate)
                .Show();
            builder.DismissEvent += (sender, args) =>
            {
                _activateRequest = false;
                handler(sender, args);
            };
            return true;
        }

        public bool RequestBluetoothDeviceSelect(int requestCode, EventHandler<DialogClickEventArgs> handler)
        {
            if (SelectedInterface != InterfaceType.Bluetooth)
            {
                return true;
            }
            if (!IsInterfaceAvailable())
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
                .SetCancelable(true)
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

        public void RequestUsbPermission(UsbDevice usbDevice)
        {
            if (!UsbSupport)
            {
                return;
            }
            if (usbDevice == null)
            {
                List<IUsbSerialDriver> availableDrivers = EdFtdiInterface.GetDriverList(_usbManager);
                if (availableDrivers.Count > 0)
                {
                    UsbDevice device = availableDrivers[0].Device;
                    if (!_usbManager.HasPermission(device))
                    {
                        usbDevice = device;
                    }
                }
            }
            if (usbDevice != null)
            {
                Android.App.PendingIntent intent = Android.App.PendingIntent.GetBroadcast(_activity, 0, new Intent(ActionUsbPermission), 0);
                _usbManager.RequestPermission(usbDevice, intent);
            }
        }

        public void SetEdiabasInterface(EdiabasNet ediabas, string btDeviceAddress)
        {
            object connectParameter = null;
            // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
            if (ediabas.EdInterfaceClass is EdInterfaceObd)
            {
                if (SelectedInterface == InterfaceType.Ftdi)
                {
                    ((EdInterfaceObd)ediabas.EdInterfaceClass).ComPort = "FTDI0";
                    connectParameter = new EdFtdiInterface.ConnectParameter(_activity, _usbManager);
                }
                else
                {
                    ((EdInterfaceObd)ediabas.EdInterfaceClass).ComPort = "BLUETOOTH:" + btDeviceAddress;
                }
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
            ediabas.EdInterfaceClass.ConnectParameter = connectParameter;
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

        public static void WriteResourceToFile(string resourceName, string fileName)
        {
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (resource != null)
                {
                    using (var file = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        resource.CopyTo(file);
                    }
                }
            }
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
                    if (externalFilesDirs.Length > 1 && externalFilesDirs[1] != null)
                    {
                        _externalWritePath = externalFilesDirs[1].AbsolutePath;
                    }
                    else if (externalFilesDirs[0] != null)
                    {
                        _externalWritePath = externalFilesDirs[0].AbsolutePath;
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

        private static string ReadProcMounts()
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
                        string storageType = sdCardEntries[2];
                        if (storageType.IndexOf("fat", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            string path = sdCardEntries[1];
                            if (path.StartsWith(externalPath, StringComparison.OrdinalIgnoreCase) &&
                                string.Compare(path, externalPath, StringComparison.OrdinalIgnoreCase) != 0)
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

        public class Receiver : BroadcastReceiver
        {
            readonly ActivityCommon _activityCommon;

            public Receiver(ActivityCommon activityCommon)
            {
                _activityCommon = activityCommon;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                string action = intent.Action;

                if (_activityCommon._bcReceiverReceivedHandler != null)
                {
                    _activityCommon._bcReceiverReceivedHandler(context, intent);
                }
                switch (action)
                {
                    case BluetoothAdapter.ActionStateChanged:
                    case ConnectivityManager.ConnectivityAction:
                        if (_activityCommon._bcReceiverUpdateDisplayHandler != null)
                        {
                            _activityCommon._bcReceiverUpdateDisplayHandler();
                        }
                        break;

                    case ActionUsbPermission:
                        if (intent.GetBooleanExtra(UsbManager.ExtraPermissionGranted, false))
                        {
                            if (_activityCommon._bcReceiverUpdateDisplayHandler != null)
                            {
                                _activityCommon._bcReceiverUpdateDisplayHandler();
                            }
                        }
                        break;
                }
            }
        }
    }
}
