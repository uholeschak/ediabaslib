using System;
using System.Globalization;

#pragma warning disable CS0618
namespace BMW.Authoring.Programming.API.Models.Implementation
{
    public class SgbmId : BMW.Authoring.Programming.API.Models.Interfaces.ISgbmId, IHideObjectMembers
    {
        public long Id { get; }

        public int MainVersion { get; }

        public int PatchVersion { get; }

        public string ProcessClass { get; }

        public int SubVersion { get; }

        public string HexString { get; }

        public SgbmId(BMW.Rheingold.CoreFramework.Contracts.Programming.ISgbmId source)
        {
            if (source != null)
            {
                Id = source.Id;
                MainVersion = source.MainVersion;
                PatchVersion = source.PatchVersion;
                ProcessClass = source.ProcessClass;
                SubVersion = source.SubVersion;
                HexString = source.HexString ?? string.Format(CultureInfo.InvariantCulture, "{0}-{1:X8}-{2:X2}.{3:X2}.{4:X2}", source.ProcessClass, source.Id, source.MainVersion, source.SubVersion, source.PatchVersion);
            }
        }

        Type IHideObjectMembers.GetType()
        {
            return GetType();
        }
    }
}
