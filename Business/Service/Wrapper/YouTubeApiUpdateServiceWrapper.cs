using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouTubeListManager.BusinessContracts.Service.Wrapper;
using YouTubeListManager.CrossCutting.Domain;
using YouTubeListManager.CrossCutting.EventArgs;
using YouTubeListManager.Logger;

using YouTubePlayListItem = Google.Apis.YouTube.v3.Data.PlaylistItem;

namespace YouTubeListAPI.Business.Service.Wrapper
{
    public class YouTubeApiUpdateServiceWrapper : YouTubeApiServiceWrapper, IYouTubeApiUpdateServiceWrapper
    {
        public YouTubeApiUpdateServiceWrapper(INlogLogger logger) : base(logger)
        {
        }

        public event EventHandler<UpdatePlayListEventArgs> PlaylistUpdated;
        private void OnPlaylistUpdated(YouTubeListManager.CrossCutting.Domain.Playlist playList)
        {
            PlaylistUpdated?.Invoke(this, new UpdatePlayListEventArgs(playList));
        }

        public event EventHandler<UpdatePlayListItemEventArgs> PlaylistItemUpdated;
        private void OnPlayListItemUpdated(YouTubeListManager.CrossCutting.Domain.Playlist playList, YouTubeListManager.CrossCutting.Domain.PlaylistItem playListItem)
        {
            PlaylistItemUpdated?.Invoke(this, new UpdatePlayListItemEventArgs(playList, playListItem));
        }

        public void UpdatePlaylists(IEnumerable<YouTubeListManager.CrossCutting.Domain.Playlist> playLists)
        {
            foreach (var playList in playLists)
                UpdateYouTubePlayList(playList);
        }

        public void UpdatePlaylistItems(YouTubeListManager.CrossCutting.Domain.Playlist playList, IEnumerable<YouTubeListManager.CrossCutting.Domain.PlaylistItem> playListItems)
        {
            foreach (var playListItem in playListItems)
            {
                InsertUpdateYouTubePlayListItem(playListItem);

                OnPlayListItemUpdated(playList, playListItem);
            }
        }

        public void DeletePlaylistItem(string playlistItemId)
        {
            PlaylistItemsResource.DeleteRequest request = YouTubeService.PlaylistItems.Delete(playlistItemId);
            try
            {
                request.ExecuteAsync();
            }
            catch (Exception exception)
            {
                logger.LogError(string.Format("Your delete playlist item({0}) request could not been served!", playlistItemId), exception);
                throw;
            }
        }

        private void InsertUpdateYouTubePlayListItem(YouTubeListManager.CrossCutting.Domain.PlaylistItem playListItem)
        {
            YouTubePlayListItem youTubePlayListItem = GetPlayListItem(playListItem.Hash);
            if (youTubePlayListItem == null)
                InsertPlaylistItem(playListItem);
            else
            {
                youTubePlayListItem.Snippet.Position = playListItem.Position;
                UpdatePlaylistItem(youTubePlayListItem);
            }
        }

        private YouTubePlayListItem CreatePlaylistItem(YouTubeListManager.CrossCutting.Domain.PlaylistItem playListItem)
        {
            return new YouTubePlayListItem
            {
                Snippet = new PlaylistItemSnippet
                {
                    PlaylistId = playListItem.Playlist.Hash,
                    Position = playListItem.Position,
                    Title = playListItem.VideoInfo.Title,
                    Thumbnails = new ThumbnailDetails
                    {
                        Standard = new Thumbnail
                        {
                            Url = playListItem.VideoInfo.ThumbnailUrl
                        },
                        Default__ = new Thumbnail
                        {
                            Url = playListItem.VideoInfo.ThumbnailUrl
                        }
                    },
                    ResourceId = new ResourceId
                    {
                        Kind = VideoInfo.VideoKind,
                        VideoId = playListItem.VideoInfo.Hash
                    }
                }
            };
        }

        private void InsertPlaylistItem(YouTubeListManager.CrossCutting.Domain.PlaylistItem playListItem)
        {
            YouTubePlayListItem youTubePlaylistItem = CreatePlaylistItem(playListItem);
            try
            {
                var request = youTubeService.PlaylistItems.Insert(youTubePlaylistItem, "snippet");
                request.ExecuteAsync(CancellationToken.None);
            }
            catch (Exception exception)
            {
                logger.LogError("Your update playlist request could not been served!", exception);
            }
        }

        private void UpdatePlaylistItem(YouTubePlayListItem playlistItem)
        {
            try
            {
                var request = youTubeService.PlaylistItems.Update(playlistItem, "snippet, status, contentDetails");
                Task<YouTubePlayListItem> response = request.ExecuteAsync(CancellationToken.None);
                var responsePlaylistItem = response.Result;
                if (responsePlaylistItem == null)
                {
                    const string error = "Your update playlist request could not been served!";
                    logger.LogError(error, new Exception(error));
                }
            }
            catch (Exception exception)
            {
                logger.LogError("Your update playlist request could not been served!", exception);
            }
        }

        private void UpdateYouTubePlayList(YouTubeListManager.CrossCutting.Domain.Playlist playList)
        {
            var youTubePlaylist = GetYouTubePlayList(playList.Hash);
            if (youTubePlaylist == null) return;

            youTubePlaylist.Snippet.Title = playList.Title;
            youTubePlaylist.Status.PrivacyStatus = Enum.GetName(typeof (PrivacyStatus), playList.PrivacyStatus).ToLower();

            try
            {
                var request = YouTubeService.Playlists.Update(youTubePlaylist, "snippet, status");
                request.ExecuteAsync(CancellationToken.None);

                OnPlaylistUpdated(playList);
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