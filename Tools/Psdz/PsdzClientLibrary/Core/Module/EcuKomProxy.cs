using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Globalization;
using PsdzClient;

namespace BMW.Rheingold.ISTA.CoreFramework.Module
{
    public class EcuKomProxy : IEcuKomApi
    {
        private readonly IEcuKom ecuKom;
        private readonly IProtocolBasic fastaService;
        public IProtocolBasic FastaService => fastaService;
        public IEcuKom EcuKom => ecuKom;

        public EcuKomProxy(IEcuKom ecuKom, IProtocolBasic fastaService)
        {
            if (ecuKom == null)
            {
                throw new ArgumentNullException("ecuKom");
            }

            this.ecuKom = ecuKom;
            this.fastaService = fastaService;
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public IEcuJob ApiJob(string ecu, string job, string param, string resultFilter = "", int retries = 0, bool fastaActive = true)
        {
            Log.Info("EcuKomProxy.ApiJob", "Method ApiJob() called with parameter ecu: {0} - job name: {1} - param: {2} - result filter: {3} - retries: {4}", ecu, job, param, resultFilter, retries);
            DateTime now = DateTime.Now;
            IEcuJob ecuJob = ecuKom.ApiJob(ecu, job, param, resultFilter, retries, fastaActive);
            if ((fastaService != null) & fastaActive)
            {
            //[-] fastaService.AddMethodCall("ApiJob", new Dictionary<string, string>
            //[-] {
            //[-] { "ecu", ecu },
            //[-] { "job", job },
            //[-] { "param", param },
            //[-] { "resultFilter", resultFilter },
            //[-] {
            //[-] "retries",
            //[-] retries.ToString(CultureInfo.InvariantCulture)
            //[-] }
            //[-] }, now).EndTime = DateTime.Now;
            //[-] fastaService.CreateAndAddEcuCommunication(ecu, new Collection<IEcuJob> { ecuJob }, doFastaRelevantFiltering: false, LayoutGroup.X);
            }

            return ecuJob;
        }

        public IEcuJob ApiJobData(string ecu, string job, byte[] param, int paramlen, string resultFilter = "", int retries = 0)
        {
            Log.Info("EcuKomProxy.ApiJobData", "Method ApiJob() called with parameter ecu: {0} - job name: {1} - param: {2} - param length: {3} - result filter: {4} - retries: {5}", ecu, job, param.ToStringItems(), paramlen, resultFilter, retries);
            DateTime now = DateTime.Now;
            IEcuJob ecuJob = ecuKom.ApiJobData(ecu, job, param, paramlen, resultFilter, retries);
            if (fastaService != null)
            {
                IDictionary<string, string> parameter = new Dictionary<string, string>
                {
                    {
                        "ecu",
                        ecu
                    },
                    {
                        "job",
                        job
                    },
                    {
                        "param",
                        param.ToStringItems()
                    },
                    {
                        "paramlength",
                        paramlen.ToString(CultureInfo.InvariantCulture)
                    },
                    {
                        "resultFilter",
                        resultFilter
                    },
                    {
                        "retries",
                        retries.ToString(CultureInfo.InvariantCulture)
                    }
                };
            //[-] fastaService.AddMethodCall("ApiJob", parameter, now).EndTime = DateTime.Now;
            //[-] fastaService.CreateAndAddEcuCommunication(ecu, new Collection<IEcuJob> { ecuJob }, doFastaRelevantFiltering: false, LayoutGroup.X);
            }

            return ecuJob;
        }

        [PreserveSource(Hint = "No change", SignatureModified = true)]
        public IEcuJob ApiJob(string ecu, string job, string param, int retries, int millisecondsTimeout)
        {
            return ApiJob(ecu, job, param, string.Empty, retries);
        }

        public IEcuJob ExecuteJobOverEnet(string icomAddress, string ecu, string job, string param, bool isDopIp, string resultFilter = "", int retries = 0)
        {
            return ecuKom.ExecuteJobOverEnet(icomAddress, ecu, job, param, isDopIp, resultFilter, retries);
        }

        public IEcuJob ExecuteJobOverEnetActivateDHCP(string icomAddress, string ecu, string job, string param, bool isDoIP, string resultFilter = "", int retries = 0)
        {
            return ecuKom.ExecuteJobOverEnetActivateDHCP(icomAddress, ecu, job, param, isDoIP, resultFilter, retries);
        }
    }
}