using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouTubeListAPI.Business.Extensions;
using YouTubeListAPI.Business.Service.Response;
using YouTubeListAPI.Business.Service.Wrapper;
using YouTubeListManager.Data.Domain;
using YouTubeListManager.Data.Repository;

using YouTubePlayList = Google.Apis.YouTube.v3.Data.Playlist;

namespace YouTubeListAPI.Business.Service
{
    public class YouTubeListManagerManagerService : IYouTubeListManagerService
    {
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

        public ServiceResponse<List<PlayListItem>> GetPlayListItems(string requestToken, string playListId)
        {
            Task<PlaylistItemListResponse> taskResponse = playlistItemResponseService.GetResponse(requestToken, playListId);

            var uniqueTrackHashes = youTubeListManagerCache.Get<PlayList>(playListId).Select(t => t.Hash).Distinct();
            var currentPlayListItems = taskResponse.Result.Items
                .Where(i => !uniqueTrackHashes.Contains(i.ContentDetails.VideoId))
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
                        Live = IsPlaylistItemStillValid(pli.Snippet)
                    }
                }).OrderBy(v => v.Position);

            var syncronizedList = SynchronizeItems(currentPlayListItems);
            var serviceResponse = new ServiceResponse<List<PlayListItem>>(taskResponse.Result.NextPageToken, syncronizedList);

            youTubeListManagerCache.Add(playListId, syncronizedList);
            return serviceResponse;
        }

        private List<PlayListItem> SynchronizeItems(IEnumerable<PlayListItem> playListItems)
        {
            var unavailableVideoHashList = playListItems.Where(i => !i.VideoInfo.Live).Select(i => i.VideoInfo.Hash);

            if (!unavailableVideoHashList.Any())
                return playListItems.ToList();

            var foundVideoList = repositoryStore.VideoInfoRepository.FindBy(v => unavailableVideoHashList.Contains(v.Hash));
            if (!foundVideoList.Any())
                return GetCleanPlaylist(playListItems);

            Dictionary<string, PlayListItem> playListDictionary = playListItems.Select(i => new KeyValuePair<string, PlayListItem>(i.VideoInfo.Hash, i))
                .ToDictionary(i => i.Key, i => i.Value);

            foreach (VideoInfo video in foundVideoList)
                playListDictionary[video.Hash].VideoInfo.Title = video.Title;

            return playListDictionary.Select(d => d.Value).ToList();
        }

        private List<PlayListItem> GetCleanPlaylist(IEnumerable<PlayListItem> playListItems)
        {
            int indexReduction = 0;
            var cleanedPlayListItems = new List<PlayListItem>();
            foreach (var playListItem in playListItems)
            {
                if (!playListItem.VideoInfo.Live) { 
                    indexReduction++;
                    youTubeApiUpdateServiceWrapper.DeletePlaylistItem(playListItem.Hash);
                }
                else
                {
                    playListItem.Position -= indexReduction;
                    cleanedPlayListItems.Add(playListItem);
                }
            }
            return cleanedPlayListItems;
        }

        private static bool IsPlaylistItemStillValid(PlaylistItemSnippet snippet)
        {
            return snippet.Title != VideoInfo.DeleteVideoTitle
                && snippet.Title != VideoInfo.PrivateVideoTitle
                && snippet.Description != VideoInfo.UnavailableVideoDescription
                && snippet.Description != VideoInfo.PrivateVideoDescription;
        }

        public PlayList GetPlayList(string playListId)
        {
            return GetPlayLists(string.Empty, true, playListId).Response.FirstOrDefault();
        }

        public ServiceResponse<List<PlayList>> GetPlaylists(string requestToken)
        {
            return GetPlayLists(requestToken, false, string.Empty);
        }

        private ServiceResponse<List<PlayList>> GetPlayLists(string requestToken, bool withPlayListItems, string playListId)
        {
            Task<PlaylistListResponse> taskResponse = playlistResponseService.GetResponse(requestToken, playListId);

            var playListIds = youTubeListManagerCache.GetPlayLists(null).Select(pl => pl.Hash).Distinct();
            var currentPlayLists = taskResponse.Result.Items
                .Where(i => !playListIds.Contains(i.Id))
                .Select(playList => CreatePlayList(playList, withPlayListItems)).ToList();
            youTubeListManagerCache.AddPlayLists(currentPlayLists);

            currentPlayLists = GetCachedIfNotAny(withPlayListItems, playListId, currentPlayLists);
            
            return new ServiceResponse<List<PlayList>>(taskResponse.Result.NextPageToken, currentPlayLists);
        }

        private List<PlayList> GetCachedIfNotAny(bool withPlayListItems, string playListId, List<PlayList> currentPlayLists)
        {
            if (!currentPlayLists.Any())
                if (withPlayListItems && !string.IsNullOrEmpty(playListId))
                {
                    currentPlayLists = youTubeListManagerCache.GetPlayLists(p => p.Hash == playListId);
                    currentPlayLists.ForEach(PopulatePlayListWithPlayListItems);
                }
                else
                    currentPlayLists = youTubeListManagerCache.GetPlayLists(null);

            return currentPlayLists;
        }

        private PlayList CreatePlayList(YouTubePlayList youTubePlayList, bool withPlayListItems)
        {
            var playList = new PlayList
            {
                Hash = youTubePlayList.Id,
                Title = youTubePlayList.Snippet.Title,
                ItemCount = youTubePlayList.ContentDetails.ItemCount,
                ThumbnailUrl = youTubePlayList.Snippet.Thumbnails.GetThumbnailUrl(),
                PrivacyStatus = (PrivacyStatus)Enum.Parse(typeof(PrivacyStatus), youTubePlayList.Status.PrivacyStatus, true)
            };

            if (withPlayListItems)
                PopulatePlayListWithPlayListItems(playList);

            return playList;
        }

        private void PopulatePlayListWithPlayListItems(PlayList playList)
        {
            ServiceResponse<List<PlayListItem>> playListItemResponse = GetPlayListItems(string.Empty, playList.Hash);
            playList.PlayListItemsNextPageToken = playListItemResponse.NextPageToken;
            playList.PlayListItems = playListItemResponse.Response;
        }

        public ServiceResponse<List<VideoInfo>> ShowSuggestions(string requestToken, string title, SearchResource.ListRequest.VideoDurationEnum videoDuration = default(SearchResource.ListRequest.VideoDurationEnum))
        {
            Task<SearchListResponse> taskResponse = searchListResponseService.GetResponse(requestToken, title, videoDuration);

            var uniqueHashes = youTubeListManagerCache.Get<VideoInfo>(title).Select(t => t.Hash).Distinct();
            var currentVideoItems = taskResponse.Result.Items
                .Where(i => !uniqueHashes.Contains(i.Id.VideoId) && i.Id.Kind == VideoInfo.VideoKind)
                .Select(i => youTubeApiListServiceWrapper.GetVideo(i.Id.VideoId))
                .Select(v => new VideoInfo
                {
                    Hash = v.Id,
                    Title = v.Snippet.Title,
                    ThumbnailUrl = v.Snippet.Thumbnails.GetThumbnailUrl(),
                    Live = true,
                    Duration = v.GetDurationFromVideoInfo()
                }).ToList();

            youTubeListManagerCache.Add(title, currentVideoItems);
            var serviceResponse = new ServiceResponse<List<VideoInfo>>(taskResponse.Result.NextPageToken, currentVideoItems);

            return serviceResponse;
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
            foundPlayList.ThumbnailUrl = playList.ThumbnailUrl;
            foundPlayList.UserId = playList.UserId;
            repositoryStore.SaveChanges();

            youTubeApiUpdateServiceWrapper.UpdatePlaylistItems(foundPlayList, playList.PlayListItems);
        }

        private void PlaylistItemUpdated(object sender, UpdatePlayListItemEventArgs eventArgs)
        {
            InsertUpdatePlayListItem(eventArgs.PlayList, eventArgs.PlayListItem);

            InsertUpdateVideoInfo(eventArgs.PlayListItem.VideoInfo);
        }

        private void InsertUpdatePlayListItem(PlayList playList, PlayListItem playListItem)
        {
            InsertUpdateVideoInfo(playListItem.VideoInfo);

            var foundPlayListItem = repositoryStore.PlayListItemRepository.FindBy(pli => pli.Hash == playListItem.Hash).FirstOrDefault();
            if (foundPlayListItem == null)
            {
                foundPlayListItem = repositoryStore.PlayListItemRepository.Create();
                repositoryStore.PlayListItemRepository.Insert(foundPlayListItem);
            }

            foundPlayListItem.Hash = playListItem.Hash;
            foundPlayListItem.Position = playListItem.Position;

            foundPlayListItem.VideoInfo = repositoryStore.VideoInfoRepository.FindBy(vi => vi.Hash == playListItem.VideoInfo.Hash).First();
            foundPlayListItem.VideoInfoId = foundPlayListItem.VideoInfo.Id;

            foundPlayListItem.PlayList = repositoryStore.PlayListRepository.FindBy(pl => pl.Hash == playList.Hash).First();
            foundPlayListItem.PlayListId = foundPlayListItem.PlayList.Id;

            repositoryStore.SaveChanges();
        }

        private void InsertUpdateVideoInfo(VideoInfo videoInfo)
        {
            var foundVideoInfo = repositoryStore.VideoInfoRepository.FindBy(vi => vi.Hash == videoInfo.Hash).FirstOrDefault();

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