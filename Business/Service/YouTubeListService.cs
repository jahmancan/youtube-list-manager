using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Logging;
using Google.Apis.Requests;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouTubeListManager.Data.Domain;
using YouTubeListManager.Data.Repository;
using YouTubeListManager.Logger;
using YouTubePlayList = Google.Apis.YouTube.v3.Data.Playlist;

namespace YouTubeListAPI.Business.Service
{
    //todo: add logging
    public class YouTubeListService : IYouTubeListService
    {
        private const string ServiceName = "YouTubeListAPI";
        private const string UnavailableVideoDescription = "This video is unavailable.";
        private const string PrivateVideoDescription = "This video is private.";
        private const string DeleteVideoTitle = "Deleted video";
        private const string PrivateVideoTitle = "Private video";
        private const string VideoKind = "youtube#video";
        private const int MaxResults = 50;
            
        private const string TitleCleanerPattern = @"(\(|\[)[\w\s]+?(\)|\])";
        private static readonly Regex TitleCleaneRegex = new Regex(TitleCleanerPattern);

        private const string MediaExtensionCleanerPattern = @"\.(mpg|mpeg|mp3|mp4|wma|avi|mov|mkv)";
        private static readonly Regex MediaExtensionCleaneRegex = new Regex(MediaExtensionCleanerPattern, RegexOptions.IgnoreCase);

        private YouTubeService youTubeService;
        private YouTubeService YouTubeService => youTubeService ?? (youTubeService = InitializeService());

        private IRepositoryStore repositoryStore;
        private INlogLogger logger;

        public YouTubeListService(IRepositoryStore repositoryStore, INlogLogger logger)
        {
            this.repositoryStore = repositoryStore;
            this.logger = logger;
        }

        public IEnumerable<Track> GetTracksBy(string playListId)
        {
            if (string.IsNullOrEmpty(playListId))
                throw new Exception("PlayListId can not be empty");

            PlaylistItemsResource.ListRequest request = YouTubeService.PlaylistItems.List("snippet, contentDetails");
            request.PlaylistId = playListId;
            request.MaxResults = MaxResults;
            try
            {
                var tracks = new List<Track>();
                var nextPageToken = string.Empty;
                while (nextPageToken != null)
                {
                    var taskResponse = request.ExecuteAsync(CancellationToken.None);
                    PlaylistItemListResponse playlistItemListResponse = taskResponse.Result;
                    request.PageToken = nextPageToken;

                    PopulateTracks(tracks, playlistItemListResponse);

                    nextPageToken = playlistItemListResponse.NextPageToken;
                }
                return tracks;
            }
            catch (Exception e)
            {
                throw new Exception("Your request could not been served!");
            }
        }

        public IEnumerable<PlayList> GetPlaylists(string playListId = default(string), bool withTracks = false)
        {
            PlaylistsResource.ListRequest request = YouTubeService.Playlists.List("snippet");
            request.MaxResults = MaxResults;
            if (!string.IsNullOrEmpty(playListId))
                request.Id = playListId;
            else
                request.Mine = true;

            try
            {
                var playLists = new List<PlayList>();
                var nextPageToken = string.Empty;
                while (nextPageToken != null)
                {
                    var taskResponse = request.ExecuteAsync(CancellationToken.None);
                    PlaylistListResponse result = taskResponse.Result;
                    request.PageToken = nextPageToken;

                    PopulatePlayList(playLists, result, withTracks);

                    nextPageToken = result.NextPageToken;
                }
                return playLists;
            }
            catch (Exception exception)
            {
                string error = "Your playlist list request could not been served!";
                logger.LogError(error, exception);
                throw exception;
            }
        }

        public IEnumerable<Track> ShowSuggestions(string title)
        {
            SearchResource.ListRequest request = YouTubeService.Search.List("id, snippet");
            request.MaxResults = MaxResults;
            request.Q = MediaExtensionCleaneRegex.Replace(TitleCleaneRegex.Replace(title, string.Empty), string.Empty);

            try
            {
                var tracks = new List<Track>();
                var nextPageToken = string.Empty;
                while (nextPageToken != null)
                {
                    var taskResponse = request.ExecuteAsync(CancellationToken.None);
                    SearchListResponse result = taskResponse.Result;
                    request.PageToken = nextPageToken;

                    var uniqueHashes = tracks.Select(t => t.Hash).Distinct();
                    var currentTracks = result.Items.Where(i => !uniqueHashes.Contains(i.Id.VideoId) && i.Id.Kind == VideoKind).Select(i => new Track
                    {
                        Hash = i.Id.VideoId,
                        Title = i.Snippet.Title,
                        ThumbnailUrl = GetThumbnailUrl(i.Snippet.Thumbnails),
                        Live = true
                    });

                    tracks.AddRange(currentTracks);
                    nextPageToken = result.NextPageToken;
                }
                return tracks;
            }
            catch (Exception exception)
            {
                string error = "Your search list request could not been served!";
                logger.LogError(error, exception);
                throw exception;
            }
        }

        public void UpdateLists(IEnumerable<PlayList> playLists)
        {
            foreach (PlayList playList in playLists)
            {
                UpdateList(playList);
            }
        }

        private YouTubePlayList GetPlayList(string hash)
        {
            PlaylistsResource.ListRequest request = YouTubeService.Playlists.List("snippet, contentDetails, status");
            request.MaxResults = MaxResults;
            request.Id = hash;
            try
            {
                var taskResponse = request.ExecuteAsync(CancellationToken.None);
                PlaylistListResponse result = taskResponse.Result;
                if (result.Items.Count == 0)
                    return null;

                return result.Items[0];
            }
            catch (Exception exception)
            {
                string error = "Your get playlist request could not been served!";
                logger.LogError(error, exception);
                throw exception;
            }
        }

        private void UpdateList(PlayList playList)
        {
            YouTubePlayList youTubePlaylist = GetPlayList(playList.Hash);
            if (youTubePlaylist == null) return;

            youTubePlaylist.Snippet.Title = playList.Title;
            youTubePlaylist.Status.PrivacyStatus = Enum.GetName(typeof(PrivacyStatus), playList.PrivacyStatus).ToLower();

            try
            {
                PlaylistsResource.UpdateRequest request = youTubeService.Playlists.Update(youTubePlaylist,
                    "snippet, status, contentDetails");
                request.ExecuteAsync();
            }
            catch (Exception e)
            {
                string error = "Your update playlist request could not been served!";
                logger.LogError(error, e);
                throw new Exception(error);
            }
        }

        private static string GetThumbnailUrl(ThumbnailDetails thumbnailDetails)
        {
            if (thumbnailDetails == null)
                return string.Empty;

            return (thumbnailDetails.Standard != null) ? thumbnailDetails.Standard.Url : thumbnailDetails.Default__.Url;
        }

        private static void PopulateTracks(List<Track> tracks, PlaylistItemListResponse playlistItemListResponse)
        {
            IEnumerable<string> uniqueTrackHashes = tracks.Select(t => t.Hash).Distinct();
            IEnumerable<Track> currentTracks = playlistItemListResponse.Items
                .Where(
                    i =>
                        !uniqueTrackHashes.Contains(i.ContentDetails.VideoId) && i.Snippet.Title != DeleteVideoTitle &&
                        i.Snippet.Title != PrivateVideoTitle && i.Snippet.Description != UnavailableVideoDescription &&
                        i.Snippet.Description != PrivateVideoDescription)
                .Select(playListItem => new Track
                {
                    Hash = playListItem.ContentDetails.VideoId,
                    Title = playListItem.Snippet.Title,
                    Duration = Int32.Parse(playListItem.ContentDetails.EndAt ?? "0"),
                    ThumbnailUrl = GetThumbnailUrl(playListItem.Snippet.Thumbnails),
                    Live = true
                });
            tracks.AddRange(currentTracks);
        }

        private void PopulatePlayList(List<PlayList> playLists, PlaylistListResponse result, bool withTracks = false)
        {
            IEnumerable<string> playListIds = playLists.Select(pl => pl.Hash).Distinct();
            IEnumerable<PlayList> currentPlayLists = result.Items
                .Where(i => !playListIds.Contains(i.Id))
                .Select(youTubePlayList => new PlayList
                {
                    Hash = youTubePlayList.Id,
                    Title = youTubePlayList.Snippet.Title,
                    Tracks = withTracks ? GetTracksBy(youTubePlayList.Id).ToList() : new List<Track>()
                });
            playLists.AddRange(currentPlayLists);
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
                string error = "Application could not be authenticated! Check if it is offline!";
                logger.LogError(error, exception);
                throw exception;
            }
        }
    }
}
