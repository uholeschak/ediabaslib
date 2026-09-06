using System;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient.Core;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PsdzClient;

#pragma warning disable CS0649
namespace BMW.Rheingold.CoreFramework
{
    public static class FaultCodeConverters
    {
        private static ICollection<XEP_FAULTCLASSES> faultClasses;

        static FaultCodeConverters()
        {
            //[-] faultClasses = DatabaseProviderFactory.Instance.GetFaultClasses();
        }

        public static string FormatFort(DTC dtc)
        {
            return GetErrorCode(dtc.F_ORT, dtc.IsVirtual, dtc.IsCombined);
        }

        public static string FormatFortStrict(DTC dtc)
        {
            return GetErrorCode(dtc.F_ORT, dtc.IsVirtual, dtc.IsCombined, strict: true);
        }

        public static string GetErrorCode(long? fOrt, bool isVirtualDTC, bool isCombined, bool strict = false)
        {
            try
            {
                if (isVirtualDTC)
                {
                    return isCombined ? string.Format(CultureInfo.InvariantCulture, $"S{fOrt:X4}", fOrt) : string.Format(CultureInfo.InvariantCulture, $"S {fOrt:X4}", fOrt);
                }
                return $"{fOrt:X6}";
            }
            catch (Exception ex)
            {
                Log.Error("FaultCodeConverters.GetErrorCode()", "Failed to format fault code. Returning \"######\" instead. {0}", ex);
                return "######";
            }
        }

        public static string GetFaultClass(DTC dtc, ICollection<ZFSResult> zfs)
        {
            try
            {
                if (dtc != null && zfs != null && dtc.Current != null && dtc.Current.F_UW_ZEIT.HasValue)
                {
                    if (zfs.FirstOrDefault((ZFSResult item) => item.STAT_DM_MELDUNG_NR == dtc.F_ORT && item.STAT_DM_ZEITSTEMPEL == (ulong?)dtc.Current.F_UW_ZEIT.Value && item.STAT_SYSKONTEXT_SPANNUNG_MIN_WERT < 9.0 && item.STAT_SYSKONTEXT_SPANNUNG_MIN_WERT >= 0.0) != null)
                    {
                        //[-] return GetFaultClassById(37750384011m).Title;
                    }
                    if (zfs.FirstOrDefault((ZFSResult item) => item.STAT_DM_MELDUNG_NR == dtc.F_ORT && item.STAT_DM_ZEITSTEMPEL == (ulong?)dtc.Current.F_UW_ZEIT.Value && item.STAT_SYSKONTEXT_SPANNUNG_MIN_WERT > 16.0) != null)
                    {
                        //[-] return GetFaultClassById(37750520971m).Title;
                    }
                }
                if (dtc != null && dtc.F_EREIGNIS_DTC == 1)
                {
                    //[-] return GetFaultClassById(37750559371m).Title;
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("FaultCodeConverters.GetFaultClass()", exception);
            }
            return null;
        }

        public static string LocalizedFaultLabel(ECU ecu, DTC dtc, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
        {
            Fault fault = new Fault(ecu, dtc, null, vehicle.Classification.IsNewFaultMemoryActive);
            fault.ResolveLabels(vehicle, ffmDynamicResolver);
            return fault.FaultLabel;
        }

        public static ILocalizedTitle LocalizedFaultLabelLanguages(ECU ecu, DTC dtc, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
        {
            Fault fault = new Fault(ecu, dtc, null, vehicle.Classification.IsNewFaultMemoryActive);
            fault.ResolveLabels(vehicle, ffmDynamicResolver);
            return fault.XepFaultLabel;
        }

        public static string LocalizedExistingLabel(ECU ecu, DTC dtc, Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
        {
            Fault fault = new Fault(ecu, dtc, null, vehicle.Classification.IsNewFaultMemoryActive);
            fault.ResolveLabels(vehicle, ffmDynamicResolver);
            return fault.ExistingLabel;
        }

        [PreserveSource(Hint = "IDatabaseProvider", SignatureModified = true)]
        public static void SetFaultClasses(PsdzDatabase db)
        {
            //[-] faultClasses = db.GetFaultClasses();
        }

        private static XEP_FAULTCLASSES GetFaultClassById(decimal id)
        {
            if (faultClasses != null)
            {
                return faultClasses.FirstOrDefault((XEP_FAULTCLASSES item) => item.Id == id);
            }
            return null;
        }
    }
}
