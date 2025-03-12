using net.sf.jni4net.attributes;

namespace BMW.Rheingold.Psdz.Client
{
    [JavaInterface]
    public interface SecureDiagnosticsCallbackHandler
    {
        [JavaMethod("([B)[B")]
        byte[] signAuthService29Challenge(byte[] par0);

        [JavaMethod("()[B")]
        byte[] getAuthService29Certificate();
    }
}