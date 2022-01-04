using Microsoft.Owin;
using Owin;
using System;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(WebPsdzClient.Startup))]

namespace WebPsdzClient
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
#if false
            app.Run(context =>
            {
                string t = DateTime.Now.Millisecond.ToString();
                return context.Response.WriteAsync(t + " Production OWIN App");
            });
#endif
        }
    }
}
