using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebPsdzClient
{
    public partial class AccessDenied : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Global.Page_Load(this);
        }

        protected void Page_Unload(object sender, EventArgs e)
        {
            Global.ClearSession(Session);
        }
    }
}