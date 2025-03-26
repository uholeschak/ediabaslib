using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Text;

namespace SqlTest
{
    internal class Program
    {
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
                string hexKey = BitConverter.ToString(Encoding.ASCII.GetBytes(DatabaseFunctions.DatabasePassword)).Replace("-", "");
                SqliteConnectionStringBuilder connectionBuilder = new SqliteConnectionStringBuilder
                {
                    DataSource = "file:" + databaseFile + "?cipher=rc4&hexkey=" + hexKey,
                    Mode = SqliteOpenMode.ReadOnly,
                };

                using (SqliteConnection sqliteConnection = new SqliteConnection(connectionBuilder.ConnectionString))
                {
                    sqliteConnection.Open();

                    string rootENameClassId = DatabaseFunctions.GetNodeClassId(sqliteConnection, @"RootEBezeichnung");
                    string typeKeyClassId = DatabaseFunctions.GetNodeClassId(sqliteConnection, @"Typschluessel");

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
    }
}
