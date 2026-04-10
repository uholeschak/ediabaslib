using System;
using WebPsdzClient.App_Data;

namespace WebPsdzClient
{
    public partial class SessionsExceeded : BasePage
    {
        protected override void Page_Load(object sender, EventArgs e)
        {
            base.Page_Load(sender, e);

            if (Global.InternalFailure)
            {
                Title = Resources.Global.InternalServerFailureTitle;
                LiteralSessionsExceeded.Text = Resources.Global.InternalServerFailure;
            }
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            Global.ClearSession(Session);
        }
    }
}