using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;
using BMW.Rheingold.Psdz.Client;
using log4net;
using PsdzClient;
using PsdzClient.Programming;
using WebPsdzClient.App_Data;

namespace WebPsdzClient
{
    public class Global : HttpApplication
    {
        public const string SessionContainerName = "SessionContainer";
        public const int MaxSessions = 20;
        public const int MinAppVer = 371;
        public static string DealerId { get; private set; }
        public static string IstaFolder { get; private set; }
        public static string SqlServer { get; private set; }
        public static string TestLicenses { get; private set; }
        public static string DisplayOptions { get; private set; }

        private static readonly ILog log = LogManager.GetLogger(typeof(Global));

        protected void Application_Start(object sender, EventArgs e)
        {
            // Code, der beim Anwendungsstart ausgeführt wird
            DealerId = ConfigurationManager.AppSettings["DealerId"];
            IstaFolder = ConfigurationManager.AppSettings["IstaFolder"];
            SqlServer = ConfigurationManager.AppSettings["SqlServer"];
            TestLicenses = ConfigurationManager.AppSettings["TestLicenses"];
            DisplayOptions = ConfigurationManager.AppSettings["DisplayOptions"];

            if (string.IsNullOrEmpty(IstaFolder) || !Directory.Exists(IstaFolder))
            {
                IstaFolder = ProgrammingJobs.GetIstaInstallLocation();
            }

            SetupLog4Net();
            log.InfoFormat("Application_Start");
            log.InfoFormat("Ista folder: {0}", IstaFolder);
            PsdzStarterGuard.Instance.ResetInitialization();
            PsdzServiceStarter.ClearIstaPIDsFile();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
        }

        protected void Application_EndRequest(Object sender, EventArgs e)
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            log.ErrorFormat("Application_Error");
        }

        protected void Application_End(object sender, EventArgs e)
        {
            log.InfoFormat("Application_End");
        }

        protected void Application_Disposed(object sender, EventArgs e)
        {
            log.InfoFormat("Application_Disposed");
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            SessionContainer.SetLogInfo(Session.SessionID);
            log.InfoFormat("Session_Start: SessionId={0}", Session.SessionID);
            if (!(Session.Contents[SessionContainerName] is SessionContainer))
            {
                int sessions = SessionContainer.GetSessionContainerCount();
                if (sessions > MaxSessions)
                {
                    log.InfoFormat("Session_Start: SessionCount exceeded={0}", sessions);
                    Response.Redirect("SessionsExceeded.aspx", false);
                    return;
                }

                string deepObdVersion = null;
                string deepObdLanguage = null;
                string hostAddress = null;
                try
                {
                    if (!string.IsNullOrEmpty(Request.UserAgent))
                    {
                        log.InfoFormat("Session_Start User agent: {0}", Request.UserAgent);
                        string[] agentParts = Request.UserAgent.Split(' ');
                        foreach (string part in agentParts)
                        {
                            if (part.StartsWith("DeepObd"))
                            {
                                string[] subParts = part.Split('/');
                                if (subParts.Length >= 3)
                                {
                                    deepObdVersion = subParts[1];
                                    deepObdLanguage = subParts[2];
                                }
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(Request.UserHostAddress))
                    {
                        string hostName = Request.UserHostName ?? string.Empty;
                        log.InfoFormat("Session_Start User host address: {0}, Name={1}", Request.UserHostAddress, hostName);
                        hostAddress = Request.UserHostAddress.Trim();
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Session_Start Exception: {0}", ex.Message);
                }

                bool valid = !string.IsNullOrEmpty(deepObdVersion);
                if (!string.IsNullOrEmpty(hostAddress) &&
                    (string.Compare(hostAddress, "127.0.0.1", StringComparison.OrdinalIgnoreCase) == 0 ||
                     string.Compare(hostAddress, "::1", StringComparison.OrdinalIgnoreCase) == 0 ||
                     hostAddress.StartsWith("192.168.", StringComparison.OrdinalIgnoreCase))
                    )
                {
                    valid = true;
                }

                if (!valid)
                {
                    log.InfoFormat("Session_Start: Invalid host");
                    Response.Redirect("AccessDenied.aspx", false);
                    return;
                }

                SessionContainer sessionContainer = new SessionContainer(Session.SessionID, DealerId);
                Int64 appVersion = 0;
                if (!string.IsNullOrEmpty(deepObdVersion))
                {
                    if (Int64.TryParse(deepObdVersion, out Int64 version))
                    {
                        appVersion = version;
                        if (appVersion < MinAppVer)
                        {
                            log.InfoFormat("Session_Start: Invalid app version");
                            Response.Redirect("AppUpdate.aspx", false);
                            return;
                        }
                    }

                    log.InfoFormat("Session_Start Storing App: Ver={0}, Lang={1}", appVersion, deepObdLanguage);
                    sessionContainer.DeepObdVersion = appVersion;
                    sessionContainer.SetLanguage(deepObdLanguage);
                }

                Session.Contents.Add(SessionContainerName, sessionContainer);
            }
        }

        protected void Session_End(object sender, EventArgs e)
        {
            SessionContainer.SetLogInfo(Session.SessionID);
            log.InfoFormat("Session_End: SessionId={0}", Session.SessionID);
            ClearSession(Session);
        }

        public static void ClearSession(HttpSessionState session)
        {
            SessionContainer.SetLogInfo(session.SessionID);
            log.InfoFormat("ClearSession: SessionId={0}", session.SessionID);

            if (session.Contents[SessionContainerName] is SessionContainer sessionContainer)
            {
                session.Contents.Remove(SessionContainerName);
                sessionContainer.Dispose();
            }
        }

        private void SetupLog4Net()
        {
            if (string.IsNullOrEmpty(IstaFolder) || !Directory.Exists(IstaFolder))
            {
                return;
            }

            string logDir = Path.Combine(IstaFolder, @"logs\client");
            string dateString = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
            string fileName = string.Format(CultureInfo.InvariantCulture, "PsdzClient-{0}.log", dateString);
            string logFile = Path.Combine(logDir, fileName);
            ProgrammingJobs.SetupLog4Net(logFile);
        }
    }
}