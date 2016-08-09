using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using YouTubeListManager.Data.Domain;
using YouTubeListManager.Data.Repository;
using YouTubeListManager.Logger;

using YouTubePlayListItem = Google.Apis.YouTube.v3.Data.PlaylistItem;

namespace YouTubeListAPI.Business.Service
{
    public class YouTubeUpdateService : YouTubeApiService, IYouTubeUpdateService
    {
        private readonly IRepositoryStore repositoryStore;

        public YouTubeUpdateService(INlogLogger logger, IRepositoryStore repositoryStore) : base(logger)
        {
            this.repositoryStore = repositoryStore;
        }

        public void UpdatePlayLists(IEnumerable<PlayList> playLists)
        {
            foreach (var playList in playLists)
            {
                UpdateYouTubePlayList(playList);

                InsertUpdatePlayList(playList);
            }
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
            UpdatePlaylistItems(foundPlayList, playList.PlayListItems);
        }

        private void UpdatePlaylistItems(PlayList playList, IEnumerable<PlayListItem> playListItems)
        {
            foreach (var playListItem in playListItems)
            {
                UpdateYouTubePlayListItem(playListItem);

                InsertUpdateVideoInfo(playListItem.VideoInfo);

                InsertUpdatePlayListItem(playList, playListItem);
            }
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

        private void UpdateYouTubePlayListItem(PlayListItem playListItem)
        {
            var youTubePlayListItem = GetPlayListItem(playListItem.Hash);

            try
            {
                var request = YouTubeService.PlaylistItems.Update(youTubePlayListItem, "snippet, status, contentDetails");
                request.ExecuteAsync(CancellationToken.None);
            }
            catch (Exception exception)
            {
                logger.LogError("Your update playlist request could not been served!", exception);
            }
        }

        private void UpdateYouTubePlayList(PlayList playList)
        {
            var youTubePlaylist = GetPlayList(playList.Hash);
            if (youTubePlaylist == null) return;

            youTubePlaylist.Snippet.Title = playList.Title;
            youTubePlaylist.Status.PrivacyStatus =
                Enum.GetName(typeof (PrivacyStatus), playList.PrivacyStatus).ToLower();

            try
            {
                var request = YouTubeService.Playlists.Update(youTubePlaylist, "snippet, status, contentDetails");
                request.ExecuteAsync(CancellationToken.None);
            }
            catch (Exception e)
            {
                const string error = "Your update playlist request could not been served!";
                logger.LogError(error, e);
                throw;
            }
        }
    }
}