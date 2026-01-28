using System;
using BMW.Rheingold.Psdz;
using PsdzClient;
using PsdzClient.Core;
using PsdzClient.Programming;
using PsdzClient.Utility;

namespace BMW.Rheingold.Psdz
{
    public class BaureiheReader
    {
        private readonly IPsdz psdz;

        [PreserveSource(Hint = "iPsdz added", SignatureModified = true)]
        public BaureiheReader(IPsdz iPsdz)
        {
            //[-] psdz = StartPsdzWebService();
            //[+] psdz = iPsdz;
            psdz = iPsdz;
        }

        public string GetBaureiheFormatted(string baureihe)
        {
            try
            {
                using (IstaIcsServiceClient istaIcsServiceClient = new IstaIcsServiceClient())
                {
                    if (istaIcsServiceClient.IsAvailable() && istaIcsServiceClient.GetFeatureEnabledStatus("UsePsdzSeriesFormatter", istaIcsServiceClient.IsAvailable()).IsActive)
                    {
                        return psdz?.BaureiheUtilityService?.GetBaureihe(baureihe) ?? FormatConverter.ConvertToBn2020ConformModelSeries(baureihe);
                    }
                }
                return FormatConverter.ConvertToBn2020ConformModelSeries(baureihe);
            }
            catch (Exception exception)
            {
                Log.ErrorException("Baureihereader logged exception while reading baureihe from psdz.", exception);
                return FormatConverter.ConvertToBn2020ConformModelSeries(baureihe);
            }
        }

        [PreserveSource(Cleaned = true, OriginalHash = "017EC1F95167348AF627A728304BD3E0")]
        private IPsdz StartPsdzWebService()
        {
            return null;
        }
    }
}