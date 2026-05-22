using log4net;
using System;
using System.Globalization;
using MySqlConnector;

namespace PsdzRpcServer;

public class PsdzLicenseCheck
{
    private const string SqlDataBase = ";Database=bmw_coding";
    private static readonly ILog log = LogManager.GetLogger(typeof(PsdzLicenseCheck));
    private string _sqlServer;
    private bool _testLicenses;
    private string _displayOptions;

    public PsdzLicenseCheck(string sqlServer, bool testLicenses = false, string displayOptions = null)
    {
        _sqlServer = sqlServer;
        _testLicenses = testLicenses;
        _displayOptions = displayOptions;
    }

    public bool ProcessLicenseRequest(string vin, string adapterSerial, bool adapterSerialValid)
    {
        bool registerAll = _testLicenses;
        log.InfoFormat("ProcessLicense RegisterAll={0}", registerAll);

        if (string.IsNullOrEmpty(vin))
        {
            log.ErrorFormat("ProcessLicense No VIN");
            return false;
        }

        bool licenseValid = false;
        bool serialValid = adapterSerialValid;
        string serial = serialValid ? adapterSerial : null;

        try
        {
            if (string.IsNullOrEmpty(_sqlServer))
            {
                log.ErrorFormat("ProcessLicense No SqlServer");
                return false;
            }

            string connectionString = _sqlServer + SqlDataBase;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                licenseValid = CheckLicense(connection, vin, out _);
                if (!licenseValid && (serialValid || registerAll))
                {
                    log.InfoFormat("ProcessLicense Adding Vin={0}, Serial={1}", vin, serial);
                    if (AddLicense(connection, vin, serial, registerAll))
                    {
                        licenseValid = true;
                    }
                    else
                    {
                        log.InfoFormat("ProcessLicense Adding failed Vin={0}, Serial={1}", vin, serial);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            log.ErrorFormat("ProcessLicense Exception: {0}", ex.Message);
            licenseValid = false;
        }

        log.InfoFormat("ProcessLicense Valid={0}", licenseValid);
        return licenseValid;
    }

    public bool CheckLicense(MySqlConnection connection, string vin, out string serial)
    {
        log.InfoFormat("CheckLicense VIN={0}", vin);

        serial = null;

        if (string.IsNullOrEmpty(vin))
        {
            log.ErrorFormat("CheckLicense No VIN");
            return false;
        }

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

    public bool HasDisplayOption(string option)
    {
        log.InfoFormat("HasDisplayOption Option={0}", option);
        string displayOptions = _displayOptions;
        if (string.IsNullOrEmpty(displayOptions))
        {
            log.InfoFormat("HasDisplayOption No options");
            return false;
        }

        string[] optionList = displayOptions.Split(';');
        foreach (string optionItem in optionList)
        {
            if (string.Compare(optionItem, option, StringComparison.OrdinalIgnoreCase) == 0)
            {
                log.InfoFormat("HasDisplayOption Option found: {0}", option);
                return true;
            }
        }

        log.InfoFormat("HasDisplayOption Option not found: {0}", option);
        return false;
    }

}
