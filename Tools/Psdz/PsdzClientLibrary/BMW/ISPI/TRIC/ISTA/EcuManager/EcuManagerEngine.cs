using BMW.ISPI.TRIC.ISTA.Contracts.Enums;
using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using BMW.Rheingold.ISTA.CoreFramework;

namespace BMW.ISPI.TRIC.ISTA.EcuManager
{
    public class EcuManagerEngine : IEcuListManager
    {
        private const string LogPrefix = "EcuManager";

        private readonly ILogger logger;

        private readonly object ecuListSyncRoot = new object();

        private readonly Dictionary<long, IIdentEcu> ecus = new Dictionary<long, IIdentEcu>();

        public EcuDataHolder DataHolder { get; }

        public EcuManagerEngine(ILogger logger, EcuDataHolder dataHolder = null)
        {
            this.logger = logger;
            DataHolder = dataHolder ?? new EcuDataHolder();
        }

        public void RegisterEcu(long idSgAdr, string ecuGroupName, EcuDataSource source, string variant = null)
        {
            EcuRecord record = new EcuRecord(idSgAdr, ecuGroupName, source, variant);
            DataHolder.Add(record);
        }

        public IReadOnlyList<EcuDataSource> GetSourcesForEcu(long idSgAdr)
        {
            return (from record in DataHolder.GetRecordsForEcu(idSgAdr)
                    select record.Source).Distinct().ToList();
        }

        public IReadOnlyList<EcuRecord> GetRecordsForEcu(long idSgAdr)
        {
            return DataHolder.GetRecordsForEcu(idSgAdr);
        }

        public void LogBucketState()
        {
            if (logger == null)
            {
                return;
            }
            foreach (KeyValuePair<EcuDataSource, IReadOnlyList<EcuRecord>> allBucket in DataHolder.GetAllBuckets())
            {
                logger.Info("EcuManager", "bucket '{0}' currently contains {1} ECU(s): [{2}]", allBucket.Key, allBucket.Value.Count, string.Join(", ", allBucket.Value.Select((EcuRecord record) => $"0x{record.IdSgAdr:X2} (group: '{record.EcuGroupName}', variant: '{record.Variant}')")));
            }
        }

        public void LogFinalEcuListComposition(IEnumerable<long> finalEcuIdSgAdrList)
        {
            if (logger == null || finalEcuIdSgAdrList == null)
            {
                return;
            }
            foreach (long finalEcuIdSgAdr in finalEcuIdSgAdrList)
            {
                List<EcuRecord> list = (from record in GetRecordsForEcu(finalEcuIdSgAdr)
                                        orderby record.Timestamp
                                        select record).ToList();
                if (list.Count == 0)
                {
                    logger.Warning("EcuManager", "final ECU list composition -> 0x{0:X2}: source is {1}! ECU was added to the Vehicle Object without being registered with the ECU Manager.", finalEcuIdSgAdr, EcuDataSource.Unknown);
                    continue;
                }
                IEnumerable<string> values = from record in list
                                             group record by record.Source into @group
                                             select @group.Last() into record
                                             select $"{record.Source}(group: '{record.EcuGroupName}', variant: '{record.Variant}')";
                logger.Info("EcuManager", "final ECU list composition -> 0x{0:X2}: [{1}]", finalEcuIdSgAdr, string.Join(", ", values));
                for (int num = 1; num < list.Count; num++)
                {
                    EcuRecord ecuRecord = list[num - 1];
                    EcuRecord ecuRecord2 = list[num];
                    if (ecuRecord.EcuGroupName != ecuRecord2.EcuGroupName)
                    {
                        logger.Warning("EcuManager", "ECU 0x{0:X2}: property 'group' changed from '{1}' (source: {2}) to '{3}' (source: {4})", finalEcuIdSgAdr, ecuRecord.EcuGroupName, ecuRecord.Source, ecuRecord2.EcuGroupName, ecuRecord2.Source);
                    }
                    if (ecuRecord.Variant != ecuRecord2.Variant)
                    {
                        logger.Warning("EcuManager", "ECU 0x{0:X2}: property 'variant' changed from '{1}' (source: {2}) to '{3}' (source: {4})", finalEcuIdSgAdr, ecuRecord.Variant, ecuRecord.Source, ecuRecord2.Variant, ecuRecord2.Source);
                    }
                }
            }
        }

        public IReadOnlyList<IIdentEcu> GetEcus()
        {
            lock (ecuListSyncRoot)
            {
                return ecus.Values.ToList();
            }
        }

        public IIdentEcu GetEcuByIdSgAdr(long idSgAdr)
        {
            lock (ecuListSyncRoot)
            {
                IIdentEcu value;
                return ecus.TryGetValue(idSgAdr, out value) ? value : null;
            }
        }

        public bool ContainsEcu(long idSgAdr)
        {
            lock (ecuListSyncRoot)
            {
                return ecus.ContainsKey(idSgAdr);
            }
        }

        public void AddEcu(IIdentEcu ecu, EcuDataSource source)
        {
            if (ecu == null)
            {
                throw new ArgumentNullException("ecu");
            }
            lock (ecuListSyncRoot)
            {
                ecus[ecu.ID_SG_ADR] = ecu;
                RegisterEcu(ecu.ID_SG_ADR, ecu.ECU_GRUPPE, source, ecu.VARIANTE ?? ecu.ECU_SGBD);
            }
        }

        public void UpdateEcu(IIdentEcu ecu, EcuDataSource source)
        {
            if (ecu == null)
            {
                throw new ArgumentNullException("ecu");
            }
            lock (ecuListSyncRoot)
            {
                ecus[ecu.ID_SG_ADR] = ecu;
                RegisterEcu(ecu.ID_SG_ADR, ecu.ECU_GRUPPE, source, ecu.VARIANTE ?? ecu.ECU_SGBD);
            }
        }

        public void RemoveEcu(long idSgAdr, EcuDataSource source)
        {
            lock (ecuListSyncRoot)
            {
                if (ecus.Remove(idSgAdr))
                {
                    logger?.Info("EcuManager", "ECU 0x{0:X2} removed (source: {1})", idSgAdr, source);
                }
            }
        }

        public void ReplaceEcus(IEnumerable<IIdentEcu> newEcus, EcuDataSource source)
        {
            lock (ecuListSyncRoot)
            {
                ecus.Clear();
                foreach (IIdentEcu item in newEcus ?? Enumerable.Empty<IIdentEcu>())
                {
                    ecus[item.ID_SG_ADR] = item;
                    RegisterEcu(item.ID_SG_ADR, item.ECU_GRUPPE, source, item.VARIANTE ?? item.ECU_SGBD);
                }
            }
        }
    }
}
