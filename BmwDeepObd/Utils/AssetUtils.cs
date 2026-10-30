using Android.Content.Res;
using EdiabasLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace BmwDeepObd
{
    public static class AssetUtils
    {
#if DEBUG
        private static readonly string Tag = typeof(AssetUtils).FullName;
#endif

        public static long GetAssetEcuFileSize(AssetManager assetManager, string assetEcuFileNames)
        {
            if (string.IsNullOrEmpty(assetEcuFileNames))
            {
                return -1;
            }

            long fileSize;
            string[] assetFiles = assetEcuFileNames.Split(";");

            try
            {
                long size = 0;
                foreach (string assetFile in assetFiles)
                {
                    using (AssetFileDescriptor fileDescriptor = assetManager.OpenFd(assetFile))
                    {
                        size += fileDescriptor.Length;
                    }
                }

                fileSize = size;
            }
            catch (Exception)
            {
                fileSize = -1;
            }

            if (fileSize <= 0)
            {
                try
                {
                    long size = 0;
                    foreach (string assetFile in assetFiles)
                    {
                        using (Stream stream = assetManager.Open(assetFile))
                        {
                            byte[] buffer = new byte[8192];

                            for (; ; )
                            {
                                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                                if (bytesRead <= 0)
                                {
                                    break;
                                }

                                size += bytesRead;
                            }
                        }
                    }

                    fileSize = size;
                }
                catch (Exception)
                {
                    fileSize = -1;
                }
            }

            return fileSize;
        }

        public static string GetAssetEcuFileBaseName(string assetEcuFileNames)
        {
            if (string.IsNullOrEmpty(assetEcuFileNames))
            {
                return string.Empty;
            }

            string[] assetFiles = assetEcuFileNames.Split(";");
            if (assetFiles.Length > 0)
            {
                string baseName = Path.GetFileNameWithoutExtension(assetFiles[0]);
                int underscoreIndex = baseName.IndexOf('_');
                if (underscoreIndex >= 0)
                {
                    baseName = baseName.Substring(0, underscoreIndex);
                }
                return baseName;
            }

            return string.Empty;
        }

        public static string GetAssetEcuFilenames()
        {
            try
            {
                List<string> assetEcuFileList = new List<string>();
                AssetManager assets = PackageUtils.GetPackageContext()?.Assets;
                if (assets != null)
                {
                    string[] assetFiles = assets.List(string.Empty);
                    if (assetFiles != null)
                    {
                        Regex regex = new Regex(@"^Ecu.*\.bin$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                        foreach (string fileName in assetFiles)
                        {
                            if (regex.IsMatch(fileName))
                            {
                                assetEcuFileList.Add(fileName);
                            }
                        }
                    }

                    assetEcuFileList = assetEcuFileList.OrderBy(n => n).ToList();
                    if (assetEcuFileList.Count > 0)
                    {
                        string assetEcuFileNames = string.Join(";", assetEcuFileList);
                        return assetEcuFileNames;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        public static bool VerifyContent(string contentFile, bool checkMd5, ActivityCommon.ProgressVerifyDelegate progressHandler)
        {
            try
            {
                string baseDir = Path.GetDirectoryName(contentFile);
                if (string.IsNullOrEmpty(baseDir))
                {
                    return false;
                }

                // ReSharper disable once AssignNullToNotNullAttribute
                bool vagUdsDirPresent = Directory.Exists(Path.Combine(baseDir, ActivityCommon.VagBaseDir));
                bool vagEcuDirPresent = Directory.Exists(Path.Combine(baseDir, ActivityCommon.EcuDirNameVag));
                if (vagUdsDirPresent && !vagEcuDirPresent)
                {
                    return false;
                }
                XDocument xmlDoc = XDocument.Load(contentFile);
                if (xmlDoc.Root == null)
                {
                    return false;
                }

                XElement[] fileNodes = xmlDoc.Root.Elements("file").ToArray();
                int fileCount = 0;
                foreach (XElement fileNode in fileNodes)
                {
                    XAttribute nameAttr = fileNode.Attribute("name");
                    if (nameAttr == null)
                    {
                        return false;
                    }
                    string fileName = nameAttr.Value;
                    if (string.IsNullOrEmpty(fileName))
                    {
                        return false;
                    }

                    bool vagUdsFile = fileName.StartsWith(PathUtils.AppendDirectorySeparatorChar(ActivityCommon.VagBaseDir));
                    if (!vagUdsDirPresent && vagUdsFile)
                    {
                        continue;   // ignore VAG UDS files if not present
                    }
                    bool vagEcuFile = fileName.StartsWith(PathUtils.AppendDirectorySeparatorChar(ActivityCommon.EcuDirNameVag));
                    if (!vagEcuDirPresent && vagEcuFile)
                    {
                        continue;   // ignore VAG ECU files if not present
                    }

                    fileCount++;
                }
                if (fileCount == 0)
                {
                    fileCount = 1;
                }

                int index = 0;
                int lastPercent = -1;
                foreach (XElement fileNode in fileNodes)
                {
                    if (progressHandler != null)
                    {
                        int percent = 100 * index / fileCount;
                        if (lastPercent != percent)
                        {
                            lastPercent = percent;
                            if (progressHandler.Invoke(percent))
                            {
                                return false;
                            }
                        }
                    }

                    XAttribute nameAttr = fileNode.Attribute("name");
                    if (nameAttr == null)
                    {
                        return false;
                    }
                    string fileName = nameAttr.Value;
                    if (string.IsNullOrEmpty(fileName))
                    {
                        return false;
                    }

                    bool vagUdsFile = fileName.StartsWith(PathUtils.AppendDirectorySeparatorChar(ActivityCommon.VagBaseDir));
                    if (!vagUdsDirPresent && vagUdsFile)
                    {
                        continue;   // ignore VAG UDS files if not present
                    }
                    bool vagEcuFile = fileName.StartsWith(PathUtils.AppendDirectorySeparatorChar(ActivityCommon.EcuDirNameVag));
                    if (!vagEcuDirPresent && vagEcuFile)
                    {
                        continue;   // ignore VAG ECU files if not present
                    }
                    XAttribute sizeAttr = fileNode.Attribute("size");
                    if (sizeAttr == null)
                    {
                        return false;
                    }
                    long fileSize = XmlConvert.ToInt64(sizeAttr.Value);

                    string fullFileName = Path.Combine(baseDir, fileName);
#if DEBUG && false
                    string encodedFileName = EdiabasNet.EncodeFilePath(fullFileName);
                    if (!string.IsNullOrEmpty(encodedFileName))
                    {
                        string decodedFilePath = EdiabasNet.DecodeFilePath(encodedFileName);
                        if (string.Compare(fullFileName, decodedFilePath, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            Android.Util.Log.Error(Tag, string.Format(CultureInfo.InvariantCulture, "Encoding invalid: {0} {1}", fullFileName, decodedFilePath));
                        }
                    }
#endif
                    // ReSharper disable once AssignNullToNotNullAttribute
                    string filePath = EdiabasNet.GetExistingEncodedFilePath(fullFileName);
                    FileInfo fileInfo = new FileInfo(filePath);
                    if (!fileInfo.Exists)
                    {
                        return false;
                    }

                    if (fileInfo.Length != fileSize)
                    {
                        return false;
                    }

                    if (checkMd5)
                    {
                        XAttribute md5Attr = fileNode.Attribute("md5");
                        if (md5Attr == null)
                        {
                            return false;
                        }

                        string md5String = md5Attr.Value;
                        using (MD5 md5 = MD5.Create())
                        {
                            using (FileStream stream = File.OpenRead(filePath))
                            {
                                byte[] md5Data = md5.ComputeHash(stream);
                                StringBuilder sb = new StringBuilder();
                                foreach (byte value in md5Data)
                                {
                                    sb.Append($"{value:X02}");
                                }

                                if (string.Compare(sb.ToString(), md5String, StringComparison.OrdinalIgnoreCase) != 0)
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    index++;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
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
    }
}
