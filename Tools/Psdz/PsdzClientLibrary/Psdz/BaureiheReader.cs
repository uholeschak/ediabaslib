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

        [PreserveSource(Hint = "iPsdz added")]
        public BaureiheReader(IPsdz iPsdz)
        {
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

        [PreserveSource(Hint = "Cleaned")]
        private IPsdz StartPsdzWebService()
        {
            return null;
        }
    }
}