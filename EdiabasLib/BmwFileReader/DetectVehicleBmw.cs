using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using BmwDeepObd;
using EdiabasLib;
#if Android
using ICSharpCode.SharpZipLib.Zip;
#endif

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace BmwFileReader
{
    public class DetectVehileBmw
    {
        public delegate bool AbortDelegate();
        public delegate void ProgressDelegate(int percent);

        public AbortDelegate AbortFunc { get; set; }
        public ProgressDelegate ProgressFunc { get; set; }

        public string Vin { get; private set; }
        public string GroupSgdb { get; private set; }
        public string ModelSeries { get; private set; }
        public string Series { get; private set; }
        public string ConstructYear { get; private set; }
        public string ConstructMonth { get; private set; }
        public string ILevelShip { get; private set; }
        public string ILevelCurrent { get; private set; }
        public string ILevelBackup { get; private set; }

        private EdiabasNet _ediabas;
        private string _bmwDir;

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

        public DetectVehileBmw(EdiabasNet ediabas, string bmwDir)
        {
            _ediabas = ediabas;
            _bmwDir = bmwDir;
        }

        public bool DetectVehicleBmwFast()
        {
            _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Try to detect vehicle BMW fast");
            ResetValues();
            HashSet<string> invalidSgbdSet = new HashSet<string>();

            try
            {
                List<Dictionary<string, EdiabasNet.ResultData>> resultSets;

                ProgressFunc?.Invoke(0);

                string detectedVin = null;
                int jobCount = ReadVinJobsBmwFast.Length + ReadIdentJobsBmwFast.Length + ReadILevelJobsBmwFast.Length;
                int index = 0;
                foreach (Tuple<string, string, string> job in ReadVinJobsBmwFast)
                {
                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Read VIN job: {0}", job.Item1);
                    try
                    {
                        if (AbortFunc != null && AbortFunc())
                        {
                            return false;
                        }

                        ProgressFunc?.Invoke(100 * index / jobCount);

                        ActivityCommon.ResolveSgbdFile(_ediabas, job.Item1);

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
                    index++;
                }

                if (string.IsNullOrEmpty(detectedVin))
                {
                    return false;
                }

                Vin = detectedVin;
                string vehicleType = null;
                string modelSeries = null;
                DateTime? cDate = null;

                foreach (Tuple<string, string, string> job in ReadIdentJobsBmwFast)
                {
                    if (AbortFunc != null && AbortFunc())
                    {
                        return false;
                    }

                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Read BR job: {0},{1}", job.Item1, job.Item2);
                    if (invalidSgbdSet.Contains(job.Item1))
                    {
                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Job ignored: {0}", job.Item1);
                        index++;
                        continue;
                    }
                    try
                    {
                        bool readFa = string.Compare(job.Item2, "C_FA_LESEN", StringComparison.OrdinalIgnoreCase) == 0;
                        ProgressFunc?.Invoke(100 * index / jobCount);

                        ActivityCommon.ResolveSgbdFile(_ediabas, job.Item1);

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
                                        ActivityCommon.ResolveSgbdFile(_ediabas, "FA");

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
                                                DateTime? dateTime = VehicleInfoBmw.ConvertConstructionDate(cDateStr);
                                                if (dateTime != null)
                                                {
                                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected construction date: {0}",
                                                        dateTime.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                                                    cDate = dateTime.Value;
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
                                            modelSeries = br;
                                            vehicleType = vtype;
                                        }

                                        if (resultDict.TryGetValue("STAT_ZEIT_KRITERIUM", out EdiabasNet.ResultData resultDataCDate))
                                        {
                                            string cDateStr = resultDataCDate.OpData as string;
                                            DateTime? dateTime = VehicleInfoBmw.ConvertConstructionDate(cDateStr);
                                            if (dateTime != null)
                                            {
                                                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected construction date: {0}",
                                                    dateTime.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                                                cDate = dateTime;
                                            }
                                        }

                                        if (vehicleType != null)
                                        {
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
                    index++;
                }

                ProgressFunc?.Invoke(100);

                if (string.IsNullOrEmpty(vehicleType))
                {
                    vehicleType = VehicleInfoBmw.GetVehicleTypeFromVin(detectedVin, _ediabas, _bmwDir);
                }

                ModelSeries = modelSeries;
                Series = vehicleType;
                if (cDate.HasValue)
                {
                    ConstructYear = cDate.Value.ToString("yyyy", CultureInfo.InvariantCulture);
                    ConstructMonth = cDate.Value.ToString("MM", CultureInfo.InvariantCulture);
                }

                VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfo = VehicleInfoBmw.GetVehicleSeriesInfo(vehicleType, cDate, _ediabas);
                if (vehicleSeriesInfo == null)
                {
                    _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Vehicle series info not found");
                    return false;
                }
                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Group SGBD: {0}", vehicleSeriesInfo.BrSgbd);
                GroupSgdb = vehicleSeriesInfo.BrSgbd;

                string iLevelShip = null;
                string iLevelCurrent = null;
                string iLevelBackup = null;
                foreach (Tuple<string, string> job in ReadILevelJobsBmwFast)
                {
                    if (AbortFunc != null && AbortFunc())
                    {
                        return false;
                    }

                    ProgressFunc?.Invoke(100 * index / jobCount);

                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Read ILevel job: {0},{1}", job.Item1, job.Item2);
                    if (invalidSgbdSet.Contains(job.Item1))
                    {
                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Job ignored: {0}", job.Item1);
                        index++;
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
                                    string.Compare(iLevel, VehicleInfoBmw.ResultUnknown,
                                        StringComparison.OrdinalIgnoreCase) != 0)
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
                                        string.Compare(iLevel, VehicleInfoBmw.ResultUnknown,
                                            StringComparison.OrdinalIgnoreCase) != 0)
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
                                        string.Compare(iLevel, VehicleInfoBmw.ResultUnknown,
                                            StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        iLevelBackup = iLevel;
                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected ILevel backup: {0}",
                                            iLevelBackup);
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

                    index++;
                }

                if (string.IsNullOrEmpty(iLevelShip))
                {
                    _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "ILevel not found");
                }
                else
                {
                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ILevel: Ship={0}, Current={1}, Backup={2}",
                        iLevelShip, iLevelCurrent, iLevelBackup);

                    ILevelShip = iLevelShip;
                    ILevelCurrent = iLevelCurrent;
                    ILevelBackup = iLevelBackup;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ResetValues()
        {
            Vin = null;
            GroupSgdb = null;
            ModelSeries = null;
            Series = null;
            ConstructYear = null;
            ConstructMonth = null;
        }

    }
}
