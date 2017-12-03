using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
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

using Playlist = YouTubeListManager.CrossCutting.Domain.Playlist;
using PlaylistItem = YouTubeListManager.CrossCutting.Domain.PlaylistItem;

using YouTubePlaylist = Google.Apis.YouTube.v3.Data.Playlist;
using YouTubePlaylistItem = Google.Apis.YouTube.v3.Data.PlaylistItem;

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

        public async Task<ServiceResponse<List<PlaylistItem>>> GetPlaylistItemsAsync(string requestToken, string playlistId)
        {
            PlaylistItemListResponse playlistItemListResponse = await playlistItemResponseService.GetResponse(requestToken, playlistId);

            return GetPlaylistItemsServiceResponse(playlistId, playlistItemListResponse);
        }

        public ServiceResponse<List<PlaylistItem>> GetPlaylistItems(string requestToken, string playListId)
        {
            Task<PlaylistItemListResponse> taskResponse = playlistItemResponseService.GetResponse(requestToken, playListId);

            return GetPlaylistItemsServiceResponse(playListId, taskResponse.Result);
        }

        private ServiceResponse<List<PlaylistItem>> GetPlaylistItemsServiceResponse(string playListId, PlaylistItemListResponse playlistItemListResponse)
        {
            var uniqueTrackHashes = youTubeListManagerCache.Get<Playlist>(playListId).Select(t => t.Hash).Distinct();
            var currentPlayListItems = playlistItemListResponse.Items
                .Where(i => !uniqueTrackHashes.Contains(i.ContentDetails.VideoId))
                .Select(CreatePlayListItem)
                .OrderBy(i => i.Position);

            var syncronizedList = SynchronizeItems(currentPlayListItems);
            var serviceResponse = new ServiceResponse<List<PlaylistItem>>(playlistItemListResponse.NextPageToken, syncronizedList);

            youTubeListManagerCache.Add(playListId, syncronizedList);
            return serviceResponse;
        }

        private PlaylistItem CreatePlayListItem(YouTubePlaylistItem playlistItem)
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

            Dictionary<string, PlaylistItem> playListDictionary = playListItems.Select(i => new KeyValuePair<string, PlaylistItem>(i.VideoInfo.Hash, i))
                .ToDictionary(i => i.Key, i => i.Value);

            foreach (VideoInfo video in foundVideoList)
                playListDictionary[video.Hash].VideoInfo.Title = video.Title;

            return playListDictionary.Select(d => d.Value).ToList();
        }

        private List<PlaylistItem> GetCleanPlaylist(IEnumerable<PlaylistItem> playListItems)
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

        public async Task<Playlist> GetPlaylistAsync(string playlistId)
        {
            return (await GetPlaylistsAsync(string.Empty, true, playlistId)).Response.FirstOrDefault();
        }


        public Playlist GetPlaylist(string playlistId)
        {
            return GetPlayLists(string.Empty, true, playlistId).Response.FirstOrDefault();
        }

        public async Task<ServiceResponse<List<Playlist>>> GetPlaylistsAsync(string requestToken)
        {
            return await GetPlaylistsAsync(requestToken, false);
        }

        public ServiceResponse<List<Playlist>> GetPlaylists(string requestToken)
        {
            return GetPlayLists(requestToken, false, string.Empty);
        }

        public ServiceResponse<List<VideoInfo>> SearchSuggestions(SearchRequest searchRequest)
        {
            Task<SearchListResponse> taskResponse = searchListResponseService.GetResponseAsync(searchRequest);

            var serviceResponse = GetSearchServiceResponse(searchRequest, taskResponse.Result);
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<VideoInfo>>> SearchSuggestionsAsync(SearchRequest searchRequest)
        {
            SearchListResponse taskResponse = await searchListResponseService.GetResponseAsync(searchRequest);

            var serviceResponse = GetSearchServiceResponse(searchRequest, taskResponse);
            return serviceResponse;
        }

        private async Task<ServiceResponse<List<Playlist>>> GetPlaylistsAsync(string requestToken, bool withPlayListItems, string playListId = null)
        {
            PlaylistListResponse taskResponse = await playlistResponseService.GetResponse(requestToken, playListId);

            return GetPlaylistServiceResponse(withPlayListItems, playListId, taskResponse);
        }

        private ServiceResponse<List<Playlist>> GetPlayLists(string requestToken, bool withPlayListItems, string playListId)
        {
            Task<PlaylistListResponse> taskResponse = playlistResponseService.GetResponse(requestToken, playListId);

            return GetPlaylistServiceResponse(withPlayListItems, playListId, taskResponse.Result);
        }

        private ServiceResponse<List<Playlist>> GetPlaylistServiceResponse(bool withPlayListItems, string playListId, PlaylistListResponse playlistListResponse)
        {
            var playListIds = youTubeListManagerCache.GetPlaylists(null).Select(pl => pl.Hash).Distinct();
            var currentPlayLists = playlistListResponse.Items
                .Where(i => !playListIds.Contains(i.Id))
                .Select(playList => CreatePlaylist(playList, withPlayListItems)).ToList();
            youTubeListManagerCache.AddPlayLists(currentPlayLists);

            currentPlayLists = GetCachedIfNotAny(withPlayListItems, playListId, currentPlayLists);

            return new ServiceResponse<List<Playlist>>(playlistListResponse.NextPageToken, currentPlayLists);
        }

        private List<Playlist> GetCachedIfNotAny(bool withPlayListItems, string playListId, List<Playlist> currentPlayLists)
        {
            if (!currentPlayLists.Any())
                if (withPlayListItems && !string.IsNullOrEmpty(playListId))
                {
                    currentPlayLists = youTubeListManagerCache.GetPlaylists(p => p.Hash == playListId);
                    currentPlayLists.ForEach(PopulatePlayListWithPlaylistItems);
                }
                else
                    currentPlayLists = youTubeListManagerCache.GetPlaylists(null);

            return currentPlayLists;
        }

        private Playlist CreatePlaylist(YouTubePlaylist youTubePlayList, bool withPlayListItems)
        {
            var playList = new Playlist
            {
                Hash = youTubePlayList.Id,
                Title = youTubePlayList.Snippet.Title,
                ItemCount = youTubePlayList.ContentDetails.ItemCount,
                ThumbnailUrl = youTubePlayList.Snippet.Thumbnails.GetThumbnailUrl(),
                PrivacyStatus = (PrivacyStatus)Enum.Parse(typeof(PrivacyStatus), youTubePlayList.Status.PrivacyStatus, true)
            };

            if (withPlayListItems)
                PopulatePlayListWithPlaylistItems(playList);

            return playList;
        }

        private void PopulatePlayListWithPlaylistItems(Playlist playlist)
        {
            ServiceResponse<List<PlaylistItem>> playlistItemResponse = GetPlaylistItems(string.Empty, playlist.Hash);
            playlist.PlaylistItemsNextPageToken = playlistItemResponse.NextPageToken;
            playlist.PlaylistItems = playlistItemResponse.Response;
        }

        

        private ServiceResponse<List<VideoInfo>> GetSearchServiceResponse(SearchRequest searchRequest, SearchListResponse searchListResponse)
        {
            var uniqueHashes = youTubeListManagerCache.Get<VideoInfo>(searchRequest.SearchKey).Select(t => t.Hash).Distinct();
            if (searchRequest.UsedVideoIdList.Count > 0)
                uniqueHashes = uniqueHashes.Union(searchRequest.UsedVideoIdList);

            var currentVideoItems = searchListResponse.Items
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

            youTubeListManagerCache.Add(searchRequest.SearchKey, currentVideoItems);
            var serviceResponse = new ServiceResponse<List<VideoInfo>>(searchListResponse.NextPageToken, currentVideoItems);
            return serviceResponse;
        }

        public ServiceResponse<List<PlaylistItem>> SynchronizePlaylistItems(string requestToken, string playlistId)
        {
            throw new NotImplementedException();
        }

        public void UpdatePlaylists(IEnumerable<Playlist> playLists)
        {
            youTubeApiUpdateServiceWrapper.UpdatePlaylists(playLists);
        }

        private void PlaylistUpdated(object sender, UpdatePlaylistEventArgs eventArgs)
        {
            InsertUpdatePlayList(eventArgs.Playlist);
        }

        private void InsertUpdatePlayList(Playlist playlist)
        {
            Playlist foundPlaylist = repositoryStore.PlaylistRepository.FindBy(p => p.Hash == playlist.Hash).FirstOrDefault();
            if (foundPlaylist == null)
            {
                foundPlaylist = repositoryStore.PlaylistRepository.Create();
                repositoryStore.PlaylistRepository.Insert(foundPlaylist);
            }

            foundPlaylist.Hash = playlist.Hash;
            foundPlaylist.Title = playlist.Title;
            foundPlaylist.PrivacyStatus = playlist.PrivacyStatus;
            foundPlaylist.ThumbnailUrl = playlist.ThumbnailUrl;
            foundPlaylist.UserId = playlist.UserId;
            repositoryStore.SaveChangesAsync();

            youTubeApiUpdateServiceWrapper.UpdatePlaylistItems(foundPlaylist, playlist.PlaylistItems);
        }

        private void PlaylistItemUpdated(object sender, UpdatePlaylistItemEventArgs eventArgs)
        {
            InsertUpdatePlayListItem(eventArgs.Playlist, eventArgs.PlaylistItem);

            InsertUpdateVideoInfo(eventArgs.PlaylistItem.VideoInfo);
        }

        private void InsertUpdatePlayListItem(Playlist playlist, PlaylistItem playlistItem)
        {
            InsertUpdateVideoInfo(playlistItem.VideoInfo);

            var foundPlayListItem = repositoryStore.PlaylistItemRepository.FindBy(pli => pli.Hash == playlistItem.Hash).FirstOrDefault();
            if (foundPlayListItem == null)
            {
                foundPlayListItem = repositoryStore.PlaylistItemRepository.Create();
                repositoryStore.PlaylistItemRepository.Insert(foundPlayListItem);
            }

            foundPlayListItem.Hash = playlistItem.Hash;
            foundPlayListItem.Position = playlistItem.Position;

            foundPlayListItem.VideoInfo = repositoryStore.VideoInfoRepository.FindBy(vi => vi.Hash == playlistItem.VideoInfo.Hash).First();
            foundPlayListItem.VideoInfoId = foundPlayListItem.VideoInfo.Id;

            foundPlayListItem.Playlist = repositoryStore.PlaylistRepository.FindBy(pl => pl.Hash == playlist.Hash).First();
            foundPlayListItem.PlaylistId = foundPlayListItem.Playlist.Id;

            repositoryStore.SaveChangesAsync();
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

            repositoryStore.SaveChangesAsync();
        }
    }
}