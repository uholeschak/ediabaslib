using BMW.Authoring.API;
using BMW.Authoring.Vehicle.Interface;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;

#pragma warning disable CS0618
namespace BMW.Authoring.Vehicle
{
    public class Vehicle : IVehicle, IHideObjectMembers
    {
        public IFaultList FaultList { get; set; }

        public ICCMList CCMList { get; set; }

        public IEcuList EcuList { get; set; }

        public IDetails Details { get; set; }

        public IEcuKom EcuKom { get; set; }

        public IProtocolBasicBase FastaProtocoler { get; set; }

        public IFFMDynamicResolver FFMResolver { get; set; }

        public IVerbraucherList VerbraucherList { get; set; }

        [Obsolete("Can be removed in the Version 4.56.00")]
        public string F2Date { get; set; }

        public string SoftwareId { get; set; }

        public ICentralErrorMemory CentralErrorMemory { get; set; }

        public Vehicle(IAuthoringModule istaModule)
        {
            FastaProtocoler = istaModule.FastaProtocolerBase;
            EcuKom = istaModule.EcuKom;
            FFMResolver = istaModule.FFMDynamicResolver;
            if (istaModule.Vehicle != null)
            {
                //[-] EcuList = new EcuList(istaModule.Vehicle);
                //[-] FaultList = new FaultList(istaModule, istaModule.DBProvider, this, istaModule.Vehicle);
                //[-] CCMList = new CCMList(istaModule.Vehicle, FaultList);
                //[-] Details = new Details(this, istaModule.Vehicle, ((Logic)istaModule.IstaOperationLogic).ProgrammingSessionDataContext);
                F2Date = istaModule.Vehicle.F2Date;
                //[-] CentralErrorMemory = new CentralErrorMemory();
                MapCentralErrorMemoryResults(istaModule);
            }
            //[-] VerbraucherList = new VerbraucherList(istaModule.Vehicle, FFMResolver, istaModule.DBProvider);
        }

        private void MapCentralErrorMemoryResults(IAuthoringModule istaModule)
        {
            //[-] if (SessionInfoAccessor.SessionInfo.CentralErrorMemoryStatus.Equals(BMW.ISPI.TRIC.ISTA.Contracts.Enums.CentralErrorMemoryStatus.ZFS))
            //[-] {
            //[-] List<ZfsResult> collection = VehicleUtilities.MapZfsResults(istaModule.Vehicle.ZFS.ToList());
            //[-] CentralErrorMemory.ZfsResult = new List<IZfsResult>(collection);
            //[-] CentralErrorMemory.CentralErrorMemoryStatus = BMW.Authoring.Vehicle.Enums.CentralErrorMemoryStatus.ZFS;
            //[-] }
            //[-] else if (SessionInfoAccessor.SessionInfo.CentralErrorMemoryStatus.Equals(BMW.ISPI.TRIC.ISTA.Contracts.Enums.CentralErrorMemoryStatus.CEM))
            //[-] {
            //[-] List<CemResult> collection2 = VehicleUtilities.MapCemResults(istaModule.Vehicle.CEM);
            //[-] CentralErrorMemory.CemResult = new List<ICemResult>(collection2);
            //[-] CentralErrorMemory.CentralErrorMemoryStatus = BMW.Authoring.Vehicle.Enums.CentralErrorMemoryStatus.CEM;
            //[-] }
        }

        Type IHideObjectMembers.GetType()
        {
            return GetType();
        }
    }
}
