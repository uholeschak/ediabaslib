using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public class PdszDatabase : IDisposable
    {
        private bool _disposed;
        private SQLiteConnection _mDbConnection;
        private string _rootENameClassId;
        private string _typeKeyClassId;

        public PdszDatabase(string istaFolder)
        {
            string databaseFile = Path.Combine(istaFolder, "SQLiteDBs", "DiagDocDb.sqlite");
            string connection = "Data Source=\"" + databaseFile + "\";";
            _mDbConnection = new SQLiteConnection(connection);

            _mDbConnection.SetPassword("6505EFBDC3E5F324");
            _mDbConnection.Open();

            _rootENameClassId = DatabaseFunctions.GetNodeClassId(_mDbConnection, @"RootEBezeichnung");
            _typeKeyClassId = DatabaseFunctions.GetNodeClassId(_mDbConnection, @"Typschluessel");
        }

        public bool GetEcuVariants(List<DetectVehicle.EcuInfo> ecuList)
        {
            bool result = true;
            foreach (DetectVehicle.EcuInfo ecuInfo in ecuList)
            {
                if (!GetEcuVariant(ecuInfo))
                {
                    result = false;
                }
            }

            return result;
        }

        private bool GetEcuVariant(DetectVehicle.EcuInfo ecuInfo)
        {
            bool result = false;
            string sql = string.Format(@"SELECT ID, " + DatabaseFunctions.SqlTitleItems + ", ECUGROUPID FROM XEP_ECUVARIANTS WHERE (lower(NAME) = '{0}')", ecuInfo.Sgbd.ToLowerInvariant());
            using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ecuInfo.VariantGroupId = reader["ECUGROUPID"].ToString().Trim();
                        ecuInfo.VariantId = reader["ID"].ToString().Trim();
                        result = true;
                    }
                }
            }

            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                if (_mDbConnection != null)
                {
                    _mDbConnection.Dispose();
                    _mDbConnection = null;
                }

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

    }
}
