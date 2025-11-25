using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Programming.API;
using BMW.Rheingold.Programming.Common;
using BMW.Rheingold.Programming.Controller.SecureCoding.Model;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Svb;
using BMW.Rheingold.Psdz.Model.Swt;
using BMW.Rheingold.Psdz.Model.Tal;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

#pragma warning disable CS0169
namespace PsdzClient.Programming
{
    [PreserveSource(Hint = "IDisposable added", AccessModified = true, InheritanceModified = true)]
	public class PsdzContext : IPsdzContext, IDisposable
	{
        [PreserveSource(Hint = "Added")]
        private const string IdrBackupFileName = "_IDR_Files.backup";

        [PreserveSource(Hint = "Added")]
        private bool _disposed;

        private bool hasVinBackupDataFolder;

        private IEnumerable<IPsdzIstufe> possibleIstufenTarget;

        [PreserveSource(Hint = "IPsdzCentralConnectionService", Placeholder = true)]
        private readonly PlaceholderType service;

        public bool IsEmptyBackupTal
        {
            get
            {
                if (IsValidBackupTal)
                {
                    return !IndividualDataBackupTal.TalLines.Any();
                }
                return true;
            }
        }

        public bool IsEmptyPrognosisTal
        {
            get
            {
                if (IsValidRestorePrognosisTal)
                {
                    return !IndividualDataRestorePrognosisTal.TalLines.Any();
                }
                return true;
            }
        }

        public bool IsEmptyRestoreTal
        {
            get
            {
                if (IsValidRestoreTal)
                {
                    return !IndividualDataRestoreTal.TalLines.Any();
                }
                return true;
            }
        }

        public bool IsEmptyTal
        {
            get
            {
                if (IsValidTal)
                {
                    return !Tal.TalLines.Any();
                }
                return true;
            }
        }

        internal BackupTalResult CheckBackupTal()
        {
            if (!IsValidBackupTal)
            {
                return BackupTalResult.Error;
            }
            if (IsEmptyBackupTal)
            {
                return BackupTalResult.Success;
            }
            return BackupTalResult.Undefined;
        }

        public string IstufeCurrent { get; private set; }

		public string IstufeLast { get; private set; }

		public string IstufeShipment { get; private set; }

        public bool IsValidBackupTal => IndividualDataBackupTal != null;

        public IVehicleProfileChecksum VpcFromVcm { get; set; }

        public bool IsValidEcuListActual
        {
            get
            {
                if (EcuListActual != null)
                {
                    return EcuListActual.Any();
                }
                return false;
            }
        }

        public bool IsValidFaActual
        {
            get
            {
                if (FaActual != null)
                {
                    return FaActual.IsValid;
                }
                return false;
            }
        }

        public bool IsValidFaTarget
        {
            get
            {
                if (FaTarget != null)
                {
                    return FaTarget.IsValid;
                }
                return false;
            }
        }

        public bool IsValidRestorePrognosisTal => IndividualDataRestorePrognosisTal != null;

        public bool IsValidRestoreTal => IndividualDataRestoreTal != null;

        public bool IsValidSollverbauung => Sollverbauung != null;

        public bool IsValidSvtActual
        {
            get
            {
                if (SvtActual != null)
                {
                    return SvtActual.IsValid;
                }
                return false;
            }
        }

        public bool IsValidTal => Tal != null;

        public string LatestPossibleIstufeTarget
        {
            get
            {
                IEnumerable<IPsdzIstufe> enumerable = possibleIstufenTarget;
                if (enumerable == null || !enumerable.Any())
                {
                    return null;
                }
                return enumerable.Last().Value;
            }
        }

        public string ProjectName { get; set; }

        public SFASessionData SFASessionData { get; set; }

        public IPsdzSwtAction SwtAction { get; internal set; }

        public string VehicleInfo { get; set; }

        public string PathToBackupData { get; set; }

        public bool PsdZBackUpModeSet { get; set; }

        public IPsdzTalFilter TalFilterForIndividualDataTal { get; private set; }

        public RequestJson NCDLastCalculationRequest { get; internal set; }

        [PreserveSource(Hint = "Modified")]
        public IPsdzConnection Connection { get; set; }

        internal IEnumerable<IPsdzEcuIdentifier> EcuListActual { get; set; }

        internal IDictionary<string, IList<string>> ExecutionOrderBottom { get; private set; }

        internal IDictionary<string, IList<string>> ExecutionOrderTop { get; private set; }

        internal IPsdzFa FaActual { get; private set; }

        internal IPsdzFa FaTarget { get; private set; }

        [PreserveSource(Hint = "List<EcuFilterOnSweLevel>", Placeholder = true)]
        internal PlaceholderType EcuSweFilter { get; set; }

        [PreserveSource(Hint = "List<EcuFilterOnSmacLevel>", Placeholder = true)]
        internal PlaceholderType EcuSmacFilter { get; set; }

        internal IPsdzTal IndividualDataBackupTal { get; set; }

        internal IPsdzTal IndividualDataRestorePrognosisTal { get; set; }

        internal IPsdzTal IndividualDataRestoreTal { get; set; }

        internal IPsdzSollverbauung Sollverbauung { get; private set; }

        internal IPsdzSvt SvtActual { get; private set; }

        public IPsdzTal Tal { get; set; }

        internal IPsdzTalFilter TalFilter { get; private set; }

        internal IPsdzTalFilter TalFilterForECUWithIDRClassicState { get; private set; }

        internal IPsdzTal TalForECUWithIDRClassicState { get; set; }

        internal IEnumerable<IPsdzTargetSelector> TargetSelectors { get; set; }

        [PreserveSource(Hint = "Added")]
        public DetectVehicle DetectVehicle { get; set; }

        [PreserveSource(Hint = "Added")]
        public Vehicle VecInfo { get; set; }

        [PreserveSource(Hint = "Added")]
        public ISvt SvtTarget { get; private set; }

        [PreserveSource(Hint = "Added")]
        public ISvt SvtCurrent { get; private set; }

        [PreserveSource(Hint = "Added")]
        public string IstaFolder { get; private set; }

        [PreserveSource(Hint = "Added")]
        public BaseEcuCharacteristics EcuCharacteristics { get; private set; }

        [PreserveSource(Hint = "ServiceLocator removed")]
        public PsdzContext(string istaFolder)
        {
            this.IstaFolder = istaFolder;
            ExecutionOrderTop = new Dictionary<string, IList<string>>();
            ExecutionOrderBottom = new Dictionary<string, IList<string>>();
            SFASessionData = new SFASessionData();
            // [IGNORE] ServiceLocator.Current.TryGetService<IPsdzCentralConnectionService>(out service);
        }

        public string GetBaseVariant(int diagnosticAddress)
        {
            if (SvtActual.Ecus.Any((IPsdzEcu ecu) => diagnosticAddress == ecu.PrimaryKey.DiagAddrAsInt))
            {
                return SvtActual.Ecus.Single((IPsdzEcu ecu) => diagnosticAddress == ecu.PrimaryKey.DiagAddrAsInt).BaseVariant;
            }
            Log.Warning("PsdzContext.GetBaseVariant", "Ecu with DiagAdr:{0} was not found.", diagnosticAddress);
            return string.Empty;
        }

        public IEnumerable<ISgbmIdChange> GetDifferentSgbmIds(int diagnosticAddress)
        {
            List<ISgbmIdChange> list = new List<ISgbmIdChange>();
            if (SvtActual != null && Sollverbauung != null)
            {
                try
                {
                    IPsdzEcu psdzEcu = SvtActual.Ecus.Single((IPsdzEcu ecu) => diagnosticAddress == ecu.PrimaryKey.DiagAddrAsInt);
                    IPsdzEcu psdzEcu2 = Sollverbauung.Svt.Ecus.Single((IPsdzEcu ecu) => diagnosticAddress == ecu.PrimaryKey.DiagAddrAsInt);
                    List<IPsdzSgbmId> list2 = psdzEcu.StandardSvk.SgbmIds.ToList();
                    List<IPsdzSgbmId> list3 = psdzEcu2.StandardSvk.SgbmIds.ToList();
                    foreach (IPsdzSgbmId currentSgbmId in list2)
                    {
                        if (!list3.Remove(currentSgbmId))
                        {
                            IPsdzSgbmId psdzSgbmId = list3.FirstOrDefault((IPsdzSgbmId targetSgbmId) => targetSgbmId.Id == currentSgbmId.Id && targetSgbmId.ProcessClass == currentSgbmId.ProcessClass);
                            list.Add(new SgbmIdChange(currentSgbmId.HexString, psdzSgbmId?.HexString));
                            if (psdzSgbmId != null && !list3.Remove(psdzSgbmId))
                            {
                                Log.Warning("PsdzContext.GetDifferntSgbmIds()", "Couldn't remove SgbmId '{0}' although it existed.", psdzSgbmId);
                            }
                        }
                    }
                    list.AddRange(list3.Select((IPsdzSgbmId x) => new SgbmIdChange(null, x.HexString)));
                }
                catch (InvalidOperationException exception)
                {
                    Log.Error("PsdzContext.GetDifferntSgbmIds()", "There doesn't exist an ECU with the diagnosticAddress '{0}'.", diagnosticAddress);
                    Log.ErrorException("PsdzContext.GetDifferntSgbmIds()", exception);
                    list = null;
                }
                catch (Exception exception2)
                {
                    Log.ErrorException("PsdzContext.GetDifferntSgbmIds()", exception2);
                    list = null;
                }
            }
            else
            {
                Log.Warning("PsdzContext.GetDifferntSgbmIds()", "Parameter ecuId is null.");
            }
            return list;
        }

        public bool? IsSoftwareUpToDate(int diagnosticAddress)
        {
            bool? result = false;
            IEnumerable<ISgbmIdChange> differentSgbmIds = GetDifferentSgbmIds(diagnosticAddress);
            result = ((differentSgbmIds != null) ? new bool?(!differentSgbmIds.Where((ISgbmIdChange a) => a.Actual != null && !a.Actual.StartsWith("NAVD-") && !a.Actual.StartsWith("ENTD-")).Any()) : ((bool?)null));
            return result;
        }

        public ICombinedEcuHousingEntry GetEcuHousingEntry(int diagnosticAddress)
        {
            ICollection<ICombinedEcuHousingEntry> combinedEcuHousingTable = EcuCharacteristics?.GetCombinedEcuHousingTable();
            if (combinedEcuHousingTable == null)
            {
                return null;
            }

            foreach (ICombinedEcuHousingEntry combinedEcuHousingEntry in combinedEcuHousingTable)
            {
                int[] requiredEcuAddresses = combinedEcuHousingEntry.RequiredEcuAddresses;
                if (requiredEcuAddresses != null)
                {
                    foreach (int ecuAddress in requiredEcuAddresses)
                    {
                        if (ecuAddress == diagnosticAddress)
                        {
                            return combinedEcuHousingEntry;
                        }
                    }
                }
            }

            return null;
        }

        public IEcuLogisticsEntry GetEcuLogisticsEntry(int diagnosticAddress)
        {
            if (EcuCharacteristics == null)
            {
                return null;
            }

            foreach (IEcuLogisticsEntry ecuLogisticsEntry in EcuCharacteristics.ecuTable)
            {
                if (diagnosticAddress == ecuLogisticsEntry.DiagAddress)
                {
                    return ecuLogisticsEntry;
                }
            }

            return null;
        }

        [PreserveSource(Hint = "Added")]
        public static string GetBackupBasePath(string istaFolder)
        {
            string pathConfig = ConfigSettings.getPathString("BMW.Rheingold.Programming.PsdzBackupDataPath", "%ISPIDATA%\\BMW\\ISPI\\data\\TRIC\\ISTA\\Temp\\");
            string pathString = null;
            if (!string.IsNullOrEmpty(pathConfig))
            {
                if (Path.IsPathRooted(pathConfig))
                {
                    if (Directory.Exists(pathConfig))
                    {
                        pathString = pathConfig;
                    }
                }
            }

            if (string.IsNullOrEmpty(pathString))
            {
                pathString = Path.Combine(istaFolder, "Temp");
            }
            return pathString;
        }

        [PreserveSource(Hint = "Return value added")]
        public bool SetPathToBackupData(string vin17)
        {
            hasVinBackupDataFolder = false;
            string pathString = GetBackupBasePath(IstaFolder);
            if (string.IsNullOrEmpty(pathString))
            {
                Log.Warning("PsdzContext.SetPathToBackupData()", "Backup data path (\"BMW.Rheingold.Programming.PsdzBackupDataPath\") is not set. Thus data recovery is disabled.");
                PathToBackupData = null;
                return false;
            }
            if (string.IsNullOrEmpty(vin17))
            {
                PathToBackupData = Path.GetFullPath(pathString);
            }
            else
            {
                hasVinBackupDataFolder = true;
                PathToBackupData = Path.GetFullPath(Path.Combine(pathString, vin17));
            }
            if (!Directory.Exists(PathToBackupData))
            {
                Log.Info("PsdzContext.SetPathToBackupData()", "Backup data path (\"{0}\") is not an existing directory. Try to create...", PathToBackupData);
                Directory.CreateDirectory(PathToBackupData);
            }
            Log.Info("PsdzContext.SetPathToBackupData()", "Backup data path: \"{0}\"", PathToBackupData);
            return true;
        }

        internal void CleanupBackupData()
        {
            try
            {
                if (string.IsNullOrEmpty(PathToBackupData) || !Directory.Exists(PathToBackupData))
                {
                    Log.Info("PsdzContext.CleanupBackupData()", "Missing backup folder: '{0}'", PathToBackupData ?? string.Empty);
                    return;
                }
                if (!string.IsNullOrEmpty(PathToBackupData) && hasVinBackupDataFolder && !Directory.EnumerateFileSystemEntries(PathToBackupData).Any())
                {
                    Directory.Delete(PathToBackupData);
                    Log.Info("PsdzContext.CleanupBackupData()", "Empty backup folder ('{0}') deleted!", PathToBackupData);
                    hasVinBackupDataFolder = false;
                }
            }
            catch (Exception e)
            {
                Log.Error("PsdzContext.CleanupBackupData() Exception: {0}", e.Message);
            }
        }

        [PreserveSource(Hint = "Added")]
        public bool HasBackupDataDir()
        {
            if (!string.IsNullOrEmpty(PathToBackupData) && this.hasVinBackupDataFolder && Directory.Exists(PathToBackupData))
            {
                return true;
            }

            return false;
        }

        [PreserveSource(Hint = "Added")]
        public bool HasBackupData()
        {
            if (!string.IsNullOrEmpty(PathToBackupData) && this.hasVinBackupDataFolder &&
                    Directory.Exists(PathToBackupData) && Directory.EnumerateFileSystemEntries(PathToBackupData).Any<string>())
            {
                return true;
            }

            return false;
        }

        [PreserveSource(Hint = "Added, keep directory")]
        public bool RemoveBackupData()
        {
            if (!string.IsNullOrEmpty(PathToBackupData) && this.hasVinBackupDataFolder)
            {
                try
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(PathToBackupData);
                    if (directoryInfo.Exists)
                    {
                        foreach (FileInfo file in directoryInfo.GetFiles())
                        {
                            file.Delete();
                        }

                        foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
                        {
                            dir.Delete();
                        }
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }

        [PreserveSource(Hint = "From ProgrammingSession.AddIdividualDataFilesToPuk")]
        public bool AddIdividualDataFilesToPuk()
        {
            try
            {
                if (!HasBackupData())
                {
                    return false;
                }

                string backupPath = PathToBackupData;
                string backupFile = backupPath.TrimEnd('\\') + IdrBackupFileName;
                if (File.Exists(backupFile))
                {
                    File.Delete(backupFile);
                }

                System.IO.Compression.ZipFile.CreateFromDirectory(backupPath, backupFile);

                return true;
            }
            catch (Exception)
            {
                // [IGNORE] ignored
            }

            return false;
        }

        [PreserveSource(Hint = "From ProgrammingSession.DownloadIndividualDataFromPuk")]
        public bool DownloadIndividualDataFromPuk()
        {
            try
            {
                if (!HasIndividualDataFilesInPuk())
                {
                    return false;
                }

                string backupPath = PathToBackupData;
                string backupFile = backupPath.TrimEnd('\\') + IdrBackupFileName;
                if (!File.Exists(backupFile))
                {
                    return false;
                }

                System.IO.Compression.ZipFile.ExtractToDirectory(backupFile, backupPath);

                return true;
            }
            catch (Exception)
            {
                // [IGNORE] ignored
            }

            return false;
        }

        [PreserveSource(Hint = "Added")]
        public bool HasIndividualDataFilesInPuk()
        {
            try
            {
                if (!HasBackupDataDir())
                {
                    return false;
                }

                string backupPath = PathToBackupData;
                string backupFile = backupPath.TrimEnd('\\') + IdrBackupFileName;
                if (!File.Exists(backupFile))
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                // [IGNORE] ignored
            }

            return false;
        }

        [PreserveSource(Hint = "From ProgrammingSession.DeleteIndividualDataFromPuk")]
        public bool DeleteIndividualDataFromPuk()
        {
            try
            {
                if (!HasBackupDataDir())
                {
                    return false;
                }

                string backupPath = PathToBackupData;
                string backupFile = backupPath.TrimEnd('\\') + IdrBackupFileName;
                if (File.Exists(backupFile))
                {
                    File.Delete(backupFile);
                }

                return true;
            }
            catch (Exception)
            {
                // [IGNORE] ignored
            }

            return false;
        }

        public void SetFaActual(IPsdzFa fa)
		{
			this.FaActual = fa;
            if (VecInfo != null)
            {   // [UH] [IGNORE] added
                VecInfo.FA = ProgrammingUtils.BuildVehicleFa(fa, DetectVehicle.BrName);
            }
		}

		public void SetFaTarget(IPsdzFa fa)
		{
			this.FaTarget = fa;
            if (VecInfo != null)
            {   // [UH] [IGNORE] added
                VecInfo.TargetFA = ProgrammingUtils.BuildVehicleFa(fa, DetectVehicle.BrName);
            }
		}

		public void SetIstufen(IPsdzIstufenTriple istufenTriple)
		{
			if (istufenTriple != null)
			{
				this.IstufeShipment = istufenTriple.Shipment;
				this.IstufeLast = istufenTriple.Last;
				this.IstufeCurrent = istufenTriple.Current;
				return;
			}
			this.IstufeShipment = null;
			this.IstufeLast = null;
			this.IstufeCurrent = null;
		}

        public void SetPossibleIstufenTarget(IEnumerable<IPsdzIstufe> possibleIstufenTarget)
		{
			this.possibleIstufenTarget = possibleIstufenTarget;
		}

        [PreserveSource(Hint = "Unmodified")]
        public void SetSollverbauung(IPsdzSollverbauung sollverbauung)
		{
			this.Sollverbauung = sollverbauung;
		}

        public void SetSvtActual(IPsdzSvt svt)
		{
			this.SvtActual = svt;
		}

        public void SetTalFilter(IPsdzTalFilter talFilter)
		{
			this.TalFilter = talFilter;
		}

        public void SetTalFilterForECUWithIDRClassicState(IPsdzTalFilter talFilter)
		{
			this.TalFilterForECUWithIDRClassicState = talFilter;
		}

        public void SetTalFilterForIndividualDataTal(IPsdzTalFilter talFilterForIndividualDataTal)
		{
			this.TalFilterForIndividualDataTal = talFilterForIndividualDataTal;
		}

        public bool UpdateVehicle(ProgrammingService2 programmingService)
        {
            EcuCharacteristics = null;
            if (VecInfo == null)
            {
                return false;
            }

            ProgrammingObjectBuilder programmingObjectBuilder = programmingService?.ProgrammingInfos?.ProgrammingObjectBuilder;
            if (programmingObjectBuilder == null)
            {
                return false;
            }

            IDiagnosticsBusinessData service = ServiceLocator.Current.GetService<IDiagnosticsBusinessData>();
            VecInfo.VehicleIdentLevel = IdentificationLevel.VINVehicleReadout;
            VecInfo.ILevelWerk = !string.IsNullOrEmpty(IstufeShipment) ? IstufeShipment : DetectVehicle.ILevelShip;
            VecInfo.ILevel = !string.IsNullOrEmpty(IstufeCurrent) ? IstufeCurrent: DetectVehicle.ILevelCurrent;
            VecInfo.VIN17 = DetectVehicle.Vin;

            if (DetectVehicle.ConstructDate != null)
            {
                VecInfo.Modelljahr = DetectVehicle.ConstructYear;
                VecInfo.Modellmonat = DetectVehicle.ConstructMonth;
                VecInfo.Modelltag = "01";

                if (string.IsNullOrEmpty(VecInfo.BaustandsJahr) || string.IsNullOrEmpty(VecInfo.BaustandsMonat))
                {
                    VecInfo.BaustandsJahr = DetectVehicle.ConstructDate.Value.ToString("yy", CultureInfo.InvariantCulture);
                    VecInfo.BaustandsMonat = DetectVehicle.ConstructDate.Value.ToString("MM", CultureInfo.InvariantCulture);
                }

                if (!VecInfo.FA.AlreadyDone)
                {
                    if (string.IsNullOrEmpty(VecInfo.FA.C_DATE))
                    {
                        VecInfo.FA.C_DATE = DetectVehicle.ConstructDate.Value.ToString("MMyy", CultureInfo.InvariantCulture);
                    }

                    if (VecInfo.FA.C_DATETIME == null)
                    {
                        VecInfo.FA.C_DATETIME = DetectVehicle.ConstructDate.Value;
                    }
                }
            }

            PsdzDatabase.VinRanges vinRangesByVin = programmingService.PsdzDatabase.GetVinRangesByVin17(VecInfo.VINType, VecInfo.VIN7, false, false);
            if (vinRangesByVin != null)
            {
                VecInfo.VINRangeType = vinRangesByVin.TypeKey;
                if (string.IsNullOrEmpty(VecInfo.Modellmonat) || string.IsNullOrEmpty(VecInfo.Modelljahr))
                {
                    if (!string.IsNullOrEmpty(vinRangesByVin.ProductionYear) && !string.IsNullOrEmpty(vinRangesByVin.ProductionMonth))
                    {
                        VecInfo.Modelljahr = vinRangesByVin.ProductionYear;
                        VecInfo.Modellmonat = vinRangesByVin.ProductionMonth.PadLeft(2, '0');
                        VecInfo.Modelltag = "01";

                        if (VecInfo.Modelljahr.Length == 4)
                        {
                            VecInfo.BaustandsJahr = VecInfo.Modelljahr.Substring(2, 2);
                            VecInfo.BaustandsMonat = VecInfo.Modellmonat;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(VecInfo.Modellmonat) && !string.IsNullOrEmpty(VecInfo.Modelljahr))
            {
                VecInfo.ProductionDate = DateTime.ParseExact(string.Format(CultureInfo.InvariantCulture, "{0}.{1}",
                    VecInfo.Modellmonat, VecInfo.Modelljahr), "MM.yyyy", new CultureInfo("de-DE"));
                // [IGNORE] VecInfo.ProductionDateSpecified is set automatically
            }

            VecInfo.Ereihe = DetectVehicle.Series;

            ClientContext clientContext = ClientContext.GetClientContext(VecInfo);
            SetFa(programmingService);
            UiBrand brand = UiBrand.BMWBMWiMINI;
            if (VecInfo.Classification.IsMotorcycle())
            {
                brand = UiBrand.BMWMotorrad;
            }

            if (clientContext != null)
            {
                clientContext.SelectedBrand = brand;
            }

            for (int i = 0; i < 2; i++)
            {
                ObservableCollection<ECU> EcuList = new ObservableCollection<ECU>();
                foreach (PsdzDatabase.EcuInfo ecuInfo in DetectVehicle.EcuListPsdz)
                {
                    ECU ecu = programmingObjectBuilder.Build(ecuInfo.PsdzEcu);
                    if (ecu == null)
                    {
                        ecu = new ECU();
                        ecu.ID_SG_ADR = ecuInfo.Address;
                    }

                    if (string.IsNullOrEmpty(ecu.ECU_NAME))
                    {
                        ecu.ECU_NAME = ecuInfo.Name;
                    }

                    if (string.IsNullOrEmpty(ecu.ECU_SGBD))
                    {
                        ecu.ECU_SGBD = ecuInfo.Sgbd;
                    }

                    if (string.IsNullOrEmpty(ecu.ECU_GRUPPE))
                    {
                        ecu.ECU_GRUPPE = ecuInfo.Grp;
                    }
                    EcuList.Add(ecu);
                }

                VecInfo.ECU = EcuList;
            }

            List<PsdzDatabase.Characteristics> characteristicsList = programmingService.PsdzDatabase.GetVehicleCharacteristics(VecInfo);
            if (characteristicsList == null)
            {
                return false;
            }

            if (!UpdateAllVehicleCharacteristics(characteristicsList, programmingService.PsdzDatabase, VecInfo))
            {
                return false;
            }

            UpdateSALocalizedItems(programmingService, clientContext);

            VecInfo.FA.AlreadyDone = true;
            if (VecInfo.ECU != null && VecInfo.ECU.Count > 1)
            {
                VecInfo.VehicleIdentAlreadyDone = true;
            }
            else
            {
                CalculateECUConfiguration();
            }

            VecInfo.BatteryType = PsdzDatabase.ResolveBatteryType(VecInfo);
            VecInfo.WithLfpBattery = VecInfo.BatteryType == PsdzDatabase.BatteryEnum.LFP;
            VecInfo.WithLfpNCarBattery = VecInfo.BatteryType == PsdzDatabase.BatteryEnum.LFP_NCAR;
            VecInfo.MainSeriesSgbd = DetectVehicle.GroupSgbd;

            // [IGNORE] DetectVehicle.SgbdAdd ist calculated by GetMainSeriesSgbdAdditional anyway
            VecInfo.MainSeriesSgbdAdditional = service.GetMainSeriesSgbdAdditional(VecInfo);

            PerformVecInfoAssignments();
            DetectVehicle.SetVehicleLifeStartDate(VecInfo);

            EcuCharacteristics = VehicleLogistics.GetCharacteristicsPublic(VecInfo);
            return true;
        }

        [PreserveSource(Hint = "From ProgrammingSession")]
        public void SetSollverbauung(ProgrammingService2 programmingService, IPsdzSollverbauung sollverbauung, IDictionary<string, string> orderNumbers = null)
        {
            EcuProgrammingInfos ecuProgrammingInfos = programmingService?.ProgrammingInfos;
            ProgrammingObjectBuilder programmingObjectBuilder = ecuProgrammingInfos?.ProgrammingObjectBuilder;

            IDictionary<string, string> useOrderNumbers = orderNumbers;
            if (useOrderNumbers == null)
            {
                useOrderNumbers = new Dictionary<string, string>();
                programmingObjectBuilder?.FillOrderNumbers(sollverbauung, useOrderNumbers);
            }

            SvtTarget = sollverbauung != null ? programmingObjectBuilder?.Build(sollverbauung, useOrderNumbers) : null;
            ecuProgrammingInfos?.SetSvkTargetForEachEcu(SvtTarget);
            SetSollverbauung(sollverbauung);
        }

        [PreserveSource(Hint = "From ProgrammingSession")]
        public void SetSvtCurrent(ProgrammingService2 programmingService, global::BMW.Rheingold.Psdz.IPsdzStandardSvt standardSvt)
        {
            SetSvtCurrent(programmingService, standardSvt, VecInfo.VIN17);
        }

        [PreserveSource(Hint = "From ProgrammingSession")]
        public void SetSvtCurrent(ProgrammingService2 programmingService, global::BMW.Rheingold.Psdz.IPsdzStandardSvt standardSvt, string vin17)
        {
            EcuProgrammingInfos ecuProgrammingInfos = programmingService?.ProgrammingInfos;
            ProgrammingObjectBuilder programmingObjectBuilder = ecuProgrammingInfos?.ProgrammingObjectBuilder;
            SvtCurrent = programmingObjectBuilder?.Build(standardSvt);
            ecuProgrammingInfos?.SetSvkCurrentForEachEcu(SvtCurrent);
            IPsdzSvt psdzSvt = programmingService?.Psdz?.ObjectBuilder?.BuildSvt(standardSvt, vin17);
            SetSvtActual(psdzSvt);
        }

        public static bool AssignVehicleCharacteristics(List<PsdzDatabase.Characteristics> characteristics, Vehicle vehicle)
        {
            if (vehicle == null)
            {
                return false; 
            }

            VehicleCharacteristicIdent vehicleCharacteristicIdent = new VehicleCharacteristicIdent(new NugetLogger());
            foreach (PsdzDatabase.Characteristics characteristic in characteristics)
            {
                if (string.IsNullOrEmpty(vehicle.VerkaufsBezeichnung) || !(characteristic.RootNodeClass == "40143490"))
                {
                    if (!vehicleCharacteristicIdent.AssignVehicleCharacteristic(characteristic.RootNodeClass, vehicle, characteristic))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool UpdateAlpinaCharacteristics(PsdzDatabase database, Vehicle vehicle)
        {
            List<PsdzDatabase.Characteristics> list = new List<PsdzDatabase.Characteristics>();
            database.GetAlpinaCharacteristics(vehicle, list);
            if (list.Any())
            {
                return AssignVehicleCharacteristics(list, vehicle);
            }

            return true;
        }

        public static bool UpdateAllVehicleCharacteristics(List<PsdzDatabase.Characteristics> characteristics, PsdzDatabase database, Vehicle vehicle)
        {
            if (database == null || vehicle == null)
            {
                return false;
            }

            if (!AssignVehicleCharacteristics(characteristics, vehicle))
            {
                return false;
            }

            if (!UpdateAlpinaCharacteristics(database, vehicle))
            {
                return false;
            }

            return true;
        }

        [PreserveSource(Hint = "Modified updates")]
        private void PerformVecInfoAssignments()
        {
            try
            {
                if (VecInfo == null)
                {
                    return;
                }
                if (VecInfo.ECU != null && VecInfo.ECU.Count > 0)
                {   // [UH] [IGNORE] ECU check added
                    GearboxUtility.PerformGearboxAssignments(VecInfo);
                }
                if (VecInfo.BNType == BNType.UNKNOWN && !string.IsNullOrEmpty(VecInfo.Ereihe))
                {
                    IDiagnosticsBusinessData service = ServiceLocator.Current.GetService<IDiagnosticsBusinessData>();
                    VecInfo.BNType = service.GetBNType(VecInfo);
                }
                // [UH] [IGNORE] Extra block start
                if (string.IsNullOrEmpty(VecInfo.Prodart))
                {
                    if (!VecInfo.Classification.IsMotorcycle())
                    {
                        VecInfo.Prodart = "P";
                    }
                    else
                    {
                        VecInfo.Prodart = "M";
                    }
                }
                if ((string.IsNullOrEmpty(VecInfo.Lenkung) || VecInfo.Lenkung == "UNBEK" || VecInfo.Lenkung.Trim() == string.Empty) && (!string.IsNullOrEmpty(VecInfo.VINType) & (VecInfo.VINType.Length == 4)))
                {
                    switch (VecInfo.VINType[3])
                    {
                        default:
                            VecInfo.Lenkung = "LL";
                            break;
                        case '1':
                        case '3':
                        case '5':
                        case 'C':
                            VecInfo.Lenkung = "LL";
                            break;
                        case '2':
                        case '6':
                            VecInfo.Lenkung = "RL";
                            break;
                    }
                }
                if (string.IsNullOrWhiteSpace(VecInfo.BaseVersion) && (!string.IsNullOrEmpty(VecInfo.VINType) & (VecInfo.VINType.Length == 4)))
                {
                    switch (VecInfo.VINType[3])
                    {
                        case '3':
                        case 'C':
                            VecInfo.BaseVersion = "US";
                            break;
                        case '1':
                        case '5':
                            VecInfo.BaseVersion = "ECE";
                            break;
                    }
                }
                if (string.IsNullOrEmpty(VecInfo.Land) || VecInfo.Land == "UNBEK")
                {
                    if (!string.IsNullOrEmpty(VecInfo.VINType) & (VecInfo.VINType.Length == 4))
                    {
                        switch (VecInfo.VINType[3])
                        {
                            default:
                                VecInfo.Land = "EUR";
                                break;
                            case '1':
                            case '2':
                                VecInfo.Land = "EUR";
                                break;
                            case '3':
                            case '4':
                            case 'C':
                                VecInfo.Land = "USA";
                                break;
                        }
                    }
                    if (VecInfo.HasSA("807") && VecInfo.Prodart == "P")
                    {
                        VecInfo.Land = "JP";
                    }
                    if (VecInfo.HasSA("8AA") && VecInfo.Prodart == "P")
                    {
                        VecInfo.Land = "CHN";
                    }
                }
                // [UH] [IGNORE] Extra block end
                if (string.IsNullOrEmpty(VecInfo.Modelljahr) && !string.IsNullOrEmpty(VecInfo.ILevelWerk))
                {
                    try
                    {
                        if (Regex.IsMatch(VecInfo.ILevelWerk, "^\\w{4}[_\\-]\\d{2}[_\\-]\\d{2}[_\\-]\\d{3}$"))
                        {
                            VecInfo.BaustandsJahr = VecInfo.ILevelWerk.Substring(5, 2);
                            VecInfo.BaustandsMonat = VecInfo.ILevelWerk.Substring(8, 2);
                            int num = Convert.ToInt32(VecInfo.ILevelWerk.Substring(5, 2), CultureInfo.InvariantCulture);
                            VecInfo.Modelljahr = ((num <= 50) ? (num + 2000) : (num + 1900)).ToString(CultureInfo.InvariantCulture);
                            VecInfo.Modellmonat = VecInfo.ILevelWerk.Substring(8, 2);
                            VecInfo.Modelltag = "01";
                            Log.Info("Missing construction date (year: {0}, month: {1}) retrieved from iLevel plant ('{2}')", VecInfo.Modelljahr, VecInfo.Modellmonat, VecInfo.ILevelWerk);
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.WarningException("VehicleIdent.finalizeFASTAHeader()", exception);
                    }
                }
                if (string.IsNullOrEmpty(VecInfo.MainSeriesSgbd))
                {   // [UH] [IGNORE] simplified
                    VecInfo.MainSeriesSgbd = VehicleLogistics.getBrSgbd(VecInfo);
                }
                if (!string.IsNullOrEmpty(VecInfo.Motor) && !(VecInfo.Motor == "UNBEK"))
                {
                    return;
                }
                ECU eCUbyECU_GRUPPE = VecInfo.getECUbyECU_GRUPPE("D_MOTOR");
                if (eCUbyECU_GRUPPE == null)
                {
                    eCUbyECU_GRUPPE = VecInfo.getECUbyECU_GRUPPE("G_MOTOR");
                }
                if (eCUbyECU_GRUPPE != null && !string.IsNullOrEmpty(eCUbyECU_GRUPPE.VARIANTE))
                {
                    Match match = Regex.Match(eCUbyECU_GRUPPE.VARIANTE, "[SNM]\\d\\d");
                    if (match.Success)
                    {
                        VecInfo.Motor = match.Value;
                    }
                }
            }
            catch (Exception exception2)
            {
                Log.WarningException("VehicleIdent.finalizeFASTAHeader()", exception2);
            }
        }

        private void CalculateECUConfiguration()
        {
            if (VecInfo.BNType != BNType.BN2020_MOTORBIKE && VecInfo.BNType != BNType.BNK01X_MOTORBIKE && VecInfo.BNType != BNType.BN2000_MOTORBIKE)
            {
                if (VecInfo.BNType == BNType.IBUS)
                {
                    VehicleLogistics.CalculateECUConfiguration(VecInfo, null);
                    if (VecInfo.ECU != null && VecInfo.ECU.Count > 1)
                    {
                        VecInfo.VehicleIdentAlreadyDone = true;
                    }
                }
                return;
            }

            VehicleLogistics.CalculateECUConfiguration(VecInfo, null);
            if (VecInfo.ECU != null && VecInfo.ECU.Count > 1)
            {
                VecInfo.VehicleIdentAlreadyDone = true;
            }
        }

        public List<PsdzDatabase.EcuInfo> GetEcuList(bool individualOnly = false)
        {
            List<PsdzDatabase.EcuInfo> ecuList = new List<PsdzDatabase.EcuInfo>();
            try
            {
                foreach (PsdzDatabase.EcuInfo ecuInfo in DetectVehicle.EcuListPsdz)
                {
                    if (individualOnly)
                    {
                        if (VecInfo.Classification.IsMotorcycle() || ecuInfo.HasIndividualData)
                        {
                            ecuList.Add(ecuInfo);
                        }
                    }
                    else
                    {
                        ecuList.Add(ecuInfo);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return ecuList;
        }

        public bool SetFa(ProgrammingService2 programmingService)
        {
            try
            {
                if (VecInfo.FA.AlreadyDone)
                {
                    return true;
                }

                if (string.IsNullOrEmpty(VecInfo.FA.LACK))
                {
                    VecInfo.FA.LACK = DetectVehicle.Paint;
                }

                if (string.IsNullOrEmpty(VecInfo.FA.POLSTER))
                {
                    VecInfo.FA.POLSTER = DetectVehicle.Upholstery;
                }

                if (string.IsNullOrEmpty(VecInfo.FA.STANDARD_FA))
                {
                    VecInfo.FA.STANDARD_FA = DetectVehicle.StandardFa;
                }

                if (string.IsNullOrEmpty(VecInfo.FA.TYPE))
                {
                    VecInfo.FA.TYPE = DetectVehicle.TypeKey;
                }

                if (VecInfo.FA.SA.Count == 0 && VecInfo.FA.HO_WORT.Count == 0 &&
                    VecInfo.FA.E_WORT.Count == 0 && VecInfo.FA.ZUSBAU_WORT.Count == 0)
                {
                    foreach (string salapa in DetectVehicle.Salapa)
                    {
                        VecInfo.FA.SA.AddIfNotContains(salapa);
                    }

                    foreach (string hoWord in DetectVehicle.HoWords)
                    {
                        VecInfo.FA.HO_WORT.AddIfNotContains(hoWord);
                    }

                    foreach (string eWord in DetectVehicle.EWords)
                    {
                        VecInfo.FA.E_WORT.AddIfNotContains(eWord);
                    }

                    foreach (string zbWord in DetectVehicle.ZbWords)
                    {
                        VecInfo.FA.ZUSBAU_WORT.AddIfNotContains(zbWord);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool UpdateSALocalizedItems(ProgrammingService2 programmingService, ClientContext clientContext)
        {
            try
            {
                if (clientContext == null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(VecInfo.Prodart) || VecInfo.BrandName == null)
                {
                    return false;
                }

                string language = clientContext.Language;
                string prodArt = PsdzDatabase.GetProdArt(VecInfo);

                FillSaLocalizedItems(programmingService, language, DetectVehicle.Salapa, prodArt);
                FillSaLocalizedItems(programmingService, language, DetectVehicle.HoWords, prodArt);
                FillSaLocalizedItems(programmingService, language, DetectVehicle.EWords, prodArt);
                FillSaLocalizedItems(programmingService, language, DetectVehicle.ZbWords, prodArt);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void FillSaLocalizedItems(ProgrammingService2 programmingService, string language, List<string> source, string prodArt)
        {
            foreach (string item in source)
            {
                string key = FormatConverterBase.FillWithZeros(item, 4, new NugetLogger());
                if (VecInfo.FA.SaLocalizedItems.FirstOrDefault(x => x.Id == key) == null)
                {
                    PsdzDatabase.SaLaPa saLaPa = programmingService.PsdzDatabase.GetSaLaPaByProductTypeAndSalesKey(prodArt, key);
                    if (saLaPa != null)
                    {
                        VecInfo.FA.SaLocalizedItems.Add(new LocalizedSAItem(key, saLaPa.EcuTranslation.GetTitle(language)));
                    }
                }
            }
        }

        public string GetLocalizedSaString()
        {
            StringBuilder sb = new StringBuilder();
            if (VecInfo.FA != null && VecInfo.FA.SaLocalizedItems.Count > 0)
            {
                foreach (LocalizedSAItem saLocalized in VecInfo.FA.SaLocalizedItems)
                {
                    if (saLocalized != null)
                    {
                        sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "SA={0}, Title='{1}'", saLocalized.Id, saLocalized.Title));
                    }
                }
            }

            return sb.ToString();
        }

        [PreserveSource(Hint = "IDisposable added")]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [PreserveSource(Hint = "IDisposable added")]
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
				if (disposing)
                {
                    if (DetectVehicle != null)
                    {
                        DetectVehicle.Dispose();
                        DetectVehicle = null;
                    }

                    VecInfo = null;
                }

                _disposed = true;
            }
        }

	}
}
