using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Google.Apis.YouTube.v3.Data;
using YouTubeListAPI.Business.Extensions;
using YouTubeListManager.Data.Domain;
using YouTubeListManager.Logger;

using YouTubePlayList = Google.Apis.YouTube.v3.Data.Playlist;
using YouTubePlayListItem = Google.Apis.YouTube.v3.Data.PlaylistItem;

namespace YouTubeListAPI.Business.Service
{
    public class YouTubeListService : YouTubeApiService, IYouTubeListService
    {
        private const string UnavailableVideoDescription = "This video is unavailable.";
        private const string PrivateVideoDescription = "This video is private.";
        private const string DeleteVideoTitle = "Deleted video";
        private const string PrivateVideoTitle = "Private video";
        private const string VideoKind = "youtube#video";
        private const int MaxResults = 50;

        public YouTubeListService(INlogLogger logger) : base(logger)
        {
        }


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
            request.Q = title.CleanTitle();

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
                            ThumbnailUrl = v.Snippet.Thumbnails.GetThumbnailUrl(),
                            Live = true,
                            Duration = v.GetDurationFromVideoInfo()
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

        private int GetDuration(PlaylistItemContentDetails contentDetails)
        {
            var videoInfo = GetVideoInfo(contentDetails.VideoId);
            if (videoInfo == null) return 0;

            return videoInfo.GetDurationFromVideoInfo();
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
                        ThumbnailUrl = playListItem.Snippet.Thumbnails.GetThumbnailUrl(),
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
    }
}