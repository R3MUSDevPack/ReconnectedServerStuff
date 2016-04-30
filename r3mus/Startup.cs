using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using System;
using System.Threading.Tasks;
using System.Web.Cors;

[assembly: OwinStartupAttribute(typeof(r3mus.Startup))]
namespace r3mus
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.Map("/signalr", map =>
            {
                //map.UseCors(CorsOptions.AllowAll);
                map.UseCors(_corsOptions_1.Value);
                //map.UseCors(_corsOptions_2.Value);
                var hubConnection = new HubConfiguration();
                map.RunSignalR(hubConnection);
            });
        }
        private static Lazy<CorsOptions> _corsOptions_1 = new Lazy<CorsOptions>(() =>
        {
            return new CorsOptions
            {
                PolicyProvider = new CorsPolicyProvider
                {
                    PolicyResolver = context =>
                    {
                        var policy = new CorsPolicy();
                        policy.Origins.Add("http://map.bladesofgrass.space");
                        policy.Origins.Add("https://map.bladesofgrass.space");
                        policy.Origins.Add("map.bladesofgrass.space");
                        policy.Origins.Add("http://map.r3mus.org");
                        policy.Origins.Add("https://map.r3mus.org");
                        policy.Origins.Add("map.r3mus.org");
                        policy.AllowAnyMethod = true;
                        policy.AllowAnyHeader = true;
                        policy.SupportsCredentials = true;
                        return Task.FromResult(policy);
                    }
                }
            };
        });
        private static Lazy<CorsOptions> _corsOptions_2 = new Lazy<CorsOptions>(() =>
        {
            return new CorsOptions
            {
                PolicyProvider = new CorsPolicyProvider
                {
                    PolicyResolver = context =>
                    {
                        var policy = new CorsPolicy();
                        policy.Origins.Add("http://localhost");
                        policy.AllowAnyMethod = true;
                        policy.AllowAnyHeader = true;
                        policy.SupportsCredentials = true;
                        return Task.FromResult(policy);
                    }
                }
            };
        });
    }
}
