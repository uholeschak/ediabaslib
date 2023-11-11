using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;
using PsdzClientLibrary.Core;

namespace BMW.Rheingold.Programming.Controller.SecureCoding.Model
{
    [DataContract]
    public class ResponseJson
    {
        public void SaveToFile(string fileDirectory)
        {
            this.signedNcd.ForEach(delegate (SignedNcd x)
            {
                Log.Info(Log.CurrentMethod(), "Saving calculated NCD: " + x.ToString, Array.Empty<object>());
                string btld = x.btld;
                string cafd = x.cafd;
                string str = (btld + cafd.Replace("cafd", string.Empty) + ".ncd").ToLower();
                byte[] ncdConvertedFromBase = x.NcdConvertedFromBase64;
                File.WriteAllBytes(fileDirectory + "\\" + str, ncdConvertedFromBase);
            });
        }

        public new string ToString
        {
            get
            {
                string[] array = new string[6];
                array[0] = "Status: ";
                array[1] = this.status.ToString;
                array[2] = " - Vin17 :";
                array[3] = this.vin17;
                array[4] = " - SignedNcd:";
                array[5] = string.Join("/", (from r in this.signedNcd
                    select r.ToString).ToArray<string>());
                return string.Concat(array);
            }
        }

        public ResponseJson.CalculationStatus CheckCalculationStatus()
        {
            string code = this.status.code;
            if (code != null)
            {
                if (code == "6000")
                {
                    return ResponseJson.CalculationStatus.Error;
                }
                if (code == "0300")
                {
                    return ResponseJson.CalculationStatus.Processing;
                }
                if (!(code == "0200"))
                {
                }
            }
            return ResponseJson.CalculationStatus.Finished;
        }

        [DataMember(Name = "status")]
        public readonly Status status;

        [DataMember(Name = "vin17")]
        public readonly string vin17;

        [DataMember(Name = "signedNcd")]
        public readonly SignedNcd[] signedNcd;

        public enum CalculationStatus
        {
            Error,
            Processing,
            Finished
        }
    }
}
