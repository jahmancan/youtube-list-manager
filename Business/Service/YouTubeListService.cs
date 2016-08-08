using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Google.Apis.Auth.OAuth2;
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

        private const string MinutesGroupName = "minutes";
        private const string SecondsGroupName = "seconds";
        private static readonly string namedDurationPattern = $@"PT(?<{MinutesGroupName}>\d+)M(?<{SecondsGroupName}>\d+)S";
        private static readonly Regex NamedDurationRegex = new Regex(namedDurationPattern, RegexOptions.IgnoreCase);

        private readonly INlogLogger logger;
        private readonly IRepositoryStore repositoryStore;

        private YouTubeService youTubeService;

        public YouTubeListService(IRepositoryStore repositoryStore, INlogLogger logger)
        {
            this.repositoryStore = repositoryStore;
            this.logger = logger;
        }

        private YouTubeService YouTubeService => youTubeService ?? (youTubeService = InitializeService());

        public IEnumerable<PlayListItem> GetPlayListItems(string playListId, bool withVideoInfo = true)
        {
            if (string.IsNullOrEmpty(playListId))
                throw new Exception("PlayListId can not be empty");

            var request = YouTubeService.PlaylistItems.List("snippet, contentDetails");
            request.PlaylistId = playListId;
            request.MaxResults = MaxResults;
            try
            {
                var tracks = new List<PlayListItem>();
                var nextPageToken = string.Empty;
                while (nextPageToken != null)
                {
                    var taskResponse = request.ExecuteAsync(CancellationToken.None);
                    var playlistItemListResponse = taskResponse.Result;
                    request.PageToken = nextPageToken;

                    PopulatePlayListItems(tracks, playlistItemListResponse, withVideoInfo);

                    nextPageToken = playlistItemListResponse.NextPageToken;
                }
                return tracks;
            }
            catch (Exception exception)
            {
                const string error = "Your playListItem list request could not been served!";
                logger.LogError(error, exception);
                throw;
            }
        }

        public IEnumerable<PlayList> GetPlaylists(string playListId = default(string), bool withPlayListItems = false)
        {
            var request = YouTubeService.Playlists.List("snippet");
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
                    var result = taskResponse.Result;
                    request.PageToken = nextPageToken;

                    PopulatePlayList(playLists, result, withPlayListItems);

                    nextPageToken = result.NextPageToken;
                }
                return playLists;
            }
            catch (Exception exception)
            {
                const string error = "Your playlist list request could not been served!";
                logger.LogError(error, exception);
                throw;
            }
        }

        public IEnumerable<VideoInfo> ShowSuggestions(string title)
        {
            var request = YouTubeService.Search.List("id, snippet");
            request.MaxResults = MaxResults;
            request.Q = MediaExtensionCleaneRegex.Replace(TitleCleaneRegex.Replace(title, string.Empty), string.Empty);

            try
            {
                var playListItems = new List<VideoInfo>();
                var nextPageToken = string.Empty;
                while (nextPageToken != null)
                {
                    var taskResponse = request.ExecuteAsync(CancellationToken.None);
                    var result = taskResponse.Result;
                    request.PageToken = nextPageToken;

                    var uniqueHashes = playListItems.Select(t => t.Hash).Distinct();
                    var currentPlayListItems = result.Items
                        .Where(i => !uniqueHashes.Contains(i.Id.VideoId) && i.Id.Kind == VideoKind)
                        .Select(i => GetVideoInfo(i.Id.VideoId))
                        .Select(v => new VideoInfo
                        {
                            Hash = v.Id,
                            Title = v.Snippet.Title,
                            ThumbnailUrl = GetThumbnailUrl(v.Snippet.Thumbnails),
                            Live = true,
                            Duration = GetDurationFromVideoInfo(v)
                        });

                    playListItems.AddRange(currentPlayListItems);
                    nextPageToken = result.NextPageToken;
                }
                return playListItems;
            }
            catch (Exception exception)
            {
                const string error = "Your search list request could not been served!";
                logger.LogError(error, exception);
                throw;
            }
        }

        public void UpdateLists(IEnumerable<PlayList> playLists)
        {
            foreach (var playList in playLists)
            {
                UpdateList(playList);

                repositoryStore.PlayListRepository.InsertUpdate(playList);
                repositoryStore.SaveChanges();
            }
        }

        private Video GetVideoInfo(string hash)
        {
            var request = YouTubeService.Videos.List("contentDetails, snippet, status");
            request.Id = hash;

            try
            {
                var taskResponse = request.ExecuteAsync(CancellationToken.None);
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

        private YouTubePlayList GetPlayList(string hash)
        {
            var request = YouTubeService.Playlists.List("snippet, contentDetails, status");
            request.MaxResults = MaxResults;
            request.Id = hash;
            try
            {
                var taskResponse = request.ExecuteAsync(CancellationToken.None);
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

        private void UpdateList(PlayList playList)
        {
            var youTubePlaylist = GetPlayList(playList.Hash);
            if (youTubePlaylist == null) return;

            youTubePlaylist.Snippet.Title = playList.Title;
            youTubePlaylist.Status.PrivacyStatus = Enum.GetName(typeof (PrivacyStatus), playList.PrivacyStatus).ToLower();

            try
            {
                var request = youTubeService.Playlists.Update(youTubePlaylist, "snippet, status, contentDetails");
                request.ExecuteAsync();
            }
            catch (Exception e)
            {
                const string error = "Your update playlist request could not been served!";
                logger.LogError(error, e);
                throw;
            }
        }

        private static string GetThumbnailUrl(ThumbnailDetails thumbnailDetails)
        {
            if (thumbnailDetails == null)
                return string.Empty;

            return (thumbnailDetails.Standard != null) ? thumbnailDetails.Standard.Url : thumbnailDetails.Default__.Url;
        }

        private int GetDuration(PlaylistItemContentDetails contentDetails)
        {
            var videoInfo = GetVideoInfo(contentDetails.VideoId);
            if (videoInfo == null) return 0;

            return GetDurationFromVideoInfo(videoInfo);
        }

        private static int GetDurationFromVideoInfo(Video videoInfo)
        {
            var duration = videoInfo.ContentDetails.Duration;

            if (NamedDurationRegex.IsMatch(duration))
            {
                var match = NamedDurationRegex.Match(duration);
                var minutes = int.Parse(match.Groups[MinutesGroupName].Value);
                var seconds = int.Parse(match.Groups[SecondsGroupName].Value);
                return (int) new TimeSpan(0, minutes, seconds).TotalSeconds;
            }

            return 0;
        }


        private void PopulatePlayListItems(List<PlayListItem> playListItems,
            PlaylistItemListResponse playlistItemListResponse, bool withVideoInfo)
        {
            var uniqueTrackHashes = playListItems.Select(t => t.Hash).Distinct();
            var currentTracks = playlistItemListResponse.Items
                .Where(
                    i =>
                        !uniqueTrackHashes.Contains(i.ContentDetails.VideoId) && i.Snippet.Title != DeleteVideoTitle &&
                        i.Snippet.Title != PrivateVideoTitle && i.Snippet.Description != UnavailableVideoDescription &&
                        i.Snippet.Description != PrivateVideoDescription)
                .Select(playListItem => new PlayListItem
                {
                    Hash = playListItem.Id,
                    Position = playListItem.Snippet.Position,
                    VideoInfo = new VideoInfo
                    {
                        Hash = playListItem.ContentDetails.VideoId,
                        Title = playListItem.Snippet.Title,
                        Duration = withVideoInfo ? GetDuration(playListItem.ContentDetails) : 0,
                        ThumbnailUrl = GetThumbnailUrl(playListItem.Snippet.Thumbnails),
                        Live = true
                    }
                })
                .OrderBy(v => v.Position);
            playListItems.AddRange(currentTracks);
        }

        private void PopulatePlayList(List<PlayList> playLists, PlaylistListResponse result,
            bool withPlayListItems = false)
        {
            var playListIds = playLists.Select(pl => pl.Hash).Distinct();
            var currentPlayLists = result.Items
                .Where(i => !playListIds.Contains(i.Id))
                .Select(playList => new PlayList
                {
                    Hash = playList.Id,
                    Title = playList.Snippet.Title,
                    PlayListItems =
                        withPlayListItems ? GetPlayListItems(playList.Id).ToList() : new List<PlayListItem>()
                });
            playLists.AddRange(currentPlayLists);
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