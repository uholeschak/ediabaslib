using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace SqlTest
{
    internal class Program
    {
        public const string DatabasePassword = "6505EFBDC3E5F324";

        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: SqlTest <database file>");
                return 1;
            }

            try
            {
                string databaseFile = args[0];
                if (!File.Exists(databaseFile))
                {
                    Console.WriteLine("Database file not found: " + databaseFile);
                    return 1;
                }

                // https://www.bricelam.net/2023/11/10/more-sqlite-encryption.html
                SqliteConnectionStringBuilder connectionBuilder = new SqliteConnectionStringBuilder
                {
                    DataSource = "file:" + databaseFile + "?cipher=rc4",
                    Mode = SqliteOpenMode.ReadOnly,
                    Password = DatabasePassword,
                };

                using (SqliteConnection sqliteConnection = new SqliteConnection(connectionBuilder.ConnectionString))
                {
                    sqliteConnection.Open();

                    string rootENameClassId = GetNodeClassId(sqliteConnection, @"RootEBezeichnung");
                    string typeKeyClassId = GetNodeClassId(sqliteConnection, @"Typschluessel");

                    Console.WriteLine("RootEBezeichnung: {0}", rootENameClassId);
                    Console.WriteLine("Typschluessel: {0}", typeKeyClassId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
                return 1;
            }

            return 0;
        }

        public static string GetNodeClassId(SqliteConnection sqliteConnection, string nodeClassName)
        {
            string result = string.Empty;
            string sql = string.Format(@"SELECT ID FROM XEP_NODECLASSES WHERE NAME = '{0}'", nodeClassName);
            using (SqliteCommand command = sqliteConnection.CreateCommand())
            {
                command.CommandText = sql;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = reader["ID"].ToString();
                    }
                }
            }

            return result;
        }
    }
}
