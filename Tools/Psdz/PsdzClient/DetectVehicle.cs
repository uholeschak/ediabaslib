using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BmwFileReader;
using EdiabasLib;

namespace PsdzClient
{
    public class DetectVehicle : IDisposable
    {
        private readonly Regex _vinRegex = new Regex(@"^(?!0{7,})([a-zA-Z0-9]{7,})$");
        private static readonly Tuple<string, string, string>[] ReadVinJobsBmwFast =
        {
            new Tuple<string, string, string>("G_ZGW", "STATUS_VIN_LESEN", "STAT_VIN"),
            new Tuple<string, string, string>("ZGW_01", "STATUS_VIN_LESEN", "STAT_VIN"),
            new Tuple<string, string, string>("G_CAS", "STATUS_FAHRGESTELLNUMMER", "STAT_FGNR17_WERT"),
            new Tuple<string, string, string>("D_CAS", "STATUS_FAHRGESTELLNUMMER", "FGNUMMER"),
        };

        private static readonly Tuple<string, string, string>[] ReadIdentJobsBmwFast =
        {
            new Tuple<string, string, string>("G_ZGW", "STATUS_VCM_GET_FA", "STAT_BAUREIHE"),
            new Tuple<string, string, string>("ZGW_01", "STATUS_VCM_GET_FA", "STAT_BAUREIHE"),
            new Tuple<string, string, string>("D_CAS", "C_FA_LESEN", "FAHRZEUGAUFTRAG"),
            new Tuple<string, string, string>("D_LM", "C_FA_LESEN", "FAHRZEUGAUFTRAG"),
            new Tuple<string, string, string>("D_KBM", "C_FA_LESEN", "FAHRZEUGAUFTRAG"),
        };

        private static readonly Tuple<string, string>[] ReadILevelJobsBmwFast =
        {
            new Tuple<string, string>("G_ZGW", "STATUS_I_STUFE_LESEN_MIT_SIGNATUR"),
            new Tuple<string, string>("G_ZGW", "STATUS_VCM_I_STUFE_LESEN"),
            new Tuple<string, string>("G_FRM", "STATUS_VCM_I_STUFE_LESEN"),
        };

        private bool _disposed;
        private EdiabasNet _ediabas;

        public string Vin { get; private set; }
        public string GroupSgdb { get; private set; }
        public string Series { get; private set; }
        public string ConstructDate { get; private set; }
        public string ILevelShip { get; private set; }
        public string ILevelCurrent { get; private set; }
        public string ILevelBackup { get; private set; }

        public DetectVehicle(string ecuPath, EdInterfaceEnet.EnetConnection enetConnection = null)
        {
            EdInterfaceEnet edInterfaceEnet = new EdInterfaceEnet();
            _ediabas = new EdiabasNet
            {
                EdInterfaceClass = edInterfaceEnet,
                AbortJobFunc = AbortEdiabasJob
            };
            _ediabas.SetConfigProperty("EcuPath", ecuPath);

            bool icomAllocate = false;
            string hostAddress = "auto:all";
            if (enetConnection != null)
            {
                icomAllocate = enetConnection.ConnectionType == EdInterfaceEnet.EnetConnection.InterfaceType.Icom;
                hostAddress = enetConnection.ToString();
            }
            edInterfaceEnet.RemoteHost = hostAddress;
            edInterfaceEnet.IcomAllocate = icomAllocate;

            ResetValues();
        }

        public bool DetectVehicleBmwFast()
        {
            ResetValues();
            HashSet<string> invalidSgbdSet = new HashSet<string>();

            try
            {
                List<Dictionary<string, EdiabasNet.ResultData>> resultSets;

                string detectedVin = null;
                foreach (Tuple<string, string, string> job in ReadVinJobsBmwFast)
                {
                    try
                    {
                        _ediabas.ResolveSgbdFile(job.Item1);

                        _ediabas.ArgString = string.Empty;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob(job.Item2);

                        resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            if (detectedVin == null)
                            {
                                detectedVin = string.Empty;
                            }
                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                            if (resultDict.TryGetValue(job.Item3, out EdiabasNet.ResultData resultData))
                            {
                                string vin = resultData.OpData as string;
                                // ReSharper disable once AssignNullToNotNullAttribute
                                if (!string.IsNullOrEmpty(vin) && _vinRegex.IsMatch(vin))
                                {
                                    detectedVin = vin;
                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected VIN: {0}", detectedVin);
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        invalidSgbdSet.Add(job.Item1);
                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "No VIN response");
                        // ignored
                    }
                }

                if (string.IsNullOrEmpty(detectedVin))
                {
                    return false;
                }

                Vin = detectedVin;
                string vehicleType = null;
                DateTime? cDate = null;

                foreach (Tuple<string, string, string> job in ReadIdentJobsBmwFast)
                {
                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Read BR job: {0},{1}", job.Item1, job.Item2);
                    if (invalidSgbdSet.Contains(job.Item1))
                    {
                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Job ignored: {0}", job.Item1);
                        continue;
                    }
                    try
                    {
                        bool readFa = string.Compare(job.Item2, "C_FA_LESEN", StringComparison.OrdinalIgnoreCase) == 0;

                        _ediabas.ResolveSgbdFile(job.Item1);

                        _ediabas.ArgString = string.Empty;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob(job.Item2);

                        resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                            if (resultDict.TryGetValue(job.Item3, out EdiabasNet.ResultData resultData))
                            {
                                if (readFa)
                                {
                                    string fa = resultData.OpData as string;
                                    if (!string.IsNullOrEmpty(fa))
                                    {
                                        _ediabas.ResolveSgbdFile("FA");

                                        _ediabas.ArgString = "1;" + fa;
                                        _ediabas.ArgBinaryStd = null;
                                        _ediabas.ResultsRequests = string.Empty;
                                        _ediabas.ExecuteJob("FA_STREAM2STRUCT");

                                        List<Dictionary<string, EdiabasNet.ResultData>> resultSetsFa = _ediabas.ResultSets;
                                        if (resultSetsFa != null && resultSetsFa.Count >= 2)
                                        {
                                            Dictionary<string, EdiabasNet.ResultData> resultDictFa = resultSetsFa[1];
                                            if (resultDictFa.TryGetValue("BR", out EdiabasNet.ResultData resultDataBa))
                                            {
                                                string br = resultDataBa.OpData as string;
                                                if (!string.IsNullOrEmpty(br))
                                                {
                                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected BR: {0}", br);
                                                    string vtype = VehicleInfoBmw.GetVehicleTypeFromBrName(br, _ediabas);
                                                    if (!string.IsNullOrEmpty(vtype))
                                                    {
                                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected vehicle type: {0}", vtype);
                                                        vehicleType = vtype;
                                                    }
                                                }
                                            }

                                            if (resultDictFa.TryGetValue("C_DATE", out EdiabasNet.ResultData resultDataCDate))
                                            {
                                                string cDateStr = resultDataCDate.OpData as string;
                                                if (!string.IsNullOrEmpty(cDateStr))
                                                {
                                                    if (DateTime.TryParseExact(cDateStr, "MMyy", null, DateTimeStyles.None, out DateTime dateTime))
                                                    {
                                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected construction date: {0}",
                                                            dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                                                        cDate = dateTime;
                                                    }
                                                }
                                            }

                                            if (vehicleType != null)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    string br = resultData.OpData as string;
                                    if (!string.IsNullOrEmpty(br))
                                    {
                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected BR: {0}", br);
                                        string vtype = VehicleInfoBmw.GetVehicleTypeFromBrName(br, _ediabas);
                                        if (!string.IsNullOrEmpty(vtype))
                                        {
                                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected vehicle type: {0}", vtype);
                                            vehicleType = vtype;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "No BR response");
                        // ignored
                    }
                }

                Series = vehicleType;

                if (cDate.HasValue)
                {
                    ConstructDate = cDate.Value.ToString("yyyy-MM", CultureInfo.InvariantCulture);
                }

                string groupSgbd = VehicleInfoBmw.GetGroupSgbdFromVehicleType(vehicleType, detectedVin, cDate, _ediabas);
                if (string.IsNullOrEmpty(groupSgbd))
                {
                    _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "No group SGBD found");
                    return false;
                }
                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Group SGBD: {0}", groupSgbd);
                GroupSgdb = groupSgbd;

                string iLevelShip = null;
                string iLevelCurrent = null;
                string iLevelBackup = null;
                foreach (Tuple<string, string> job in ReadILevelJobsBmwFast)
                {
                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Read ILevel job: {0},{1}", job.Item1, job.Item2);
                    if (invalidSgbdSet.Contains(job.Item1))
                    {
                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Job ignored: {0}", job.Item1);
                        continue;
                    }
                    try
                    {
                        _ediabas.ResolveSgbdFile(job.Item1);

                        _ediabas.ArgString = string.Empty;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob(job.Item2);

                        resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            if (detectedVin == null)
                            {
                                detectedVin = string.Empty;
                            }

                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                            if (resultDict.TryGetValue("STAT_I_STUFE_WERK", out EdiabasNet.ResultData resultData))
                            {
                                string iLevel = resultData.OpData as string;
                                if (!string.IsNullOrEmpty(iLevel) && iLevel.Length >= 4 &&
                                    string.Compare(iLevel, VehicleInfoBmw.ResultUnknown, StringComparison.OrdinalIgnoreCase) != 0)
                                {
                                    iLevelShip = iLevel;
                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected ILevel ship: {0}", iLevelShip);
                                }
                            }

                            if (!string.IsNullOrEmpty(iLevelShip))
                            {
                                if (resultDict.TryGetValue("STAT_I_STUFE_HO", out resultData))
                                {
                                    string iLevel = resultData.OpData as string;
                                    if (!string.IsNullOrEmpty(iLevel) && iLevel.Length >= 4 &&
                                        string.Compare(iLevel, VehicleInfoBmw.ResultUnknown, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        iLevelCurrent = iLevel;
                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected ILevel current: {0}", iLevelCurrent);
                                    }
                                }

                                if (string.IsNullOrEmpty(iLevelCurrent))
                                {
                                    iLevelCurrent = iLevelShip;
                                }

                                if (resultDict.TryGetValue("STAT_I_STUFE_HO_BACKUP", out resultData))
                                {
                                    string iLevel = resultData.OpData as string;
                                    if (!string.IsNullOrEmpty(iLevel) && iLevel.Length >= 4 &&
                                        string.Compare(iLevel, VehicleInfoBmw.ResultUnknown, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        iLevelBackup = iLevel;
                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected ILevel backup: {0}", iLevelBackup);
                                    }
                                }

                                break;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "No ILevel response");
                        // ignored
                    }
                }

                if (string.IsNullOrEmpty(iLevelShip))
                {
                    _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "ILevel not found");
                    return false;
                }

                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ILevel: Ship={0}, Current={1}, Backup={2}", iLevelShip, iLevelCurrent, iLevelBackup);

                ILevelShip = iLevelShip;
                ILevelCurrent = iLevelCurrent;
                ILevelBackup = iLevelBackup;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool AbortEdiabasJob()
        {
            return false;
        }

        private void ResetValues()
        {
            Vin = null;
            GroupSgdb = null;
            Series = null;
            ConstructDate = null;
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
                if (_ediabas != null)
                {
                    _ediabas.Dispose();
                    _ediabas = null;
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
