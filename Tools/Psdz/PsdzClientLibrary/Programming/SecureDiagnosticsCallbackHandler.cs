using net.sf.jni4net.attributes;
using PsdzClientLibrary;

namespace BMW.Rheingold.Psdz.Client
{
    [PreserveSource(Removed = true)]
    [JavaInterface]
    public interface SecureDiagnosticsCallbackHandler
    {
        [JavaMethod("([B)[B")]
        byte[] signAuthService29Challenge(byte[] par0);

        [JavaMethod("()[B")]
        byte[] getAuthService29Certificate();
    }
}