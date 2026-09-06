using BMW.Rheingold.CoreFramework.DatabaseProvider;
using System;
using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    [DataContract]
    public struct FaultId
    {
        [DataMember]
        public decimal? DtcId { get; private set; }

        [DataMember]
        public long? FehlerOrt { get; private set; }

        [DataMember]
        public string EcuVariante { get; private set; }

        [DataMember]
        public int? RelevantDtcContextIndex { get; private set; }

        public FaultId(decimal? dtcId, long? fehlerOrt, string ecuVariante)
        {
            DtcId = dtcId;
            FehlerOrt = fehlerOrt;
            EcuVariante = ecuVariante;
            RelevantDtcContextIndex = null;
        }

        public FaultId(Fault fault)
        {
            DtcId = fault.DTC?.Id;
            FehlerOrt = fault.DTC?.F_ORT;
            EcuVariante = fault.ECU?.VARIANTE;
            RelevantDtcContextIndex = fault.RelevantDtcContextIndex;
        }

        private bool Equals(FaultId faultIdToCompare)
        {
            if (RelevantDtcContextIndex != faultIdToCompare.RelevantDtcContextIndex)
            {
                return false;
            }
            if (DtcId.HasValue)
            {
                if (faultIdToCompare.DtcId.HasValue)
                {
                    return DtcId.Value == faultIdToCompare.DtcId.Value;
                }
                return false;
            }
            if (FehlerOrt == faultIdToCompare.FehlerOrt)
            {
                if (!string.IsNullOrEmpty(EcuVariante) || !string.IsNullOrEmpty(faultIdToCompare.EcuVariante))
                {
                    if (!string.IsNullOrEmpty(EcuVariante) && !string.IsNullOrEmpty(faultIdToCompare.EcuVariante))
                    {
                        return EcuVariante.Equals(faultIdToCompare.EcuVariante, StringComparison.OrdinalIgnoreCase);
                    }
                    return false;
                }
                return true;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is FaultId)
            {
                return Equals((FaultId)obj);
            }
            if (obj is Fault)
            {
                return Equals(new FaultId((Fault)obj));
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (DtcId?.GetHashCode() ?? 0) ^ (FehlerOrt?.GetHashCode() ?? 0) ^ (EcuVariante?.GetHashCode() ?? 0) ^ (RelevantDtcContextIndex?.GetHashCode() ?? (-1));
        }
    }
}
