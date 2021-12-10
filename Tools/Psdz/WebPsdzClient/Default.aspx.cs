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
            AddKeepAlive();
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

        private void AddKeepAlive()
        {
            int reloadTimeout = (this.Session.Timeout * 60000) - 30000;
            string script = @"
<script type='text/javascript'>
    function Reload()
    {
        location.reload(true);
    }

    window.setInterval('Reload()'," + reloadTimeout.ToString(CultureInfo.InvariantCulture) + @");
</script>
";

            if (!ClientScript.IsClientScriptBlockRegistered(ReloadScriptName))
            {
                ClientScript.RegisterClientScriptBlock(GetType(), ReloadScriptName, script);
            }
        }
    }
}