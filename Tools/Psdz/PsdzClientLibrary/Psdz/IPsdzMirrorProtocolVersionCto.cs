namespace BMW.Rheingold.Psdz.Model.Tal
{
    public interface IPsdzMirrorProtocolVersionCto
    {
        int VERSION_BYTE_SIZE { get; set; }

        int MajorVersion { get; set; }

        int MinorVersion { get; set; }

        int DEFAULT_MAJOR_VERSION { get; set; }

        int DEFAULT_MINOR_VERSION { get; set; }
    }
}