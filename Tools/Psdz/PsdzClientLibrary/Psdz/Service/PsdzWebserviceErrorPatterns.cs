using System.Text.RegularExpressions;
using BMW.Rheingold.Psdz;

namespace BMW.Rheingold.Psdz
{
    public static class PsdzWebserviceErrorPatterns
    {
        public static readonly (Regex Pattern, PsdzWebserviceStartFailureReason Reason)[] Patterns = new (Regex, PsdzWebserviceStartFailureReason)[7]
        {
            (new Regex("Address already in use", RegexOptions.IgnoreCase | RegexOptions.Compiled), PsdzWebserviceStartFailureReason.PortInUse),
            (new Regex("BindException", RegexOptions.IgnoreCase | RegexOptions.Compiled), PsdzWebserviceStartFailureReason.PortInUse),
            (new Regex("NoClassDefFoundError|ClassNotFoundException|Error opening zip file", RegexOptions.IgnoreCase | RegexOptions.Compiled), PsdzWebserviceStartFailureReason.JarMissingOrCorrupt),
            (new Regex("Could not create the Java Virtual Machine|Unsupported major\\.minor version", RegexOptions.IgnoreCase | RegexOptions.Compiled), PsdzWebserviceStartFailureReason.JavaRuntimeFaulty),
            (new Regex("Application failed to start|Failed to start bean|Tomcat.*failed state", RegexOptions.IgnoreCase | RegexOptions.Compiled), PsdzWebserviceStartFailureReason.SpringBootStartupError),
            (new Regex("Access is denied|Permission denied|AccessDeniedException", RegexOptions.IgnoreCase | RegexOptions.Compiled), PsdzWebserviceStartFailureReason.AccessDenied),
            (new Regex("(cert|sdp).*(missing|corrupt|invalid)", RegexOptions.IgnoreCase | RegexOptions.Compiled), PsdzWebserviceStartFailureReason.SdpFaulty)
        };
    }
}