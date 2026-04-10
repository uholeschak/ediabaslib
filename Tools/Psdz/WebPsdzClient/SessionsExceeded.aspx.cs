using System;
using WebPsdzClient.App_Data;

namespace WebPsdzClient
{
    public partial class SessionsExceeded : BasePage
    {
        public const string ReasonInternalFailure = "internal_failure";

        protected override void Page_Load(object sender, EventArgs e)
        {
            base.Page_Load(sender, e);

            string reason = Request.QueryString["reason"];
            if (string.Compare(reason, ReasonInternalFailure, StringComparison.OrdinalIgnoreCase) == 0)
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