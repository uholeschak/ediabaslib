using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebPsdzClient.App_Data;

namespace WebPsdzClient
{
    public partial class AccessDenied : BasePage
    {
        protected override void Page_Load(object sender, EventArgs e)
        {
            base.Page_Load(sender, e);
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            Global.ClearSession(Session);
        }
    }
}