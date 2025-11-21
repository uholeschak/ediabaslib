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
    [PreserveSource(Hint = "Singleton removed")]
    public class Reactor : ReactorEngine
    {
        public Reactor(IReactorVehicle reactorVehicle, ILogger logger, DataHolder dataHolder = null)
            : base(reactorVehicle, logger, dataHolder)
        {
        }
    }
}
