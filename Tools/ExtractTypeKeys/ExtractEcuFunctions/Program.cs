using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace ExtractEcuFunctions
{
    static class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("No Database specified");
                return 1;
            }
            if (args.Length < 2)
            {
                Console.WriteLine("No output directory specified");
                return 1;
            }
            string outDir = args[1];
            if (string.IsNullOrEmpty(outDir))
            {
                Console.WriteLine("Output directory empty");
                return 1;
            }

            try
            {
                List<string> zipFiles = new List<string>();
                string connection = "Data Source=\"" + args[0] + "\";";
                using (SQLiteConnection mDbConnection = new SQLiteConnection(connection))
                {
                    mDbConnection.SetPassword("6505EFBDC3E5F324");
                    mDbConnection.Open();

                    string sqlNodeClassReadIdent = string.Format(@"SELECT ID FROM XEP_NODECLASSES WHERE (TRIM(NAME) = '{0}')", "ECUFixedFunctionReadingIdentification");
                    SQLiteCommand commandSqlnodeClassReadIdent = new SQLiteCommand(sqlNodeClassReadIdent, mDbConnection);
                    string nodeClassReadIdent = null;
                    using (SQLiteDataReader reader = commandSqlnodeClassReadIdent.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            nodeClassReadIdent = reader["ID"].ToString();
                        }
                    }

                    if (string.IsNullOrEmpty(nodeClassReadIdent))
                    {
                        Console.WriteLine("Node class ECUFixedFunctionReadingIdentification not found");
                        return 1;
                    }

                    mDbConnection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return 0;
        }

        private static bool CreateZip(List<string> inputFiles, string archiveFilenameOut)
        {
            try
            {
                if (File.Exists(archiveFilenameOut))
                {
                    File.Delete(archiveFilenameOut);
                }
                FileStream fsOut = File.Create(archiveFilenameOut);
                ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                zipStream.SetLevel(3);

                try
                {
                    foreach (string filename in inputFiles)
                    {

                        FileInfo fi = new FileInfo(filename);
                        string entryName = Path.GetFileName(filename);

                        ZipEntry newEntry = new ZipEntry(entryName)
                        {
                            DateTime = fi.LastWriteTime,
                            Size = fi.Length
                        };
                        zipStream.PutNextEntry(newEntry);

                        byte[] buffer = new byte[4096];
                        using (FileStream streamReader = File.OpenRead(filename))
                        {
                            StreamUtils.Copy(streamReader, zipStream, buffer);
                        }
                        zipStream.CloseEntry();
                    }
                }
                finally
                {
                    zipStream.IsStreamOwner = true;
                    zipStream.Close();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
