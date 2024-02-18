
public static class DatabaseFunctions
{
    public const string DatabasePassword = "6505EFBDC3E5F324";

    public const string SqlTitleItems =
        "TITLE_DEDE, TITLE_ENGB, TITLE_ENUS, " +
        "TITLE_FR, TITLE_TH, TITLE_SV, " +
        "TITLE_IT, TITLE_ES, TITLE_ID, " +
        "TITLE_KO, TITLE_EL, TITLE_TR, " +
        "TITLE_ZHCN, TITLE_RU, TITLE_NL, " +
        "TITLE_PT, TITLE_ZHTW, TITLE_JA, " +
        "TITLE_CSCZ, TITLE_PLPL";

#if NET || MS_SQLITE
    public static string GetNodeClassId(Microsoft.Data.Sqlite.SqliteConnection sqliteConnection, string nodeClassName)
    {
        string result = string.Empty;
        string sql = string.Format(@"SELECT ID FROM XEP_NODECLASSES WHERE NAME = '{0}'", nodeClassName);
        using (Microsoft.Data.Sqlite.SqliteCommand command = sqliteConnection.CreateCommand())
        {
            command.CommandText = sql;
            using (Microsoft.Data.Sqlite.SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result = reader["ID"].ToString();
                }
            }
        }

        return result;
    }
#else
    public static string GetNodeClassId(System.Data.SQLite.SQLiteConnection mDbConnection, string nodeClassName)
    {
        string result = string.Empty;
        string sql = string.Format(@"SELECT ID FROM XEP_NODECLASSES WHERE NAME = '{0}'", nodeClassName);
        using (System.Data.SQLite.SQLiteCommand command = new System.Data.SQLite.SQLiteCommand(sql, mDbConnection))
        {
            using (System.Data.SQLite.SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result = reader["ID"].ToString();
                }
            }
        }

        return result;
    }
#endif
}
