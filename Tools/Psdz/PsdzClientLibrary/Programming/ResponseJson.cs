using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

#pragma warning disable CS0649
namespace BMW.Rheingold.Programming.Controller.SecureCoding.Model
{
    [DataContract]
    internal class ResponseJson
    {
        internal enum CalculationStatus
        {
            Error,
            Processing,
            Finished
        }

        [DataMember(Name = "status")]
        public readonly Status status;

        [DataMember(Name = "vin17")]
        public readonly string vin17;

        [DataMember(Name = "signedNcd")]
        public readonly SignedNcd[] signedNcd;

        [DataMember(Name = "vpcRequest")]
        public readonly string vpcRequest;

        [DataMember(Name = "vpcResponse")]
        public readonly string vpcResponse;

        [DataMember(Name = "FP")]
        public readonly string fp;

        internal void SaveToFile(string fileDirectory)
        {
            signedNcd.ForEach(delegate (SignedNcd x)
            {
                Log.Info(Log.CurrentMethod(), "Saving calculated NCD: " + x.ToString());
                string btld = x.btld;
                string cafd = x.cafd;
                string text = (btld + cafd.Replace("cafd", string.Empty) + ".ncd").ToLower();
                byte[] ncdConvertedFromBase = x.NcdConvertedFromBase64;
                File.WriteAllBytes(fileDirectory + "\\" + text, ncdConvertedFromBase);
            });
        }

        public override string ToString()
        {
            object[] obj = new object[5]
            {
                status?.ToString(),
                vin17,
                vpcRequest,
                vpcResponse,
                null
            };
            SignedNcd[] array = signedNcd;
            obj[4] = ((array != null && array.Any()) ? string.Join("/", signedNcd.Select((SignedNcd r) => r?.ToString())) : string.Empty);
            return string.Format("Status: {0} - Vin17 :{1} - vpcRequest:{2} - vpcResponse:{3} - SignedNcd:{4}", obj);
        }

        internal CalculationStatus CheckCalculationStatus()
        {
            Log.Info(Log.CurrentMethod(), "Status returned by SCB: StatusCode: " + status.code + " - ErrorMessage :" + status.errorMessage + " - AppErrorId:" + status.appErrorId);
            switch (status.code)
            {
                case "6000":
                case "0250":
                case "1080":
                    return CalculationStatus.Error;
                case "0300":
                    return CalculationStatus.Processing;
                case "0200":
                    return CalculationStatus.Finished;
                default:
                    return CalculationStatus.Error;
            }
        }
    }
}
