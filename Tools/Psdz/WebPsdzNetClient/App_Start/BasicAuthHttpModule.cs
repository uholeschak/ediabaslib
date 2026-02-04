using System;
using System.Configuration;
using System.Net.Http.Headers;
using System.Security.Cryptography;
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

        private const string AuthUser = "DeepObd";
        private const string AuthPwd = "BmwCoding";
        public static string AccessPwd { get; private set; }

        public void Init(HttpApplication context)
        {
            log.InfoFormat("BasicAuthHttpModule Init");
            // Register event handlers
            context.AuthenticateRequest += OnApplicationAuthenticateRequest;
            context.EndRequest += OnApplicationEndRequest;
            AccessPwd = ConfigurationManager.AppSettings["AccessPwd"];
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
            bool passwordAccepted = false;
            if (string.Compare(username, AuthUser, StringComparison.Ordinal) == 0)
            {
                for (int dateType = 0; dateType < 3; dateType++)
                {
                    try
                    {
                        DateTime date = DateTime.Now;
                        switch (dateType)
                        {
                            case 1:
                                date = date.AddDays(1);
                                break;

                            case 2:
                                date = date.AddDays(-1);
                                break;
                        }

                        string dayString = date.ToString("yyyy-MM-dd");
                        string encodeString = AuthPwd + dayString;
                        byte[] pwdArray = Encoding.ASCII.GetBytes(encodeString);
                        string md5Pwd;
                        using (MD5 md5 = MD5.Create())
                        {
                            md5Pwd = BitConverter.ToString(md5.ComputeHash(pwdArray)).Replace("-", "");
                        }

                        if (string.Compare(password, md5Pwd, StringComparison.Ordinal) == 0)
                        {
                            if (dateType > 0)
                            {
                                log.InfoFormat("CheckPassword Accepted date: {0}", dayString);
                            }

                            passwordAccepted = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("CheckPassword Exception: {0}", ex.Message);
                    }
                }

                if (!string.IsNullOrEmpty(AccessPwd))
                {
                    if (string.Compare(password, AccessPwd, StringComparison.Ordinal) == 0)
                    {
                        log.InfoFormat("CheckPassword Accepted: {0}", AccessPwd);
                        passwordAccepted = true;
                    }
                }
            }

            if (!passwordAccepted)
            {
                log.ErrorFormat("CheckPassword Rejected Name: {0}, Password: {1}", username, password);
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
                try
                {
                    AuthenticationHeaderValue authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);

                    // RFC 2617 sec 1.2, "scheme" name is case-insensitive
                    if (authHeaderVal.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase) &&
                        authHeaderVal.Parameter != null)
                    {
                        AuthenticateUser(authHeaderVal.Parameter);
                    }
                    else
                    {
                        log.ErrorFormat("OnApplicationAuthenticateRequest Invalid auth header: {0}", authHeader);
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