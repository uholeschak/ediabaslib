using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using BmwFileReader;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace ExtractEcuFunctions
{
    static class Program
    {
        const string DbPassword = "6505EFBDC3E5F324";

        private const string SqlTitleItems =
            "TITLE_DEDE, TITLE_ENGB, TITLE_ENUS, " +
            "TITLE_FR, TITLE_TH, TITLE_SV, " +
            "TITLE_IT, TITLE_ES, TITLE_ID, " +
            "TITLE_KO, TITLE_EL, TITLE_TR, " +
            "TITLE_ZHCN, TITLE_RU, TITLE_NL, " +
            "TITLE_PT, TITLE_ZHTW, TITLE_JA, " +
            "TITLE_CSCZ, TITLE_PLPL";

        private const string SqlPreOpItems =
                "PREPARINGOPERATORTEXT_DEDE, PREPARINGOPERATORTEXT_ENGB, PREPARINGOPERATORTEXT_ENUS, " +
                "PREPARINGOPERATORTEXT_FR, PREPARINGOPERATORTEXT_TH, PREPARINGOPERATORTEXT_SV, " +
                "PREPARINGOPERATORTEXT_IT, PREPARINGOPERATORTEXT_ES, PREPARINGOPERATORTEXT_ID, " +
                "PREPARINGOPERATORTEXT_KO, PREPARINGOPERATORTEXT_EL, PREPARINGOPERATORTEXT_TR, " +
                "PREPARINGOPERATORTEXT_ZHCN, PREPARINGOPERATORTEXT_RU, PREPARINGOPERATORTEXT_NL, " +
                "PREPARINGOPERATORTEXT_PT, PREPARINGOPERATORTEXT_ZHTW, PREPARINGOPERATORTEXT_JA, " +
                "PREPARINGOPERATORTEXT_CSCZ, PREPARINGOPERATORTEXT_PLPL";

        private const string SqlProcItems =
                "PROCESSINGOPERATORTEXT_DEDE, PROCESSINGOPERATORTEXT_ENGB, PROCESSINGOPERATORTEXT_ENUS, " +
                "PROCESSINGOPERATORTEXT_FR, PROCESSINGOPERATORTEXT_TH, PROCESSINGOPERATORTEXT_SV, " +
                "PROCESSINGOPERATORTEXT_IT, PROCESSINGOPERATORTEXT_ES, PROCESSINGOPERATORTEXT_ID, " +
                "PROCESSINGOPERATORTEXT_KO, PROCESSINGOPERATORTEXT_EL, PROCESSINGOPERATORTEXT_TR, " +
                "PROCESSINGOPERATORTEXT_ZHCN, PROCESSINGOPERATORTEXT_RU, PROCESSINGOPERATORTEXT_NL, " +
                "PROCESSINGOPERATORTEXT_PT, PROCESSINGOPERATORTEXT_ZHTW, PROCESSINGOPERATORTEXT_JA, " +
                "PROCESSINGOPERATORTEXT_CSCZ, PROCESSINGOPERATORTEXT_PLPL";

        private const string SqlPostOpItems =
                "POSTOPERATORTEXT_DEDE, POSTOPERATORTEXT_ENGB, POSTOPERATORTEXT_ENUS, " +
                "POSTOPERATORTEXT_FR, POSTOPERATORTEXT_TH, POSTOPERATORTEXT_SV, " +
                "POSTOPERATORTEXT_IT, POSTOPERATORTEXT_ES, POSTOPERATORTEXT_ID, " +
                "POSTOPERATORTEXT_KO, POSTOPERATORTEXT_EL, POSTOPERATORTEXT_TR, " +
                "POSTOPERATORTEXT_ZHCN, POSTOPERATORTEXT_RU, POSTOPERATORTEXT_NL, " +
                "POSTOPERATORTEXT_PT, POSTOPERATORTEXT_ZHTW, POSTOPERATORTEXT_JA, " +
                "POSTOPERATORTEXT_CSCZ, POSTOPERATORTEXT_PLPL";

        static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            TextWriter outTextWriter = args.Length >= 0 ? Console.Out : null;
            TextWriter logTextWriter = args.Length >= 3 ? Console.Out : null;

            if (args.Length < 1)
            {
                outTextWriter?.WriteLine("No Database specified");
                return 1;
            }
            if (args.Length < 2)
            {
                outTextWriter?.WriteLine("No output directory specified");
                return 1;
            }

            string outDir = args[1];
            if (string.IsNullOrEmpty(outDir))
            {
                outTextWriter?.WriteLine("Output directory empty");
                return 1;
            }

            string ecuName = null;
            if (args.Length >= 3)
            {
                ecuName = args[2];
            }

            try
            {
                string outDirSub = Path.Combine(outDir, "EcuFunctions");
                string zipFile = Path.Combine(outDir, "EcuFunctions.zip");
                try
                {
                    if (Directory.Exists(outDirSub))
                    {
                        Directory.Delete(outDirSub, true);
                        Thread.Sleep(1000);
                    }
                    Directory.CreateDirectory(outDirSub);
                }
                catch (Exception)
                {
                    // ignored
                }

                try
                {
                    if (File.Exists(zipFile))
                    {
                        File.Delete(zipFile);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                string connection = "Data Source=\"" + args[0] + "\";";
                List<String> ecuNameList;
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (string.IsNullOrEmpty(ecuName))
                {
                    ecuNameList = GetEcuNameList(connection);
                    if (ecuNameList == null)
                    {
                        outTextWriter?.WriteLine("Creating ECU name list failed");
                        return 1;
                    }
                }
                else
                {
                    ecuNameList = new List<string> { ecuName };
                }

                List<Thread> threadList = new List<Thread>();
                foreach (string name in ecuNameList)
                {
                    // limit number of active tasks
                    for (; ; )
                    {
                        threadList.RemoveAll(thread => !thread.IsAlive);
                        int activeThreads = threadList.Count(thread => thread.IsAlive);
                        if (activeThreads < 16)
                        {
                            break;
                        }
                        Thread.Sleep(200);
                    }

                    Thread serializeThread = new Thread(() =>
                    {
                        SerializeEcuFunction(outTextWriter, logTextWriter, connection, outDirSub, name);
                    });
                    serializeThread.Start();
                    threadList.Add(serializeThread);
                }

                foreach (Thread compileThread in threadList)
                {
                    compileThread.Join();
                }

                if (!CreateZipFile(outDirSub, zipFile))
                {
                    outTextWriter?.WriteLine("Create ZIP failed");
                    return 1;
                }
            }
            catch (Exception e)
            {
                outTextWriter?.WriteLine(e);
            }
            return 0;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static bool SerializeEcuFunction(TextWriter outTextWriter, TextWriter logTextWriter, string connection, string outDirSub, string ecuName)
        {
            try
            {
                using (SQLiteConnection mDbConnection = new SQLiteConnection(connection))
                {
                    mDbConnection.SetPassword(DbPassword);
                    mDbConnection.Open();

                    outTextWriter?.WriteLine("*** ECU: {0} ***", ecuName);
                    EcuFunctionStructs.EcuVariant ecuVariant = GetEcuVariantFunctions(outTextWriter, logTextWriter, mDbConnection, ecuName);

                    if (ecuVariant != null)
                    {
                        logTextWriter?.WriteLine(ecuVariant);

                        string xmlFile = Path.Combine(outDirSub, ecuName.ToLowerInvariant() + ".xml");
                        XmlSerializer serializer = new XmlSerializer(ecuVariant.GetType());
                        using (TextWriter writer = new StreamWriter(xmlFile))
                        {
                            serializer.Serialize(writer, ecuVariant);
                        }
                    }

                    mDbConnection.Close();
                }

                return true;
            }
            catch (Exception e)
            {
                outTextWriter?.WriteLine(e);
                return false;
            }
        }

        private static List<string> GetEcuNameList(string connection)
        {
            try
            {
                List<string> ecuNameList;
                using (SQLiteConnection mDbConnection = new SQLiteConnection(connection))
                {
                    mDbConnection.SetPassword(DbPassword);
                    mDbConnection.Open();

                    ecuNameList = GetEcuNameList(mDbConnection);

                    mDbConnection.Close();
                }

                return ecuNameList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static List<string> GetEcuNameList(SQLiteConnection mDbConnection)
        {
            List<string> ecuNameList = new List<string>();
            string sql = @"SELECT NAME FROM XEP_ECUVARIANTS";
            using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ecuNameList.Add(reader["NAME"].ToString());
                    }
                }
            }

            return ecuNameList;
        }

        private static EcuFunctionStructs.EcuTranslation GetTranslation(SQLiteDataReader reader, string prefix = "TITLE")
        {
            return new EcuFunctionStructs.EcuTranslation(
                reader[prefix + "_DEDE"].ToString(),
                reader[prefix + "_ENUS"].ToString(),
                reader[prefix + "_FR"].ToString(),
                reader[prefix + "_TH"].ToString(),
                reader[prefix + "_SV"].ToString(),
                reader[prefix + "_IT"].ToString(),
                reader[prefix + "_ES"].ToString(),
                reader[prefix + "_ID"].ToString(),
                reader[prefix + "_KO"].ToString(),
                reader[prefix + "_EL"].ToString(),
                reader[prefix + "_TR"].ToString(),
                reader[prefix + "_ZHCN"].ToString(),
                reader[prefix + "_RU"].ToString(),
                reader[prefix + "_NL"].ToString(),
                reader[prefix + "_PT"].ToString(),
                reader[prefix + "_JA"].ToString(),
                reader[prefix + "_CSCZ"].ToString(),
                reader[prefix + "_PLPL"].ToString()
                );
        }

        private static EcuFunctionStructs.EcuVariant GetEcuVariant(SQLiteConnection mDbConnection, string ecuName)
        {
            EcuFunctionStructs.EcuVariant ecuVariant = null;
            string sql = string.Format(@"SELECT ID, " + SqlTitleItems + ", ECUGROUPID FROM XEP_ECUVARIANTS WHERE (lower(NAME) = '{0}')", ecuName.ToLowerInvariant());
            using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string groupId = reader["ECUGROUPID"].ToString();
                        ecuVariant = new EcuFunctionStructs.EcuVariant(reader["ID"].ToString(),
                            groupId,
                            GetTranslation(reader),
                            GetEcuGroupFunctionIds(mDbConnection, groupId));
                    }
                }
            }

            return ecuVariant;
        }

        private static List<string> GetEcuGroupFunctionIds(SQLiteConnection mDbConnection, string groupId)
        {
            List<string> ecuGroupFunctionIds = new List<string>();
            string sql = string.Format(@"SELECT ID FROM XEP_ECUGROUPFUNCTIONS WHERE ECUGROUPID = {0}", groupId);
            using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ecuGroupFunctionIds.Add(reader["ID"].ToString());
                    }
                }
            }

            return ecuGroupFunctionIds;
        }

        private static string GetNodeClassName(SQLiteConnection mDbConnection, string nodeClass)
        {
            string result = string.Empty;
            string sql = string.Format(@"SELECT NAME FROM XEP_NODECLASSES WHERE ID = {0}", nodeClass);
            using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = reader["NAME"].ToString();
                    }
                }
            }

            return result;
        }

        private static List<EcuFunctionStructs.EcuJob> GetFixedFuncStructJobsList(SQLiteConnection mDbConnection, EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct)
        {
            List<EcuFunctionStructs.EcuJob> ecuJobList = new List<EcuFunctionStructs.EcuJob>();
            string sql = string.Format(@"SELECT JOBS.ID JOBID, FUNCTIONNAMEJOB, NAME, PHASE, RANK " +
                                       "FROM XEP_ECUJOBS JOBS, XEP_REFECUJOBS REFJOBS WHERE JOBS.ID = REFJOBS.ECUJOBID AND REFJOBS.ID = {0}", ecuFixedFuncStruct.Id);
            using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ecuJobList.Add(new EcuFunctionStructs.EcuJob(reader["JOBID"].ToString(),
                            reader["FUNCTIONNAMEJOB"].ToString(),
                            reader["NAME"].ToString(),
                            reader["PHASE"].ToString(),
                            reader["RANK"].ToString()));
                    }
                }
            }

            foreach (EcuFunctionStructs.EcuJob ecuJob in ecuJobList)
            {
                List<EcuFunctionStructs.EcuJobParameter> ecuJobParList = new List<EcuFunctionStructs.EcuJobParameter>();
                sql = string.Format(
                    @"SELECT PARAM.ID PARAMID, PARAMVALUE, FUNCTIONNAMEPARAMETER, ADAPTERPATH, NAME, ECUJOBID " +
                    "FROM XEP_ECUPARAMETERS PARAM, XEP_REFECUPARAMETERS REFPARAM WHERE " +
                    "PARAM.ID = REFPARAM.ECUPARAMETERID AND REFPARAM.ID = {0} AND PARAM.ECUJOBID = {1}", ecuFixedFuncStruct.Id, ecuJob.Id);
                using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ecuJobParList.Add(new EcuFunctionStructs.EcuJobParameter(reader["PARAMID"].ToString(),
                                reader["PARAMVALUE"].ToString(),
                                reader["ADAPTERPATH"].ToString(),
                                reader["NAME"].ToString()));
                        }
                    }
                }

                ecuJob.EcuJobParList = ecuJobParList;

                List<EcuFunctionStructs.EcuJobResult> ecuJobResultList = new List<EcuFunctionStructs.EcuJobResult>();
                sql = string.Format(
                    @"SELECT RESULTS.ID RESULTID, " + SqlTitleItems + ", FUNCTIONNAMERESULT, ADAPTERPATH, NAME, STEUERGERAETEFUNKTIONENRELEVAN, LOCATION, UNIT, UNITFIXED, FORMAT, MULTIPLIKATOR, OFFSET, RUNDEN, ZAHLENFORMAT, ECUJOBID " +
                    "FROM XEP_ECURESULTS RESULTS, XEP_REFECURESULTS REFRESULTS WHERE " +
                    "ECURESULTID = RESULTS.ID AND REFRESULTS.ID = {0} AND RESULTS.ECUJOBID = {1}", ecuFixedFuncStruct.Id, ecuJob.Id);
                using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ecuJobResultList.Add(new EcuFunctionStructs.EcuJobResult(reader["RESULTID"].ToString(),
                                GetTranslation(reader),
                                reader["FUNCTIONNAMERESULT"].ToString(),
                                reader["ADAPTERPATH"].ToString(),
                                reader["NAME"].ToString(),
                                reader["STEUERGERAETEFUNKTIONENRELEVAN"].ToString(),
                                reader["LOCATION"].ToString(),
                                reader["UNIT"].ToString(),
                                reader["UNITFIXED"].ToString(),
                                reader["FORMAT"].ToString(),
                                reader["MULTIPLIKATOR"].ToString(),
                                reader["OFFSET"].ToString(),
                                reader["RUNDEN"].ToString(),
                                reader["ZAHLENFORMAT"].ToString()));
                        }
                    }
                }

                foreach (EcuFunctionStructs.EcuJobResult ecuJobResult in ecuJobResultList)
                {
                    ecuJobResult.EcuResultStateValueList = GetResultStateValueList(mDbConnection, ecuJobResult);
                }

                ecuJob.EcuJobResultList = ecuJobResultList;
            }

            return ecuJobList;
        }

        private static List<EcuFunctionStructs.EcuResultStateValue> GetResultStateValueList(SQLiteConnection mDbConnection, EcuFunctionStructs.EcuJobResult ecuJobResult)
        {
            List<EcuFunctionStructs.EcuResultStateValue> ecuResultStateValueList = new List<EcuFunctionStructs.EcuResultStateValue>();
            string sql = string.Format(@"SELECT ID, " + SqlTitleItems + ", STATEVALUE, VALIDFROM, VALIDTO, PARENTID " +
                                       "FROM XEP_STATEVALUES WHERE (PARENTID IN (SELECT STATELISTID FROM XEP_REFSTATELISTS WHERE (ID = {0})))", ecuJobResult.Id);
            using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ecuResultStateValueList.Add(new EcuFunctionStructs.EcuResultStateValue(reader["ID"].ToString(),
                            GetTranslation(reader),
                            reader["STATEVALUE"].ToString(),
                            reader["VALIDFROM"].ToString(),
                            reader["VALIDTO"].ToString(),
                            reader["PARENTID"].ToString()));
                    }
                }
            }

            return ecuResultStateValueList;
        }

        private static List<EcuFunctionStructs.EcuFixedFuncStruct> GetEcuFixedFuncStructList(SQLiteConnection mDbConnection, string parentId)
        {
            List<EcuFunctionStructs.EcuFixedFuncStruct> ecuFixedFuncStructList = new List<EcuFunctionStructs.EcuFixedFuncStruct>();
            string sql = string.Format(@"SELECT ID, NODECLASS, " + SqlTitleItems + ", " +
                                       SqlPreOpItems + ", " + SqlProcItems + ", " + SqlPostOpItems + ", " +
                                       "SORT_ORDER, ACTIVATION, ACTIVATION_DURATION_MS " +
                                       "FROM XEP_ECUFIXEDFUNCTIONS WHERE (PARENTID = {0})", parentId);
            using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string nodeClass = reader["NODECLASS"].ToString();
                        EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct = new EcuFunctionStructs.EcuFixedFuncStruct(reader["ID"].ToString(),
                            nodeClass,
                            GetNodeClassName(mDbConnection, nodeClass),
                            GetTranslation(reader),
                            GetTranslation(reader, "PREPARINGOPERATORTEXT"),
                            GetTranslation(reader, "PROCESSINGOPERATORTEXT"),
                            GetTranslation(reader, "POSTOPERATORTEXT"),
                            reader["SORT_ORDER"].ToString(),
                            reader["ACTIVATION"].ToString(),
                            reader["ACTIVATION_DURATION_MS"].ToString());

                        ecuFixedFuncStruct.EcuJobList = GetFixedFuncStructJobsList(mDbConnection, ecuFixedFuncStruct);
                        ecuFixedFuncStructList.Add(ecuFixedFuncStruct);
                    }
                }
            }

            return ecuFixedFuncStructList;
        }

        private static EcuFunctionStructs.EcuVariant GetEcuVariantFunctions(TextWriter outTextWriter, TextWriter logTextWriter, SQLiteConnection mDbConnection, string ecuName)
        {
            EcuFunctionStructs.EcuVariant ecuVariant = GetEcuVariant(mDbConnection, ecuName);
            if (ecuVariant == null)
            {
                outTextWriter?.WriteLine("ECU variant not found");
                return null;
            }

            List<EcuFunctionStructs.RefEcuVariant> refEcuVariantList = new List<EcuFunctionStructs.RefEcuVariant>();
            {
                string sql = string.Format(@"SELECT ID, ECUVARIANTID FROM XEP_REFECUVARIANTS WHERE ECUVARIANTID = {0}", ecuVariant.Id);
                using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            refEcuVariantList.Add(new EcuFunctionStructs.RefEcuVariant(reader["ID"].ToString(),
                                reader["ECUVARIANTID"].ToString()));
                        }
                    }
                }
            }

            int fixFuncCount = 0;
            ecuVariant.RefEcuVariantList = refEcuVariantList;

            foreach (EcuFunctionStructs.RefEcuVariant refEcuVariant in refEcuVariantList)
            {
                List<EcuFunctionStructs.EcuFixedFuncStruct> ecuFixedFuncStructList = GetEcuFixedFuncStructList(mDbConnection, refEcuVariant.Id);
                fixFuncCount += ecuFixedFuncStructList.Count;
                refEcuVariant.FixedFuncStructList = ecuFixedFuncStructList;
            }

            List<EcuFunctionStructs.EcuVarFunc> ecuVarFunctionsList = new List<EcuFunctionStructs.EcuVarFunc>();
            foreach (string ecuGroupFunctionId in ecuVariant.GroupFunctionIds)
            {
                string sql = string.Format(@"SELECT ID, VISIBLE, NAME, OBD_RELEVANZ FROM XEP_ECUVARFUNCTIONS WHERE (lower(NAME) = '{0}') AND (ECUGROUPFUNCTIONID = {1})", ecuName.ToLowerInvariant(), ecuGroupFunctionId);
                using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ecuVarFunctionsList.Add(new EcuFunctionStructs.EcuVarFunc(reader["ID"].ToString(), ecuGroupFunctionId));
                        }
                    }
                }
            }

            foreach (EcuFunctionStructs.EcuVarFunc ecuVarFunc in ecuVarFunctionsList)
            {
                logTextWriter?.WriteLine(ecuVarFunc);
            }

            List<EcuFunctionStructs.EcuFuncStruct> ecuFuncStructList = new List<EcuFunctionStructs.EcuFuncStruct>();
            foreach (EcuFunctionStructs.EcuVarFunc ecuVarFunc in ecuVarFunctionsList)
            {
                string sql = string.Format(@"SELECT REFFUNCS.ECUFUNCSTRUCTID FUNCSTRUCTID, " + SqlTitleItems + ", MULTISELECTION " +
                        "FROM XEP_ECUFUNCSTRUCTURES FUNCS, XEP_REFECUFUNCSTRUCTS REFFUNCS WHERE FUNCS.ID = REFFUNCS.ECUFUNCSTRUCTID AND REFFUNCS.ID = {0}", ecuVarFunc.Id);
                using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ecuFuncStructList.Add(new EcuFunctionStructs.EcuFuncStruct(reader["FUNCSTRUCTID"].ToString(),
                                GetTranslation(reader),
                                reader["MULTISELECTION"].ToString()));
                        }
                    }
                }
            }

            foreach (EcuFunctionStructs.EcuFuncStruct ecuFuncStruct in ecuFuncStructList)
            {
                List<EcuFunctionStructs.EcuFixedFuncStruct> ecuFixedFuncStructList = GetEcuFixedFuncStructList(mDbConnection, ecuFuncStruct.Id);
                fixFuncCount += ecuFixedFuncStructList.Count;
                ecuFuncStruct.FixedFuncStructList = ecuFixedFuncStructList;

                if (ecuFuncStruct.MultiSelect.ConvertToInt() > 0)
                {
                    foreach (EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct in ecuFixedFuncStructList)
                    {
                        if (ecuFixedFuncStruct.GetNodeClassType() == EcuFunctionStructs.EcuFixedFuncStruct.NodeClassType.ControlActuator)
                        {
                            outTextWriter.WriteLine("Actuator multi select!");
                        }
                    }
                }
            }

            if (fixFuncCount == 0)
            {
                outTextWriter?.WriteLine("No ECU fix functions found");
                return null;
            }

            ecuVariant.EcuFuncStructList = ecuFuncStructList;

            return ecuVariant;
        }

        private static bool CreateZipFile(string inDir, string outFile, string key = null)
        {
            try
            {
                AesCryptoServiceProvider crypto = null;
                FileStream fsOut = null;
                ZipOutputStream zipStream = null;
                try
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        crypto = new AesCryptoServiceProvider
                        {
                            Mode = CipherMode.CBC,
                            Padding = PaddingMode.PKCS7,
                            KeySize = 256
                        };
                        using (SHA256Managed sha256 = new SHA256Managed())
                        {
                            crypto.Key = sha256.ComputeHash(Encoding.ASCII.GetBytes(key));
                        }
                        using (var md5 = MD5.Create())
                        {
                            crypto.IV = md5.ComputeHash(Encoding.ASCII.GetBytes(key));
                        }
                    }

                    fsOut = File.Create(outFile);
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (crypto != null)
                    {
                        CryptoStream crStream = new CryptoStream(fsOut,
                            crypto.CreateEncryptor(), CryptoStreamMode.Write);
                        zipStream = new ZipOutputStream(crStream);
                    }
                    else
                    {
                        zipStream = new ZipOutputStream(fsOut);
                    }


                    zipStream.SetLevel(9); //0-9, 9 being the highest level of compression

                    // This setting will strip the leading part of the folder path in the entries, to
                    // make the entries relative to the starting folder.
                    // To include the full path for each entry up to the drive root, assign folderOffset = 0.
                    int folderOffset = inDir.Length + (inDir.EndsWith("\\") ? 0 : 1);

                    CompressFolder(inDir, zipStream, folderOffset);
                }
                finally
                {
                    if (zipStream != null)
                    {
                        zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
                        zipStream.Close();
                    }
                    fsOut?.Close();
                    crypto?.Dispose();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {
            string[] files = Directory.GetFiles(path);

            foreach (string filename in files)
            {

                FileInfo fi = new FileInfo(filename);

                string entryName = filename.Substring(folderOffset); // Makes the name in zip based on the folder
                entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
                ZipEntry newEntry = new ZipEntry(entryName);
                newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity

                // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
                // A password on the ZipOutputStream is required if using AES.
                //   newEntry.AESKeySize = 256;

                // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
                // you need to do one of the following: Specify UseZip64.Off, or set the Size.
                // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
                // but the zip will be in Zip64 format which not all utilities can understand.
                //   zipStream.UseZip64 = UseZip64.Off;
                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);

                // Zip the file in buffered chunks
                // the "using" will close the stream even if an exception occurs
                byte[] buffer = new byte[4096];
                using (FileStream streamReader = File.OpenRead(filename))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }

            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                CompressFolder(folder, zipStream, folderOffset);
            }
        }
    }
}
