using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using PsdzClient.Programing;
using WebPsdzClient.App_Data;

namespace WebPsdzClient
{
    public class Global : HttpApplication
    {
        public const string DealerId = "32395";
        public const string IstaFolder = "E:\\ISTA-D-4.31.16";
        public const string SessionContainerName = "SessionContainer";

        public override void Init()
        {
            base.Init();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            // Code, der beim Anwendungsstart ausgeführt wird
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
        }

        protected void Application_End(object sender, EventArgs e)
        {
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            if (!(Session.Contents[SessionContainerName] is SessionContainer))
            {
                SessionContainer sessionContainer = new SessionContainer(Session.SessionID, DealerId);
                Session.Contents.Add(SessionContainerName, sessionContainer);
            }
        }

        protected void Session_End(object sender, EventArgs e)
        {
            if (Session.Contents[SessionContainerName] is SessionContainer sessionContainer)
            {
                Session.Contents.Remove(SessionContainerName);
                sessionContainer.Dispose();
            }
        }
    }
}