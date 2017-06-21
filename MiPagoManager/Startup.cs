using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MiPagoManager.Startup))]
namespace MiPagoManager
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
