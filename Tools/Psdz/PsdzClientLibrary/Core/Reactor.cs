using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient;

namespace PsdzClient.Core
{
    public class Reactor : ReactorEngine
    {
        private static Reactor singleton;
        [PreserveSource(Hint = "Instance disabled", KeepAttribute = true)]
        [Obsolete("Use only constructor", true)]
        public static Reactor Instance
        {
            get
            {
                if (singleton == null)
                {
                    throw new InvalidOperationException("Fusion reactor is not initialized, fire 'Initialize' before using it.");
                }

                return singleton;
            }
        }

        [PreserveSource(Hint = "Changed to public", SignatureModified = true)]
        public Reactor(IReactorVehicle reactorVehicle, ILogger logger, DataHolder dataHolder = null) : base(reactorVehicle, logger, dataHolder)
        {
        }

        public static void Initialize(IReactorVehicle reactorVehicle, ILogger logger, DataHolder dataHolder = null)
        {
            if (singleton != null)
            {
                logger.Error("Reactor.Initialize()", "Fusior reactor is already initialized!");
                if (dataHolder == null)
                {
                    dataHolder = singleton.dataHolder;
                }
            }

            singleton = new Reactor(reactorVehicle, logger, dataHolder ?? new DataHolder());
        }
    }
}