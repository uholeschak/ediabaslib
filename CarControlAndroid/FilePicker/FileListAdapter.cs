using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace CarControlAndroid.FilePicker
{
    public class FileInfoEx
    {
        public FileInfoEx(FileSystemInfo fileSystemInfo, string rootDir)
        {
            _fileSystemInfo = fileSystemInfo;
            _rootDir = rootDir;
        }

        private readonly FileSystemInfo _fileSystemInfo;
        private readonly string _rootDir;

        public FileSystemInfo FileSysInfo
        {
            get
            {
                return _fileSystemInfo;
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
        public void AddDirectoryContents(IEnumerable<FileInfoEx> directoryContents)
        {
            Clear();
            // Notify the _adapter that things have changed or that there is nothing 
            // to display.
            IEnumerable<FileInfoEx> fileInfoExs = directoryContents as FileInfoEx[] ?? directoryContents.ToArray();
            if (fileInfoExs.Any())
            {
#if false
                // .AddAll was only introduced in API level 11 (Android 3.0). 
                // If the "Minimum Android to Target" is set to Android 3.0 or 
                // higher, then this code will be used.
                AddAll(directoryContents.ToArray());
#else
                // This is the code to use if the "Minimum Android to Target" is
                // set to a pre-Android 3.0 API (i.e. Android 2.3.3 or lower).
                lock (this)
                    foreach (var fsi in fileInfoExs)
                    {
                        Add(fsi);
                    }
#endif

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
                viewHolder.Update("..", Resource.Drawable.folder);
            }
            else
            {
                viewHolder.Update(fileSystemEntry.FileSysInfo.Name, fileSystemEntry.FileSysInfo.IsDirectory() ? Resource.Drawable.folder : Resource.Drawable.file);
            }

            return row;
        }
    }
}
