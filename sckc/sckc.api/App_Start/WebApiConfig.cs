using System.Web.Http;
using System.Web.Http.Cors;

namespace sckc.api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
#if DEBUG
            // Web API configuration and services
            EnableCorsAttribute cors = new EnableCorsAttribute("http://localhost:4200", "*", "*"); 
            config.EnableCors(cors);
#endif
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
