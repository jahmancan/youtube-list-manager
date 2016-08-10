using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouTubeListManager.Logger;

namespace YouTubeListAPI.Business.Service
{
    public abstract class YouTubeApiServiceWrapper
    {
        protected YouTubeService youTubeService;

        protected INlogLogger logger;
        private IYouTubeServiceProvider youTubeServiceProvider;

        protected YouTubeApiServiceWrapper(INlogLogger logger, IYouTubeServiceProvider youTubeServiceProvider)
        {
            this.logger = logger;
            this.youTubeServiceProvider = youTubeServiceProvider;

            youTubeService = this.youTubeServiceProvider.GetInstance();
        }

        protected Video GetVideo(string hash)
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

        protected PlaylistItem GetPlayListItem(string hash)
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

        protected Playlist GetYouTubePlayList(string hash)
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
    }
}
