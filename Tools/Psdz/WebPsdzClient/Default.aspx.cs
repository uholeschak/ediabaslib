using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebPsdzClient
{
    public partial class _Default : Page
    {
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
            int int_MilliSecondsTimeOut = (this.Session.Timeout * 60000) - 30000;
            string str_Script = @"
<script type='text/javascript'>
    //Number of Reconnects
    var count=0;
    //Maximum reconnects setting
    var max = 5;
    function Reconnect()
    {
        count++;
        if (count < max)
        {
            window.status = 'Link to Server Refreshed ' + count.toString()+' time(s)' ;
            var img = new Image(1,1);
            img.src = 'Reconnect.aspx';
        }
    }

    window.setInterval('Reconnect()'," + int_MilliSecondsTimeOut.ToString() + @");
</script>
";

            Page.RegisterClientScriptBlock("Reconnect", str_Script);
        }
    }
}