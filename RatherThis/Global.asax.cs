using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using RatherThis.Infrastructure;

namespace RatherThis
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Love",
                "love-sex",
                new { controller = "question", action = "index", qcat = 1 }
                );

            routes.MapRoute(
                "sports",
                "sports",
                new { controller = "question", action = "index", qcat = 2 }
                );

            routes.MapRoute(
                "jeremy-lin",
                "linsanity",
                new { controller = "question", action = "index", qcat = 2 }
                );

            routes.MapRoute(
                "tech games",
                "tech-games",
                new { controller = "question", action = "index", qcat = 3 }
                );

            routes.MapRoute(
                "news politics",
                "news-politics",
                new { controller = "question", action = "index", qcat = 4 }
                );

            routes.MapRoute(
                "people life",
                "people-life",
                new { controller = "question", action = "index", qcat = 5 }
                );

            routes.MapRoute(
                "life",
                "life",
                new { controller = "question", action = "index", qcat = 5 }
                );

            routes.MapRoute(
                "school-work",
                "school-work",
                new { controller = "question", action = "index", qcat = 6 }
                );

            routes.MapRoute(
                "school",
                "school",
                new { controller = "question", action = "index", qcat = 6 }
                );

            routes.MapRoute(
                "work",
                "work",
                new { controller = "question", action = "index", qcat = 6 }
                );

            routes.MapRoute(
                "rest",
                "rest",
                new { controller = "question", action = "index", qcat = 0 }
                );

            routes.MapRoute(
                "all",
                "",
                new { controller = "question", action = "index", qcat = -1 }
                );


            routes.MapRoute(
                "UserDetail",
                "User/{username}",
                new { controller = "account", action = "userdetail" }
            );

            routes.MapRoute(
                "QuestionDetail",
                "Questions/{qid}",
                new { controller = "question", action = "detail" }
            );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "question", action = "index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            //tell mvc to use our ninject controller factory
            ControllerBuilder.Current.SetControllerFactory(new NinjectControllerFactory());
        }
    }
}