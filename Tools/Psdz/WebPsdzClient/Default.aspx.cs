using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebPsdzClient.App_Data;

namespace WebPsdzClient
{
    public partial class _Default : Page
    {
        private const string ReloadScriptName = "Reload";

        protected void Page_Init(object sender, EventArgs e)
        {

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        protected void Page_Unload(object sender, EventArgs e)
        {

        }

        protected void ButtonStartHost_Click(object sender, EventArgs e)
        {

        }

        protected void ButtonStopHost_Click(object sender, EventArgs e)
        {

        }

        protected void TimerUpdate_Tick(object sender, EventArgs e)
        {
            UpdateStatus(true);
        }

        private SessionContainer GetSessionContainer()
        {
            if (Session.Contents[Global.SessionContainerName] is SessionContainer sessionContainer)
            {
                return sessionContainer;
            }

            return null;
        }

        private void UpdateStatus(bool increment = false)
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            if (increment)
            {
                sessionContainer.TestCounter++;
            }

            TextBoxStatus.Text = string.Format(CultureInfo.InvariantCulture, "Counter: {0}", sessionContainer.TestCounter);
            ButtonStartHost.Enabled = (sessionContainer.TestCounter & 0x01) == 0;
        }
    }
}