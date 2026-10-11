using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BugShield.Portal.Startup))]
namespace BugShield.Portal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
