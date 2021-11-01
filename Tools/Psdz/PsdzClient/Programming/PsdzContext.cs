using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace PsdzClient.Programming
{
	public class PsdzContext : IPsdzContext, IDisposable
	{
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

		public IPsdzSwtAction SwtAction { get; internal set; }

		public string VehicleInfo { get; set; }

		public string PathToBackupData { get; set; }

		public bool PsdZBackUpModeSet { get; set; }

		public IPsdzTalFilter TalFilterForIndividualDataTal { get; private set; }

        public IPsdzConnection Connection { get; set; }

        public DetectVehicle DetectVehicle { get; set; }

        public Vehicle Vehicle { get; set; }

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

		public string GetBaseVariant(int diagnosticAddress)
		{
			if (this.SvtActual.Ecus.Any((IPsdzEcu ecu) => diagnosticAddress == ecu.PrimaryKey.DiagAddrAsInt))
			{
				return this.SvtActual.Ecus.Single((IPsdzEcu ecu) => diagnosticAddress == ecu.PrimaryKey.DiagAddrAsInt).BaseVariant;
			}
			return string.Empty;
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
			if (!Directory.Exists(this.PathToBackupData))
			{
				Directory.CreateDirectory(this.PathToBackupData);
			}

            return true;
        }

		public void CleanupBackupData()
		{
			if (!string.IsNullOrEmpty(this.PathToBackupData) && this.hasVinBackupDataFolder && !Directory.EnumerateFileSystemEntries(this.PathToBackupData).Any<string>())
			{
				Directory.Delete(this.PathToBackupData);
				this.hasVinBackupDataFolder = false;
			}
		}

        public void SetFaActual(IPsdzFa fa)
		{
			this.FaActual = fa;
		}

        public void SetFaTarget(IPsdzFa fa)
		{
			this.FaTarget = fa;
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

        public void UpdateVehicle(ProgrammingObjectBuilder programmingObjectBuilder, IPsdzStandardSvt psdzStandardSvt)
        {
            if (Vehicle == null)
            {
                return;
            }

            Vehicle.ILevelWerk = IstufeShipment;
            Vehicle.ILevel = IstufeCurrent;
            Vehicle.BNType = GetBnType();
            Vehicle.VIN17 = DetectVehicle.Vin;
            Vehicle.Modelljahr = DetectVehicle.ConstructYear;
            Vehicle.Modellmonat = DetectVehicle.ConstructMonth;

            Vehicle.FA = ProgrammingUtils.BuildVehicleFa(FaActual, DetectVehicle.ModelSeries);

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

                Vehicle.ECU = EcuList;
            }
		}

		public BNType GetBnType()
        {
            switch (DetectVehicle.BnType)
            {
                case VehicleInfoBmw.BnType.BN2000:
                    return BNType.BN2000;

                case VehicleInfoBmw.BnType.BN2020:
                    return BNType.BN2020;

                case VehicleInfoBmw.BnType.IBUS:
                    return BNType.IBUS;

                case VehicleInfoBmw.BnType.BN2000_MOTORBIKE:
                    return BNType.BN2000_MOTORBIKE;

                case VehicleInfoBmw.BnType.BN2020_MOTORBIKE:
                    return BNType.BN2020_MOTORBIKE;

                case VehicleInfoBmw.BnType.BNK01X_MOTORBIKE:
                    return BNType.BNK01X_MOTORBIKE;

                case VehicleInfoBmw.BnType.BEV2010:
                    return BNType.BEV2010;

                case VehicleInfoBmw.BnType.BN2000_MORGAN:
                    return BNType.BN2000_MORGAN;

                case VehicleInfoBmw.BnType.BN2000_WIESMANN: 
                    return BNType.BN2000_WIESMANN;

                case VehicleInfoBmw.BnType.BN2000_RODING:
                    return BNType.BN2000_RODING;

                case VehicleInfoBmw.BnType.BN2000_PGO:
                    return BNType.BN2000_PGO;

                case VehicleInfoBmw.BnType.BN2000_GIBBS:
                    return BNType.BN2000_GIBBS;

                case VehicleInfoBmw.BnType.BN2020_CAMPAGNA:
                    return BNType.BN2020_CAMPAGNA;
            }

            return BNType.UNKNOWN;
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
                CleanupBackupData();
                if (DetectVehicle != null)
                {
                    DetectVehicle.Dispose();
                    DetectVehicle = null;
                }

                Vehicle = null;

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
