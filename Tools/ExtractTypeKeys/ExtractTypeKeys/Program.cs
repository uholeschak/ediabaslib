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
                writer = (args.Length < 2) ? Console.Out : new StreamWriter(args[1]);
                string connection = "Data Source=\"" + args[0] + "\";";
                using (SQLiteConnection mDbConnection = new SQLiteConnection(connection))
                {
                    mDbConnection.SetPassword("6505EFBDC3E5F324");
                    mDbConnection.Open();

                    string sql = @"SELECT t.NAME AS TYPEKEY, c.NAME AS EREIHE FROM XEP_CHARACTERISTICS t INNER JOIN XEP_VEHICLES v ON (v.TYPEKEYID = t.ID) INNER JOIN XEP_CHARACTERISTICS c ON (v.CHARACTERISTICID = c.ID) INNER JOIN XEP_CHARACTERISTICROOTS r ON (r.ID = c.PARENTID AND r.NODECLASS=40140802) WHERE t.NODECLASS = 40135042 ORDER BY TYPEKEY";
                    SQLiteCommand command = new SQLiteCommand(sql, mDbConnection);
                    int count = 0;
                    using (SQLiteDataReader reader = command.ExecuteReader())
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
