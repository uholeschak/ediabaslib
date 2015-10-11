using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace BmwDeepObd.FilePicker
{
    public class FileInfoEx
    {
        public FileInfoEx(FileSystemInfo fileSystemInfo, string displayName, string rootDir)
        {
            _fileSystemInfo = fileSystemInfo;
            _displayName = displayName;
            _rootDir = rootDir;
        }

        private readonly FileSystemInfo _fileSystemInfo;
        private readonly string _displayName;
        private readonly string _rootDir;

        public FileSystemInfo FileSysInfo
        {
            get
            {
                return _fileSystemInfo;
            }
        }

        public string DisplayName
        {
            get
            {
                return _displayName;
            }
        }

        public string RootDir
        {
            get
            {
                return _rootDir;
            }
        }
    }

    public class FileListAdapter : ArrayAdapter<FileInfoEx>
    {
        private readonly Context _context;

        public FileListAdapter(Context context, IList<FileInfoEx> fsi)
            : base(context, Resource.Layout.file_picker_list_item, Android.Resource.Id.Text1, fsi)
        {
            _context = context;
        }

        /// <summary>
        ///   We provide this method to get around some of the
        /// </summary>
        /// <param name="directoryContents"> </param>
        /// <param name="filter">Filter expression</param>
        public void AddDirectoryContents(IEnumerable<FileInfoEx> directoryContents, string filter)
        {
            Clear();
            // Notify the _adapter that things have changed or that there is nothing 
            // to display.
            IEnumerable<FileInfoEx> fileInfoExs = directoryContents as FileInfoEx[] ?? directoryContents.ToArray();
            if (fileInfoExs.Any())
            {
                lock (this)
                {
                    foreach (var fsi in fileInfoExs)
                    {
                        bool addFile = true;
                        if (!string.IsNullOrEmpty(filter) && fsi.RootDir == null && !fsi.FileSysInfo.IsDirectory())
                        {
                            string baseName = Path.GetFileNameWithoutExtension(fsi.FileSysInfo.Name);
                            if (baseName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                            {
                                addFile = false;
                            }
                        }
                        if (addFile)
                        {
                            Add(fsi);
                        }
                    }
                }

                NotifyDataSetChanged();
            }
            else
            {
                NotifyDataSetInvalidated();
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            FileInfoEx fileSystemEntry = GetItem(position);

            FileListRowViewHolder viewHolder;
            View row;
            if (convertView == null)
            {
                row = _context.GetLayoutInflater().Inflate(Resource.Layout.file_picker_list_item, parent, false);
                viewHolder = new FileListRowViewHolder(row.FindViewById<TextView>(Resource.Id.file_picker_text), row.FindViewById<ImageView>(Resource.Id.file_picker_image));
                row.Tag = viewHolder;
            }
            else
            {
                row = convertView;
                viewHolder = (FileListRowViewHolder)row.Tag;
            }
            if (fileSystemEntry.RootDir != null)
            {
                viewHolder.Update(fileSystemEntry.DisplayName, Resource.Drawable.folder);
            }
            else
            {
                viewHolder.Update(fileSystemEntry.DisplayName, fileSystemEntry.FileSysInfo.IsDirectory() ? Resource.Drawable.folder : Resource.Drawable.file);
            }

            return row;
        }
    }
}
