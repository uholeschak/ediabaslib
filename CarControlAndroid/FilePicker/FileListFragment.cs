using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace CarControlAndroid.FilePicker
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
        public string DefaultInitialDirectory = "/";
        private FileListAdapter _adapter;
        private List<string> _extensionList;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            string initDir = Activity.Intent.GetStringExtra(FilePickerActivity.ExtraInitDir) ?? "/";
            if (Directory.Exists(initDir))
            {
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(initDir);
                    dir.GetFileSystemInfos();
                    DefaultInitialDirectory = initDir;
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

            _adapter = new FileListAdapter(Activity, new FileInfoEx[0]);
            ListAdapter = _adapter;
        }

        public override void OnListItemClick(ListView l, View v, int position, long id)
        {
            FileInfoEx fileSystemInfo = _adapter.GetItem(position);

            if (fileSystemInfo.RootDir != null)
            {
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
                    RefreshFilesList(fileSystemInfo.FileSysInfo.FullName);
                }
            }

            base.OnListItemClick(l, v, position, id);
        }

        public override void OnResume()
        {
            base.OnResume();
            RefreshFilesList(DefaultInitialDirectory);
        }

        public void RefreshFilesList(string directory)
        {
            IList<FileInfoEx> visibleThings = new List<FileInfoEx>();
            DirectoryInfo dir = new DirectoryInfo(directory);

            try
            {
                string rootDir = Path.GetDirectoryName(directory);
                if (!string.IsNullOrEmpty(rootDir))
                {
                    visibleThings.Add(new FileInfoEx(null, rootDir));
                }
                foreach (var item in dir.GetFileSystemInfos().Where(item => item.IsVisible()))
                {
                    bool add = true;
                    if (item.IsFile())
                    {
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
                    }
                    if (add)
                    {
                        visibleThings.Add(new FileInfoEx(item, null));
                    }
                }
            }
            catch (Exception)
            {
                Toast.MakeText(Activity, GetString(Resource.String.access_dir_failed) + " " + directory, ToastLength.Long).Show();
                return;
            }

            _adapter.AddDirectoryContents(visibleThings);

            // If we don't do this, then the ListView will not update itself when then data set 
            // in the adapter changes. It will appear to the user that nothing has happened.
            ListView.RefreshDrawableState();

            //Log.Verbose("FileListFragment", "Displaying the contents of directory {0}.", directory);
        }
    }
}
