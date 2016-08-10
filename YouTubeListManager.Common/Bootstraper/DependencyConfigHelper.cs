using Microsoft.Practices.Unity;
using YouTubeListAPI.Business.Service;
using YouTubeListManager.Data.Repository;
using YouTubeListManager.Logger;

namespace YouTubeListManager.Common.Bootstraper
{
    public static class DependencyConfigHelper
    {
        public static void Register(IUnityContainer container)
        {
            container.RegisterType(typeof(IRepository<>), typeof(Repository<>));
            container.RegisterType<IRepositoryStore, RepositoryStore>();
            container.RegisterType<IYouTubeListService, YouTubeListServiceWrapper>();
            container.RegisterType<IYouTubeUpdateService, YouTubeUpdateServiceWrapper>();
        }
    }
}
