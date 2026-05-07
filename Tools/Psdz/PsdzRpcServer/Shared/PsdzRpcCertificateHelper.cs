using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace PsdzRpcServer.Shared
{
    public static class PsdzRpcCertificateHelper
    {
        public static X509Certificate2 LoadCertificate(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

#if NET9_0_OR_GREATER
            string ext = Path.GetExtension(path);
            if (string.CompareOrdinal(ext, ".pfx") == 0 || string.CompareOrdinal(ext, ".p12") == 0)
            {
                return X509CertificateLoader.LoadPkcs12FromFile(path, password: null);
            }

            return X509CertificateLoader.LoadCertificateFromFile(path);
#else
            return new X509Certificate2(path);
#endif
        }

        public static X509Certificate2 LoadEmbeddedCertificate(Assembly assembly, string resourceName)
        {
            string fullName = Array.Find(assembly.GetManifestResourceNames(),
                n => n.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));

            if (fullName == null)
            {
                return null;
            }

            using (Stream stream = assembly.GetManifestResourceStream(fullName))
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

#if NET9_0_OR_GREATER
                string ext = Path.GetExtension(resourceName).ToLowerInvariant();
                if (ext == ".pfx" || ext == ".p12")
                    return X509CertificateLoader.LoadPkcs12(bytes, password: null);

                return X509CertificateLoader.LoadCertificate(bytes);
#else
                return new X509Certificate2(bytes);
#endif
            }
        }
    }
}
