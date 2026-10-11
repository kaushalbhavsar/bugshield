using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BUGSKAN.Portal.Startup))]
namespace BUGSKAN.Portal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
