﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Exceptions;
using BMW.Rheingold.Psdz.Model.Kds;
using BMW.Rheingold.Psdz.Model.Sfa;

namespace BMW.Rheingold.Psdz
{
    [ServiceKnownType(typeof(PsdzKdsActionStatusResultCto))]
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPsdzProgressListener))]
    [ServiceKnownType(typeof(PsdzSecureTokenEto))]
    [ServiceKnownType(typeof(PsdzKdsIdCto))]
    [ServiceKnownType(typeof(PsdzConnection))]
    [ServiceKnownType(typeof(PsdzKdsClientsForRefurbishResultCto))]
    [ServiceKnownType(typeof(PsdzPerformQuickKdsCheckResultCto))]
    [ServiceKnownType(typeof(PsdzReadPublicKeyResultCto))]
    public interface IKdsService
    {
        [FaultContract(typeof(PsdzRuntimeException))]
        [OperationContract]
        IPsdzKdsClientsForRefurbishResultCto GetKdsClientsForRefurbish(IPsdzConnection connection, int retries, int timeBetweenRetries);

        [FaultContract(typeof(PsdzRuntimeException))]
        [OperationContract]
        IPsdzPerformQuickKdsCheckResultCto PerformQuickKdsCheck(IPsdzConnection connection, IPsdzKdsIdCto kdsId, int retries, int timeBetweenRetries);

        [FaultContract(typeof(PsdzRuntimeException))]
        [OperationContract]
        IPsdzReadPublicKeyResultCto ReadPublicKey(IPsdzConnection connection, IPsdzKdsIdCto kdsId, int retries, int timeBetweenRetries);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzKdsActionStatusResultCto SwitchOnComponentTheftProtection(IPsdzConnection connection, IPsdzKdsIdCto kdsId, int retries, int timeBetweenRetries);

        [FaultContract(typeof(PsdzRuntimeException))]
        [OperationContract]
        IPsdzKdsActionStatusResultCto PerformRefurbishProcess(IPsdzConnection connection, IPsdzKdsIdCto kdsId, IPsdzSecureTokenEto secureToken, PsdzKdsActionIdEto psdzKdsActionId, int retries, int timeBetweenRetries);
    }
}
