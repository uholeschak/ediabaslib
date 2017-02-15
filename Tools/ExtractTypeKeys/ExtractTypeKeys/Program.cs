using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractTypeKeys
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("No Database specified");
                return;
            }
            TextWriter writer = null;
            try
            {
                string outFile = (args.Length < 2) ? string.Empty : args[1];
                writer = string.IsNullOrEmpty(outFile) ? Console.Out : new StreamWriter(outFile);
                string connection = "Data Source=\"" + args[0] + "\";";
                using (SQLiteConnection mDbConnection = new SQLiteConnection(connection))
                {
                    mDbConnection.SetPassword("6505EFBDC3E5F324");
                    mDbConnection.Open();

                    string sql1 = @"SELECT t.NAME AS TYPEKEY, c.NAME AS EREIHE FROM XEP_CHARACTERISTICS t INNER JOIN XEP_VEHICLES v ON (v.TYPEKEYID = t.ID) INNER JOIN XEP_CHARACTERISTICS c ON (v.CHARACTERISTICID = c.ID) INNER JOIN XEP_CHARACTERISTICROOTS r ON (r.ID = c.PARENTID AND r.NODECLASS=40140802) WHERE t.NODECLASS = 40135042 ORDER BY TYPEKEY";
                    SQLiteCommand command1 = new SQLiteCommand(sql1, mDbConnection);
                    int count = 0;
                    using (SQLiteDataReader reader = command1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            writer.Write("{\"" + reader["TYPEKEY"] + "\", \"" + reader["EREIHE"] + "\"},");
                            count++;
                            if (count % 10 == 0)
                            {
                                writer.WriteLine();
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(outFile))
                    {
                        string vinRangeFile = Path.Combine(Path.GetDirectoryName(outFile), "vinranges.txt");
                        using (StreamWriter swVinranges = new StreamWriter(vinRangeFile))
                        {
                            string sql2 = @"SELECT v.VINBANDFROM AS VINBANDFROM, v.VINBANDTO AS VINBANDTO, v.TYPSCHLUESSEL AS TYPEKEY FROM VINRANGES v";
                            SQLiteCommand command2 = new SQLiteCommand(sql2, mDbConnection);
                            using (SQLiteDataReader reader = command2.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    swVinranges.WriteLine(reader["VINBANDFROM"] + "," + reader["VINBANDTO"] + "," + reader["TYPEKEY"]);
                                }
                            }
                        }
                    }
                    mDbConnection.Close();
                }
                writer.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                writer?.Close();
            }
        }
    }
}
