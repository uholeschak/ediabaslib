using System.IO;
using Android.Content;
using Android.Views;

namespace BmwDeepObd.FilePicker
{
    public static class Helpers
    {
        /// <summary>
        ///   Will obtain an instance of a LayoutInflater for the specified Context.
        /// </summary>
        /// <param name="context"> </param>
        /// <returns> </returns>
        public static LayoutInflater GetLayoutInflater(this Context context)
        {
            return context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
        }

        /// <summary>
        ///   This method will tell us if the given FileSystemInfo instance is a directory.
        /// </summary>
        /// <param name="fsi"> </param>
        /// <returns> </returns>
        public static bool IsDirectory(this FileSystemInfo fsi)
        {
            if (fsi == null || !fsi.Exists)
            {
                return false;
            }

            return (fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
        }

        /// <summary>
        ///   This method will tell us if the the given FileSystemInfo instance is a file.
        /// </summary>
        /// <param name="fsi"> </param>
        /// <returns> </returns>
        public static bool IsFile(this FileSystemInfo fsi)
        {
            if (fsi == null || !fsi.Exists)
            {
                return false;
            }
            return !IsDirectory(fsi);
        }

        public static bool IsVisible(this FileSystemInfo fsi)
        {
            if (fsi == null || !fsi.Exists)
            {
                return false;
            }

            var isHidden = (fsi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
            return !isHidden;
        }

        public static bool HasFileExtension(this FileSystemInfo fsi, string extension)
        {
            if (!IsFile(fsi))
            {
                return false;
            }
            string ext = Path.GetExtension(fsi.FullName);
            if (string.Compare(ext, extension, System.StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }
            return true;
        }
    }
}
