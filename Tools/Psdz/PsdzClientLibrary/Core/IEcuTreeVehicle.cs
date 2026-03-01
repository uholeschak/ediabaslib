using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System;
using System.Collections.Generic;
using static PsdzClient.PsdzDatabase;

namespace PsdzClient.Core
{
    public interface IEcuTreeVehicle
    {
        IList<IEcuTreeEcu> ECU { get; }

        string VINRangeType { get; }

        string Hybridkennzeichen { get; }

        string Produktlinie { get; }

        string Ereihe { get; }

        string Motor { get; }

        string Getriebe { get; }

        string Baureihenverbund { get; }

        string Prodart { get; }

        string Typ { get; }

        string ILevelWerk { get; }

        DateTime ProductionDate { get; }

        bool ProductionDateSpecified { get; }

        DateTime? C_DATETIME { get; }

        BordnetType BordnetType { get; }

        BordnetsData BordnetsData { get; set; }

        IEcuTreeEcu getECU(long? sgAdr);

        IEcuTreeEcu getECU(long? sgAdr, long? subAddress);

        bool HasSA(string sa);

        int GetCustomHashCode();
    }
}