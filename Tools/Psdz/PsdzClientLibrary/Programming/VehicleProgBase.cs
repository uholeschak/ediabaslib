using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System;
using PsdzClientLibrary.Psdz;

namespace PsdzClient.Programming
{
    public abstract class VehicleProgBase : ProgrammingMessageListener
    {
        private readonly IVehicle vehicle;

        public IVehicle VehicleInfo => vehicle;

        internal VehicleProgBase(IVehicle vehicle, IProgMsgListener progMsgListener)
            : base(progMsgListener)
        {
            if (vehicle == null)
            {
                throw new ArgumentException("Param 'vehicle' must not be null!");
            }
            this.vehicle = vehicle;
        }
    }
}
