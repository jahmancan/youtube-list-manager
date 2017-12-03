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
                new { controller = "Playlist", action = "Get" }
            );

            routes.MapRoute(
                "PlayListGetAsync",
                "api/playlist/getasync/{hash}",
                new { controller = "Playlist", action = "GetAsync" }
            );

            routes.MapRoute(
                "PlayListGetAll",
                "api/playlist/getall/{requestToken}",
                new { controller = "Playlist", action = "GetAll", requestToken = UrlParameter.Optional }
            );

            routes.MapRoute(
               "PlayListGetAllAsync",
               "playlist/getallasync/{requestToken}",
               new { controller = "Playlist", action = "GetAllAsync", requestToken = UrlParameter.Optional }
           );

            routes.MapRoute(
                "PlayListItemGet",
                "api/playlistitem/get/{playListId}/{requestToken}",
                new { controller = "PlaylistItem", action = "Get", requestToken = UrlParameter.Optional }
            );

            routes.MapRoute(
               "PlayListItemGetAsync",
               "api/playlistitem/getasync/{playListId}/{requestToken}",
               new { controller = "PlaylistItem", action = "GetAsync", requestToken = UrlParameter.Optional }
           );

            routes.MapRoute(
                name: "angular",
                url: "{*url}",
                defaults: new { controller = "ListManager", action = "Index" } // The view that bootstraps Angular 2
            );
        }
    }
}
