using System;
using System.Data.SQLite;

public static class DatabaseFunctions
{
    public static string GetNodeClassId(SQLiteConnection mDbConnection, string nodeClassName)
    {
        string result = string.Empty;
        string sql = string.Format(@"SELECT ID FROM XEP_NODECLASSES WHERE NAME = '{0}'", nodeClassName);
        using (SQLiteCommand command = new SQLiteCommand(sql, mDbConnection))
        {
            using (SQLiteDataReader reader = command.ExecuteReader())
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
