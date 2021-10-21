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
        public class EcuInfo
        {
            public EcuInfo(string name, Int64 address, string description, string sgbd, string grp)
            {
                Name = name;
                Address = address;
                Description = description;
                Sgbd = sgbd;
                Grp = grp;
                VariantId = string.Empty;
                VariantGroupId = string.Empty;
                SwiActions = new List<SwiAction>();
            }

            public string Name { get; set; }

            public Int64 Address { get; set; }

            public string Description { get; set; }

            public string Sgbd { get; set; }

            public string Grp { get; set; }

            public string VariantId { get; set; }

            public string VariantGroupId { get; set; }

            public List<SwiAction> SwiActions { get; set; }
        }

        public class SwiAction
        {
            public SwiAction(string id, string name, string actionCategory, string selectable, string showInPlan, string executable, string nodeClass)
            {
                Id = id;
                Name = name;
                ActionCategory = actionCategory;
                Selectable = selectable;
                ShowInPlan = showInPlan;
                Executable = executable;
                NodeClass = nodeClass;
            }

            public string Id { get; set; }

            public string Name { get; set; }

            public string ActionCategory { get; set; }

            public string Selectable { get; set; }

            public string ShowInPlan { get; set; }

            public string Executable { get; set; }

            public string NodeClass { get; set; }
        }

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

        public bool GetEcuVariants(List<EcuInfo> ecuList)
        {
            bool result = true;
            foreach (EcuInfo ecuInfo in ecuList)
            {
                if (GetEcuVariant(ecuInfo))
                {
                    GetSwiActionsForEcuVariant(ecuInfo);
                }
                else
                {
                    result = false;
                }
            }

            return result;
        }

        private bool GetEcuVariant(EcuInfo ecuInfo)
        {
            bool result = false;
            string sql = string.Format(@"SELECT ID, " + DatabaseFunctions.SqlTitleItems + ", ECUGROUPID FROM XEP_ECUVARIANTS WHERE (lower(NAME) = '{0}')", ecuInfo.Sgbd.ToLowerInvariant());

            try
            {
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
            }
            catch (Exception)
            {
                return false;
            }

            return result;
        }

        private bool GetSwiActionsForEcuVariant(EcuInfo ecuInfo)
        {
            if (string.IsNullOrEmpty(ecuInfo.VariantId))
            {
                return false;
            }

            string sql = string.Format(@"SELECT ID, NAME, ACTIONCATEGORY, SELECTABLE, SHOW_IN_PLAN, EXECUTABLE, " + DatabaseFunctions.SqlTitleItems +
                                       ", NODECLASS FROM XEP_SWIACTION WHERE ID IN (SELECT SWI_ACTION_ID FROM XEP_REF_ECUVARIANTS_SWIACTION WHERE ECUVARIANT_ID = {0})",
                                        ecuInfo.VariantId);
            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, _mDbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string id = reader["ID"].ToString().Trim();
                            string name = reader["NAME"].ToString().Trim();
                            string actionCategory = reader["ACTIONCATEGORY"].ToString().Trim();
                            string selectable = reader["SELECTABLE"].ToString().Trim();
                            string showInPlan = reader["SHOW_IN_PLAN"].ToString().Trim();
                            string executable = reader["EXECUTABLE"].ToString().Trim();
                            string nodeclass = reader["NODECLASS"].ToString().Trim();
                            SwiAction swiAction = new SwiAction(id, name, actionCategory, selectable, showInPlan, executable, nodeclass);
                            ecuInfo.SwiActions.Add(swiAction);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        
            return true;
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
