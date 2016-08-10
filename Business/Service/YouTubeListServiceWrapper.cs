using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouTubeListAPI.Business.Extensions;
using YouTubeListManager.Data.Domain;
using YouTubeListManager.Logger;

namespace YouTubeListAPI.Business.Service
{
    public class YouTubeListService : YouTubeApiService, IYouTubeListService
    {
        private const string UnavailableVideoDescription = VideoInfo.UnavailableVideoDescription;
        private const string PrivateVideoDescription = VideoInfo.PrivateVideoDescription;
        private const string DeleteVideoTitle = VideoInfo.DeleteVideoTitle;
        private const string PrivateVideoTitle = VideoInfo.PrivateVideoTitle;
        private const string VideoKind = VideoInfo.VideoKind;
        private const int MaxResults = 50;

        public YouTubeListService(INlogLogger logger, IYouTubeServiceProvider youTubeServiceProvider) : base(logger, youTubeServiceProvider)
        {
        }


        public IEnumerable<PlayListItem> GetPlayListItems(string playListId)
        {
            if (string.IsNullOrEmpty(playListId))
                throw new Exception("PlayListId can not be empty");

            var request = youTubeService.PlaylistItems.List("snippet, contentDetails");
            request.PlaylistId = playListId;
            request.MaxResults = MaxResults;
            try
            {
                var tracks = new List<PlayListItem>();
                var nextPageToken = string.Empty;
                while (nextPageToken != null)
                {
                    Task<PlaylistItemListResponse> taskResponse = request.ExecuteAsync();
                    var playlistItemListResponse = taskResponse.Result;
                    request.PageToken = nextPageToken;

                    PopulatePlayListItems(tracks, playlistItemListResponse, GetVideo);

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

        public PlayList GetPlayList(string playListId)
        {
            return GetPlayLists(playListId, true).FirstOrDefault();
        }

        public IEnumerable<PlayList> GetPlaylists()
        {
            return GetPlayLists(string.Empty);
        }

        public IEnumerable<VideoInfo> ShowSuggestions(string title)
        {
            var request = youTubeService.Search.List("id, snippet");
            request.MaxResults = MaxResults;
            request.Q = title.CleanTitle();

            try
            {
                var videoList = new List<VideoInfo>();
                var nextPageToken = string.Empty;
                while (nextPageToken != null)
                {
                    var taskResponse = request.ExecuteAsync();
                    var result = taskResponse.Result;
                    request.PageToken = nextPageToken;

                    PopulateVideoList(videoList, result, GetVideo);
                    nextPageToken = result.NextPageToken;
                }
                return videoList;
            }
            catch (Exception exception)
            {
                const string error = "Your search list request could not been served!";
                logger.LogError(error, exception);
                throw;
            }
        }

        private IEnumerable<PlayList> GetPlayLists(string playListId, bool withPlayListItems = false)
        {
            PlaylistsResource.ListRequest request = youTubeService.Playlists.List("snippet");
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
                    Task<PlaylistListResponse> taskResponse = request.ExecuteAsync();
                    var result = taskResponse.Result;
                    request.PageToken = nextPageToken;

                    PopulatePlayList(playLists, result, GetPlayListItems, withPlayListItems);

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


        private delegate Video GetVideoDelegate(string hash);

        private delegate IEnumerable<PlayListItem> GetPlayListItemsDelegate(string hash);


        private static void PopulateVideoList(List<VideoInfo> videoList, SearchListResponse result, GetVideoDelegate getVideoDelegate)
        {
            var uniqueHashes = videoList.Select(t => t.Hash).Distinct();
            var currentPlayListItems = result.Items
                .Where(i => !uniqueHashes.Contains(i.Id.VideoId) && i.Id.Kind == VideoKind)
                .Select(i => getVideoDelegate(i.Id.VideoId))
                .Select(v => new VideoInfo
                {
                    Hash = v.Id,
                    Title = v.Snippet.Title,
                    ThumbnailUrl = v.Snippet.Thumbnails.GetThumbnailUrl(),
                    Live = true,
                    Duration = v.GetDurationFromVideoInfo()
                });

            videoList.AddRange(currentPlayListItems);
        }

        private static int GetDuration(PlaylistItemContentDetails contentDetails, GetVideoDelegate getVideoDelegate)
        {
            var videoInfo = getVideoDelegate(contentDetails.VideoId);
            if (videoInfo == null) return 0;

            return videoInfo.GetDurationFromVideoInfo();
        }

        private static void PopulatePlayListItems(List<PlayListItem> playListItems, PlaylistItemListResponse playlistItemListResponse, GetVideoDelegate getVideoDelegate)
        {
            var uniqueTrackHashes = playListItems.Select(t => t.Hash).Distinct();
            var currentTracks = playlistItemListResponse.Items
                .Where(
                    i =>
                        !uniqueTrackHashes.Contains(i.ContentDetails.VideoId) && i.Snippet.Title != DeleteVideoTitle &&
                        i.Snippet.Title != PrivateVideoTitle && i.Snippet.Description != UnavailableVideoDescription &&
                        i.Snippet.Description != PrivateVideoDescription)
                .Select(pli => new PlayListItem
                {
                    Hash = pli.Id,
                    Position = pli.Snippet.Position,
                    VideoInfo = new VideoInfo
                    {
                        Hash = pli.ContentDetails.VideoId,
                        Title = pli.Snippet.Title,
                        Duration = GetDuration(pli.ContentDetails, getVideoDelegate),
                        ThumbnailUrl = pli.Snippet.Thumbnails.GetThumbnailUrl(),
                        Live = true
                    }
                })
                .OrderBy(v => v.Position);
            playListItems.AddRange(currentTracks);
        }

        private static void PopulatePlayList(List<PlayList> playLists, PlaylistListResponse result, GetPlayListItemsDelegate getPlayListItemsDelegate,
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
                        withPlayListItems ? getPlayListItemsDelegate(playList.Id).ToList() : new List<PlayListItem>()
                });
            playLists.AddRange(currentPlayLists);
        }
    }
}