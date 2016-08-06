using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Data;
using Microsoft.Practices.Unity;
using YouTubeListManager.Controllers;
using YouTubeListManager.Data.Repository;
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

            container.RegisterType<DbContext, YouTubeListManagerContext>(new InjectionConstructor(connectionString));
            container.RegisterType(typeof (IRepository<>), typeof (Repository<>));
            container.RegisterType<IRepositoryStore, RepositoryStore>();

            GlobalConfiguration.Configuration.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(container);

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        } 
    }
}