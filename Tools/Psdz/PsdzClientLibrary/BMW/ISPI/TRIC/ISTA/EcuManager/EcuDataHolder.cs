using System;
using System.Collections.Generic;
using System.Linq;
using BMW.ISPI.TRIC.ISTA.Contracts.Enums;

namespace BMW.ISPI.TRIC.ISTA.EcuManager
{
    public sealed class EcuDataHolder
    {
        private readonly object syncRoot = new object();

        private readonly Dictionary<EcuDataSource, List<EcuRecord>> buckets = new Dictionary<EcuDataSource, List<EcuRecord>>();

        public void Add(EcuRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }
            lock (syncRoot)
            {
                if (!buckets.TryGetValue(record.Source, out var value))
                {
                    value = new List<EcuRecord>();
                    buckets[record.Source] = value;
                }
                value.Add(record);
            }
        }

        public IReadOnlyList<EcuRecord> GetBucket(EcuDataSource source)
        {
            lock (syncRoot)
            {
                if (buckets.TryGetValue(source, out var value))
                {
                    return value.ToList();
                }
                return new List<EcuRecord>();
            }
        }

        public IReadOnlyDictionary<EcuDataSource, IReadOnlyList<EcuRecord>> GetAllBuckets()
        {
            lock (syncRoot)
            {
                return ((IEnumerable<KeyValuePair<EcuDataSource, List<EcuRecord>>>)buckets).ToDictionary((Func<KeyValuePair<EcuDataSource, List<EcuRecord>>, EcuDataSource>)((KeyValuePair<EcuDataSource, List<EcuRecord>> kvp) => kvp.Key), (Func<KeyValuePair<EcuDataSource, List<EcuRecord>>, IReadOnlyList<EcuRecord>>)((KeyValuePair<EcuDataSource, List<EcuRecord>> kvp) => kvp.Value.ToList()));
            }
        }

        public IReadOnlyList<EcuRecord> GetRecordsForEcu(long idSgAdr)
        {
            lock (syncRoot)
            {
                return (from record in buckets.Values.SelectMany((List<EcuRecord> bucket) => bucket)
                    where record.IdSgAdr == idSgAdr
                    select record).ToList();
            }
        }

        public void Clear()
        {
            lock (syncRoot)
            {
                buckets.Clear();
            }
        }
    }
}
