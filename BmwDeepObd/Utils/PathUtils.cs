using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BmwDeepObd
{
    public static class PathUtils
    {
#if DEBUG
        private static readonly string Tag = typeof(PathUtils).FullName;
#endif

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
            System.Uri fromUri = new System.Uri(AppendDirectorySeparatorChar(fromPath));
            System.Uri toUri = new System.Uri(AppendDirectorySeparatorChar(toPath));

            if (fromUri.Scheme != toUri.Scheme)
            {   // path can't be made relative.
                return toPath; 
            }

            System.Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = System.Uri.UnescapeDataString(relativeUri.ToString());

            if (string.Equals(toUri.Scheme, System.Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static string AppendDirectorySeparatorChar(string path)
        {
            // Append a slash only if the path is a directory and does not have a slash.
            if (!Path.HasExtension(path) &&
                !path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return path + Path.DirectorySeparatorChar;
            }
            return path;
        }

        public static string CreateValidFileName(string s, char replaceChar = '_', char[] includeChars = null)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            if (includeChars != null) invalid = invalid.Union(includeChars).ToArray();
            return string.Join(string.Empty, s.ToCharArray().Select(o => invalid.Contains(o) ? replaceChar : o));
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

        public static string GetTruncatedPathName(string path, int maxLength = 30)
        {
            try
            {
                if (path == null)
                {
                    return string.Empty;
                }

                StringBuilder sb = new StringBuilder();
                List<string> dirList = path.Split(Path.DirectorySeparatorChar).ToList();
                dirList.RemoveAll(string.IsNullOrWhiteSpace);

                int maxRootParts = 1;
                if (dirList.Count >= 1)
                {
                    int sumLength = dirList[dirList.Count - 1].Length + 1;
                    for (int i = 0; i < dirList.Count - 1; i++)
                    {
                        sumLength += dirList[i].Length + 1;
                        if (sumLength > maxLength)
                        {
                            break;
                        }
                        maxRootParts = i + 1;
                    }
                }

                for (int i = 0; i < dirList.Count; i++)
                {
                    sb.Append(Path.DirectorySeparatorChar.ToString());
                    sb.Append(dirList[i]);
                    if (i + 1 >= maxRootParts)
                    {
                        if (dirList.Count > maxRootParts + 1)
                        {
                            sb.Append(Path.DirectorySeparatorChar.ToString());
                            sb.Append("...");
                        }

                        if (dirList.Count > maxRootParts)
                        {
                            sb.Append(Path.DirectorySeparatorChar.ToString());
                            sb.Append(dirList[dirList.Count - 1]);
                        }

                        break;
                    }
                }

                string name = sb.ToString();
                if (string.IsNullOrEmpty(name))
                {
                    name = Path.DirectorySeparatorChar.ToString();
                }

                return name;
            }
            catch (Exception )
            {
                return string.Empty;
            }
        }
    }
}
