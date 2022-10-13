using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Programming.API;
using BMW.Rheingold.Programming.Common;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Svb;
using BMW.Rheingold.Psdz.Model.Swt;
using BMW.Rheingold.Psdz.Model.Tal;
using BMW.Rheingold.Psdz.Model.Tal.TalFilter;
using BmwFileReader;
using PsdzClient.Core;
using PsdzClient.Utility;

namespace PsdzClient.Programming
{
	public class PsdzContext : IPsdzContext, IDisposable
	{
        private const string IdrBackupFileName = "_IDR_Files.backup";

        public enum BackupTalResult
        {
            Success,
            Failed,
            Error,
            Undefined,
            SuccessEmpty
        }
		
        public PsdzContext(string istaFolder)
        {
            this.IstaFolder = istaFolder;
			this.ExecutionOrderTop = new Dictionary<string, IList<string>>();
			this.ExecutionOrderBottom = new Dictionary<string, IList<string>>();
		}

		public bool IsEmptyBackupTal
		{
			get
			{
				return !this.IsValidBackupTal || !this.IndividualDataBackupTal.TalLines.Any<IPsdzTalLine>();
			}
		}

		public bool IsEmptyPrognosisTal
		{
			get
			{
				return !this.IsValidRestorePrognosisTal || !this.IndividualDataRestorePrognosisTal.TalLines.Any<IPsdzTalLine>();
			}
		}

		public bool IsEmptyRestoreTal
		{
			get
			{
				return !this.IsValidRestoreTal || !this.IndividualDataRestoreTal.TalLines.Any<IPsdzTalLine>();
			}
		}

		public bool IsEmptyTal
		{
			get
			{
				return !this.IsValidTal || !this.Tal.TalLines.Any<IPsdzTalLine>();
			}
		}

        public BackupTalResult CheckBackupTal()
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

		public bool IsValidBackupTal
		{
			get
			{
				return this.IndividualDataBackupTal != null;
			}
		}

		public bool IsValidEcuListActual
		{
			get
			{
				return this.EcuListActual != null && this.EcuListActual.Any<IPsdzEcuIdentifier>();
			}
		}

		public bool IsValidFaActual
		{
			get
			{
				return this.FaActual != null && this.FaActual.IsValid;
			}
		}

		public bool IsValidFaTarget
		{
			get
			{
				return this.FaTarget != null && this.FaTarget.IsValid;
			}
		}

		public bool IsValidRestorePrognosisTal
		{
			get
			{
				return this.IndividualDataRestorePrognosisTal != null;
			}
		}

		public bool IsValidRestoreTal
		{
			get
			{
				return this.IndividualDataRestoreTal != null;
			}
		}

		public bool IsValidSollverbauung
		{
			get
			{
				return this.Sollverbauung != null;
			}
		}

		public bool IsValidSvtActual
		{
			get
			{
				return this.SvtActual != null && this.SvtActual.IsValid;
			}
		}

		public bool IsValidTal
		{
			get
			{
				return this.Tal != null;
			}
		}

		public string LatestPossibleIstufeTarget
		{
			get
			{
				IEnumerable<IPsdzIstufe> enumerable = this.possibleIstufenTarget;
				if (enumerable != null && enumerable.Any<IPsdzIstufe>())
				{
					return enumerable.Last<IPsdzIstufe>().Value;
				}
				return null;
			}
		}

		public string ProjectName { get; set; }

		public IPsdzSwtAction SwtAction { get; set; }

		public string VehicleInfo { get; set; }

		public string PathToBackupData { get; set; }

        public bool PsdZBackUpModeSet { get; set; }

		public IPsdzTalFilter TalFilterForIndividualDataTal { get; private set; }

        public IPsdzConnection Connection { get; set; }

        public DetectVehicle DetectVehicle { get; set; }

        public Vehicle VecInfo { get; set; }

        public IEnumerable<IPsdzEcuIdentifier> EcuListActual { get; set; }

        public IDictionary<string, IList<string>> ExecutionOrderBottom { get; private set; }

        public IDictionary<string, IList<string>> ExecutionOrderTop { get; private set; }

        public IPsdzFa FaActual { get; private set; }

        public IPsdzFa FaTarget { get; private set; }

        public IPsdzTal IndividualDataBackupTal { get; set; }

        public IPsdzTal IndividualDataRestorePrognosisTal { get; set; }

        public IPsdzTal IndividualDataRestoreTal { get; set; }

        public IPsdzSollverbauung Sollverbauung { get; private set; }

        public IPsdzSvt SvtActual { get; private set; }

        public IPsdzTal Tal { get; set; }

        public IPsdzTalFilter TalFilter { get; private set; }

        public IPsdzTalFilter TalFilterForECUWithIDRClassicState { get; private set; }

        public IPsdzTal TalForECUWithIDRClassicState { get; set; }

        public IEnumerable<IPsdzTargetSelector> TargetSelectors { get; set; }

        public string IstaFolder { get; private set; }

        public BaseEcuCharacteristics EcuCharacteristics { get; private set; }

        public string GetBaseVariant(int diagnosticAddress)
		{
			if (this.SvtActual.Ecus.Any((IPsdzEcu ecu) => diagnosticAddress == ecu.PrimaryKey.DiagAddrAsInt))
			{
				return this.SvtActual.Ecus.Single((IPsdzEcu ecu) => diagnosticAddress == ecu.PrimaryKey.DiagAddrAsInt).BaseVariant;
			}
			return string.Empty;
		}

        public ICombinedEcuHousingEntry GetEcuHousingEntry(int diagnosticAddress)
        {
            if (EcuCharacteristics == null || EcuCharacteristics.combinedEcuHousingTable == null)
            {
                return null;
            }

            foreach (ICombinedEcuHousingEntry combinedEcuHousingEntry in EcuCharacteristics.combinedEcuHousingTable)
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

        public bool SetPathToBackupData(string vin17)
		{
			this.hasVinBackupDataFolder = false;
			string pathString = Path.Combine(IstaFolder, "Temp");
			if (string.IsNullOrEmpty(pathString))
			{
				this.PathToBackupData = null;
				return false;
			}
			if (string.IsNullOrEmpty(vin17))
			{
				this.PathToBackupData = Path.GetFullPath(pathString);
			}
            else
			{
				this.hasVinBackupDataFolder = true;
                this.PathToBackupData = Path.GetFullPath(Path.Combine(pathString, vin17));
            }

            if (!string.IsNullOrEmpty(PathToBackupData) && !Directory.Exists(this.PathToBackupData))
			{
				Directory.CreateDirectory(this.PathToBackupData);
			}

            return true;
        }

        public void CleanupBackupData()
        {
            if (!string.IsNullOrEmpty(PathToBackupData) && this.hasVinBackupDataFolder &&
                Directory.Exists(PathToBackupData) && !Directory.EnumerateFileSystemEntries(PathToBackupData).Any<string>())
            {
                Directory.Delete(PathToBackupData);
            }

            if (!HasBackupDataDir())
            {
                this.hasVinBackupDataFolder = false;
            }
        }

        public bool HasBackupDataDir()
        {
            if (!string.IsNullOrEmpty(PathToBackupData) && this.hasVinBackupDataFolder && Directory.Exists(PathToBackupData))
            {
                return true;
            }

            return false;
        }

        public bool HasBackupData()
        {
            if (!string.IsNullOrEmpty(PathToBackupData) && this.hasVinBackupDataFolder &&
                    Directory.Exists(PathToBackupData) && Directory.EnumerateFileSystemEntries(PathToBackupData).Any<string>())
            {
                return true;
            }

            return false;
        }

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

        public bool SaveIDRFilesToPuk()
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
                // ignored
            }

            return false;
        }

        public bool GetIDRFilesFromPuk()
        {
            try
            {
                if (!HasIDRFilesInPuk())
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
                // ignored
            }

            return false;
        }

        public bool HasIDRFilesInPuk()
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
                // ignored
            }

            return false;
        }

        public bool DeleteIDRFilesFromPuk()
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
                // ignored
            }

            return false;
        }

        public void SetFaActual(IPsdzFa fa)
		{
			this.FaActual = fa;
            if (VecInfo != null)
            {
                VecInfo.FA = ProgrammingUtils.BuildVehicleFa(fa, DetectVehicle.ModelSeries);
            }
		}

		public void SetFaTarget(IPsdzFa fa)
		{
			this.FaTarget = fa;
            if (VecInfo != null)
            {
                VecInfo.TargetFA = ProgrammingUtils.BuildVehicleFa(fa, DetectVehicle.ModelSeries);
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

        public bool UpdateVehicle(ProgrammingService programmingService)
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

            VecInfo.VehicleIdentLevel = IdentificationLevel.VINVehicleReadout;
            VecInfo.VehicleIdentAlreadyDone = true;
            VecInfo.ILevelWerk = !string.IsNullOrEmpty(IstufeShipment) ? IstufeShipment : DetectVehicle.ILevelShip;
            VecInfo.ILevel = !string.IsNullOrEmpty(IstufeCurrent) ? IstufeCurrent: DetectVehicle.ILevelCurrent;
            VecInfo.VIN17 = DetectVehicle.Vin;
            if (DetectVehicle.ConstructDate != null)
            {
                VecInfo.Modelljahr = DetectVehicle.ConstructYear;
                VecInfo.Modellmonat = DetectVehicle.ConstructMonth;
                VecInfo.Modelltag = "01";
                VecInfo.ProductionDate = DetectVehicle.ConstructDate.Value;
                VecInfo.ProductionDateSpecified = true;

                if (string.IsNullOrEmpty(VecInfo.BaustandsJahr) || string.IsNullOrEmpty(VecInfo.BaustandsMonat))
                {
                    VecInfo.BaustandsJahr = DetectVehicle.ConstructDate.Value.ToString("yy", CultureInfo.InvariantCulture);
                    VecInfo.BaustandsMonat = DetectVehicle.ConstructDate.Value.ToString("MM", CultureInfo.InvariantCulture);
                }

                if (string.IsNullOrEmpty(VecInfo.FA.C_DATE))
                {
                    VecInfo.FA.C_DATE = DetectVehicle.ConstructDate.Value.ToString("MMyy", CultureInfo.InvariantCulture);
                }

                if (VecInfo.FA.C_DATETIME == null)
                {
                    VecInfo.FA.C_DATETIME = DetectVehicle.ConstructDate.Value;
                }
            }

            VecInfo.Ereihe = DetectVehicle.Series;
            VecInfo.SetVINRangeTypeFromVINRanges();

            CharacteristicExpression.EnumBrand brand = CharacteristicExpression.EnumBrand.BMWBMWiMINI;
            if (VecInfo.IsMotorcycle())
            {
                brand = CharacteristicExpression.EnumBrand.BMWMotorrad;
            }

            ClientContext clientContext = ClientContext.GetClientContext(VecInfo);
            if (clientContext != null)
            {
                clientContext.SelectedBrand = brand;
            }

            for (int i = 0; i < 2; i++)
            {
                ObservableCollection<ECU> EcuList = new ObservableCollection<ECU>();
                foreach (PdszDatabase.EcuInfo ecuInfo in DetectVehicle.EcuList)
                {
                    IEcuObj ecuObj = programmingObjectBuilder.Build(ecuInfo.PsdzEcu);
                    ECU ecu = programmingObjectBuilder.Build(ecuObj);
                    if (ecu != null)
                    {
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
                }

                VecInfo.ECU = EcuList;
            }

            List<PdszDatabase.Characteristics> characteristicsList = programmingService.PdszDatabase.GetVehicleCharacteristics(VecInfo);
            if (characteristicsList == null)
            {
                return false;
            }
            VehicleCharacteristicIdent vehicleCharacteristicIdent = new VehicleCharacteristicIdent();

            foreach (PdszDatabase.Characteristics characteristics in characteristicsList)
            {
                if (!vehicleCharacteristicIdent.AssignVehicleCharacteristic(characteristics.RootNodeClass, VecInfo, characteristics))
                {
                    return false;
                }
            }

            VecInfo.BNType = VehicleLogistics.getBNType(VecInfo);
            VecInfo.FA.AlreadyDone = true;
            VecInfo.BNMixed = VehicleLogistics.getBNMixed(VecInfo.Ereihe, VecInfo.FA);
            VecInfo.WithLfpBattery = programmingService.PdszDatabase.ResolveBatteryType(VecInfo) == PdszDatabase.BatteryEnum.LFP;
            VecInfo.MainSeriesSgbd = VehicleLogistics.getBrSgbd(VecInfo);
            EcuCharacteristics = VehicleLogistics.GetCharacteristics(VecInfo);
            return true;
        }

        public List<PdszDatabase.EcuInfo> GetEcuList(bool individualOnly = false)
        {
            List<PdszDatabase.EcuInfo> ecuList = new List<PdszDatabase.EcuInfo>();
            try
            {
                foreach (PdszDatabase.EcuInfo ecuInfo in DetectVehicle.EcuList)
                {
                    if (individualOnly)
                    {
                        if (VecInfo.IsMotorcycle() || ecuInfo.HasIndividualData)
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

        private bool _disposed;
		private bool hasVinBackupDataFolder;

		private IEnumerable<IPsdzIstufe> possibleIstufenTarget;

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
                if (DetectVehicle != null)
                {
                    DetectVehicle.Dispose();
                    DetectVehicle = null;
                }

                VecInfo = null;

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
