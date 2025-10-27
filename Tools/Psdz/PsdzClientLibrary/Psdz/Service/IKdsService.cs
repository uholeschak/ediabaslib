﻿using System.ServiceModel;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Exceptions;
using BMW.Rheingold.Psdz.Model.Kds;
using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPsdzProgressListener))]
    [ServiceKnownType(typeof(PsdzKdsIdCto))]
    [ServiceKnownType(typeof(PsdzConnection))]
    [ServiceKnownType(typeof(PsdzKdsClientsForRefurbishResultCto))]
    [ServiceKnownType(typeof(PsdzPerformQuickKdsCheckResultCto))]
    [ServiceKnownType(typeof(PsdzReadPublicKeyResultCto))]
    [ServiceKnownType(typeof(PsdzKdsActionStatusResultCto))]
    [ServiceKnownType(typeof(PsdzSecureTokenEto))]
    public interface IKdsService
    {
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzKdsClientsForRefurbishResultCto GetKdsClientsForRefurbish(IPsdzConnection connection, int retries, int timeBetweenRetries);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzPerformQuickKdsCheckResultCto PerformQuickKdsCheck(IPsdzConnection connection, IPsdzKdsIdCto kdsId, int retries, int timeBetweenRetries);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzPerformQuickKdsCheckResultCto PerformQuickKdsCheckSP25(IPsdzConnection connection, IPsdzKdsIdCto kdsId, int retries = 3, int timeBetweenRetries = 10000);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzReadPublicKeyResultCto ReadPublicKey(IPsdzConnection connection, IPsdzKdsIdCto kdsId, int retries, int timeBetweenRetries);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzKdsActionStatusResultCto SwitchOnComponentTheftProtection(IPsdzConnection connection, IPsdzKdsIdCto kdsId, int retries, int timeBetweenRetries);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzKdsActionStatusResultCto PerformRefurbishProcess(IPsdzConnection connection, IPsdzKdsIdCto kdsId, IPsdzSecureTokenEto secureToken, PsdzKdsActionIdEto psdzKdsActionId, int retries, int timeBetweenRetries);
    }
}
