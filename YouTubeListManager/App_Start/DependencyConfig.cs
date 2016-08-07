using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Google.Apis.Logging;
using Microsoft.Practices.Unity;
using YouTubeListAPI.Business.Service;
using YouTubeListManager.Controllers;
using YouTubeListManager.Data;
using YouTubeListManager.Data.Repository;
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
            string connectionString = ConfigurationManager.ConnectionStrings[ConnectionKey].ConnectionString;

            container.RegisterType<INlogLogger, NLogLogger>();

            container.RegisterType<DbContext, YouTubeListManagerContext>(new InjectionConstructor(connectionString));
            container.RegisterType(typeof (IRepository<>), typeof (Repository<>));
            container.RegisterType<IRepositoryStore, RepositoryStore>();
            container.RegisterType<IYouTubeListService, YouTubeListService>();
            

            GlobalConfiguration.Configuration.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(container);

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        } 
    }
}