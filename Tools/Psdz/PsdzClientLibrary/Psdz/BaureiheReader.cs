using BMW.Rheingold.CoreFramework;
using PsdzClient;
using System;
using BMW.ISPI.IstaServices.Client;
using BMW.Rheingold.CoreFramework.Contracts;

namespace BMW.Rheingold.Programming.Common
{
    public class BaureiheReader
    {
        public string GetBaureiheFormatted(string baureihe)
        {
            try
            {
                using (IstaIcsServiceClient istaIcsServiceClient = new IstaIcsServiceClient())
                {
                    if (istaIcsServiceClient.IsAvailable() && istaIcsServiceClient.GetFeatureEnabledStatus("UsePsdzSeriesFormatter", istaIcsServiceClient.IsAvailable()).IsActive)
                    {
                        //[-]return new PsdzWebServiceWrapper().BaureiheUtilityService?.GetBaureihe(baureihe) ?? FormatConverter.ConvertToBn2020ConformModelSeries(baureihe);
                        //[+]return psdz?.BaureiheUtilityService?.GetBaureihe(baureihe) ?? FormatConverter.ConvertToBn2020ConformModelSeries(baureihe);
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

        [PreserveSource(Hint = "psdz added", SuppressWarning = true)]
        private readonly IPsdz psdz;
        [PreserveSource(Hint = "iPsdz added", SignatureModified = true)]
        public BaureiheReader(IPsdz iPsdz)
        {
            //[+] psdz = iPsdz;
            psdz = iPsdz;
        }

        [PreserveSource(Cleaned = true, OriginalHash = "017EC1F95167348AF627A728304BD3E0")]
        private IPsdz StartPsdzWebService()
        {
            return null;
        }
    }
}