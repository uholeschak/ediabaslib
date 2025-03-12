using log4net;
using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.UI;

namespace WebPsdzClient.App_Data
{
    public class BasePage : Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BasePage));

        protected override void InitializeCulture()
        {
            SessionContainer.SetLogInfo(Session.SessionID);
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                base.InitializeCulture();
                return;
            }

            if (sessionContainer.DeepObdVersion <= 0)
            {   // set browser culture as session culture
                if (!sessionContainer.LanguageSet)
                {
                    CultureInfo culture = Thread.CurrentThread.CurrentUICulture;
                    sessionContainer.SetLanguage(culture.TwoLetterISOLanguageName);
                }
            }

            string language = sessionContainer.GetLanguage();
            if (!string.IsNullOrEmpty(language))
            {
                try
                {
                    CultureInfo culture = CultureInfo.CreateSpecificCulture(language.ToLowerInvariant());
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = culture;
                    Culture = culture.TwoLetterISOLanguageName;
                    UICulture = culture.TwoLetterISOLanguageName;
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("InitializeCulture Exception: {0}", ex.Message);
                }
            }

            base.InitializeCulture();
        }

        protected virtual void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
        }

        protected SessionContainer GetSessionContainer()
        {
            if (Session.Contents[Global.SessionContainerName] is SessionContainer sessionContainer)
            {
                return sessionContainer;
            }

            log.ErrorFormat("GetSessionContainer No SessionContainer");
            return null;
        }

        protected Control GetPostBackControl()
        {
            Control control = null;
            string ctrlname = Request.Params.Get("__EVENTTARGET");
            if (!string.IsNullOrEmpty(ctrlname))
            {
                control = FindControl(ctrlname);
            }
            else
            {
                foreach (string ctl in Request.Form)
                {
                    Control c = FindControl(ctl);
                    if (c is System.Web.UI.WebControls.Button)
                    {
                        control = c;
                        break;
                    }
                }

            }

            return control;
        }
    }
}
