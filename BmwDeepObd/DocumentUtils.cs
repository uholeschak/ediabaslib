using Android.Content;
using Android.OS;
using Android.OS.Storage;
using AndroidX.DocumentFile.Provider;
using System;
using System.Collections.Generic;
using System.IO;

namespace BmwDeepObd
{
    public static class DocumentUtils
    {
#if DEBUG
        private static readonly string Tag = typeof(DocumentUtils).FullName;
#endif

        public static bool IsDocumentTreeSupported()
        {
            // basically support starts with Android N, but makes no sense
            if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
            {
                return true;
            }
            return false;
        }

        public static string GetDocumentPath(DocumentFile documentFile)
        {
            if (documentFile?.Uri == null)
            {
                return null;
            }

            try
            {
                string docId = Android.Provider.DocumentsContract.GetTreeDocumentId(documentFile.Uri);
                if (!string.IsNullOrEmpty(docId))
                {
                    string[] parts = docId.Split(':');
                    if (parts.Length > 1)
                    {
                        string volumeId = parts[0];
                        string docPath = parts[1];
                        string volumePath = GetVolumePath(volumeId);
                        if (!string.IsNullOrEmpty(docPath) && !string.IsNullOrEmpty(volumePath))
                        {
                            string fullPath = Path.Combine(volumePath, docPath);
                            return fullPath;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return documentFile.Uri.Path;
        }

        public static List<string> GetPersistedStorages()
        {
            List<string> storageList = new List<string>();
            if (IsDocumentTreeSupported())
            {
                IList<UriPermission> uriPermissions = Android.App.Application.Context.ContentResolver?.PersistedUriPermissions;
                if (uriPermissions != null)
                {
                    foreach (UriPermission uriPermission in uriPermissions)
                    {
                        try
                        {
                            if (uriPermission.Uri != null && uriPermission.IsWritePermission)
                            {
                                string docId = Android.Provider.DocumentsContract.GetTreeDocumentId(uriPermission.Uri);
                                if (!string.IsNullOrEmpty(docId))
                                {
                                    string[] parts = docId.Split(':');
                                    if (parts.Length > 1)
                                    {
                                        string volumeId = parts[0];
                                        string docPath = parts[1];
                                        string volumePath = GetVolumePath(volumeId);
                                        if (!string.IsNullOrEmpty(docPath) && !string.IsNullOrEmpty(volumePath))
                                        {
                                            string fullPath = Path.Combine(volumePath, docPath);
                                            storageList.Add(fullPath);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }
            }

            return storageList;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416: Validate platform compatibility")]
        public static string GetVolumePath(string volumeId)
        {
            try
            {
                if (!IsDocumentTreeSupported())
                {
                    return null;
                }

                if (string.IsNullOrEmpty(volumeId))
                {
                    return null;
                }

                StorageManager storageManager = Android.App.Application.Context.GetSystemService(Context.StorageService) as StorageManager;
                IList<StorageVolume> storageVolumes = storageManager?.StorageVolumes;
                if (storageVolumes == null)
                {
                    return null;
                }

                foreach (StorageVolume storageVolume in storageVolumes)
                {
                    if (storageVolume.IsPrimary && ActivityCommon.PrimaryVolumeName.Equals(volumeId))
                    {
                        return storageVolume.Directory?.Path;
                    }

                    string uuid = storageVolume.Uuid;
                    if (!string.IsNullOrEmpty(uuid) && uuid.Equals(volumeId))
                    {
                        return storageVolume.Directory?.Path;
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool CopyDocumentsRecursive(DocumentFile documentSrc, DocumentFile documentDst, ActivityCommon.ProgressDocumentCopyDelegate progressHandler)
        {
            try
            {
                if (documentSrc?.Uri == null || documentDst?.Uri == null)
                {
                    return false;
                }

                if (!documentSrc.Exists())
                {
                    return false;
                }

                if (documentSrc.IsFile)
                {
                    DocumentFile fileDst = documentDst;
                    if (documentDst.IsDirectory)
                    {
                        DocumentFile oldFile = documentDst.FindFile(documentSrc.Name);
                        if (oldFile != null)
                        {
#if DEBUG
                            Android.Util.Log.Info(Tag, string.Format("Delete file: Name={0}, URI={1}", documentSrc.Name, documentSrc.Uri.ToString()));
#endif
                            oldFile.Delete();
                        }
#if DEBUG
                        Android.Util.Log.Info(Tag, string.Format("Create file: Name={0}, URI={1}", documentSrc.Name, documentSrc.Uri.ToString()));
#endif
                        fileDst = documentDst.CreateFile(ActivityCommon.MimeTypeAppAny, documentSrc.Name);
                    }

                    if (!fileDst.IsFile)
                    {
                        return false;
                    }

                    if (progressHandler != null)
                    {
                        if (!progressHandler.Invoke(documentSrc.Name))
                        {
                            return false;
                        }
                    }

                    ContentResolver contentResolver = Android.App.Application.Context.ContentResolver;
                    if (contentResolver == null)
                    {
                        return false;
                    }
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("Copy file: Name={0}, Type={1}, URI={2}", documentSrc.Name, documentSrc.Type, documentSrc.Uri.ToString()));
#endif
                    using (Stream inputStream = contentResolver.OpenInputStream(documentSrc.Uri))
                    {
                        if (inputStream == null)
                        {
                            return false;
                        }

                        using (Stream outputStream = contentResolver.OpenOutputStream(fileDst.Uri))
                        {
                            if (outputStream == null)
                            {
                                return false;
                            }

                            inputStream.CopyTo(outputStream);
                        }
                    }

                    return true;
                }

                if (!documentSrc.IsDirectory || !documentDst.IsDirectory)
                {
                    return false;
                }

                DocumentFile oldDir = documentDst.FindFile(documentSrc.Name);
                if (oldDir != null)
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, string.Format("Delete dir: Name={0}, URI={1}", documentSrc.Name, documentSrc.Uri.ToString()));
#endif
                    oldDir.Delete();
                }

#if DEBUG
                Android.Util.Log.Info(Tag, string.Format("Create dir: Name={0}, URI={1}", documentSrc.Name, documentSrc.Uri.ToString()));
#endif
                DocumentFile subDirDst = documentDst.CreateDirectory(documentSrc.Name);
                DocumentFile[] files = documentSrc.ListFiles();
                foreach (DocumentFile documentFile in files)
                {
                    if (documentFile.IsFile)
                    {
                        DocumentFile oldFile = subDirDst.FindFile(documentFile.Name);
                        if (oldFile != null)
                        {
#if DEBUG
                            Android.Util.Log.Info(Tag, string.Format("Delete file: Name={0}, URI={1}", documentFile.Name, documentFile.Uri.ToString()));
#endif
                            oldFile.Delete();
                        }
#if DEBUG
                        Android.Util.Log.Info(Tag, string.Format("Create file: Name={0}, URI={1}", documentFile.Name, documentFile.Uri.ToString()));
#endif
                        DocumentFile dstFile = subDirDst.CreateFile(ActivityCommon.MimeTypeAppAny, documentFile.Name);
                        if (!CopyDocumentsRecursive(documentFile, dstFile, progressHandler))
                        {
                            return false;
                        }
                        continue;
                    }

                    if (documentFile.IsDirectory)
                    {
                        if (!CopyDocumentsRecursive(documentFile, subDirDst, progressHandler))
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static int GetDocumentCount(DocumentFile document, ActivityCommon.ProgressDocumentCopyDelegate progressHandler)
        {
            try
            {
                if (document == null)
                {
                    return -1;
                }

                if (document.IsFile)
                {
                    if (progressHandler != null)
                    {
                        if (!progressHandler.Invoke(document.Name))
                        {
                            return -1;
                        }
                    }

                    if (document.Exists())
                    {
                        return 1;
                    }

                    return 0;
                }

                if (!document.IsDirectory)
                {
                    return -1;
                }

                int fileCount = 0;
                DocumentFile[] files = document.ListFiles();
                foreach (DocumentFile documentFile in files)
                {
                    if (documentFile.IsFile)
                    {
                        if (document.Exists())
                        {
                            fileCount++;
                        }
                        continue;
                    }

                    if (documentFile.IsDirectory)
                    {
                        int count = GetDocumentCount(documentFile, progressHandler);
                        if (count < 0)
                        {
                            return -1;
                        }

                        fileCount += count;
                    }
                }

                return fileCount;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public static bool DocumentExists(DocumentFile document)
        {
            try
            {
                if (document == null)
                {
                    return false;
                }

                if (document.Exists())
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }
    }
}
