using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(sckc.azure.Startup))]
namespace sckc.azure
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
