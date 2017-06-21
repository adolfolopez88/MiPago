using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MiPago
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "MiPago.Controllers" }
            );

            //routes.IgnoreRoute("{file}.cgi");

            routes.MapRoute("cgi", "{*allcgi}", new { allcgi = @".*\.(cgi|html)" }, namespaces: new[] { "MiPago.Controllers" });
        }
    }
}
