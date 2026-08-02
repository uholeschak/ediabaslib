using PsdzClient.Core;
using System;
using System.IO;

namespace BMW.Rheingold.ISTA.CoreFramework.Utility
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public static class APIUtilities
    {
        public static void DeleteLocalFile(string path)
        {
            try
            {
                string tempFolder = ConfigSettings.GetTempFolder();
                if (Path.GetFullPath(Path.GetDirectoryName(path)).Equals(Path.GetFullPath(tempFolder)))
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        Log.Info("APIUtilities.DeleteLocalFile", "The file {0} was successfully deleted", path);
                    }
                    else
                    {
                        Log.Info("APIUtilities.DeleteLocalFile", "The file {0} should be deleted, but was not found", path);
                    }
                }
                else
                {
                    Log.Info("APIUtilities.DeleteLocalFile", "The file {0} was not in the folder for temporary files", path);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("APIUtilities.DeleteLocalFile()", exception);
            }
        }
    }
}
