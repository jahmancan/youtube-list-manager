using Microsoft.Practices.Unity;
using YouTubeListAPI.Business;
using YouTubeListAPI.Business.Service;
using YouTubeListAPI.Business.Service.Response;
using YouTubeListAPI.Business.Service.Wrapper;
using YouTubeListManager.BusinessContracts;
using YouTubeListManager.BusinessContracts.Service;
using YouTubeListManager.BusinessContracts.Service.Response;
using YouTubeListManager.BusinessContracts.Service.Wrapper;
using YouTubeListManager.Data.Repository;
using YouTubeListManager.DataContracts.Repository;

namespace YouTubeListManager.Common.Bootstraper
{
    public static class DependencyConfigHelper
    {
        public static void Register(IUnityContainer container)
        {
            container.RegisterType(typeof(IRepository<>), typeof(Repository<>));
            container.RegisterType<IRepositoryStore, RepositoryStore>();
            container.RegisterType<IYouTubeListManagerCache, YouTubeListManagerCache>(new ContainerControlledLifetimeManager());
            container.RegisterType<IPlaylistResponseService, PlaylistResponseService>();
            container.RegisterType<IPlaylistItemResponseService, PlaylistItemResponseService>();
            container.RegisterType<ISearchListResponseService, SearchListResponseService>();
            container.RegisterType<IYouTubeApiUpdateServiceWrapper, YouTubeApiUpdateServiceWrapper>();
            container.RegisterType<IYouTubeListManagerService, YouTubeListManagerManagerService>();
        }
    }
}
