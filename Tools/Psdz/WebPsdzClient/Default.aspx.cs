using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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

        private void UpdateStatus(bool increment = false)
        {
            int? counter = Session.Contents["Counter"] as int?;
            if (!counter.HasValue)
            {
                counter = 0;
            }
            else
            {
                if (increment)
                {
                    counter++;
                }
            }

            Session.Contents["Counter"] = counter;

            TextBoxStatus.Text = string.Format(CultureInfo.InvariantCulture, "Counter: {0}", counter);
            ButtonStartHost.Enabled = (counter & 0x01) == 0;
        }
    }
}