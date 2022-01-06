using Microsoft.Owin;
using Owin;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Cors;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.Cookies;

[assembly: OwinStartup(typeof(WebPsdzClient.Startup))]

namespace WebPsdzClient
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            var hubConfiguration = new HubConfiguration();
            hubConfiguration.EnableDetailedErrors = false;
            app.MapSignalR();
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            string originsSetting = ConfigurationManager.AppSettings["Origins"];
            if (!string.IsNullOrEmpty(originsSetting))
            {
                if (string.Compare(originsSetting.Trim(), "*", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    app.UseCors(CorsOptions.AllowAll);
                }
                else
                {
                    string[] allowedOrigins = originsSetting.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    CorsPolicy corsPolicy = new CorsPolicy()
                    {
                        AllowAnyHeader = true,
                        AllowAnyMethod = true,
                        SupportsCredentials = true
                    };

                    foreach (string origin in allowedOrigins)
                    {
                        corsPolicy.Origins.Add(origin.Trim());
                    }

                    CorsPolicyProvider policyProvider = new CorsPolicyProvider()
                    {
                        PolicyResolver = (context) => Task.FromResult(corsPolicy)
                    };

                    CorsOptions corsOptions = new CorsOptions()
                    {
                        PolicyProvider = policyProvider
                    };

                    app.UseCors(corsOptions);
                }
            }

            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login")
            });
            // Use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            // clientId: "",
            // clientSecret: "");

            //app.UseTwitterAuthentication(
            // consumerKey: "",
            // consumerSecret: "");

            //app.UseFacebookAuthentication(
            // appId: "",
            // appSecret: "");

            //app.UseGoogleAuthentication();
        }
    }
}
