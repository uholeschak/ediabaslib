using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
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
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            sessionContainer.Cts = new CancellationTokenSource();
            sessionContainer.StartProgrammingServiceTask(Global.IstaFolder).ContinueWith(task =>
            {
                sessionContainer.TaskActive = false;
                sessionContainer.Cts.Dispose();
                sessionContainer.Cts = null;
                UpdateStatus();
            });

            sessionContainer.TaskActive = true;
            UpdateStatus();
        }

        protected void ButtonStopHost_Click(object sender, EventArgs e)
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            sessionContainer.StopProgrammingServiceTask().ContinueWith(task =>
            {
                sessionContainer.TaskActive = false;
                UpdateStatus();
            });

            sessionContainer.TaskActive = false;
            UpdateStatus();
        }

        protected void TimerUpdate_Tick(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        private SessionContainer GetSessionContainer()
        {
            if (Session.Contents[Global.SessionContainerName] is SessionContainer sessionContainer)
            {
                return sessionContainer;
            }

            return null;
        }

        private void UpdateStatus()
        {
            SessionContainer sessionContainer = GetSessionContainer();
            if (sessionContainer == null)
            {
                return;
            }

            bool active = sessionContainer.TaskActive;
            bool abortPossible = sessionContainer.Cts != null;
            bool hostRunning = false;
            bool vehicleConnected = false;
            bool talPresent = false;
            if (!active)
            {
                hostRunning = sessionContainer.ProgrammingJobs.ProgrammingService != null && sessionContainer.ProgrammingJobs.ProgrammingService.IsPsdzPsdzServiceHostInitialized();
            }

            ButtonStartHost.Enabled = !active && !hostRunning;
            ButtonStopHost.Enabled = !active && hostRunning;

            TextBoxStatus.Text = sessionContainer.StatusText;
            UpdatePanelStatus.Update();
        }
    }
}