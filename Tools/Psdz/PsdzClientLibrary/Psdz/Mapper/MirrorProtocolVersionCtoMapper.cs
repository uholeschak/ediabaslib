using BMW.Rheingold.Psdz.Model.Tal;

namespace BMW.Rheingold.Psdz
{
    internal static class MirrorProtocolVersionCtoMapper
    {
        public static MirrorProtocolVersionCtoModel map(PsdzMirrorProtocolVersionCto psdzObject)
        {
            if (psdzObject == null)
            {
                return null;
            }

            return new MirrorProtocolVersionCtoModel
            {
                MajorVersion = psdzObject.MajorVersion,
                MinorVersion = psdzObject.MinorVersion,
                DEFAULT_MAJOR_VERSION = psdzObject.DEFAULT_MAJOR_VERSION,
                DEFAULT_MINOR_VERSION = psdzObject.DEFAULT_MINOR_VERSION,
                VERSION_BYTE_SIZE = psdzObject.VERSION_BYTE_SIZE
            };
        }

        public static PsdzMirrorProtocolVersionCto map(MirrorProtocolVersionCtoModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new PsdzMirrorProtocolVersionCto
            {
                MajorVersion = model.MajorVersion,
                MinorVersion = model.MinorVersion,
                DEFAULT_MAJOR_VERSION = model.DEFAULT_MAJOR_VERSION,
                DEFAULT_MINOR_VERSION = model.DEFAULT_MINOR_VERSION,
                VERSION_BYTE_SIZE = model.VERSION_BYTE_SIZE
            };
        }
    }
}