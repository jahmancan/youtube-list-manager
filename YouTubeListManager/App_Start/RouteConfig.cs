using System.Web.Mvc;
using System.Web.Routing;

namespace YouTubeListManager
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "PlayListGet",
                "api/playlist/get/{hash}",
                new { controller = "PlayList", action = "Get" }
            );

            routes.MapRoute(
                "PlayListGetAll",
                "api/playlist/getall/{requestToken}",
                new { controller = "PlayList", action = "GetAll", requestToken = UrlParameter.Optional }
            );

            routes.MapRoute(
               "PlayListGetAllAsync",
               "api/playlist/getallasync/{requestToken}",
               new { controller = "PlayList", action = "GetAllAsync", requestToken = UrlParameter.Optional }
           );

            routes.MapRoute(
                "PlayListItemGet",
                "api/playlistitem/get/{playListId}/{requestToken}",
                new { controller = "PlayListItem", action = "Get", requestToken = UrlParameter.Optional }
            );

            
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Manager", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
