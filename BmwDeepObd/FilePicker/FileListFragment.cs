using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.Fragment.App;

// ReSharper disable LoopCanBeConvertedToQuery

namespace BmwDeepObd.FilePicker
{
    /// <summary>
    ///   A ListFragment that will show the files and subdirectories of a given directory.
    /// </summary>
    /// <remarks>
    ///   <para> This was placed into a ListFragment to make this easier to share this functionality with with tablets. </para>
    ///   <para> Note that this is a incomplete example. It lacks things such as the ability to go back up the directory tree, or any special handling of a file when it is selected. </para>
    /// </remarks>
    public class FileListFragment : ListFragment
    {
        public class InstanceData
        {
            public InstanceData()
            {
                DefaultInitialDirectory = ActivityCommon.ExternalWritePath;
                if (string.IsNullOrEmpty(DefaultInitialDirectory))
                {
                    DefaultInitialDirectory = ActivityCommon.ExternalPath;
                }
                if (string.IsNullOrEmpty(DefaultInitialDirectory))
                {
                    DefaultInitialDirectory = Path.DirectorySeparatorChar.ToString();
                }
            }
            public string DefaultInitialDirectory { get; set; }
        }

        private InstanceData _instanceData = new InstanceData();
        private bool _activityRecreated;
        private InputMethodManager _imm;
        private View _contentView;
        private FileListAdapter _adapter;
        private List<string> _extensionList;
        private Regex _fileNameRegex;
        private IList<FileInfoEx> _visibleFiles;
        private string _fileNameFilter = string.Empty;
        private bool _allowDirChange;
        private bool _dirSelect;
        private bool _showCurrentDir;
        private bool _showFiles;
        private bool _showFileExtensions;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (savedInstanceState != null)
            {
                _activityRecreated = true;
                _instanceData = BaseActivity.GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
            }

            _imm = (InputMethodManager)Context.GetSystemService(Context.InputMethodService);
            _contentView = Activity.FindViewById<View>(Android.Resource.Id.Content);

            string initDir = Activity.Intent?.GetStringExtra(FilePickerActivity.ExtraInitDir) ?? string.Empty;
            if (Directory.Exists(initDir))
            {
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(initDir);
                    dir.GetFileSystemInfos();
                    if (!_activityRecreated && _instanceData != null)
                    {
                        _instanceData.DefaultInitialDirectory = initDir;
                    }
                }
                catch
                {
                    // ignored
                }
            }

            _extensionList = new List<string>();
            string fileExtensions = Activity.Intent?.GetStringExtra(FilePickerActivity.ExtraFileExtensions);
            if (!string.IsNullOrEmpty(fileExtensions))
            {
                string[] extensions = fileExtensions.Split(';');
                foreach (string extension in extensions)
                {
                    _extensionList.Add(extension);
                }
            }

            string fileFilter = Activity.Intent?.GetStringExtra(FilePickerActivity.ExtraFileRegex);
            _fileNameRegex = null;
            if (!string.IsNullOrEmpty(fileFilter))
            {
                _fileNameRegex = new Regex(fileFilter, RegexOptions.IgnoreCase);
            }

            _allowDirChange = Activity.Intent?.GetBooleanExtra(FilePickerActivity.ExtraDirChange, true) ?? true;
            _dirSelect = Activity.Intent?.GetBooleanExtra(FilePickerActivity.ExtraDirSelect, false) ?? false;
            _showCurrentDir = Activity.Intent?.GetBooleanExtra(FilePickerActivity.ExtraShowCurrentDir, false) ?? false;
            _showFiles = Activity.Intent?.GetBooleanExtra(FilePickerActivity.ExtraShowFiles, true) ?? true;
            _showFileExtensions = Activity.Intent?.GetBooleanExtra(FilePickerActivity.ExtraShowExtension, true) ?? true;

            _adapter = new FileListAdapter(Activity, Array.Empty<FileInfoEx>());
            ListAdapter = _adapter;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ListView.LongClickable = _dirSelect;
            ListView.ItemLongClick += (sender, args) =>
            {
                if (_dirSelect)
                {
                    string fileName = null;
                    FileInfoEx fileSystemInfo = _adapter.GetItem(args.Position);
                    if (fileSystemInfo != null)
                    {
                        switch (fileSystemInfo.FileType)
                        {
                            case FileInfoType.File:
                                if (fileSystemInfo.FileSysInfo != null && fileSystemInfo.FileSysInfo.IsDirectory())
                                {
                                    fileName = fileSystemInfo.FileSysInfo.FullName;
                                }
                                break;

                            case FileInfoType.CurrentDir:
                                fileName = fileSystemInfo.RootDir;
                                break;
                        }

                        if (!string.IsNullOrEmpty(fileName))
                        {
                            Intent intent = new Intent();
                            intent.PutExtra(FilePickerActivity.ExtraFileName, fileName);

                            Activity.SetResult(Android.App.Result.Ok, intent);
                            Activity.Finish();
                            args.Handled = true;
                        }
                    }
                }
            };
        }

        public override void OnListItemClick(ListView l, View v, int position, long id)
        {
            FileInfoEx fileSystemInfo = _adapter.GetItem(position);
            if (fileSystemInfo == null)
            {
                return;
            }

            if (fileSystemInfo.RootDir != null)
            {
                switch (fileSystemInfo.FileType)
                {
                    case FileInfoType.CurrentDir:
                    {
                        Intent intent = new Intent();
                        intent.PutExtra(FilePickerActivity.ExtraFileName, fileSystemInfo.RootDir);

                        Activity.SetResult(Android.App.Result.Ok, intent);
                        Activity.Finish();
                        return;
                    }
                }

                _instanceData.DefaultInitialDirectory = fileSystemInfo.RootDir;
                RefreshFilesList(fileSystemInfo.RootDir);
            }
            else
            {
                if (fileSystemInfo.FileSysInfo != null)
                {
                    if (fileSystemInfo.FileSysInfo.IsFile())
                    {
                        // Do something with the file.  In this case we just pop some toast.
                        //Log.Verbose("FileListFragment", "The file {0} was clicked.", fileSystemInfo.FullName);
                        Intent intent = new Intent();
                        intent.PutExtra(FilePickerActivity.ExtraFileName, fileSystemInfo.FileSysInfo.FullName);

                        Activity.SetResult(Android.App.Result.Ok, intent);
                        Activity.Finish();
                    }
                    else
                    {
                        if (_dirSelect)
                        {
                            Toast.MakeText(Activity, Resource.String.file_picker_dir_select, ToastLength.Short)?.Show();
                        }

                        // Dig into this directory, and display it's contents
                        _instanceData.DefaultInitialDirectory = fileSystemInfo.FileSysInfo.FullName;
                        RefreshFilesList(fileSystemInfo.FileSysInfo.FullName);
                    }
                }
            }

            base.OnListItemClick(l, v, position, id);
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            BaseActivity.StoreInstanceState(outState, _instanceData);
            base.OnSaveInstanceState(outState);
        }

        public override void OnStart()
        {
            base.OnStart();

            _fileNameFilter = string.Empty;
            if (Activity is FilePickerActivity filePicker)
            {
                filePicker.FilterEvent += NewFileFilter;
            }
        }

        public override void OnStop()
        {
            base.OnStop();

            if (Activity is FilePickerActivity filePicker)
            {
                filePicker.FilterEvent -= NewFileFilter;
            }
        }

        public override void OnResume()
        {
            base.OnResume();
            RefreshFilesList(_instanceData.DefaultInitialDirectory);
        }

        public void RefreshFilesList(string directory)
        {
            _visibleFiles = null;
            IList<FileInfoEx> visibleThings = new List<FileInfoEx>();
            DirectoryInfo dir = new DirectoryInfo(directory);

            try
            {
                if (_allowDirChange)
                {
                    if (Build.VERSION.SdkInt < BuildVersionCodes.Q)
                    {
#pragma warning disable 618
                        Java.IO.File extDir = Android.OS.Environment.ExternalStorageDirectory;
#pragma warning restore 618
                        string extState = Android.OS.Environment.ExternalStorageState;
                        if (extDir != null && extState != null && extDir.IsDirectory && extState.Equals(Android.OS.Environment.MediaMounted))
                        {
                            string name = ActivityCommon.GetTruncatedPathName(extDir.AbsolutePath);
                            if (!string.IsNullOrEmpty(name))
                            {
                                visibleThings.Add(new FileInfoEx(null, FileInfoType.Link, "->" + name, extDir.AbsolutePath));
                            }
                        }
                    }

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
                    {   // writing to external disk is only allowed in special directories.
                        Java.IO.File[] externalFilesDirs = Android.App.Application.Context.GetExternalFilesDirs(null);
                        if (externalFilesDirs != null)
                        {
                            foreach (Java.IO.File file in externalFilesDirs)
                            {
                                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                                if (file != null && file.IsDirectory && !string.IsNullOrEmpty(file.AbsolutePath))
                                {
                                    string name = ActivityCommon.GetTruncatedPathName(file.AbsolutePath);
                                    string extState = Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop ?
                                        Android.OS.Environment.GetExternalStorageState(file) : Android.OS.Environment.ExternalStorageState;
                                    if (!string.IsNullOrEmpty(name) && extState != null && extState.Equals(Android.OS.Environment.MediaMounted))
                                    {
                                        visibleThings.Add(new FileInfoEx(null, FileInfoType.Link,"->" + name, file.AbsolutePath));
                                    }
                                }
                            }
                        }
                    }

                    if (_showCurrentDir)
                    {
                        visibleThings.Add(new FileInfoEx(null, FileInfoType.CurrentDir, ".", directory));
                    }

                    string rootDir = Path.GetDirectoryName(directory);
                    if (!string.IsNullOrEmpty(rootDir))
                    {
                        visibleThings.Add(new FileInfoEx(null, FileInfoType.ParentDir,"..", rootDir));
                    }
                }
                foreach (var item in dir.GetFileSystemInfos().Where(item => item.IsVisible()))
                {
                    bool add = _allowDirChange;
                    string displayName = item.Name;
                    if (item.IsFile())
                    {
                        if (!_showFiles)
                        {
                            add = false;
                        }
                        else
                        {
                            if (!_showFileExtensions)
                            {
                                displayName = Path.GetFileNameWithoutExtension(item.Name);
                            }
                            if (_extensionList.Count > 0)
                            {
                                add = false;
                                foreach (string extension in _extensionList)
                                {
                                    if (item.HasFileExtension(extension))
                                    {
                                        add = true;
                                        break;
                                    }
                                }
                            }
                            if (_fileNameRegex != null)
                            {
                                if (!_fileNameRegex.IsMatch(item.Name))
                                {
                                    add = false;
                                }
                            }
                        }
                    }
                    if (add)
                    {
                        visibleThings.Add(new FileInfoEx(item, FileInfoType.File, displayName, null));
                    }
                }
            }
            catch (Exception)
            {
                Toast.MakeText(Activity, GetString(Resource.String.access_dir_failed) + " " + directory, ToastLength.Long)?.Show();
                return;
            }

            _visibleFiles = visibleThings;
            RefreshFilterFileList();
        }

        protected void NewFileFilter(string fileNameFilter, bool submit)
        {
            _fileNameFilter = fileNameFilter;
            RefreshFilterFileList();
            if (submit)
            {
                HideKeyboard();
            }
        }

        protected void RefreshFilterFileList()
        {
            if (_visibleFiles == null)
            {
                return;
            }
            _adapter.AddDirectoryContents(_visibleFiles, _fileNameFilter);

            // If we don't do this, then the ListView will not update itself when then data set 
            // in the adapter changes. It will appear to the user that nothing has happened.
            ListView.RefreshDrawableState();
        }

        private void HideKeyboard()
        {
            _imm?.HideSoftInputFromWindow(_contentView.WindowToken, HideSoftInputFlags.None);
        }
    }
}
