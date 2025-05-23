﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Tal;

namespace BMW.Rheingold.Psdz.Client
{
    internal sealed class TalExecutionServiceClient : PsdzDuplexClientBase<ITalExecutionService, IPsdzProgressListener>, ITalExecutionService
    {
        internal TalExecutionServiceClient(IPsdzProgressListener progressListener, Binding binding, EndpointAddress remoteAddress)
            : base(progressListener, binding, remoteAddress)
        {
        }

        public IPsdzTal ExecuteTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzVin vin, IPsdzFa faTarget, TalExecutionSettings talExecutionConfig, string backupDataPath, CancellationToken ct)
        {
            return CallFunction((ITalExecutionService m) => m.ExecuteTal(connection, tal, svtTarget, vin, faTarget, talExecutionConfig, backupDataPath, ct));
        }

        public IPsdzTal ExecuteTalFile(IPsdzConnection connection, string pathToTal, string vin, string pathToFa, TalExecutionSettings talExecutionSettings, CancellationToken ct)
        {
            return CallFunction((ITalExecutionService m) => m.ExecuteTalFile(connection, pathToTal, vin, pathToFa, talExecutionSettings, ct));
        }

        public IPsdzTal ExecuteHDDUpdate(IPsdzConnection connection, IPsdzTal tal, IPsdzFa fa, IPsdzVin vin, TalExecutionSettings talExecutionSettings)
        {
            return CallFunction((ITalExecutionService m) => m.ExecuteHDDUpdate(connection, tal, fa, vin, talExecutionSettings));
        }
    }
}
