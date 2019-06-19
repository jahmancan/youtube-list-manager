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
                "PlayListGetAsync",
                "api/playlist/getasync/{hash}/{isOffline}/{withPlaylistItems}",
                new { controller = "Playlist", action = "GetAsync", isOffline = UrlParameter.Optional, withPlaylistItems = UrlParameter.Optional }
            );


            routes.MapRoute(
               "PlayListGetAllAsync",
               "playlist/getallasync/{requestToken}/{isOffline}",
               new { controller = "Playlist", action = "GetAllAsync", requestToken = UrlParameter.Optional, isOffline = UrlParameter.Optional }
           );


            routes.MapRoute(
               "PlayListItemGetAsync",
               "api/playlistitem/getasync/{playListId}/{requestToken}/{isOffline}",
               new { controller = "PlaylistItem", action = "GetAsync", requestToken = UrlParameter.Optional, isOffline = UrlParameter.Optional }
           );

            routes.MapRoute(
                "SearchVideo",
                "video/post",
                new { controller = "Video", action = "Post" }
            );

            routes.MapRoute(
                name: "angular",
                url: "{*url}",
                defaults: new { controller = "ListManager", action = "Index" } // The view that bootstraps Angular 2
            );
        }
    }
}
