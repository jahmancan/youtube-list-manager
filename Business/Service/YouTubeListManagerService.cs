using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;
using YouTubeListAPI.Business.Extensions;
using YouTubeListAPI.Business.Service.Response;
using YouTubeListAPI.Business.Service.Wrapper;
using YouTubeListManager.Data.Domain;
using YouTubeListManager.Data.Repository;

namespace YouTubeListAPI.Business.Service
{
    public class YouTubeListManagerManagerService : IYouTubeListManagerService
    {
        private const string UnavailableVideoDescription = VideoInfo.UnavailableVideoDescription;
        private const string PrivateVideoDescription = VideoInfo.PrivateVideoDescription;
        private const string DeleteVideoTitle = VideoInfo.DeleteVideoTitle;
        private const string PrivateVideoTitle = VideoInfo.PrivateVideoTitle;
        private const string VideoKind = VideoInfo.VideoKind;

        private IRepositoryStore repositoryStore;
        private IYouTubeApiListServiceWrapper youTubeApiListServiceWrapper;
        private IYouTubeApiUpdateServiceWrapper youTubeApiUpdateServiceWrapper;
        private IPlaylistResponseService playlistResponseService;
        private IPlaylistItemResponseService playlistItemResponseService;
        private ISearchListResponseService searchListResponseService;
        private IYouTubeListManagerCache youTubeListManagerCache;

        public YouTubeListManagerManagerService(
            IYouTubeListManagerCache youTubeListManagerCache,
            IPlaylistResponseService playlistResponseService,
            IPlaylistItemResponseService playlistItemResponseService,
            ISearchListResponseService searchListResponseService,
            IYouTubeApiListServiceWrapper youTubeApiListServiceWrapper,
            IYouTubeApiUpdateServiceWrapper youTubeApiUpdateServiceWrapper,
            IRepositoryStore repositoryStore
            )
        {
            this.youTubeListManagerCache = youTubeListManagerCache;
            this.playlistResponseService = playlistResponseService;
            this.playlistItemResponseService = playlistItemResponseService;
            this.searchListResponseService = searchListResponseService;
            this.youTubeApiListServiceWrapper = youTubeApiListServiceWrapper;
            this.youTubeApiUpdateServiceWrapper = youTubeApiUpdateServiceWrapper;
            this.repositoryStore = repositoryStore;

            this.youTubeApiUpdateServiceWrapper.PlaylistUpdated += PlaylistUpdated;
            this.youTubeApiUpdateServiceWrapper.PlaylistItemUpdated += PlaylistItemUpdated;
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
                .OrderBy(v => v.Position).ToList();

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
                    PrivacyStatus = (PrivacyStatus)Enum.Parse(typeof(PrivacyStatus), playList.Status.PrivacyStatus, true),
                    PlayListItems = withPlayListItems ? GetPlayListItems(string.Empty, playList.Id).ToList() : new List<PlayListItem>()
                }).ToList();
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
                }).ToList();

            youTubeListManagerCache.SearchList[title].AddRange(currentPlayListItems);
            return currentPlayListItems;
        }

        public void UpdatePlayLists(IEnumerable<PlayList> playLists)
        {
            youTubeApiUpdateServiceWrapper.UpdatePlayLists(playLists);
        }

        private int GetDuration(PlaylistItemContentDetails contentDetails)
        {
            var videoInfo = youTubeApiListServiceWrapper.GetVideo(contentDetails.VideoId);
            if (videoInfo == null) return 0;

            return videoInfo.GetDurationFromVideoInfo();
        }

        private void PlaylistUpdated(object sender, UpdatePlayListEventArgs eventArgs)
        {
            InsertUpdatePlayList(eventArgs.PlayList);
        }

        private void InsertUpdatePlayList(PlayList playList)
        {
            PlayList foundPlayList = repositoryStore.PlayListRepository.FindBy(p => p.Hash == playList.Hash).FirstOrDefault();
            if (foundPlayList == null)
            {
                foundPlayList = repositoryStore.PlayListRepository.Create();
                repositoryStore.PlayListRepository.Insert(foundPlayList);
            }

            foundPlayList.Hash = playList.Hash;
            foundPlayList.Title = playList.Title;
            foundPlayList.PrivacyStatus = playList.PrivacyStatus;
            foundPlayList.UserId = playList.UserId;
            repositoryStore.SaveChanges();

            //todo: check reload
            youTubeApiUpdateServiceWrapper.UpdatePlaylistItems(foundPlayList, playList.PlayListItems);
        }

        private void PlaylistItemUpdated(object sender, UpdatePlayListItemEventArgs eventArgs)
        {
            InsertUpdatePlayListItem(eventArgs.PlayList, eventArgs.PlayListItem);

            InsertUpdateVideoInfo(eventArgs.PlayListItem.VideoInfo);    
        }

        private void InsertUpdatePlayListItem(PlayList playList, PlayListItem playListItem)
        {
            var foundPlayListItem =
                repositoryStore.PlayListItemRepository.FindBy(pli => pli.Hash == playListItem.Hash).FirstOrDefault();
            if (foundPlayListItem == null)
            {
                foundPlayListItem = repositoryStore.PlayListItemRepository.Create();
                repositoryStore.PlayListItemRepository.Insert(foundPlayListItem);
            }

            foundPlayListItem.Hash = playListItem.Hash;
            foundPlayListItem.Position = playListItem.Position;

            foundPlayListItem.VideoInfo =
                repositoryStore.VideoInfoRepository.FindBy(vi => vi.Hash == playListItem.VideoInfo.Hash).First();
            foundPlayListItem.VideoInfoId = foundPlayListItem.VideoInfo.Id;

            foundPlayListItem.PlayList =
                repositoryStore.PlayListRepository.FindBy(pl => pl.Hash == playList.Hash).First();
            foundPlayListItem.PlayListId = foundPlayListItem.PlayList.Id;

            repositoryStore.SaveChanges();
        }

        private void InsertUpdateVideoInfo(VideoInfo videoInfo)
        {
            var foundVideoInfo =
                    repositoryStore.VideoInfoRepository.FindBy(vi => vi.Hash == videoInfo.Hash)
                        .FirstOrDefault();

            if (foundVideoInfo == null)
            {
                foundVideoInfo = repositoryStore.VideoInfoRepository.Create();
                repositoryStore.VideoInfoRepository.Insert(foundVideoInfo);
            }

            foundVideoInfo.Hash = videoInfo.Hash;
            foundVideoInfo.Duration = videoInfo.Duration;
            foundVideoInfo.Live = videoInfo.Live;
            foundVideoInfo.Title = videoInfo.Title;
            foundVideoInfo.ThumbnailUrl = videoInfo.ThumbnailUrl;

            repositoryStore.SaveChanges();
        }
    }
}