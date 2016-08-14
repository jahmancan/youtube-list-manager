using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouTubeListManager.Logger;

namespace YouTubeListAPI.Business.Service.Wrapper
{
    public abstract class YouTubeApiServiceWrapper : IYouTubeApiServiceWrapper
    {
        private const string ServiceName = "YouTubeListAPI";

        protected YouTubeService youTubeService;

        protected INlogLogger logger;
        protected YouTubeService YouTubeService => youTubeService ?? (youTubeService = InitializeService());

        public YouTubeApiServiceWrapper()
        {
            
        }

        protected YouTubeApiServiceWrapper(INlogLogger logger)
        {
            this.logger = logger;
        }

        public Video GetVideo(string hash)
        {
            var request = youTubeService.Videos.List("contentDetails, snippet, status");
            request.Id = hash;

            try
            {
                var taskResponse = request.ExecuteAsync();
                var result = taskResponse.Result;
                if (result.Items.Count == 0)
                    return null;

                return result.Items[0];
            }
            catch (Exception exception)
            {
                const string error = "Your get video by ID request could not been served!";
                logger.LogError(error, exception);
                throw;
            }
        }

        public PlaylistItem GetPlayListItem(string hash)
        {
            PlaylistItemsResource.ListRequest request = youTubeService.PlaylistItems.List("snippet, contentDetails, status");
            request.Id = hash;
            try
            {
                Task<PlaylistItemListResponse> taskResponse = request.ExecuteAsync();
                var result = taskResponse.Result;
                if (result.Items.Count == 0)
                    return null;

                return result.Items[0];
            }
            catch (Exception exception)
            {
                const string error = "Your get playlist item request could not been served!";
                logger.LogError(error, exception);
                throw;
            }
        }

        public Playlist GetYouTubePlayList(string hash)
        {
            var request = youTubeService.Playlists.List("snippet, contentDetails, status");
            request.Id = hash;
            try
            {
                var taskResponse = request.ExecuteAsync();
                var result = taskResponse.Result;
                if (result.Items.Count == 0)
                    return null;

                return result.Items[0];
            }
            catch (Exception exception)
            {
                const string error = "Your get playlist request could not been served!";
                logger.LogError(error, exception);
                throw;
            }
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
                        new[] { YouTubeService.Scope.Youtube },
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
