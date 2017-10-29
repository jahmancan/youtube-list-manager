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
using Playlist = YouTubeListManager.Data.Domain.Playlist;
using PlaylistItem = YouTubeListManager.Data.Domain.PlaylistItem;
using YouTubePlayList = Google.Apis.YouTube.v3.Data.Playlist;
using YouTubePlayListItem = Google.Apis.YouTube.v3.Data.PlaylistItem;

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

        public ServiceResponse<List<PlaylistItem>> GetPlaylistItems(string requestToken, string playListId)
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

        public Playlist GetPlayList(string playListId)
        {
            return GetPlayLists(string.Empty, true, playListId).Response.FirstOrDefault();
        }

        public ServiceResponse<List<Playlist>> GetPlaylists(string requestToken)
        {
            return GetPlayLists(requestToken, false, string.Empty);
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

            return new ServiceResponse<List<Playlist>>(taskResponse.Result.NextPageToken, currentPlayLists);
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
            var playList = new Playlist
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

        private void PopulatePlayListWithPlayListItems(Playlist playlist)
        {
            ServiceResponse<List<PlaylistItem>> playListItemResponse = GetPlaylistItems(string.Empty, playlist.Hash);
            playlist.PlaylistItemsNextPageToken = playListItemResponse.NextPageToken;
            playlist.PlaylistItems = playListItemResponse.Response;
        }

        public ServiceResponse<List<VideoInfo>> SearchSuggestions(SearchRequest searchRequest)
        {
            Task<SearchListResponse> taskResponse = searchListResponseService.GetResponse(searchRequest);

            var uniqueHashes = youTubeListManagerCache.Get<VideoInfo>(searchRequest.SearchKey).Select(t => t.Hash).Distinct();
            if (searchRequest.UsedVideoIdList.Count > 0)
                uniqueHashes = uniqueHashes.Union(searchRequest.UsedVideoIdList);

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

            youTubeListManagerCache.Add(searchRequest.SearchKey, currentVideoItems);
            var serviceResponse = new ServiceResponse<List<VideoInfo>>(taskResponse.Result.NextPageToken, currentVideoItems);

            return serviceResponse;
        }

        public void UpdatePlayLists(IEnumerable<Playlist> playLists)
        {
            youTubeApiUpdateServiceWrapper.UpdatePlayLists(playLists);
        }

        private void PlaylistUpdated(object sender, UpdatePlayListEventArgs eventArgs)
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
            repositoryStore.SaveChanges();

            youTubeApiUpdateServiceWrapper.UpdatePlaylistItems(foundPlaylist, playlist.PlaylistItems);
        }

        private void PlaylistItemUpdated(object sender, UpdatePlayListItemEventArgs eventArgs)
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
            foundPlayListItem.PlayListId = foundPlayListItem.Playlist.Id;

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