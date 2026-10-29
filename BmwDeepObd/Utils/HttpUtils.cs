using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace BmwDeepObd
{
    public static class HttpUtils
    {
#if DEBUG
        private static readonly string Tag = typeof(HttpUtils).FullName;
#endif

        /// <summary>
        /// Normalizes a <see cref="MultipartFormDataContent"/> to the format used by browsers and curl.
        /// Required since the hoster (IONOS) tightened the PHP multipart parsing rules
        /// (CVE-2026-109618, CVE-2026-109619). Requests in the default .NET format are rejected
        /// with HTTP 400 and the hoster error page.
        /// <list type="bullet">
        /// <item><description>
        /// Removes the quotes from the boundary parameter of the Content-Type header
        /// (<c>boundary="..."</c> -&gt; <c>boundary=...</c>).
        /// Quoted boundaries are valid according to RFC 2046, but are rejected by the server.
        /// </description></item>
        /// <item><description>
        /// Removes the <c>filename*</c> parameter (RFC 5987) from the Content-Disposition header of all parts.
        /// .NET always adds it for file parts, but RFC 7578 (section 4.2) prohibits it for multipart/form-data.
        /// </description></item>
        /// <item><description>
        /// Quotes the <c>name</c> and <c>filename</c> parameters if not already quoted.
        /// Quotes are mandatory for file names containing non token characters (spaces, umlauts, etc.).
        /// </description></item>
        /// </list>
        /// The body is not modified, the boundary inside the body is never quoted.
        /// Must be called after all parts have been added and before sending the request.
        /// </summary>
        /// <param name="content">Multipart content, may be null</param>
        /// <returns>The same content instance (modified in place) or null</returns>
        public static MultipartFormDataContent FixMultipartBoundary(MultipartFormDataContent content)
        {
            if (content == null)
            {
                return null;
            }

            NameValueHeaderValue boundaryParam = content.Headers.ContentType?.Parameters
                .FirstOrDefault(p => string.Compare(p.Name, "boundary", StringComparison.OrdinalIgnoreCase) == 0);
            if (boundaryParam?.Value != null)
            {
                boundaryParam.Value = boundaryParam.Value.Trim('"');
            }

            foreach (HttpContent part in content)
            {
                ContentDispositionHeaderValue disposition = part.Headers.ContentDisposition;
                if (disposition == null)
                {
                    continue;
                }

                // filename* (RFC 5987) is not allowed in multipart/form-data (RFC 7578)
                disposition.FileNameStar = null;

                if (!string.IsNullOrEmpty(disposition.Name) && !disposition.Name.StartsWith("\""))
                {
                    disposition.Name = "\"" + disposition.Name + "\"";
                }

                if (!string.IsNullOrEmpty(disposition.FileName) && !disposition.FileName.StartsWith("\""))
                {
                    disposition.FileName = "\"" + disposition.FileName + "\"";
                }
            }

            return content;
        }

        public static WebProxy GetProxySettings()
        {
            try
            {
                string proxyHost = Java.Lang.JavaSystem.GetProperty("http.proxyHost");
                string proxyPort = Java.Lang.JavaSystem.GetProperty("http.proxyPort");
                string nonProxyHosts = Java.Lang.JavaSystem.GetProperty("http.nonProxyHosts");

                if (!string.IsNullOrEmpty(proxyHost))
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(proxyHost.TrimEnd('/'));
                    string address = string.Empty;
                    foreach (IPAddress ipAddress in hostEntry.AddressList)
                    {
                        if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            address = ipAddress.ToString();
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(address))
                    {
                        return null;
                    }

                    int portNum = 0;
                    if (!string.IsNullOrEmpty(proxyPort))
                    {
                        if (!int.TryParse(proxyPort, NumberStyles.Integer, CultureInfo.InvariantCulture, out portNum))
                        {
                            portNum = 0;
                        }
                    }

                    if (portNum < 1 || portNum > 0xFFFF)
                    {
                        return null;
                    }

                    address += string.Format(CultureInfo.InvariantCulture, ":{0}", portNum);
                    string[] bypassList = Array.Empty<string>();
                    if (!string.IsNullOrEmpty(nonProxyHosts))
                    {
                        bypassList = nonProxyHosts.Split('|');
                    }

                    return new WebProxy(address, true, bypassList);
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        public static bool? HasValidProxyHost()
        {
            try
            {
                string proxyHost = Java.Lang.JavaSystem.GetProperty("http.proxyHost");
                if (string.IsNullOrEmpty(proxyHost))
                {
                    return null;
                }

                if (GetProxySettings() == null)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
