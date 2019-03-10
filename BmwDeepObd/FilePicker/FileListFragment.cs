using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

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
        private FileListAdapter _adapter;
        private List<string> _extensionList;
        private Regex _fileNameRegex;
        private IList<FileInfoEx> _visibleFiles;
        private string _fileNameFilter;
        private bool _allowDirChange;
        private bool _showFileExtensions;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (savedInstanceState != null)
            {
                _activityRecreated = true;
                _instanceData = ActivityCommon.GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
            }

            string initDir = Activity.Intent.GetStringExtra(FilePickerActivity.ExtraInitDir) ?? string.Empty;
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
            string fileExtensions = Activity.Intent.GetStringExtra(FilePickerActivity.ExtraFileExtensions);
            if (!string.IsNullOrEmpty(fileExtensions))
            {
                string[] extensions = fileExtensions.Split(';');
                foreach (string extension in extensions)
                {
                    _extensionList.Add(extension);
                }
            }

            string fileFilter = Activity.Intent.GetStringExtra(FilePickerActivity.ExtraFileRegex);
            _fileNameRegex = null;
            if (!string.IsNullOrEmpty(fileFilter))
            {
                _fileNameRegex = new Regex(fileFilter, RegexOptions.IgnoreCase);
            }

            _allowDirChange = Activity.Intent.GetBooleanExtra(FilePickerActivity.ExtraDirChange, true);
            _showFileExtensions = Activity.Intent.GetBooleanExtra(FilePickerActivity.ExtraShowExtension, true);

            _adapter = new FileListAdapter(Activity, new FileInfoEx[0]);
            ListAdapter = _adapter;
        }

        public override void OnListItemClick(ListView l, View v, int position, long id)
        {
            FileInfoEx fileSystemInfo = _adapter.GetItem(position);

            if (fileSystemInfo.RootDir != null)
            {
                _instanceData.DefaultInitialDirectory = fileSystemInfo.RootDir;
                RefreshFilesList(fileSystemInfo.RootDir);
            }
            else
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
                    // Dig into this directory, and display it's contents
                    _instanceData.DefaultInitialDirectory = fileSystemInfo.FileSysInfo.FullName;
                    RefreshFilesList(fileSystemInfo.FileSysInfo.FullName);
                }
            }

            base.OnListItemClick(l, v, position, id);
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            ActivityCommon.StoreInstanceState(outState, _instanceData);
            base.OnSaveInstanceState(outState);
        }

        public override void OnStart()
        {
            base.OnStart();

            _fileNameFilter = null;
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
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
                    {   // writing to external disk is only allowed in special directories.
                        Java.IO.File[] externalFilesDirs = Android.App.Application.Context.GetExternalFilesDirs(null);
                        foreach (Java.IO.File file in externalFilesDirs)
                        {
                            if (file != null && file.IsDirectory && !string.IsNullOrEmpty(file.AbsolutePath))
                            {
                                string name = ActivityCommon.GetTruncatedPathName(file.AbsolutePath);
                                if (!string.IsNullOrEmpty(name))
                                {
                                    visibleThings.Add(new FileInfoEx(null, "->" + name, file.AbsolutePath));
                                }
                            }
                        }
                    }

                    string rootDir = Path.GetDirectoryName(directory);
                    if (!string.IsNullOrEmpty(rootDir))
                    {
                        visibleThings.Add(new FileInfoEx(null, "..", rootDir));
                    }
                }
                foreach (var item in dir.GetFileSystemInfos().Where(item => item.IsVisible()))
                {
                    bool add = _allowDirChange;
                    string displayName = item.Name;
                    if (item.IsFile())
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
                    if (add)
                    {
                        visibleThings.Add(new FileInfoEx(item, displayName, null));
                    }
                }
            }
            catch (Exception)
            {
                Toast.MakeText(Activity, GetString(Resource.String.access_dir_failed) + " " + directory, ToastLength.Long).Show();
                return;
            }

            _visibleFiles = visibleThings;
            RefreshFilterFileList();
        }

        protected void NewFileFilter(string fileNamefilter)
        {
            _fileNameFilter = fileNamefilter;
            RefreshFilterFileList();
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
    }
}
