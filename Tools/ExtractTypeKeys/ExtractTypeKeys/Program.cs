using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace ExtractTypeKeys
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

                    string typeKeysFile = Path.Combine(outDir, "typekeys.txt");
                    zipFiles.Add(typeKeysFile);
                    using (StreamWriter swTypeKeys = new StreamWriter(typeKeysFile))
                    {
                        string sql1 = @"SELECT t.NAME AS TYPEKEY, c.NAME AS EREIHE FROM XEP_CHARACTERISTICS t INNER JOIN XEP_VEHICLES v ON (v.TYPEKEYID = t.ID) INNER JOIN XEP_CHARACTERISTICS c ON (v.CHARACTERISTICID = c.ID) INNER JOIN XEP_CHARACTERISTICROOTS r ON (r.ID = c.PARENTID AND r.NODECLASS=40140802) WHERE t.NODECLASS = 40135042 ORDER BY TYPEKEY";
                        using (SQLiteCommand command = new SQLiteCommand(sql1, mDbConnection))
                        {
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    swTypeKeys.WriteLine(reader["TYPEKEY"] + "," + reader["EREIHE"]);
                                }
                            }
                        }
                    }

                    string vinRangeFile = Path.Combine(outDir, "vinranges.txt");
                    zipFiles.Add(vinRangeFile);
                    using (StreamWriter swVinranges = new StreamWriter(vinRangeFile))
                    {
                        string sql2 = @"SELECT v.VINBANDFROM AS VINBANDFROM, v.VINBANDTO AS VINBANDTO, v.TYPSCHLUESSEL AS TYPEKEY FROM VINRANGES v";
                        using (SQLiteCommand command = new SQLiteCommand(sql2, mDbConnection))
                        {
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    swVinranges.WriteLine(reader["VINBANDFROM"] + "," + reader["VINBANDTO"] + "," + reader["TYPEKEY"]);
                                }
                            }
                        }
                    }

                    mDbConnection.Close();
                    if (!CreateZip(zipFiles, Path.Combine(outDir, "Database.zip")))
                    {
                        Console.WriteLine("Creating Zip file failed");
                        return 1;
                    }
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
