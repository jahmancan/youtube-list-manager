using System;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using YouTubeListManager.Logger;

namespace YouTubeListAPI.Business.Service
{
    public class YouTubeServiceProvider : IYouTubeServiceProvider
    {
        private const string ServiceName = "YouTubeListAPI";

        private readonly INlogLogger logger;

        private YouTubeService youTubeService;

        public YouTubeServiceProvider(INlogLogger logger)
        {
            this.logger = logger;
        }

        protected YouTubeService YouTubeService => youTubeService ?? (youTubeService = InitializeService());

        public YouTubeService GetInstance()
        {
            return YouTubeService;
        }

        private YouTubeService InitializeService()
        {
            var userCredential = Authenticate();
            var initializer = new BaseClientService.Initializer
            {
                HttpClientInitializer = userCredential,
                ApplicationName = ServiceName
            };

            return new YouTubeService(initializer);
        }

        private UserCredential Authenticate()
        {
            var configFilePath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            configFilePath += @"\bin\config.json";

            try
            {
                UserCredential userCredential;

                using (var stream = new FileStream(configFilePath, FileMode.Open, FileAccess.Read))
                {
                    userCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        new[] {YouTubeService.Scope.Youtube},
                        "user",
                        CancellationToken.None,
                        new FileDataStore(ServiceName)
                        ).Result;
                }

                return userCredential;
            }
            catch (Exception exception)
            {
                const string error = "Application could not be authenticated! Check if it is offline!";
                logger.LogError(error, exception);
                throw;
            }
        }
    }
}