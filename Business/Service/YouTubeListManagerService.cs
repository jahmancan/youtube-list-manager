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
using YouTubeListManager.Logger;
using Playlist = YouTubeListManager.CrossCutting.Domain.Playlist;
using PlaylistItem = YouTubeListManager.CrossCutting.Domain.PlaylistItem;

using YouTubePlaylist = Google.Apis.YouTube.v3.Data.Playlist;
using YouTubePlaylistItem = Google.Apis.YouTube.v3.Data.PlaylistItem;

namespace YouTubeListAPI.Business.Service
{
    public class YouTubeListManagerManagerService : IYouTubeListManagerService
    {
        private const string QuotaLimitError = "daily limit exceeded";

        private IRepositoryStore repositoryStore;
        private IYouTubeApiListServiceWrapper youTubeApiListServiceWrapper;
        private IYouTubeApiUpdateServiceWrapper youTubeApiUpdateServiceWrapper;
        private IPlaylistResponseService playlistResponseService;
        private IPlaylistItemResponseService playlistItemResponseService;
        private ISearchListResponseService searchListResponseService;
        private IYouTubeListManagerCache youTubeListManagerCache;
        private INlogLogger logger;

        public YouTubeListManagerManagerService(
            IYouTubeListManagerCache youTubeListManagerCache,
            IPlaylistResponseService playlistResponseService,
            IPlaylistItemResponseService playlistItemResponseService,
            ISearchListResponseService searchListResponseService,
            IYouTubeApiListServiceWrapper youTubeApiListServiceWrapper,
            IYouTubeApiUpdateServiceWrapper youTubeApiUpdateServiceWrapper,
            IRepositoryStore repositoryStore,
            INlogLogger logger
            )
        {
            this.youTubeListManagerCache = youTubeListManagerCache;
            this.playlistResponseService = playlistResponseService;
            this.playlistItemResponseService = playlistItemResponseService;
            this.searchListResponseService = searchListResponseService;
            this.youTubeApiListServiceWrapper = youTubeApiListServiceWrapper;
            this.youTubeApiUpdateServiceWrapper = youTubeApiUpdateServiceWrapper;
            this.repositoryStore = repositoryStore;
            this.logger = logger;

            this.youTubeApiUpdateServiceWrapper.PlaylistUpdated += PlaylistUpdated;
            this.youTubeApiUpdateServiceWrapper.PlaylistItemUpdated += PlaylistItemUpdated;
        }

        public async Task<ServiceResponse<List<PlaylistItem>>> GetPlaylistItemsAsync(string requestToken, string playlistId, bool onlyOfflineUsage = false)
        {
            if (onlyOfflineUsage)
            {
                return GetOfflinePlaylistitems(playlistId);
            }

            try
            {
                PlaylistItemListResponse playlistItemListResponse = await playlistItemResponseService.GetResponse(requestToken, playlistId);

                return await GetPlaylistItemsServiceResponse(playlistId, playlistItemListResponse);
            }
            catch (Exception exception)
            {
                if (!exception.Message.ToLower().Contains(QuotaLimitError))
                    throw;

                return GetOfflinePlaylistitems(playlistId);
            }

            return GetOfflinePlaylistitems(playlistId);
        }

        //public ServiceResponse<List<PlaylistItem>> GetPlaylistItems(string requestToken, string playlistId, bool onlyOfflineUsage = false)
        //{
        //    if (onlyOfflineUsage)
        //    {
        //        return GetOfflinePlaylistitems(playlistId);
        //    }

        //    try
        //    {
        //        Task<PlaylistItemListResponse> taskResponse = playlistItemResponseService.GetResponse(requestToken, playlistId);

        //        return GetPlaylistItemsServiceResponse(playlistId, taskResponse.Result);
        //    }
        //    catch (Exception exception)
        //    {
        //        if (!exception.Message.ToLower().Contains(QuotaLimitError))
        //            throw;

        //        return GetOfflinePlaylistitems(playlistId);
        //    }
        //}

        private ServiceResponse<List<PlaylistItem>> GetOfflinePlaylistitems(string hash)
        {
            try
            {
                var offlinePlaylists = repositoryStore.PlaylistItemRepository.FindBy(x => x.Playlist.Hash.ToLower() == hash.ToLower()).ToList();
                return new ServiceResponse<List<PlaylistItem>>(null, offlinePlaylists.Select(x => new PlaylistItem(x)).ToList());
            }
            catch (Exception exception)
            {
                var error = string.Format("Could not fetch offline playlist items from datastore for hash: {hash}");
                logger.LogError(error, exception);
                return new ServiceResponse<List<PlaylistItem>>(null, new List<PlaylistItem>());
            }
        }

        private async Task<ServiceResponse<List<PlaylistItem>>> GetPlaylistItemsServiceResponse(string playListId, PlaylistItemListResponse playlistItemListResponse)
        {
            var uniqueTrackHashes = youTubeListManagerCache.Get<Playlist>(playListId).Select(t => t.Hash).Distinct();
            var currentPlayListItems = playlistItemListResponse.Items
                .Where(i => !uniqueTrackHashes.Contains(i.ContentDetails.VideoId))
                .Select(CreatePlayListItem)
                .OrderBy(i => i.Position);

            var syncronizedList = await SynchronizeItems(currentPlayListItems);
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

        private async Task<List<PlaylistItem>> SynchronizeItems(IEnumerable<PlaylistItem> playListItems)
        {
            //var playlistItemList = playlistItems.ToList();
            ////var videoHashes = playlistItemList.Select(pl => pl.VideoInfo.Hash)
            ////var currentMaxPosition = playlistItemList.Max(pl => pl.Position);
            //var currentMinPosition = playlistItems.Min(pl => pl.Position);

            //var localPlaylistItems = repositoryStore.PlaylistItemRepository.FindBy(pl => pl.Playlist.Hash == playlistHash && pl.Position <= currentMinPosition + 50 && currentMinPosition >= pl.Position).ToList();

            //var unavailableVideoHashList = playlistItemList.Where(i => !i.VideoInfo.Live).Select(i => i.VideoInfo.Hash);
            //var localVideoHashList = playlistItemList.Select(i => i.VideoInfo.Hash);

            //var removedVideoHashList = localPlaylistItems.Where(i =>
            //    !localVideoHashList.Contains(i.VideoInfo.Hash) || unavailableVideoHashList.Contains(i.VideoInfo.Hash)).Select(i => i.VideoInfo.Hash).ToList();

            //if (!removedVideoHashList.Any())
            //    return playlistItemList;

            //var foundMissingVideoList = repositoryStore.VideoInfoRepository.FindBy(v => removedVideoHashList.Contains(v.Hash));
            //if (!foundMissingVideoList.Any())
            //    return GetCleanPlaylist(playlistItems);
            //else
            //{
            //    foreach (var videoInfo in foundMissingVideoList)
            //    {
            //        videoInfo.Live = false;
            //    }
            //    await repositoryStore.SaveChangesAsync();
            //}

            //Dictionary<string, PlaylistItem> playListDictionary = localPlaylistItems.Select(i => new KeyValuePair<string, PlaylistItem>(i.VideoInfo.Hash, i))
            //    .ToDictionary(i => i.Key, i => i.Value);

            //foreach (VideoInfo video in foundMissingVideoList)
            //    playListDictionary[video.Hash].VideoInfo.Title = video.Title;

            //var synchronizedItems = playListDictionary.Select(d => d.Value).ToList();
            //synchronizedItems.ForEach(pi =>
            //{
            //    pi.Playlist = null;
            //    pi.VideoInfo.PlaylistItems = new List<PlaylistItem>();
            //}
            //);
            //return synchronizedItems;

            var unavailableVideoHashList = playListItems.Where(i => !i.VideoInfo.Live).Select(i => i.VideoInfo.Hash);

            if (!unavailableVideoHashList.Any())
                return playListItems.ToList();

            var foundVideoList = repositoryStore.VideoInfoRepository.FindBy(v => unavailableVideoHashList.Contains(v.Hash));
            if (!foundVideoList.Any())
                return await GetCleanPlaylist(playListItems);

            Dictionary<string, PlaylistItem> playListDictionary = playListItems.Select(i => new KeyValuePair<string, PlaylistItem>(i.VideoInfo.Hash, i))
                .ToDictionary(i => i.Key, i => i.Value);

            foreach (VideoInfo video in foundVideoList)
                playListDictionary[video.Hash].VideoInfo.Title = video.Title;

            return playListDictionary.Select(d => d.Value).ToList();
        }

        private async Task<List<PlaylistItem>> GetCleanPlaylist(IEnumerable<PlaylistItem> playListItems)
        {
            int indexReduction = 0;
            var cleanedPlayListItems = new List<PlaylistItem>();
            foreach (var playListItem in playListItems)
            {
                if (!playListItem.VideoInfo.Live)
                {
                    indexReduction++;
                    await youTubeApiUpdateServiceWrapper.DeletePlaylistItem(playListItem.Hash);
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

        public async Task<Playlist> GetPlaylistAsync(string playlistId, bool onlyOfflineUsage = false, bool withPlaylistItems = true)
        {
            return (await GetPlaylistsAsync(string.Empty, withPlaylistItems, playlistId, onlyOfflineUsage)).Response.FirstOrDefault();
        }

        public async Task<ServiceResponse<List<Playlist>>> GetPlaylistsAsync(string requestToken, bool onlyOfflineUsage = false)
        {
            return await GetPlaylistsAsync(requestToken, false, onlyOfflineUsage: onlyOfflineUsage);
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

        private async Task<ServiceResponse<List<Playlist>>> GetPlaylistsAsync(string requestToken, bool withPlayListItems, string playListId = null, bool onlyOfflineUsage = false)
        {
            if (onlyOfflineUsage)
            {
                return GetOfflinePlaylists();
            }
            try
            {
                PlaylistListResponse taskResponse = await playlistResponseService.GetResponse(requestToken, playListId);

                return await GetPlaylistServiceResponse(withPlayListItems, playListId, taskResponse);
            }
            catch (Exception exception)
            {
                if (!exception.Message.ToLower().Contains(QuotaLimitError))
                    throw;

                return GetOfflinePlaylists();
            }
        }

        private ServiceResponse<List<Playlist>> GetOfflinePlaylists()
        {
            try
            {
                var playlists = repositoryStore.PlaylistRepository.GetAll().ToList().Select(x => new Playlist(x)).ToList();
                return new ServiceResponse<List<Playlist>>(null, playlists);
            }
            catch (Exception exception)
            {
                var error = "Could not fetch offline playlist from datastore";
                logger.LogError(error, exception);
                return new ServiceResponse<List<Playlist>>(null, new List<Playlist>());
            }
        }

        private async Task<ServiceResponse<List<Playlist>>> GetPlaylistServiceResponse(bool withPlayListItems, string playListId, PlaylistListResponse playlistListResponse)
        {
            var playListIds = youTubeListManagerCache.GetPlaylists(null).Select(pl => pl.Hash).Distinct();
            var currentPlayLists = playlistListResponse.Items
                .Where(i => !playListIds.Contains(i.Id))
                .Select(playList => CreatePlaylist(playList, withPlayListItems)).ToList();
            youTubeListManagerCache.AddPlayLists(currentPlayLists);

            currentPlayLists = await GetCachedIfNotAny(withPlayListItems, playListId, currentPlayLists);

            return new ServiceResponse<List<Playlist>>(playlistListResponse.NextPageToken, currentPlayLists);
        }

        private async Task<List<Playlist>> GetCachedIfNotAny(bool withPlayListItems, string playListId, List<Playlist> currentPlayLists)
        {
            if (!currentPlayLists.Any())
            {
                if (withPlayListItems && !string.IsNullOrEmpty(playListId))
                {
                    currentPlayLists = youTubeListManagerCache.GetPlaylists(p => p.Hash == playListId);
                    var tasks = currentPlayLists.Select(PopulatePlayListWithPlaylistItemsAsync);
                    await Task.WhenAll(tasks);
                }
                else
                    currentPlayLists = youTubeListManagerCache.GetPlaylists(null);
            }
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
                PopulatePlayListWithPlaylistItemsAsync(playList);

            return playList;
        }

        private async Task PopulatePlayListWithPlaylistItemsAsync(Playlist playlist)
        {
            ServiceResponse<List<PlaylistItem>> playlistItemResponse = await GetPlaylistItemsAsync(string.Empty, playlist.Hash);
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