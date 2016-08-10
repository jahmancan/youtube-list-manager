using System;
using System.Data.Entity;
using System.Threading;
using Google.Apis.YouTube.v3;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using YouTubeListAPI.Business.Service;
using YouTubeListManager.Common.Bootstraper;
using YouTubeListManager.Data;
using YouTubeListManager.Logger;
using YouTubeListManager.Test.Common;
using YouTubeListManager.Test.Common.Helpers;

namespace YouTubeListManager.Test
{
    public class BootStrapper
    {
        protected readonly Mock<YouTubeService> youTubeServiceMock = new Mock<YouTubeService>();
        protected readonly Mock<INlogLogger> nlogLoggerMock = new Mock<INlogLogger>();
        protected readonly UnityContainer container = new UnityContainer();

        public BootStrapper()
        {
            container.RegisterInstance(nlogLoggerMock.Object);

            TestDbHelper dbHelper = new TestDbHelper();
            string connectionString = dbHelper.GetConnectionString();
            container.RegisterType<DbContext, YouTubeListManagerContext>(new InjectionConstructor(connectionString));

            DependencyConfigHelper.Register(container);

            Mock<IYouTubeServiceProvider> youTubeServiceProviderMock = new Mock<IYouTubeServiceProvider>();
            youTubeServiceProviderMock.Setup(p => p.GetInstance()).Returns(youTubeServiceMock.Object);
            container.RegisterInstance(youTubeServiceProviderMock.Object);
        }
    }
}
