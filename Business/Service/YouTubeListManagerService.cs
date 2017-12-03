using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;
using YouTubeListManager.BusinessContracts;
using YouTubeListManager.BusinessContracts.Service;
using YouTubeListManager.BusinessContracts.Service.Response;
using YouTubeListManager.BusinessContracts.Service.Wrapper;
using YouTubeListManager.CrossCutting.Domain;
using YouTubeListManager.CrossCutting.EventArgs;
using YouTubeListManager.CrossCutting.Extensions;
using YouTubeListManager.CrossCutting.Request;
using YouTubeListManager.CrossCutting.Response;
using YouTubeListManager.DataContracts.Repository;

using YouTubePlayList = Google.Apis.YouTube.v3.Data.Playlist;
using YouTubePlayListItem = Google.Apis.YouTube.v3.Data.PlaylistItem;

using Playlist = YouTubeListManager.CrossCutting.Domain.Playlist;
using PlaylistItem = YouTubeListManager.CrossCutting.Domain.PlaylistItem;

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

        public async Task<ServiceResponse<List<PlaylistItem>>> GetPlayListItemsAsync(string requestToken, string playlistId)
        {
            return await playlistItemResponseService.GetResponse(requestToken, playlistId).ContinueWith(taskResponse =>
            {
                var uniqueTrackHashes = youTubeListManagerCache.Get<Playlist>(playlistId).Select(t => t.Hash).Distinct();
                var currentPlayListItems = taskResponse.Result.Items
                    .Where(i => !uniqueTrackHashes.Contains(i.ContentDetails.VideoId))
                    .Select(CreatePlayListItem)
                    .OrderBy(i => i.Position);

                var syncronizedList = SynchronizeItems(currentPlayListItems);
                var serviceResponse = new ServiceResponse<List<PlaylistItem>>(taskResponse.Result.NextPageToken, syncronizedList);

                youTubeListManagerCache.Add(playlistId, syncronizedList);
                return serviceResponse;
            });
        }

        public ServiceResponse<List<PlaylistItem>> GetPlayListItems(string requestToken, string playListId)
        {
            Task<PlaylistItemListResponse> taskResponse = playlistItemResponseService.GetResponse(requestToken, playListId);

            var uniqueTrackHashes = youTubeListManagerCache.Get<Playlist>(playListId).Select(t => t.Hash).Distinct();
            var currentPlayListItems = taskResponse.Result.Items
                .Where(i => !uniqueTrackHashes.Contains(i.ContentDetails.VideoId))
                .Select(CreatePlayListItem)
                .OrderBy(i => i.Position);

            var syncronizedList = SynchronizeItems(currentPlayListItems);
            var serviceResponse = new ServiceResponse<List<PlaylistItem>>(taskResponse.Result.NextPageToken, syncronizedList);

            youTubeListManagerCache.Add(playListId, syncronizedList);
            return serviceResponse;
        }

        

        private PlaylistItem CreatePlayListItem(YouTubePlayListItem playlistItem)
        {
            var video = youTubeApiListServiceWrapper.GetVideo(playlistItem.ContentDetails.VideoId);

            return new PlaylistItem
            {
                Hash = playlistItem.Id,
                Position = playlistItem.Snippet.Position,
                VideoInfo = new VideoInfo
                {
                    Hash = playlistItem.ContentDetails.VideoId,
                    Title = playlistItem.Snippet.Title,
                    Duration = video?.GetDurationFromVideoInfo() ?? 0, //same as (video == null) ? 0 : video.GetDurationFromVideoInfo()
                    ThumbnailUrl = playlistItem.Snippet.Thumbnails.GetThumbnailUrl(),
                    Live = IsPlaylistItemStillValid(playlistItem.Snippet, video?.Status.PrivacyStatus ?? "private")
                }
            };
        }

        private List<PlaylistItem> SynchronizeItems(IEnumerable<PlaylistItem> playListItems)
        {
            var unavailableVideoHashList = playListItems.Where(i => !i.VideoInfo.Live).Select(i => i.VideoInfo.Hash);

            if (!unavailableVideoHashList.Any())
                return playListItems.ToList();

            var foundVideoList = repositoryStore.VideoInfoRepository.FindBy(v => unavailableVideoHashList.Contains(v.Hash));
            if (!foundVideoList.Any())
                return GetCleanPlaylist(playListItems);

            Dictionary<string, PlaylistItem> playistDictionary = playListItems.Select(i => new KeyValuePair<string, PlaylistItem>(i.VideoInfo.Hash, i))
                .ToDictionary(i => i.Key, i => i.Value);

            foreach (VideoInfo video in foundVideoList)
                playistDictionary[video.Hash].VideoInfo.Title = video.Title;

            return playistDictionary.Select(d => d.Value).ToList();
        }

        private List<PlaylistItem> GetCleanPlaylist(IEnumerable<YouTubeListManager.CrossCutting.Domain.PlaylistItem> playListItems)
        {
            int indexReduction = 0;
            var cleanedPlayListItems = new List<PlaylistItem>();
            foreach (var playListItem in playListItems)
            {
                if (!playListItem.VideoInfo.Live)
                {
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

        private static bool IsPlaylistItemStillValid(PlaylistItemSnippet snippet, string privacyStatus)
        {
            return snippet.Title != VideoInfo.DeleteVideoTitle
                && snippet.Title != VideoInfo.PrivateVideoTitle
                && snippet.Description != VideoInfo.UnavailableVideoDescription
                && snippet.Description != VideoInfo.PrivateVideoDescription
                && (PrivacyStatus)Enum.Parse(typeof(PrivacyStatus), privacyStatus, true) != PrivacyStatus.Private;
        }

        public async Task<Playlist> GetPlayListAsync(string playlistId)
        {
            return await Task.Run(() => GetPlayLists(string.Empty, true, playlistId).Response.FirstOrDefault());
        }

        public Playlist GetPlayList(string playListId)
        {
            return GetPlayLists(string.Empty, true, playListId).Response.FirstOrDefault();
        }

        public async Task<ServiceResponse<List<Playlist>>> GetPlayListsAsync(string requestToken)
        {
            return null;
            //return await GetPlayLists(requestToken, false, string.Empty);
        }

        public ServiceResponse<List<Playlist>> GetPlayLists(string requestToken)
        {
            return GetPlayLists(requestToken, false, string.Empty);
        }

        public Task<ServiceResponse<List<VideoInfo>>> SearchSuggestionsAsync(SearchRequest searchRequest)
        {
            throw new NotImplementedException();
        }

        private ServiceResponse<List<Playlist>> GetPlayLists(string requestToken, bool withPlayListItems, string playListId)
        {
            Task<PlaylistListResponse> taskResponse = playlistResponseService.GetResponse(requestToken, playListId);

            var playListIds = youTubeListManagerCache.GetPlayLists(null).Select(pl => pl.Hash).Distinct();
            var currentPlayLists = taskResponse.Result.Items
                .Where(i => !playListIds.Contains(i.Id))
                .Select(playList => CreatePlayList(playList, withPlayListItems)).ToList();
            youTubeListManagerCache.AddPlayLists(currentPlayLists);

            currentPlayLists = GetCachedIfNotAny(withPlayListItems, playListId, currentPlayLists);

            return new ServiceResponse<List<Playlist>>(taskResponse.Result.NextPageToken, (List<Playlist>)currentPlayLists);
        }

        private List<Playlist> GetCachedIfNotAny(bool withPlayListItems, string playListId, List<Playlist> currentPlayLists)
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

        private Playlist CreatePlayList(YouTubePlayList youTubePlayList, bool withPlayListItems)
        {
            var playList = new Playlist(youTubePlayList);

            if (withPlayListItems)
                PopulatePlayListWithPlayListItems(playList);

            return playList;
        }

        private void PopulatePlayListWithPlayListItems(Playlist playList)
        {
            ServiceResponse<List<PlaylistItem>> playListItemResponse = GetPlayListItems(string.Empty, playList.Hash);
            playList.PlaylistItemsNextPageToken = playListItemResponse.NextPageToken;
            playList.PlayListItems = playListItemResponse.Response;
        }

        public ServiceResponse<List<VideoInfo>> SearchSuggestions(SearchRequest searchRequest)
        {
            Task<SearchListResponse> taskResponse = searchListResponseService.GetResponseAsync(searchRequest);

            var uniqueHashes = youTubeListManagerCache.Get<VideoInfo>(searchRequest.SearchKey).Select(t => t.Hash).Distinct();
            if (searchRequest.UsedVideoIdList.Count > 0)
                uniqueHashes = uniqueHashes.Union(searchRequest.UsedVideoIdList);

            var currentVideoItems = taskResponse.Result.Items
                .Where(i => !uniqueHashes.Contains(i.Id.VideoId) && i.Id.Kind == VideoInfo.VideoKind)
                .Select(i => youTubeApiListServiceWrapper.GetVideo(i.Id.VideoId))
                .Select(v => new VideoInfo(v)).ToList();

            youTubeListManagerCache.Add(searchRequest.SearchKey, currentVideoItems);
            ServiceResponse<List<VideoInfo>> serviceResponse = new ServiceResponse<List<VideoInfo>>(taskResponse.Result.NextPageToken, currentVideoItems);

            return serviceResponse;
        }

        public ServiceResponse<List<PlaylistItem>> SynchronizePlayListItems(string requestToken, string playlistId)
        {
            throw new NotImplementedException();
        }

        public void UpdatePlayLists(IEnumerable<Playlist> playLists)
        {
            youTubeApiUpdateServiceWrapper.UpdatePlaylists(playLists);
        }

        private void PlaylistUpdated(object sender, UpdatePlayListEventArgs eventArgs)
        {
            InsertUpdatePlayList(eventArgs.PlayList);
        }

        private void InsertUpdatePlayList(Playlist playList)
        {
            Playlist foundPlayList = repositoryStore.PlayListRepository.FindBy(p => p.Hash == playList.Hash).FirstOrDefault();
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

        private void InsertUpdatePlayListItem(Playlist playList, PlaylistItem playListItem)
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

            foundPlayListItem.Playlist = repositoryStore.PlayListRepository.FindBy(pl => pl.Hash == playList.Hash).First();
            foundPlayListItem.PlaylistId = foundPlayListItem.Playlist.Id;

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
            foundVideoInfo.PrivacyStatus = videoInfo.PrivacyStatus;

            repositoryStore.SaveChanges();
        }
    }
}