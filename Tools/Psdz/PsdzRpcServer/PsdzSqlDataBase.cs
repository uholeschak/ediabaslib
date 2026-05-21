using log4net;
using PsdzClient.Core;
using System;
using System.Globalization;
using MySqlConnector;

namespace PsdzRpcServer;

public class PsdzSqlDataBase
{
    private static readonly ILog log = LogManager.GetLogger(typeof(PsdzSqlDataBase));

    public bool CheckLicense(MySqlConnection connection, string vin, out string serial)
    {
        log.InfoFormat("CheckLicense VIN={0}", vin);

        serial = null;
        string matchVin = null;

        try
        {
            string sqlSelect = string.Format(CultureInfo.InvariantCulture, "SELECT `vin`, `serial` FROM `bmw_coding`.`licenses` WHERE UPPER(`vin`) = UPPER('{0}')", vin);
            using (MySqlCommand command = new MySqlCommand(sqlSelect, connection))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        matchVin = reader["vin"].ToString();
                        serial = reader["serial"].ToString();
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            log.ErrorFormat("CheckLicense Exception: {0}", ex.Message);
            return false;
        }

        if (string.IsNullOrEmpty(matchVin))
        {
            log.ErrorFormat("CheckLicense Not valid");
            return false;
        }

        log.InfoFormat("CheckLicense Valid");
        return true;
    }

    public bool AddLicense(MySqlConnection connection, string vin, string serial, bool registerAll)
    {
        log.InfoFormat("AddLicense VIN={0}, Serial={1}, RegisterAll={2}", vin ?? string.Empty, serial ?? string.Empty, registerAll);

        if (string.IsNullOrEmpty(vin))
        {
            log.ErrorFormat("AddLicense No VIN");
            return false;
        }

        try
        {
            string serialUsedVin = null;
            if (!string.IsNullOrEmpty(serial) && !registerAll)
            {
                string sqlSelect = string.Format(CultureInfo.InvariantCulture, "SELECT `vin`, `serial` FROM `bmw_coding`.`licenses` WHERE `serial` = '{0}'", serial);
                using (MySqlCommand command = new MySqlCommand(sqlSelect, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string matchVin = reader["vin"].ToString();
                            if (string.Compare(matchVin, vin, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                serialUsedVin = matchVin;
                                break;
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(serialUsedVin))
            {
                log.ErrorFormat("AddLicense Serial used by VIN: {0}", serialUsedVin);
                return false;
            }

            string sqlUpdate;
            if (string.IsNullOrEmpty(serial))
            {
                sqlUpdate = string.Format(CultureInfo.InvariantCulture, "INSERT INTO `bmw_coding`.`licenses` (`vin`) VALUES (UPPER('{0}')) AS `new` ON DUPLICATE KEY UPDATE `vin` = `new`.`vin`", vin);
            }
            else
            {
                sqlUpdate = string.Format(CultureInfo.InvariantCulture, "INSERT INTO `bmw_coding`.`licenses` (`vin`, `serial`) VALUES (UPPER('{0}'), '{1}') AS `new` ON DUPLICATE KEY UPDATE `vin` = `new`.`vin`, `serial` = '{1}'", vin, serial);
            }
            using (MySqlCommand command = new MySqlCommand(sqlUpdate, connection))
            {
                int modifiedRows = command.ExecuteNonQuery();
                if (modifiedRows < 0)
                {
                    log.ErrorFormat("AddLicense Adding VIN failed: {0}", vin);
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            log.ErrorFormat("AddLicense Exception: {0}", ex.Message);
            return false;
        }

        log.InfoFormat("AddLicense VIN: {0} added", vin);
        return true;
    }
}
