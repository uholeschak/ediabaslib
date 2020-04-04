using System;
using System.Collections.Generic;
using System.Data.SQLite;
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

namespace ExtractEcuFunctions
{
    static class Program
    {
        const string DbPassword = "6505EFBDC3E5F324";

        private static List<string> LangList = new List<string>
        {
            "de", "en", "fr", "th",
            "sv", "it", "es", "id",
            "ko", "el", "tr", "zh",
            "ru", "nl", "pt", "ja",
            "cs", "pl",
        };

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

        private static readonly HashSet<string> FaultCodeLabelIdHashSet = new HashSet<string>();
        private static readonly HashSet<string> FaultModeLabelIdHashSet = new HashSet<string>();
        private static readonly HashSet<string> EnvCondLabelIdHashSet = new HashSet<string>();
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

                string connection = "Data Source=\"" + args[0] + "\";";
                if (!InitGlobalData(connection))
                {
                    outTextWriter?.WriteLine("Init failed");
                    return 1;
                }

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
        // ReSharper disable once UnusedParameter.Local
        private static bool SerializeEcuFaultData(TextWriter outTextWriter, TextWriter logTextWriter, string connection, string outDirSub, string language)
        {
            try
            {
                using (SQLiteConnection mDbConnection = new SQLiteConnection(connection))
                {
                    mDbConnection.SetPassword(DbPassword);
                    mDbConnection.Open();

                    outTextWriter?.WriteLine("*** Fault data {0} ***", language);
                    EcuFunctionStructs.EcuFaultData ecuFaultData = new EcuFunctionStructs.EcuFaultData
                    {
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

        private static bool InitGlobalData(string connection)
        {
            try
            {
                using (SQLiteConnection mDbConnection = new SQLiteConnection(connection))
                {
                    mDbConnection.SetPassword(DbPassword);
                    mDbConnection.Open();

                    EnvDiscreteNodeClassId = DatabaseFunctions.GetNodeClassId(mDbConnection, "EnvironmentalConditionTextDiscrete");

                    mDbConnection.Close();

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

        private static EcuFunctionStructs.EcuTranslation GetTranslation(SQLiteDataReader reader, string prefix = "TITLE", string language = null)
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

        private static List<EcuFunctionStructs.EcuFaultCode> GetFaultCodes(SQLiteConnection mDbConnection, string variantId)
        {
            List<EcuFunctionStructs.EcuFaultCode> ecuFaultCodeList = new List<EcuFunctionStructs.EcuFaultCode>();
            string sql = string.Format(@"SELECT ID, CODE FROM XEP_FAULTCODES WHERE ECUVARIANTID = {0}", variantId);
            using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        EcuFunctionStructs.EcuFaultCode ecuFaultCode = new EcuFunctionStructs.EcuFaultCode(
                            reader["ID"].ToString(),
                            reader["CODE"].ToString());
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

        private static List<EcuFunctionStructs.EcuFaultCodeLabel> GetFaultCodeLabels(SQLiteConnection mDbConnection, string language)
        {
            List<EcuFunctionStructs.EcuFaultCodeLabel> ecuFaultCodeLabelList = new List<EcuFunctionStructs.EcuFaultCodeLabel>();
            string sql = @"SELECT ID LABELID, CODE, SAECODE, " + SqlTitleItems + ", RELEVANCE, DATATYPE " +
                         @"FROM XEP_FAULTLABELS";
            using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string labelId = reader["LABELID"].ToString();
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

        private static EcuFunctionStructs.EcuFaultCodeLabel GetFaultCodeLabel(SQLiteConnection mDbConnection, EcuFunctionStructs.EcuFaultCode ecuFaultCode)
        {
            EcuFunctionStructs.EcuFaultCodeLabel ecuFaultCodeLabel = null;
            string sql = string.Format(@"SELECT LABELS.ID LABELID, CODE, SAECODE, " + SqlTitleItems + ", RELEVANCE, DATATYPE " +
                                       @"FROM XEP_FAULTLABELS LABELS, XEP_REFFAULTLABELS REFLABELS" +
                                       @" WHERE CODE = {0} AND LABELS.ID = REFLABELS.LABELID AND REFLABELS.ID = {1}", ecuFaultCode.Code, ecuFaultCode.Id);
            using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ecuFaultCodeLabel = new EcuFunctionStructs.EcuFaultCodeLabel(reader["LABELID"].ToString(),
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

        private static List<EcuFunctionStructs.EcuFaultModeLabel> GetFaultModeLabels(SQLiteConnection mDbConnection, string language)
        {
            List<EcuFunctionStructs.EcuFaultModeLabel> ecuFaultModeLabelList = new List<EcuFunctionStructs.EcuFaultModeLabel>();
            string sql = @"SELECT ID LABELID, CODE, " + SqlTitleItems + ", RELEVANCE, ERWEITERT " +
                         @"FROM XEP_FAULTMODELABELS ORDER BY LABELID";
            using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string labelId = reader["LABELID"].ToString();
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

        private static List<EcuFunctionStructs.EcuFaultModeLabel> GetFaultModeLabelList(SQLiteConnection mDbConnection, EcuFunctionStructs.EcuFaultCode ecuFaultCode)
        {
            List<EcuFunctionStructs.EcuFaultModeLabel> ecuFaultModeLabelList = new List<EcuFunctionStructs.EcuFaultModeLabel>();
            string sql = string.Format(@"SELECT LABELS.ID LABELID, CODE, " + SqlTitleItems + ", RELEVANCE, ERWEITERT " +
                                       @"FROM XEP_FAULTMODELABELS LABELS, XEP_REFFAULTLABELS REFLABELS" +
                                       @" WHERE LABELS.ID = REFLABELS.LABELID AND REFLABELS.ID = {0} ORDER BY LABELID", ecuFaultCode.Id);
            using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ecuFaultModeLabelList.Add(new EcuFunctionStructs.EcuFaultModeLabel(reader["LABELID"].ToString(),
                            reader["CODE"].ToString(),
                            GetTranslation(reader),
                            reader["RELEVANCE"].ToString(),
                            reader["ERWEITERT"].ToString()));
                    }
                }
            }

            return ecuFaultModeLabelList;
        }

        private static List<EcuFunctionStructs.EcuEnvCondLabel> GetEnvCondLabels(SQLiteConnection mDbConnection, string language)
        {
            List<EcuFunctionStructs.EcuEnvCondLabel> ecuEnvCondLabelList = new List<EcuFunctionStructs.EcuEnvCondLabel>();
            string sql = @"SELECT ID, NODECLASS, " + SqlTitleItems + ", RELEVANCE, BLOCKANZAHL, UWIDENTTYP, UWIDENT, UNIT " +
                         @"FROM XEP_ENVCONDSLABELS ORDER BY ID";
            using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string labelId = reader["ID"].ToString();
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

        private static List<EcuFunctionStructs.EcuEnvCondLabel> GetEnvCondLabelList(SQLiteConnection mDbConnection,
            EcuFunctionStructs.EcuFaultCode ecuFaultCode, string variantId)
        {
            List<EcuFunctionStructs.EcuEnvCondLabel> ecuEnvCondLabelList = new List<EcuFunctionStructs.EcuEnvCondLabel>();
            string sql = string.Format(@"SELECT ID, NODECLASS, " + SqlTitleItems + ", RELEVANCE, BLOCKANZAHL, UWIDENTTYP, UWIDENT, UNIT " +
                       @"FROM XEP_ENVCONDSLABELS" +
                       @" WHERE ID IN (SELECT LABELID FROM XEP_REFFAULTLABELS, XEP_FAULTCODES WHERE CODE = {0} AND ECUVARIANTID = {1} AND XEP_REFFAULTLABELS.ID = XEP_FAULTCODES.ID) ORDER BY ID",
                        ecuFaultCode.Code, variantId);
            using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ecuEnvCondLabelList.Add(new EcuFunctionStructs.EcuEnvCondLabel(reader["ID"].ToString(),
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
                            EcuFunctionStructs.EcuJobResult ecuJobResult = new EcuFunctionStructs.EcuJobResult(
                                reader["RESULTID"].ToString(),
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

        private static List<EcuFunctionStructs.EcuResultStateValue> GetResultStateValueList(SQLiteConnection mDbConnection, string id, string language = null)
        {
            List<EcuFunctionStructs.EcuResultStateValue> ecuResultStateValueList = new List<EcuFunctionStructs.EcuResultStateValue>();
            string sql = string.Format(@"SELECT ID, " + SqlTitleItems + ", STATEVALUE, VALIDFROM, VALIDTO, PARENTID " +
                                       "FROM XEP_STATEVALUES WHERE (PARENTID IN (SELECT STATELISTID FROM XEP_REFSTATELISTS WHERE (ID = {0})))", id);
            using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ecuResultStateValueList.Add(new EcuFunctionStructs.EcuResultStateValue(reader["ID"].ToString(),
                            GetTranslation(reader, "TITLE", language),
                            reader["STATEVALUE"].ToString(),
                            reader["VALIDFROM"].ToString(),
                            reader["VALIDTO"].ToString(),
                            reader["PARENTID"].ToString()));
                    }
                }
            }

            return ecuResultStateValueList;
        }

        // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetEcuFixedFunctionsByParentId
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

        // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetEcuFixedFunctionsForEcuVariant
        private static EcuFunctionStructs.EcuVariant GetEcuVariantFunctions(TextWriter outTextWriter, TextWriter logTextWriter, SQLiteConnection mDbConnection, string ecuName)
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
                // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetEcuVariantFunctionByNameAndEcuGroupFunctionId
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
                // from: DatabaseProvider.SQLiteConnector.dll BMW.Rheingold.DatabaseProvider.SQLiteConnector.DatabaseProviderSQLite.GetEcuFunctionStructureById
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
