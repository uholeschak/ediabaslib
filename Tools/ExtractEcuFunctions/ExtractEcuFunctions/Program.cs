using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using BmwFileReader;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Data.Sqlite;
using PsdzClient.Core;

namespace ExtractEcuFunctions
{
    static class Program
    {
        public class DbInfo
        {
            public DbInfo(string version, DateTime dateTime)
            {
                Version = version;
                DateTime = dateTime;
            }

            public string Version { get; set; }

            public DateTime DateTime { get; set; }
        }

        private static List<string> LangList = new List<string>
        {
            "de", "en", "fr", "th",
            "sv", "it", "es", "id",
            "ko", "el", "tr", "zh",
            "ru", "nl", "pt", "ja",
            "cs", "pl",
        };

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

        private static readonly HashSet<string> FaultCodeLabelIdHashSet = new HashSet<string>();
        private static readonly HashSet<string> FaultModeLabelIdHashSet = new HashSet<string>();
        private static readonly HashSet<string> EnvCondLabelIdHashSet = new HashSet<string>();
        private static Dictionary<long, string> RootClassDict;
        private static string TypeKeyClassId = string.Empty;
        private static string EnvDiscreteNodeClassId = string.Empty;

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

                string hexKey = BitConverter.ToString(Encoding.ASCII.GetBytes(DatabaseFunctions.DatabasePassword)).Replace("-", "");
                SqliteConnectionStringBuilder connectionBuilder = new SqliteConnectionStringBuilder
                {
                    DataSource = "file:" + args[0] + "?cipher=rc4&hexkey=" + hexKey,
                    Mode = SqliteOpenMode.ReadOnly,
                };

                string connection = connectionBuilder.ConnectionString;
                if (!InitGlobalData(connection))
                {
                    outTextWriter?.WriteLine("Init failed");
                    return 1;
                }

                Dictionary<string, List<string>> typeKeyInfoList = new Dictionary<string, List<string>>();
                int infoIndex = 0;
                foreach (KeyValuePair<long, string> rootClassPair in RootClassDict)
                {
                    if (!ExtractTypeKeyClassInfo(outTextWriter, connection, rootClassPair.Key, typeKeyInfoList, infoIndex))
                    {
                        outTextWriter?.WriteLine("ExtractTypeKeyClassInfo Index: {0} failed", infoIndex);
                        return 1;
                    }

                    infoIndex++;
                }

                if (!WriteTypeKeyClassInfo(outTextWriter, typeKeyInfoList, outDirSub))
                {
                    outTextWriter?.WriteLine("WriteTypeKeyClassInfo failed");
                    return 1;
                }

                if (!WriteVinRanges(outTextWriter, connection, outDirSub))
                {
                    outTextWriter?.WriteLine("Write VinRanges failed");
                    return 1;
                }

                //return 0;

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

                foreach (Thread processThread in threadList)
                {
                    processThread.Join();
                }

                List<Thread> threadListFaultData = new List<Thread>();
                foreach (string language in LangList)
                {
                    Thread serializeThread = new Thread(() =>
                    {
                        SerializeEcuFaultData(outTextWriter, logTextWriter, connection, outDirSub, language);
                    });
                    serializeThread.Start();
                    threadListFaultData.Add(serializeThread);
                }

                foreach (Thread processThread in threadListFaultData)
                {
                    processThread.Join();
                }

                outTextWriter?.WriteLine("Creating ZIP file");
                if (!CreateZipFile(outDirSub, zipFile))
                {
                    outTextWriter?.WriteLine("Create ZIP failed");
                    return 1;
                }

                outTextWriter?.WriteLine("Deleting output directory");
                try
                {
                    if (Directory.Exists(outDirSub))
                    {
                        Directory.Delete(outDirSub, true);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            catch (Exception e)
            {
                outTextWriter?.WriteLine(e);
            }
            return 0;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        // ReSharper disable once UnusedParameter.Local
        private static bool SerializeEcuFaultData(TextWriter outTextWriter, TextWriter logTextWriter, string connection, string outDirSub, string language)
        {
            try
            {
                using (SqliteConnection mDbConnection = new SqliteConnection(connection))
                {
                    mDbConnection.Open();

                    outTextWriter?.WriteLine("*** Fault data {0} ***", language);
                    DbInfo dbInfo = GetDbInfo(mDbConnection);
                    EcuFunctionStructs.EcuFaultData ecuFaultData = new EcuFunctionStructs.EcuFaultData
                    {
                        DatabaseVersion = dbInfo.Version,
                        DatabaseDate = dbInfo.DateTime,
                        EcuFaultCodeLabelList = GetFaultCodeLabels(mDbConnection, language),
                        EcuFaultModeLabelList = GetFaultModeLabels(mDbConnection, language),
                        EcuEnvCondLabelList = GetEnvCondLabels(mDbConnection, language)
                    };
                    //logTextWriter?.WriteLine(ecuFaultData);

                    string xmlFile = Path.Combine(outDirSub, "faultdata_" + language + ".xml");
                    XmlSerializer serializer = new XmlSerializer(ecuFaultData.GetType());
                    XmlWriterSettings settings = new XmlWriterSettings
                    {
                        Indent = true,
                        IndentChars = "\t"
                    };
                    using (XmlWriter writer = XmlWriter.Create(xmlFile, settings))
                    {
                        serializer.Serialize(writer, ecuFaultData);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                outTextWriter?.WriteLine(e);
                return false;
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private static bool SerializeEcuFunction(TextWriter outTextWriter, TextWriter logTextWriter, string connection, string outDirSub, string ecuName)
        {
            try
            {
                using (SqliteConnection mDbConnection = new SqliteConnection(connection))
                {
                    mDbConnection.Open();

                    outTextWriter?.WriteLine("*** ECU: {0} ***", ecuName);
                    EcuFunctionStructs.EcuVariant ecuVariant = GetEcuVariantFunctions(outTextWriter, logTextWriter, mDbConnection, ecuName);

                    if (ecuVariant != null)
                    {
                        logTextWriter?.WriteLine(ecuVariant);

                        string xmlFile = Path.Combine(outDirSub, ecuName.ToLowerInvariant() + ".xml");
                        XmlSerializer serializer = new XmlSerializer(ecuVariant.GetType());
                        XmlWriterSettings settings = new XmlWriterSettings
                        {
                            Indent = true,
                            IndentChars = "\t"
                        };
                        using (XmlWriter writer = XmlWriter.Create(xmlFile, settings))
                        {
                            serializer.Serialize(writer, ecuVariant);
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                outTextWriter?.WriteLine(e);
                return false;
            }
        }

        private static bool InitGlobalData(string connection)
        {
            try
            {
                using (SqliteConnection mDbConnection = new SqliteConnection(connection))
                {
                    mDbConnection.Open();

                    long[] rootClassValues = (long[])Enum.GetValues(typeof(VehicleCharacteristic));
                    RootClassDict = new Dictionary<long, string>();
                    foreach (long rootClassValue in rootClassValues)
                    {
                        if (rootClassValue > 0)
                        {
                            string rootClassname = GetCharacteristicRootName(mDbConnection, rootClassValue);
                            if (!string.IsNullOrEmpty(rootClassname))
                            {
                                RootClassDict.Add(rootClassValue, rootClassname);
                            }
                        }
                    }

                    TypeKeyClassId = DatabaseFunctions.GetNodeClassId(mDbConnection, @"Typschluessel");
                    EnvDiscreteNodeClassId = DatabaseFunctions.GetNodeClassId(mDbConnection, "EnvironmentalConditionTextDiscrete");

                    if (string.IsNullOrEmpty(TypeKeyClassId))
                    {
                        return false;
                    }

                    if (string.IsNullOrEmpty(EnvDiscreteNodeClassId))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static List<string> GetEcuNameList(string connection)
        {
            try
            {
                List<string> ecuNameList;
                using (SqliteConnection mDbConnection = new SqliteConnection(connection))
                {
                    mDbConnection.Open();

                    ecuNameList = GetEcuNameList(mDbConnection);
                }

                return ecuNameList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static List<string> GetEcuNameList(SqliteConnection mDbConnection)
        {
            List<string> ecuNameList = new List<string>();
            string sql = @"SELECT NAME FROM XEP_ECUVARIANTS";
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ecuNameList.Add(reader["NAME"].ToString()?.Trim());
                    }
                }
            }

            return ecuNameList;
        }

        private static EcuFunctionStructs.EcuTranslation GetTranslation(SqliteDataReader reader, string prefix = "TITLE", string language = null)
        {
            return new EcuFunctionStructs.EcuTranslation(
                language == null || language.ToLowerInvariant() == "de" ? reader[prefix + "_DEDE"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "en" ? reader[prefix + "_ENUS"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "fr" ? reader[prefix + "_FR"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "th" ? reader[prefix + "_TH"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "sv" ? reader[prefix + "_SV"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "it" ? reader[prefix + "_IT"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "es" ? reader[prefix + "_ES"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "id" ? reader[prefix + "_ID"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "ko" ? reader[prefix + "_KO"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "el" ? reader[prefix + "_EL"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "tr" ? reader[prefix + "_TR"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "zh" ? reader[prefix + "_ZHCN"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "ru" ? reader[prefix + "_RU"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "nl" ? reader[prefix + "_NL"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "pt" ? reader[prefix + "_PT"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "ja" ? reader[prefix + "_JA"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "cs" ? reader[prefix + "_CSCZ"].ToString() : string.Empty,
                language == null || language.ToLowerInvariant() == "pl" ? reader[prefix + "_PLPL"].ToString() : string.Empty
                );
        }

        // from GetCharacteristicsByTypeKeyId
        private static bool ExtractTypeKeyClassInfo(TextWriter outTextWriter, string connection, long rootClassId, Dictionary<string, List<string>> typeKeyInfoList, int infoIndex)
        {
            try
            {
                using (SqliteConnection mDbConnection = new SqliteConnection(connection))
                {
                    mDbConnection.Open();

                    outTextWriter?.WriteLine("*** Extract TypeKeyInfo start ClassId={0} ***", rootClassId);
                    string sql = $"SELECT t.NAME AS TYPEKEY, c.NAME AS VALUE FROM XEP_CHARACTERISTICS t INNER JOIN XEP_VEHICLES v ON (v.TYPEKEYID = t.ID)" +
                                 $" INNER JOIN XEP_CHARACTERISTICS c ON (v.CHARACTERISTICID = c.ID) INNER JOIN XEP_CHARACTERISTICROOTS r ON" +
                                 $" (r.ID = c.PARENTID AND r.NODECLASS = {rootClassId}) WHERE t.NODECLASS = {TypeKeyClassId} ORDER BY TYPEKEY";
                    using (SqliteCommand command = mDbConnection.CreateCommand())
                    {
                        command.CommandText = sql;
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string typeKey = reader["TYPEKEY"].ToString();
                                string typeValue = reader["VALUE"].ToString();

                                if (string.IsNullOrEmpty(typeKey) || string.IsNullOrEmpty(typeValue))
                                {
                                    continue;
                                }

                                if (!typeKeyInfoList.TryGetValue(typeKey, out List<string> storageList))
                                {
                                    storageList = new List<string>();
                                    typeKeyInfoList.Add(typeKey, storageList);
                                }

                                while (storageList.Count < infoIndex + 1)
                                {
                                    storageList.Add(string.Empty);
                                }

                                storageList[infoIndex] = typeValue;
                            }
                        }
                    }
                }

                outTextWriter?.WriteLine("*** Write TypeKeyInfo done ***");

                return true;
            }
            catch (Exception e)
            {
                outTextWriter?.WriteLine(e);
                return false;
            }
        }

        private static string GetCharacteristicRootName(SqliteConnection mDbConnection, long rootClassId)
        {
            string rootName = null;
            string sql = $"SELECT TITLE_DEDE FROM XEP_CHARACTERISTICROOTS WHERE (NODECLASS = {rootClassId})";
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rootName = reader["TITLE_DEDE"].ToString()?.Trim();
                        break;
                    }
                }
            }

            return rootName;
        }

        private static string RemoveNonAsciiChars(string text)
        {
            try
            {
                return new ASCIIEncoding().GetString(Encoding.ASCII.GetBytes(text.ToCharArray()));
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static bool WriteTypeKeyClassInfo(TextWriter outTextWriter, Dictionary<string, List<string>> typeKeyInfoList, string outDirSub)
        {
            try
            {
                outTextWriter?.WriteLine("*** Write TypeKeyInfo start ***");
                string typeKeysFile = Path.Combine(outDirSub, "typekeyinfo.txt");
                int itemCount = 0;
                using (StreamWriter swTypeKeys = new StreamWriter(typeKeysFile, false, new UTF8Encoding(true), 0x1000))
                {
                    StringBuilder sbHeader = new StringBuilder();
                    foreach (KeyValuePair<long, string> rootClassPair in RootClassDict)
                    {
                        if (sbHeader.Length > 0)
                        {
                            sbHeader.Append("|");
                        }
                        else
                        {
                            sbHeader.Append("#");
                        }
                        sbHeader.Append(RemoveNonAsciiChars(rootClassPair.Value));
                        itemCount++;
                    }
                    swTypeKeys.WriteLine(sbHeader.ToString());

                    foreach (KeyValuePair<string, List<string>> typeKeyPair in typeKeyInfoList)
                    {
                        StringBuilder sbLine = new StringBuilder();
                        sbLine.Append(typeKeyPair.Key);
                        int items = 1;
                        foreach (string value in typeKeyPair.Value)
                        {
                            sbLine.Append("|");
                            sbLine.Append(value);
                            items++;
                        }

                        while (items < itemCount)
                        {
                            sbLine.Append("|");
                            items++;
                        }
                        swTypeKeys.WriteLine(sbLine.ToString());
                    }
                }

                outTextWriter?.WriteLine("*** Write TypeKeys done ***");

                return true;
            }
            catch (Exception e)
            {
                outTextWriter?.WriteLine(e);
                return false;
            }
        }

        private static bool WriteVinRanges(TextWriter outTextWriter, string connection, string outDirSub)
        {
            try
            {
                using (SqliteConnection mDbConnection = new SqliteConnection(connection))
                {
                    mDbConnection.Open();

                    outTextWriter?.WriteLine("*** Write VinRanges start ***");
                    string vinRangeSpecFile = null;
                    StreamWriter swVinrangesSpec = null;

                    try
                    {
                        string vinRangeFile = Path.Combine(outDirSub, "vinranges.txt");
                        using (StreamWriter swVinranges = new StreamWriter(vinRangeFile, false, new UTF8Encoding(true), 0x1000))
                        {
                            string sql = @"SELECT v.VINBANDFROM AS VINBANDFROM, v.VINBANDTO AS VINBANDTO, v.VIN17_4_7 AS VIN17_4_7, v.TYPSCHLUESSEL AS TYPEKEY" +
                                         @", v.PRODUCTIONDATEYEAR AS PRODYEAR, v.PRODUCTIONDATEMONTH AS PRODMONTH, v.RELEASESTATE AS RELEASESTATE, v.GEARBOX_TYPE AS GEARBOX_TYPE FROM VINRANGES v";
                            using (SqliteCommand command = mDbConnection.CreateCommand())
                            {
                                command.CommandText = sql;
                                using (SqliteDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        string vinbandFrom = reader["VINBANDFROM"].ToString();
                                        string vinbandTo = reader["VINBANDTO"].ToString();
                                        string line = vinbandFrom + "," + vinbandTo + "," + reader["VIN17_4_7"] + "," + reader["TYPEKEY"] + "," + reader["PRODYEAR"] + "," + reader["PRODMONTH"] +
                                                      "," + reader["RELEASESTATE"] + "," + reader["GEARBOX_TYPE"];

                                        if (!string.IsNullOrEmpty(vinbandFrom) && !string.IsNullOrEmpty(vinbandTo))
                                        {
                                            swVinranges.WriteLine(line);

                                            char bandStart = vinbandFrom.ToLowerInvariant()[0];
                                            char bandEnd = vinbandFrom.ToLowerInvariant()[0];
                                            if (bandStart != bandEnd)
                                            {
                                                outTextWriter?.WriteLine("*** Band range detected: {0}-{1}", vinbandFrom, vinbandTo);
                                            }

                                            for (char band = bandStart; band <= bandEnd; band++)
                                            {
                                                string vinRangeSpecFileNew = Path.Combine(outDirSub, $"vinranges_{band}.txt");

                                                if (string.IsNullOrEmpty(vinRangeSpecFile) || string.Compare(vinRangeSpecFile, vinRangeSpecFileNew, StringComparison.OrdinalIgnoreCase) != 0)
                                                {
                                                    if (swVinrangesSpec != null)
                                                    {
                                                        swVinrangesSpec.Dispose();
                                                        swVinrangesSpec = null;
                                                    }
                                                }

                                                if (swVinrangesSpec == null)
                                                {
                                                    if (File.Exists(vinRangeSpecFileNew))
                                                    {
                                                        swVinrangesSpec = new StreamWriter(vinRangeSpecFileNew, true, new UTF8Encoding(false), 0x1000);
                                                    }
                                                    else
                                                    {
                                                        swVinrangesSpec = new StreamWriter(vinRangeSpecFileNew, false, new UTF8Encoding(true), 0x1000);
                                                    }
                                                    vinRangeSpecFile = vinRangeSpecFileNew;
                                                }

                                                swVinrangesSpec.WriteLine(line);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (swVinrangesSpec != null)
                        {
                            swVinrangesSpec.Dispose();
                        }
                    }
                }

                outTextWriter?.WriteLine("*** Write VinRanges done ***");

                return true;
            }
            catch (Exception e)
            {
                outTextWriter?.WriteLine(e);
                return false;
            }
        }

        private static DbInfo GetDbInfo(SqliteConnection mDbConnection)
        {
            DbInfo dbInfo = null;
            string sql = @"SELECT VERSION, CREATIONDATE FROM RG_VERSION";
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string version = reader["VERSION"].ToString()?.Trim();
                        DateTime dateTime = reader.GetDateTime(1);
                        dbInfo = new DbInfo(version, dateTime);
                        break;
                    }
                }
            }

            return dbInfo;
        }

        private static EcuFunctionStructs.EcuVariant GetEcuVariant(SqliteConnection mDbConnection, string ecuName)
        {
            EcuFunctionStructs.EcuVariant ecuVariant = null;
            string name = ecuName.ToLowerInvariant();
            if (string.Compare(ecuName, "ews3p", StringComparison.OrdinalIgnoreCase) == 0)
            {
                name = "ews3";
            }

            string sql = string.Format(@"SELECT ID, " + DatabaseFunctions.SqlTitleItems + ", ECUGROUPID FROM XEP_ECUVARIANTS WHERE (lower(NAME) = '{0}')", name);
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string groupId = reader["ECUGROUPID"].ToString()?.Trim();
                        ecuVariant = new EcuFunctionStructs.EcuVariant(reader["ID"].ToString()?.Trim(),
                            groupId,
                            GetEcuGroupName(mDbConnection, groupId),
                            GetTranslation(reader),
                            GetEcuGroupFunctionIds(mDbConnection, groupId));
                    }
                }
            }

            if (ecuVariant != null)
            {
                EcuFunctionStructs.EcuClique ecuClique = FindEcuClique(mDbConnection, ecuVariant);
                if (ecuClique != null)
                {
                    ecuVariant.EcuClique = ecuClique;
                }
            }
            return ecuVariant;
        }

        private static List<EcuFunctionStructs.EcuFaultCode> GetFaultCodes(SqliteConnection mDbConnection, string variantId)
        {
            List<EcuFunctionStructs.EcuFaultCode> ecuFaultCodeList = new List<EcuFunctionStructs.EcuFaultCode>();
            // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetXepFaultCodeByEcuVariantId
            string sql = string.Format(@"SELECT ID, CODE, DATATYPE, RELEVANCE FROM XEP_FAULTCODES WHERE ECUVARIANTID = {0}", variantId);
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        EcuFunctionStructs.EcuFaultCode ecuFaultCode = new EcuFunctionStructs.EcuFaultCode(
                            reader["ID"].ToString()?.Trim(),
                            reader["CODE"].ToString(),
                            reader["DATATYPE"].ToString(),
                            reader["RELEVANCE"].ToString());
                        ecuFaultCodeList.Add(ecuFaultCode);
                        EcuFunctionStructs.EcuFaultCodeLabel ecuFaultCodeLabel = GetFaultCodeLabel(mDbConnection, ecuFaultCode);
                        List<EcuFunctionStructs.EcuFaultModeLabel> ecuFaultModeLabelList = GetFaultModeLabelList(mDbConnection, ecuFaultCode);
                        List<EcuFunctionStructs.EcuEnvCondLabel> ecuEnvCondLabelList = GetEnvCondLabelList(mDbConnection, ecuFaultCode, variantId);

                        string ecuFaultLabelId = string.Empty;
                        if (ecuFaultCodeLabel != null)
                        {
                            ecuFaultLabelId = ecuFaultCodeLabel.Id;
                            lock (FaultCodeLabelIdHashSet)
                            {
                                FaultCodeLabelIdHashSet.Add(ecuFaultCodeLabel.Id);
                            }
                        }

                        List<string> ecuFaultModeLabelIdList = new List<string>();
                        if (ecuFaultModeLabelList != null)
                        {
                            foreach (EcuFunctionStructs.EcuFaultModeLabel ecuFaultModeLabel in ecuFaultModeLabelList)
                            {
                                ecuFaultModeLabelIdList.Add(ecuFaultModeLabel.Id);
                                lock (FaultModeLabelIdHashSet)
                                {
                                    FaultModeLabelIdHashSet.Add(ecuFaultModeLabel.Id);
                                }
                            }
                        }

                        List<string> ecuEnvCondLabelIdList = new List<string>();
                        if (ecuEnvCondLabelList != null)
                        {
                            foreach (EcuFunctionStructs.EcuEnvCondLabel ecuEnvCondLabel in ecuEnvCondLabelList)
                            {
                                ecuEnvCondLabelIdList.Add(ecuEnvCondLabel.Id);
                                lock (EnvCondLabelIdHashSet)
                                {
                                    EnvCondLabelIdHashSet.Add(ecuEnvCondLabel.Id);
                                }
                            }
                        }

                        ecuFaultCode.EcuFaultCodeLabelId = ecuFaultLabelId;
                        ecuFaultCode.EcuFaultCodeLabel = ecuFaultCodeLabel;
                        ecuFaultCode.EcuFaultModeLabelList = ecuFaultModeLabelList;
                        ecuFaultCode.EcuFaultModeLabelIdList = ecuFaultModeLabelIdList;
                        ecuFaultCode.EcuEnvCondLabelList = ecuEnvCondLabelList;
                        ecuFaultCode.EcuEnvCondLabelIdList = ecuEnvCondLabelIdList;
                    }
                }
            }

            return ecuFaultCodeList;
        }

        // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetFaultLabelXepFaultLabel
        private static List<EcuFunctionStructs.EcuFaultCodeLabel> GetFaultCodeLabels(SqliteConnection mDbConnection, string language)
        {
            List<EcuFunctionStructs.EcuFaultCodeLabel> ecuFaultCodeLabelList = new List<EcuFunctionStructs.EcuFaultCodeLabel>();
            string sql = @"SELECT ID LABELID, CODE, SAECODE, " + DatabaseFunctions.SqlTitleItems + ", RELEVANCE, DATATYPE " +
                         @"FROM XEP_FAULTLABELS";
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string labelId = reader["LABELID"].ToString()?.Trim();
                        bool addItem;
                        lock (FaultCodeLabelIdHashSet)
                        {
                            addItem = FaultCodeLabelIdHashSet.Contains(labelId);
                        }

                        if (addItem)
                        {
                            ecuFaultCodeLabelList.Add(new EcuFunctionStructs.EcuFaultCodeLabel(labelId,
                                reader["CODE"].ToString(),
                                reader["SAECODE"].ToString(),
                                GetTranslation(reader, "TITLE", language),
                                reader["RELEVANCE"].ToString(),
                                reader["DATATYPE"].ToString()));
                        }
                    }
                }
            }

            return ecuFaultCodeLabelList;
        }

        // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetFaultLabelXepFaultLabel
        private static EcuFunctionStructs.EcuFaultCodeLabel GetFaultCodeLabel(SqliteConnection mDbConnection, EcuFunctionStructs.EcuFaultCode ecuFaultCode)
        {
            EcuFunctionStructs.EcuFaultCodeLabel ecuFaultCodeLabel = null;
            string sql = string.Format(@"SELECT LABELS.ID LABELID, CODE, SAECODE, " + DatabaseFunctions.SqlTitleItems + ", RELEVANCE, DATATYPE " +
                                       @"FROM XEP_FAULTLABELS LABELS, XEP_REFFAULTLABELS REFLABELS" +
                                       @" WHERE CODE = {0} AND LABELS.ID = REFLABELS.LABELID AND REFLABELS.ID = {1}", ecuFaultCode.Code, ecuFaultCode.Id);
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ecuFaultCodeLabel = new EcuFunctionStructs.EcuFaultCodeLabel(reader["LABELID"].ToString()?.Trim(),
                            reader["CODE"].ToString(),
                            reader["SAECODE"].ToString(),
                            GetTranslation(reader),
                            reader["RELEVANCE"].ToString(),
                            reader["DATATYPE"].ToString());
                        break;
                    }
                }
            }

            return ecuFaultCodeLabel;
        }

        // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetFaultModeLabelById
        private static List<EcuFunctionStructs.EcuFaultModeLabel> GetFaultModeLabels(SqliteConnection mDbConnection, string language)
        {
            List<EcuFunctionStructs.EcuFaultModeLabel> ecuFaultModeLabelList = new List<EcuFunctionStructs.EcuFaultModeLabel>();
            string sql = @"SELECT ID LABELID, CODE, " + DatabaseFunctions.SqlTitleItems + ", RELEVANCE, ERWEITERT " +
                         @"FROM XEP_FAULTMODELABELS ORDER BY LABELID";
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string labelId = reader["LABELID"].ToString()?.Trim();
                        bool addItem;
                        lock (FaultModeLabelIdHashSet)
                        {
                            addItem = FaultModeLabelIdHashSet.Contains(labelId);
                        }

                        if (addItem)
                        {
                            ecuFaultModeLabelList.Add(new EcuFunctionStructs.EcuFaultModeLabel(labelId,
                                reader["CODE"].ToString(),
                                GetTranslation(reader, "TITLE", language),
                                reader["RELEVANCE"].ToString(),
                                reader["ERWEITERT"].ToString()));
                        }
                    }
                }
            }

            return ecuFaultModeLabelList;
        }

        // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetFaultModeLabelById
        private static List<EcuFunctionStructs.EcuFaultModeLabel> GetFaultModeLabelList(SqliteConnection mDbConnection, EcuFunctionStructs.EcuFaultCode ecuFaultCode)
        {
            List<EcuFunctionStructs.EcuFaultModeLabel> ecuFaultModeLabelList = new List<EcuFunctionStructs.EcuFaultModeLabel>();
            string sql = string.Format(@"SELECT LABELS.ID LABELID, CODE, " + DatabaseFunctions.SqlTitleItems + ", RELEVANCE, ERWEITERT " +
                                       @"FROM XEP_FAULTMODELABELS LABELS, XEP_REFFAULTLABELS REFLABELS" +
                                       @" WHERE LABELS.ID = REFLABELS.LABELID AND REFLABELS.ID = {0} ORDER BY LABELID", ecuFaultCode.Id);
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ecuFaultModeLabelList.Add(new EcuFunctionStructs.EcuFaultModeLabel(reader["LABELID"].ToString()?.Trim(),
                            reader["CODE"].ToString(),
                            GetTranslation(reader),
                            reader["RELEVANCE"].ToString(),
                            reader["ERWEITERT"].ToString()));
                    }
                }
            }

            return ecuFaultModeLabelList;
        }

        // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetEnvCondLabels
        private static List<EcuFunctionStructs.EcuEnvCondLabel> GetEnvCondLabels(SqliteConnection mDbConnection, string language)
        {
            List<EcuFunctionStructs.EcuEnvCondLabel> ecuEnvCondLabelList = new List<EcuFunctionStructs.EcuEnvCondLabel>();
            string sql = @"SELECT ID, NODECLASS, " + DatabaseFunctions.SqlTitleItems + ", RELEVANCE, BLOCKANZAHL, UWIDENTTYP, UWIDENT, UNIT " +
                         @"FROM XEP_ENVCONDSLABELS ORDER BY ID";
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string labelId = reader["ID"].ToString()?.Trim();
                        bool addItem;
                        lock (EnvCondLabelIdHashSet)
                        {
                            addItem = EnvCondLabelIdHashSet.Contains(labelId);
                        }

                        if (addItem)
                        {
                            ecuEnvCondLabelList.Add(new EcuFunctionStructs.EcuEnvCondLabel(labelId,
                                reader["NODECLASS"].ToString(),
                                GetTranslation(reader, "TITLE", language),
                                reader["RELEVANCE"].ToString(),
                                reader["BLOCKANZAHL"].ToString(),
                                reader["UWIDENTTYP"].ToString(),
                                reader["UWIDENT"].ToString(),
                                reader["UNIT"].ToString()));
                        }
                    }
                }
            }

            foreach (EcuFunctionStructs.EcuEnvCondLabel ecuEnvCondLabel in ecuEnvCondLabelList)
            {
                if (string.Compare(ecuEnvCondLabel.NodeClass, EnvDiscreteNodeClassId, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ecuEnvCondLabel.EcuResultStateValueList = GetResultStateValueList(mDbConnection, ecuEnvCondLabel.Id, language);
                }
            }

            return ecuEnvCondLabelList;
        }

        // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetEnvCondLabels
        private static List<EcuFunctionStructs.EcuEnvCondLabel> GetEnvCondLabelList(SqliteConnection mDbConnection,
            EcuFunctionStructs.EcuFaultCode ecuFaultCode, string variantId)
        {
            List<EcuFunctionStructs.EcuEnvCondLabel> ecuEnvCondLabelList = new List<EcuFunctionStructs.EcuEnvCondLabel>();
            string sql = string.Format(@"SELECT ID, NODECLASS, " + DatabaseFunctions.SqlTitleItems + ", RELEVANCE, BLOCKANZAHL, UWIDENTTYP, UWIDENT, UNIT " +
                       @"FROM XEP_ENVCONDSLABELS" +
                       @" WHERE ID IN (SELECT LABELID FROM XEP_REFFAULTLABELS, XEP_FAULTCODES WHERE CODE = {0} AND ECUVARIANTID = {1} AND XEP_REFFAULTLABELS.ID = XEP_FAULTCODES.ID) ORDER BY ID",
                        ecuFaultCode.Code, variantId);
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ecuEnvCondLabelList.Add(new EcuFunctionStructs.EcuEnvCondLabel(reader["ID"].ToString()?.Trim(),
                            reader["NODECLASS"].ToString(),
                            GetTranslation(reader),
                            reader["RELEVANCE"].ToString(),
                            reader["BLOCKANZAHL"].ToString(),
                            reader["UWIDENTTYP"].ToString(),
                            reader["UWIDENT"].ToString(),
                            reader["UNIT"].ToString()));
                    }
                }
            }

            foreach (EcuFunctionStructs.EcuEnvCondLabel ecuEnvCondLabel in ecuEnvCondLabelList)
            {
                if (string.Compare(ecuEnvCondLabel.NodeClass, EnvDiscreteNodeClassId, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ecuEnvCondLabel.EcuResultStateValueList = GetResultStateValueList(mDbConnection, ecuEnvCondLabel.Id);
                }
            }

            return ecuEnvCondLabelList;
        }

        private static EcuFunctionStructs.EcuClique FindEcuClique(SqliteConnection mDbConnection, EcuFunctionStructs.EcuVariant ecuVariant)
        {
            if (ecuVariant == null)
            {
                return null;
            }

            string cliqueId = GetRefEcuCliqueId(mDbConnection, ecuVariant.Id);
            if (string.IsNullOrEmpty(cliqueId))
            {
                return null;
            }

            return GetEcuCliqueById(mDbConnection, cliqueId);
        }

        private static EcuFunctionStructs.EcuClique GetEcuCliqueById(SqliteConnection mDbConnection, string ecuCliqueId)
        {
            if (string.IsNullOrEmpty(ecuCliqueId))
            {
                return null;
            }

            EcuFunctionStructs.EcuClique ecuClique = null;
            string sql = string.Format(@"SELECT ID, CLIQUENKURZBEZEICHNUNG, ECUREPID FROM XEP_ECUCLIQUES WHERE (ID = {0})", ecuCliqueId);
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ecuClique = new EcuFunctionStructs.EcuClique(reader["ID"].ToString()?.Trim(),
                            reader["CLIQUENKURZBEZEICHNUNG"].ToString()?.Trim(),
                            reader["ECUREPID"].ToString()?.Trim());
                    }
                }
            }

            if (ecuClique != null)
            {
                string ecuRepsName = GetEcuRepsNameById(mDbConnection, ecuClique.EcuRepId);
                if (!string.IsNullOrEmpty(ecuRepsName))
                {
                    ecuClique.EcuRepsName = ecuRepsName;
                }
            }

            return ecuClique;
        }

        private static string GetRefEcuCliqueId(SqliteConnection mDbConnection, string ecuRefId)
        {
            if (string.IsNullOrEmpty(ecuRefId))
            {
                return null;
            }

            string cliqueId = null;
            string sql = string.Format(@"SELECT ECUCLIQUEID FROM XEP_REFECUCLIQUES WHERE (ID = {0})", ecuRefId);
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cliqueId = reader["ECUCLIQUEID"].ToString()?.Trim();
                    }
                }
            }

            return cliqueId;
        }

        public static string GetEcuRepsNameById(SqliteConnection mDbConnection, string ecuId)
        {
            if (string.IsNullOrEmpty(ecuId))
            {
                return null;
            }

            string ecuRepsName = null;
            string sql = string.Format(@"SELECT STEUERGERAETEKUERZEL FROM XEP_ECUREPS WHERE (ID = {0})", ecuId);
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ecuRepsName = reader["STEUERGERAETEKUERZEL"].ToString()?.Trim();
                    }
                }
            }

            return ecuRepsName;
        }


        // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetEcuGroupFunctionsByEcuGroupId
        private static List<string> GetEcuGroupFunctionIds(SqliteConnection mDbConnection, string groupId)
        {
            List<string> ecuGroupFunctionIds = new List<string>();
            string sql = string.Format(@"SELECT ID FROM XEP_ECUGROUPFUNCTIONS WHERE ECUGROUPID = {0}", groupId);
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ecuGroupFunctionIds.Add(reader["ID"].ToString()?.Trim());
                    }
                }
            }

            return ecuGroupFunctionIds;
        }

        private static string GetEcuGroupName(SqliteConnection mDbConnection, string groupId)
        {
            string ecuGroupName = null;
            string sql = string.Format(@"SELECT NAME FROM XEP_ECUGROUPS WHERE ID = {0}", groupId);
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ecuGroupName = reader["NAME"].ToString()?.Trim();
                    }
                }
            }

            return ecuGroupName;
        }

        // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetXepNodeClassNameById
        private static string GetNodeClassName(SqliteConnection mDbConnection, string nodeClass)
        {
            string result = string.Empty;
            string sql = string.Format(@"SELECT NAME FROM XEP_NODECLASSES WHERE ID = {0}", nodeClass);
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = reader["NAME"].ToString()?.Trim();
                    }
                }
            }

            return result;
        }

        private static List<EcuFunctionStructs.EcuJob> GetFixedFuncStructJobsList(SqliteConnection mDbConnection, EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct)
        {
            List<EcuFunctionStructs.EcuJob> ecuJobList = new List<EcuFunctionStructs.EcuJob>();
            // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetEcuJobsWithParameters
            string sql = string.Format(@"SELECT JOBS.ID JOBID, FUNCTIONNAMEJOB, NAME, PHASE, RANK " +
                                       "FROM XEP_ECUJOBS JOBS, XEP_REFECUJOBS REFJOBS WHERE JOBS.ID = REFJOBS.ECUJOBID AND REFJOBS.ID = {0}", ecuFixedFuncStruct.Id);
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ecuJobList.Add(new EcuFunctionStructs.EcuJob(reader["JOBID"].ToString()?.Trim(),
                            reader["FUNCTIONNAMEJOB"].ToString()?.Trim(),
                            reader["NAME"].ToString()?.Trim(),
                            reader["PHASE"].ToString(),
                            reader["RANK"].ToString()));
                    }
                }
            }

            foreach (EcuFunctionStructs.EcuJob ecuJob in ecuJobList)
            {
                // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetEcuParameters
                List<EcuFunctionStructs.EcuJobParameter> ecuJobParList = new List<EcuFunctionStructs.EcuJobParameter>();
                sql = string.Format(
                    @"SELECT PARAM.ID PARAMID, PARAMVALUE, FUNCTIONNAMEPARAMETER, ADAPTERPATH, NAME, ECUJOBID " +
                    "FROM XEP_ECUPARAMETERS PARAM, XEP_REFECUPARAMETERS REFPARAM WHERE " +
                    "PARAM.ID = REFPARAM.ECUPARAMETERID AND REFPARAM.ID = {0} AND PARAM.ECUJOBID = {1}", ecuFixedFuncStruct.Id, ecuJob.Id);
                using (SqliteCommand command = mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ecuJobParList.Add(new EcuFunctionStructs.EcuJobParameter(reader["PARAMID"].ToString()?.Trim(),
                                reader["PARAMVALUE"].ToString()?.Trim(),
                                reader["ADAPTERPATH"].ToString(),
                                reader["NAME"].ToString()?.Trim()));
                        }
                    }
                }

                ecuJob.EcuJobParList = ecuJobParList;

                List<EcuFunctionStructs.EcuJobResult> ecuJobResultList = new List<EcuFunctionStructs.EcuJobResult>();
                // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetEcuResults
                sql = string.Format(
                    @"SELECT RESULTS.ID RESULTID, " + DatabaseFunctions.SqlTitleItems + ", FUNCTIONNAMERESULT, ADAPTERPATH, NAME, STEUERGERAETEFUNKTIONENRELEVAN, LOCATION, UNIT, UNITFIXED, FORMAT, MULTIPLIKATOR, OFFSET, RUNDEN, ZAHLENFORMAT, ECUJOBID " +
                    "FROM XEP_ECURESULTS RESULTS, XEP_REFECURESULTS REFRESULTS WHERE " +
                    "ECURESULTID = RESULTS.ID AND REFRESULTS.ID = {0} AND RESULTS.ECUJOBID = {1}", ecuFixedFuncStruct.Id, ecuJob.Id);
                using (SqliteCommand command = mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EcuFunctionStructs.EcuJobResult ecuJobResult = new EcuFunctionStructs.EcuJobResult(
                                reader["RESULTID"].ToString()?.Trim(),
                                GetTranslation(reader),
                                reader["FUNCTIONNAMERESULT"].ToString()?.Trim(),
                                reader["ADAPTERPATH"].ToString(),
                                reader["NAME"].ToString()?.Trim(),
                                reader["STEUERGERAETEFUNKTIONENRELEVAN"].ToString(),
                                reader["LOCATION"].ToString(),
                                reader["UNIT"].ToString(),
                                reader["UNITFIXED"].ToString(),
                                reader["FORMAT"].ToString(),
                                reader["MULTIPLIKATOR"].ToString(),
                                reader["OFFSET"].ToString(),
                                reader["RUNDEN"].ToString(),
                                reader["ZAHLENFORMAT"].ToString());

                            if (ecuJobResult.EcuFuncRelevant.ConvertToInt() > 0)
                            {
                                ecuJobResultList.Add(ecuJobResult);
                            }
                        }
                    }
                }

                foreach (EcuFunctionStructs.EcuJobResult ecuJobResult in ecuJobResultList)
                {
                    ecuJobResult.EcuResultStateValueList = GetResultStateValueList(mDbConnection, ecuJobResult.Id);
                }

                ecuJob.EcuJobResultList = ecuJobResultList;
            }

            return ecuJobList;
        }

        // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetEcuResultStateValues
        private static List<EcuFunctionStructs.EcuResultStateValue> GetResultStateValueList(SqliteConnection mDbConnection, string id, string language = null)
        {
            List<EcuFunctionStructs.EcuResultStateValue> ecuResultStateValueList = new List<EcuFunctionStructs.EcuResultStateValue>();
            string sql = string.Format(@"SELECT ID, " + DatabaseFunctions.SqlTitleItems + ", STATEVALUE, VALIDFROM, VALIDTO, PARENTID " +
                                       "FROM XEP_STATEVALUES WHERE (PARENTID IN (SELECT STATELISTID FROM XEP_REFSTATELISTS WHERE (ID = {0})))", id);
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ecuResultStateValueList.Add(new EcuFunctionStructs.EcuResultStateValue(reader["ID"].ToString()?.Trim(),
                            GetTranslation(reader, "TITLE", language),
                            reader["STATEVALUE"].ToString(),
                            reader["VALIDFROM"].ToString(),
                            reader["VALIDTO"].ToString(),
                            reader["PARENTID"].ToString()?.Trim()));
                    }
                }
            }

            return ecuResultStateValueList;
        }

        // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetEcuFixedFunctionsByParentId
        private static List<EcuFunctionStructs.EcuFixedFuncStruct> GetEcuFixedFuncStructList(SqliteConnection mDbConnection, string parentId)
        {
            List<EcuFunctionStructs.EcuFixedFuncStruct> ecuFixedFuncStructList = new List<EcuFunctionStructs.EcuFixedFuncStruct>();
            string sql = string.Format(@"SELECT ID, NODECLASS, " + DatabaseFunctions.SqlTitleItems + ", " +
                                       SqlPreOpItems + ", " + SqlProcItems + ", " + SqlPostOpItems + ", " +
                                       "SORT_ORDER, ACTIVATION, ACTIVATION_DURATION_MS " +
                                       "FROM XEP_ECUFIXEDFUNCTIONS WHERE (PARENTID = {0})", parentId);
            using (SqliteCommand command = mDbConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string nodeClass = reader["NODECLASS"].ToString();
                        EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct = new EcuFunctionStructs.EcuFixedFuncStruct(reader["ID"]?.ToString()?.Trim(),
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

        // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetEcuFixedFunctionsForEcuVariant
        private static EcuFunctionStructs.EcuVariant GetEcuVariantFunctions(TextWriter outTextWriter, TextWriter logTextWriter, SqliteConnection mDbConnection, string ecuName)
        {
            EcuFunctionStructs.EcuVariant ecuVariant = GetEcuVariant(mDbConnection, ecuName);
            if (ecuVariant == null)
            {
                outTextWriter?.WriteLine("ECU variant not found");
                return null;
            }

            ecuVariant.EcuFaultCodeList = GetFaultCodes(mDbConnection, ecuVariant.Id);
            int faultCodeCount = 0;
            if (ecuVariant.EcuFaultCodeList != null)
            {
                faultCodeCount = ecuVariant.EcuFaultCodeList.Count;
            }

            List<EcuFunctionStructs.RefEcuVariant> refEcuVariantList = new List<EcuFunctionStructs.RefEcuVariant>();
            {
                string sql = string.Format(@"SELECT ID, ECUVARIANTID FROM XEP_REFECUVARIANTS WHERE ECUVARIANTID = {0}", ecuVariant.Id);
                using (SqliteCommand command = mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            refEcuVariantList.Add(new EcuFunctionStructs.RefEcuVariant(reader["ID"].ToString(),
                                reader["ECUVARIANTID"].ToString()?.Trim()));
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
                // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetEcuVariantFunctionByNameAndEcuGroupFunctionId
                string sql = string.Format(@"SELECT ID, VISIBLE, NAME, OBD_RELEVANZ FROM XEP_ECUVARFUNCTIONS WHERE (lower(NAME) = '{0}') AND (ECUGROUPFUNCTIONID = {1})", ecuName.ToLowerInvariant(), ecuGroupFunctionId);
                using (SqliteCommand command = mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ecuVarFunctionsList.Add(new EcuFunctionStructs.EcuVarFunc(reader["ID"].ToString()?.Trim(), ecuGroupFunctionId));
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
                // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetEcuFunctionStructureById
                string sql = string.Format(@"SELECT REFFUNCS.ECUFUNCSTRUCTID FUNCSTRUCTID, NODECLASS, " + DatabaseFunctions.SqlTitleItems + ", MULTISELECTION, PARENTID, SORT_ORDER " +
                        "FROM XEP_ECUFUNCSTRUCTURES FUNCS, XEP_REFECUFUNCSTRUCTS REFFUNCS WHERE FUNCS.ID = REFFUNCS.ECUFUNCSTRUCTID AND REFFUNCS.ID = {0}", ecuVarFunc.Id);
                using (SqliteCommand command = mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string nodeClass = reader["NODECLASS"].ToString();
                            ecuFuncStructList.Add(new EcuFunctionStructs.EcuFuncStruct(reader["FUNCSTRUCTID"].ToString()?.Trim(),
                                nodeClass,
                                GetNodeClassName(mDbConnection, nodeClass),
                                GetTranslation(reader),
                                reader["MULTISELECTION"].ToString(),
                                reader["PARENTID"].ToString(),
                                reader["SORT_ORDER"].ToString()));
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

            if (fixFuncCount == 0 && faultCodeCount == 0)
            {
                outTextWriter?.WriteLine("No ECU fix functions or fault codes found");
                return null;
            }

            ecuVariant.EcuFuncStructList = ecuFuncStructList;

            return ecuVariant;
        }

        private static bool CreateZipFile(string inDir, string outFile, string key = null)
        {
            try
            {
                Aes crypto = null;
                FileStream fsOut = null;
                ZipOutputStream zipStream = null;
                try
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        crypto = Aes.Create();
                        crypto.Mode = CipherMode.CBC;
                        crypto.Padding = PaddingMode.PKCS7;
                        crypto.KeySize = 256;

                        using (SHA256 sha256 = SHA256.Create())
                        {
                            crypto.Key = sha256.ComputeHash(Encoding.ASCII.GetBytes(key));
                        }
                        using (MD5 md5 = MD5.Create())
                        {
                            crypto.IV = md5.ComputeHash(Encoding.ASCII.GetBytes(key));
                        }
                    }

                    fsOut = File.Create(outFile);
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (crypto != null)
                    {
                        CryptoStream crStream = new CryptoStream(fsOut, crypto.CreateEncryptor(), CryptoStreamMode.Write);
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
