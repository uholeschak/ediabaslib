using BMW.ISPI.TRIC.ISTA.Contracts.Enums;
using System;

namespace BMW.ISPI.TRIC.ISTA.EcuManager
{
    public sealed class EcuRecord
    {
        public long IdSgAdr { get; }

        public string EcuGroupName { get; }

        public string Variant { get; }

        public EcuDataSource Source { get; }

        public DateTime Timestamp { get; }

        public EcuRecord(long idSgAdr, string ecuGroupName, EcuDataSource source, string variant = null)
        {
            IdSgAdr = idSgAdr;
            EcuGroupName = ecuGroupName;
            Source = source;
            Variant = variant;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"ECU 0x{IdSgAdr:X2} ({EcuGroupName}, variant '{Variant}') from source '{Source}' at {Timestamp:O}";
        }
    }
}
