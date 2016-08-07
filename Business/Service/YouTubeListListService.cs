using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business.Service
{
    //todo: add logging
    public class YouTubeListListService : IYouTubeListService
    {
        private const string ServiceName = "YouTubeListAPI";
        private const string UnavailableVideoDescription = "This video is unavailable.";
        private const string PrivateVideoDescription = "This video is private.";
        private const string DeleteVideoTitle = "Deleted video";
        private const string PrivateVideoTitle = "Private video";

        private YouTubeService youTubeService;
        private YouTubeService YouTubeService => youTubeService ?? (youTubeService = InitializeService());

        //todo: refactor to async
        public IList<Track> GetTracksBy(string playListId, int maxResults = 50)
        {
            if (string.IsNullOrEmpty(playListId))
                throw new Exception("PlayListId can not be empty");
            if (maxResults > 50)
                throw new Exception("Google can not retrieve more than 50 items per request");

            PlaylistItemsResource.ListRequest request  = YouTubeService.PlaylistItems.List("snippet, contentDetails");
            request.PlaylistId = playListId;
            request.MaxResults = maxResults;
            try
            {
                var tracks = new List<Track>();
                var nextPageToken = string.Empty;
                while (nextPageToken != null)
                {
                    var response = request.Execute();
                    request.PageToken = nextPageToken;

                    IEnumerable<string> uniqueTrackHashes = tracks.Select(t => t.Hash).Distinct();
                    IEnumerable<Track> currentTracks = response.Items
                        .Where(i => !uniqueTrackHashes.Contains(i.ContentDetails.VideoId) && i.Snippet.Title != DeleteVideoTitle && i.Snippet.Title != PrivateVideoTitle && i.Snippet.Description != UnavailableVideoDescription && i.Snippet.Description != PrivateVideoDescription)
                        .Select(playListItem => new Track
                        {
                            Hash = playListItem.ContentDetails.VideoId,
                            Title = playListItem.Snippet.Title,
                            Duration = Int32.Parse(playListItem.ContentDetails.EndAt ?? "0"),
                            ThumbnailUrl = (playListItem.Snippet.Thumbnails.Standard != null) ? playListItem.Snippet.Thumbnails.Standard.Url : playListItem.Snippet.Thumbnails.Default__.Url,
                            Live = true
                        });
                    tracks.AddRange(currentTracks);

                    nextPageToken = response.NextPageToken;
                }
                return tracks;
            }
            catch (Exception e)
            {
                throw new Exception("Your request could not been served!");
            }
        }

        //todo: refactor to async
        public IEnumerable<PlayList> GetPlaylists(int maxResults = 50)
        {
            if (maxResults > 50)
                throw new Exception("Google can not retrieve more than 50 items per request");

            PlaylistsResource.ListRequest request = YouTubeService.Playlists.List("snippet");
            request.MaxResults = maxResults;
            request.Mine = true;
            try
            {
                var playLists = new List<PlayList>();
                var nextPageToken = string.Empty;
                while (nextPageToken != null)
                {
                    var response = request.Execute();
                    request.PageToken = nextPageToken;

                    IEnumerable<string> playListIds = playLists.Select(pl => pl.Hash).Distinct();
                    IEnumerable<PlayList> currentPlayLists = response.Items
                        .Where(i => !playListIds.Contains(i.Id))
                        .Select(youTubePlayList => new PlayList
                    {
                        Hash = youTubePlayList.Id,
                        Title = youTubePlayList.Snippet.Title,
                        Tracks = GetTracksBy(youTubePlayList.Id)
                    });
                    playLists.AddRange(currentPlayLists);

                    nextPageToken = response.NextPageToken;
                }
                return playLists;
            }
            catch (Exception e)
            {
                throw new Exception("Your request could not been served!");
            }
        }

        private YouTubeService InitializeService()
        {
            UserCredential userCredential = Authenticate();
            BaseClientService.Initializer initializer = new BaseClientService.Initializer
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
            catch
            {
                throw new Exception("Application could not be authenticated! Check if it is offline!");
            }
        }
    }
}
