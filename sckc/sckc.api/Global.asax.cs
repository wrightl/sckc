using System.Web.Http;
using UrlRewrite;

namespace sckc.api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            RewriteModule.Initialize(null, url => true, url => true);
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
