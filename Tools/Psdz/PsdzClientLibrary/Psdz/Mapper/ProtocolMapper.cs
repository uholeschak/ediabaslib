using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Communications;
using BMW.Rheingold.Psdz.Model.Tal;

namespace BMW.Rheingold.Psdz
{
    internal class ProtocolMapper : NullableEnumMapper<PsdzProtocol, ProtocolModel>
    {
        protected override IDictionary<PsdzProtocol?, ProtocolModel?> CreateMap()
        {
            return new Dictionary<PsdzProtocol?, ProtocolModel?>
            {
                {
                    PsdzProtocol.KWP2000,
                    ProtocolModel.KWP2000
                },
                {
                    PsdzProtocol.UDS,
                    ProtocolModel.UDS
                },
                {
                    PsdzProtocol.HTTP,
                    ProtocolModel.HTTP
                },
                {
                    PsdzProtocol.MIRROR,
                    ProtocolModel.MIRROR
                }
            };
        }
    }
}