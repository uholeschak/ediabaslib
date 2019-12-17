using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace ExtractEcuFunctions
{
    public class EcuVariant
    {
        public EcuVariant(string id, string groupId)
        {
            Id = id;
            GroupId = groupId;
        }

        public string Id { get; }
        public string GroupId { get; }
        public List<string> GroupFunctionIds { get; set; }
    }

    public class EcuVarFunc
    {
        public EcuVarFunc(string id, string groupFuncId, EcuVariant ecuVariant)
        {
            Id = id;
            GroupFuncId = groupFuncId;
            EcuVariant = ecuVariant;
        }

        public string Id { get; }
        public string GroupFuncId { get; }
        public EcuVariant EcuVariant { get; set; }
    }

    public class EcuRefFunc
    {
        public EcuRefFunc(string funcStructId, EcuVarFunc ecuVarFunc)
        {
            FuncStructId = funcStructId;
            EcuVarFunc = ecuVarFunc;
        }

        public string FuncStructId { get; }
        public EcuVarFunc EcuVarFunc { get; set; }
    }

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
            if (args.Length < 3)
            {
                Console.WriteLine("No ECU name specified");
                return 1;
            }

            string outDir = args[1];
            if (string.IsNullOrEmpty(outDir))
            {
                Console.WriteLine("Output directory empty");
                return 1;
            }

            string ecuName = args[2];
            if (string.IsNullOrEmpty(ecuName))
            {
                Console.WriteLine("ECU name empty");
                return 1;
            }

            try
            {
                string connection = "Data Source=\"" + args[0] + "\";";
                using (SQLiteConnection mDbConnection = new SQLiteConnection(connection))
                {
                    mDbConnection.SetPassword("6505EFBDC3E5F324");
                    mDbConnection.Open();

                    string nodeClassReadIdent = null;
                    {
                        string sql = string.Format(@"SELECT ID FROM XEP_NODECLASSES WHERE (TRIM(NAME) = '{0}')", "ECUFixedFunctionReadingIdentification");
                        SQLiteCommand command = new SQLiteCommand(sql, mDbConnection);
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                nodeClassReadIdent = reader["ID"].ToString();
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(nodeClassReadIdent))
                    {
                        Console.WriteLine("Node class ECUFixedFunctionReadingIdentification not found");
                        return 1;
                    }

                    List<EcuVariant> ecuVarList = new List<EcuVariant>();
                    {
                        string sql = string.Format(@"SELECT ID, ECUGROUPID FROM XEP_ECUVARIANTS WHERE (lower(NAME) = '{0}')", ecuName.ToLowerInvariant());
                        SQLiteCommand command = new SQLiteCommand(sql, mDbConnection);
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ecuVarList.Add(new EcuVariant(reader["ID"].ToString(), reader["ECUGROUPID"].ToString()));
                            }
                        }
                    }

                    if (ecuVarList.Count == 0)
                    {
                        Console.WriteLine("ECU variant not found");
                        return 1;
                    }

                    foreach (EcuVariant ecuVariant in ecuVarList)
                    {
                        List<string> ecuGroupFunctionIds = new List<string>();
                        string sql = string.Format(@"SELECT ID FROM XEP_ECUGROUPFUNCTIONS WHERE ECUGROUPID = {0}", ecuVariant.GroupId);
                        SQLiteCommand command = new SQLiteCommand(sql, mDbConnection);
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ecuGroupFunctionIds.Add(reader["ID"].ToString());
                            }
                        }

                        if (ecuGroupFunctionIds.Count == 0)
                        {
                            Console.WriteLine("ECU group function id not found");
                            return 1;
                        }

                        ecuVariant.GroupFunctionIds = ecuGroupFunctionIds;
                    }

                    List<EcuVarFunc> ecuVarFunctionsList = new List<EcuVarFunc>();
                    foreach (EcuVariant ecuVariant in ecuVarList)
                    {
                        foreach (string ecuGroupFunctionId in ecuVariant.GroupFunctionIds)
                        {
                            string sql = string.Format(@"SELECT ID, VISIBLE, NAME, OBD_RELEVANZ FROM XEP_ECUVARFUNCTIONS WHERE (lower(NAME) = '{0}') AND ECUGROUPFUNCTIONID = {1}", ecuName.ToLowerInvariant(), ecuGroupFunctionId);
                            SQLiteCommand command = new SQLiteCommand(sql, mDbConnection);
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    ecuVarFunctionsList.Add(new EcuVarFunc(reader["ID"].ToString(), ecuGroupFunctionId, ecuVariant));
                                }
                            }
                        }
                    }

                    if (ecuVarFunctionsList.Count == 0)
                    {
                        Console.WriteLine("ECU var functions not found");
                        return 1;
                    }

                    List<EcuRefFunc> ecuRefFuncStructList = new List<EcuRefFunc>();
                    foreach (EcuVarFunc ecuVarFunc in ecuVarFunctionsList)
                    {
                        string sql = string.Format(@"SELECT ECUFUNCSTRUCTID FROM XEP_REFECUFUNCSTRUCTS WHERE ID = {0}", ecuVarFunc.Id);
                        SQLiteCommand command = new SQLiteCommand(sql, mDbConnection);
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ecuRefFuncStructList.Add(new EcuRefFunc(reader["ECUFUNCSTRUCTID"].ToString(), ecuVarFunc));
                            }
                        }
                    }

                    if (ecuRefFuncStructList.Count == 0)
                    {
                        Console.WriteLine("ECU ref functions not found");
                        return 1;
                    }

                    List<string> ecuFuncStructList = new List<string>();
                    foreach (EcuRefFunc ecuRefFunc in ecuRefFuncStructList)
                    {
                        string sql = string.Format(@"SELECT TITLE_ENUS, TITLE_DEDE, TITLE_RU FROM XEP_ECUFUNCSTRUCTURES WHERE ID = {0}", ecuRefFunc.FuncStructId);
                        SQLiteCommand command = new SQLiteCommand(sql, mDbConnection);
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string funcStructName = "ENUS='" + reader["TITLE_ENUS"] + "', DE='" + reader["TITLE_DEDE"] + "', RU='" + reader["TITLE_RU"] + "'";
                                ecuFuncStructList.Add(funcStructName);
                            }
                        }
                    }

                    if (ecuFuncStructList.Count == 0)
                    {
                        Console.WriteLine("ECU function structures not found");
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
