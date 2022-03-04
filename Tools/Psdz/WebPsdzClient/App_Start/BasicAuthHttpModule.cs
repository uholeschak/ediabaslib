using System;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;

namespace WebPsdzClient
{
    public class BasicAuthHttpModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            // Register event handlers
            context.AuthenticateRequest += OnApplicationAuthenticateRequest;
            context.EndRequest += OnApplicationEndRequest;
        }

        private static void SetPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
        }

        private static bool CheckPassword(string username, string password)
        {
            return username == "deepobd" && password == "deepobdbmw";
        }

        private static void AuthenticateUser(string credentials)
        {
            try
            {
                var encoding = Encoding.GetEncoding("iso-8859-1");
                credentials = encoding.GetString(Convert.FromBase64String(credentials));

                int separator = credentials.IndexOf(':');
                string name = credentials.Substring(0, separator);
                string password = credentials.Substring(separator + 1);

                if (CheckPassword(name, password))
                {
                    GenericIdentity identity = new GenericIdentity(name);
                    SetPrincipal(new GenericPrincipal(identity, null));
                }
                else
                {
                    // Invalid username or password.
                    HttpContext.Current.Response.StatusCode = 401;
                }
            }
            catch (FormatException)
            {
                // Credentials were not formatted correctly.
                HttpContext.Current.Response.StatusCode = 401;
            }
        }

        private static void OnApplicationAuthenticateRequest(object sender, EventArgs e)
        {
            HttpRequest request = HttpContext.Current.Request;
            string authHeader = request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authHeader))
            {
                try
                {
                    AuthenticationHeaderValue authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);

                    // RFC 2617 sec 1.2, "scheme" name is case-insensitive
                    if (authHeaderVal.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase) &&
                        authHeaderVal.Parameter != null)
                    {
                        AuthenticateUser(authHeaderVal.Parameter);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        // If the request was unauthorized, add the WWW-Authenticate header 
        // to the response.
        private static void OnApplicationEndRequest(object sender, EventArgs e)
        {
            var response = HttpContext.Current.Response;
            if (response.StatusCode == 401)
            {
                response.Headers.Add("WWW-Authenticate", "Basic");
            }
        }

        public void Dispose()
        {
        }
    }
}