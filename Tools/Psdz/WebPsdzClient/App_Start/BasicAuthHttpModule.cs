using System;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using log4net;

namespace WebHostBasicAuth.Modules
{
    public class BasicAuthHttpModule : IHttpModule
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BasicAuthHttpModule));

        public void Init(HttpApplication context)
        {
            log.InfoFormat("BasicAuthHttpModule Init");
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
            log.InfoFormat("CheckPassword Name: {0}, Password: {1}", username, password);

            bool passwordAccepted = false;
            if (string.Compare(username, "deepobd", StringComparison.Ordinal) == 0)
            {
                if (string.Compare(password, "deepobdbmw", StringComparison.Ordinal) == 0)
                {
                    passwordAccepted = true;
                }
            }

            if (passwordAccepted)
            {
                log.InfoFormat("CheckPassword Accepted");
            }
            else
            {
                log.ErrorFormat("CheckPassword Rejected");
            }

            return passwordAccepted;
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
            catch (FormatException ex)
            {
                log.ErrorFormat("AuthenticateUser Exception: {0}", ex.Message);
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
                log.InfoFormat("OnApplicationAuthenticateRequest Header: {0}", authHeader);
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
                catch (Exception ex)
                {
                    log.ErrorFormat("OnApplicationAuthenticateRequest Exception: {0}", ex.Message);
                }
            }
        }

        // If the request was unauthorized, add the WWW-Authenticate header 
        // to the response.
        private static void OnApplicationEndRequest(object sender, EventArgs e)
        {
            HttpResponse response = HttpContext.Current.Response;
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