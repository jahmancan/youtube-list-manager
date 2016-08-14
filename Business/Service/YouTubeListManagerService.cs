using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;
using YouTubeListAPI.Business.Extensions;
using YouTubeListAPI.Business.Service.Response;
using YouTubeListAPI.Business.Service.Wrapper;
using YouTubeListManager.Data.Domain;

namespace YouTubeListAPI.Business.Service
{
    public class YouTubeListManagerService : IYouTubeListService
    {
        private const string UnavailableVideoDescription = VideoInfo.UnavailableVideoDescription;
        private const string PrivateVideoDescription = VideoInfo.PrivateVideoDescription;
        private const string DeleteVideoTitle = VideoInfo.DeleteVideoTitle;
        private const string PrivateVideoTitle = VideoInfo.PrivateVideoTitle;
        private const string VideoKind = VideoInfo.VideoKind;

        private IYouTubeApiListServiceWrapper youTubeApiListServiceWrapper;
        private IPlaylistResponseService playlistResponseService;
        private IPlaylistItemResponseService playlistItemResponseService;
        private ISearchListResponseService searchListResponseService;
        private IYouTubeListManagerCache youTubeListManagerCache;

        public YouTubeListManagerService(
            IYouTubeListManagerCache youTubeListManagerCache,
            IPlaylistResponseService playlistResponseService,
            IPlaylistItemResponseService playlistItemResponseService,
            ISearchListResponseService searchListResponseService,
            IYouTubeApiListServiceWrapper youTubeApiListServiceWrapper
            )
        {
            this.youTubeListManagerCache = youTubeListManagerCache;
            this.playlistResponseService = playlistResponseService;
            this.playlistItemResponseService = playlistItemResponseService;
            this.searchListResponseService = searchListResponseService;
            this.youTubeApiListServiceWrapper = youTubeApiListServiceWrapper;
        }

        public IEnumerable<PlayListItem> GetPlayListItems(string requestToken, string playListId)
        {

            Task<PlaylistItemListResponse> taskResponse = playlistItemResponseService.GetResponse(requestToken, playListId);

            var uniqueTrackHashes = youTubeListManagerCache.GetPlayListItems(playListId).Select(t => t.Hash).Distinct();
            var currentSearchItems = taskResponse.Result.Items
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
                        Duration = GetDuration(pli.ContentDetails),
                        ThumbnailUrl = pli.Snippet.Thumbnails.GetThumbnailUrl(),
                        Live = true
                    }
                })
                .OrderBy(v => v.Position);

            youTubeListManagerCache.PlayListItems[playListId].AddRange(currentSearchItems);

            return currentSearchItems;
        }

        public PlayList GetPlayList(string playListId)
        {
            return GetPlayLists(string.Empty, true, playListId).FirstOrDefault();
        }

        public IEnumerable<PlayList> GetPlaylists(string requestToken)
        {
            //todo: return complex obect... PlayList with token
            return GetPlayLists(requestToken, false, string.Empty);
        }

        private IEnumerable<PlayList> GetPlayLists(string requestToken, bool withPlayListItems, string playListId)
        {
            Task<PlaylistListResponse> taskResponse = playlistResponseService.GetResponse(requestToken, playListId);

            var playListIds = youTubeListManagerCache.PlayLists.Select(pl => pl.Hash).Distinct();
            var currentPlayLists = taskResponse.Result.Items
                .Where(i => !playListIds.Contains(i.Id))
                .Select(playList => new PlayList
                {
                    Hash = playList.Id,
                    Title = playList.Snippet.Title,
                    PlayListItems = new List<PlayListItem>()
                    //todo: refactor this...
                    //withPlayListItems ? GetPlayListItems(string.Empty, playList.Id).ToList() : new List<PlayListItem>()
                });
            youTubeListManagerCache.PlayLists.AddRange(currentPlayLists);

            return currentPlayLists;
        }

        public IEnumerable<VideoInfo> ShowSuggestions(string requestToken, string title)
        {
            youTubeApiListServiceWrapper.ExecuteAsyncRequestSearch(requestToken, title);
            Task<SearchListResponse> taskResponse = searchListResponseService.GetResponse(requestToken, title);

            var uniqueHashes = youTubeListManagerCache.GetSearchList(title).Select(t => t.Hash).Distinct();
            var currentPlayListItems = taskResponse.Result.Items
                .Where(i => !uniqueHashes.Contains(i.Id.VideoId) && i.Id.Kind == VideoKind)
                .Select(i => youTubeApiListServiceWrapper.GetVideo(i.Id.VideoId))
                .Select(v => new VideoInfo
                {
                    Hash = v.Id,
                    Title = v.Snippet.Title,
                    ThumbnailUrl = v.Snippet.Thumbnails.GetThumbnailUrl(),
                    Live = true,
                    Duration = v.GetDurationFromVideoInfo()
                });

            youTubeListManagerCache.SearchList[title].AddRange(currentPlayListItems);
            return currentPlayListItems;
        }

        private int GetDuration(PlaylistItemContentDetails contentDetails)
        {
            var videoInfo = youTubeApiListServiceWrapper.GetVideo(contentDetails.VideoId);
            if (videoInfo == null) return 0;

            return videoInfo.GetDurationFromVideoInfo();
        }
    }
}
