using Microsoft.Practices.Unity;
using YouTubeListAPI.Business;
using YouTubeListAPI.Business.Service;
using YouTubeListAPI.Business.Service.Response;
using YouTubeListAPI.Business.Service.Wrapper;
using YouTubeListManager.Data.Repository;

namespace YouTubeListManager.Common.Bootstraper
{
    public static class DependencyConfigHelper
    {
        public static void Register(IUnityContainer container)
        {
            container.RegisterType(typeof(IRepository<>), typeof(Repository<>));
            container.RegisterType<IRepositoryStore, RepositoryStore>();
            container.RegisterType<IYouTubeListManagerCache, YouTubeListManagerCache>();
            container.RegisterType<IPlaylistResponseService, PlaylistResponseService>();
            container.RegisterType<IPlaylistItemResponseService, PlaylistItemResponseService>();
            container.RegisterType<ISearchListResponseService, SearchListResponseService>();
            container.RegisterType<IYouTubeUpdateService, YouTubeApiUpdateServiceWrapper>();
            container.RegisterType<IYouTubeListService, YouTubeListManagerService>();
        }
    }
}
