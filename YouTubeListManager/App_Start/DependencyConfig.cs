using System.Configuration;
using System.Data.Entity;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using YouTubeListAPI.Business.Service.Wrapper;
using YouTubeListManager.BusinessContracts.Service.Wrapper;
using YouTubeListManager.Common.Bootstraper;
using YouTubeListManager.Data;
using YouTubeListManager.Logger;
using UnityDependencyResolver = Microsoft.Practices.Unity.Mvc.UnityDependencyResolver;

namespace YouTubeListManager
{
    public static class DependencyConfig
    {
        private const string ConnectionKey = "DefaultConnection";

        public static void Register()
        {
            IUnityContainer container = new UnityContainer();

            container.RegisterType<INlogLogger, NlogLogger>();

            string connectionString = ConfigurationManager.ConnectionStrings[ConnectionKey].ConnectionString;
            container.RegisterType<DbContext, YouTubeListManagerContext>(new InjectionConstructor(connectionString));

            container.RegisterType<IYouTubeApiListServiceWrapper, YouTubeApiListServiceWrapper>();

            DependencyConfigHelper.Register(container);

            GlobalConfiguration.Configuration.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(container);
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        } 
    }
}