using System;
using BMW.ISPI.TRIC.ISTA.EcuManager;
using BMW.Rheingold.ISTA.CoreFramework;

namespace BMW.Rheingold.CoreFramework.EcuManager
{
    public static class EcuManager
    {
        private static EcuManagerEngine singleton;

        public static EcuManagerEngine Instance
        {
            get
            {
                if (singleton == null)
                {
                    throw new InvalidOperationException("ECU Manager is not initialized, call 'Initialize' before using it.");
                }
                return singleton;
            }
        }

        public static void Initialize(ILogger logger, EcuDataHolder dataHolder = null)
        {
            if (singleton != null)
            {
                logger?.Warning("EcuManager.Initialize()", "ECU Manager is already initialized!");
                if (dataHolder == null)
                {
                    dataHolder = singleton.DataHolder;
                }
            }
            singleton = new EcuManagerEngine(logger, dataHolder ?? new EcuDataHolder());
            logger?.Info("EcuManager.Initialize()", "ECU Manager initialized.");
        }

        public static EcuManagerEngine TryGetInstance()
        {
            return singleton;
        }
    }
}
