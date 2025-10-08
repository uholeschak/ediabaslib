﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Programming.Data.Ecu;
using PsdzClient.Core;

namespace PsdzClient.Programming
{
    // ToDo: Check on update
    // BMW.Rheingold.CoreFramework.Contracts.Programming.Bus
    //[Obsolete("use IBusObject instead")]
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum Bus
    {
        ACan,
        BodyCan,
        DCan,
        FaCan,
        KCan,
        Unknown,
        Unbekannt,
        Most,
        FlexRay,
        B2Can,
        Ethernet,
        SCan,
        LeCan,
        LpCan,
        LrCan,
        IkCan,
        SfCan,
        ZsgCan,
        ZgwCan,
        Le2Can,
        InfraCan,
        B3Can,
        SystemBusEthernet,
        AeCanFd,
        ApCanFd,
        FasCanFd,
        UssCanFd,
        SrrCanFd,
        Ipb11CanFd,
        Ipb12CanFd,
        Ipb13CanFd,
        Ipb14CanFd,
        Ipb15CanFd,
        Ipb16CanFd,
        Ipb17CanFd,
        Zim11CanFd,
        Zim12CanFd
    }

    // ToDo: Check on update
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcuObj
    {
        string EcuRep { get; }

        string EcuGroup { get; }

        string BaseVariant { get; }

        string BnTnName { get; }

        // [UH] [Obsolete("use BusCons instead")]
        IList<Bus> BusConnections { get; }

        IList<IBusObject> BusCons { get; }

        IList<string> BusConnectionsAsString { get; }

        // [UH] [Obsolete("use DiagBus instead")]
        Bus DiagnosticBus { get; }

        IBusObject DiagBus { get; }

        IEcuDetailInfo EcuDetailInfo { get; }

        IEcuIdentifier EcuIdentifier { get; }

        IEcuStatusInfo EcuStatusInfo { get; }

        string EcuVariant { get; }

        int? GatewayDiagAddrAsInt { get; }

        string SerialNumber { get; }

        IStandardSvk StandardSvk { get; }

        string OrderNumber { get; }

        IEcuPdxInfo EcuPdxInfo { get; }

        bool IsSmartActuator { get; }
    }
}
