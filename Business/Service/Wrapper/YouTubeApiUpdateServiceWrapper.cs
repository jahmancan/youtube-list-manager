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
using Playlist = YouTubeListManager.CrossCutting.Domain.Playlist;
using PlaylistItem = YouTubeListManager.CrossCutting.Domain.PlaylistItem;

using YouTubePlaylist = Google.Apis.YouTube.v3.Data.Playlist;
using YouTubePlayListItem = Google.Apis.YouTube.v3.Data.PlaylistItem;

namespace YouTubeListAPI.Business.Service.Wrapper
{
    public class YouTubeApiUpdateServiceWrapper : YouTubeApiServiceWrapper, IYouTubeApiUpdateServiceWrapper
    {
        public YouTubeApiUpdateServiceWrapper(INlogLogger logger) : base(logger)
        {
        }

        public event EventHandler<UpdatePlaylistEventArgs> PlaylistUpdated;
        private void OnPlaylistUpdated(Playlist playlist)
        {
            PlaylistUpdated?.Invoke(this, new UpdatePlaylistEventArgs(playlist));
        }

        public event EventHandler<UpdatePlaylistItemEventArgs> PlaylistItemUpdated;
        private void OnPlayListItemUpdated(Playlist playlist, PlaylistItem playlistItem)
        {
            PlaylistItemUpdated?.Invoke(this, new UpdatePlaylistItemEventArgs(playlist, playlistItem));
        }

        public void UpdatePlaylists(IEnumerable<Playlist> playLists)
        {
            foreach (var playList in playLists)
                UpdateYouTubePlayList(playList);
        }

        public void UpdatePlaylistItems(Playlist playlist, IEnumerable<PlaylistItem> playListItems)
        {
            foreach (var playListItem in playListItems)
            {
                InsertUpdateYouTubePlayListItem(playListItem);

                OnPlayListItemUpdated(playlist, playListItem);
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

        private void InsertUpdateYouTubePlayListItem(PlaylistItem playlistItem)
        {
            YouTubePlayListItem youTubePlayListItem = GetPlayListItem(playlistItem.Hash);
            if (youTubePlayListItem == null)
                InsertPlaylistItem(playlistItem);
            else
            {
                youTubePlayListItem.Snippet.Position = playlistItem.Position;
                UpdatePlaylistItem(youTubePlayListItem);
            }
        }

        private YouTubePlayListItem CreatePlaylistItem(PlaylistItem playlistItem)
        {
            return new YouTubePlayListItem
            {
                Snippet = new PlaylistItemSnippet
                {
                    PlaylistId = playlistItem.Playlist.Hash,
                    Position = playlistItem.Position,
                    Title = playlistItem.VideoInfo.Title,
                    Thumbnails = new ThumbnailDetails
                    {
                        Standard = new Thumbnail
                        {
                            Url = playlistItem.VideoInfo.ThumbnailUrl
                        },
                        Default__ = new Thumbnail
                        {
                            Url = playlistItem.VideoInfo.ThumbnailUrl
                        }
                    },
                    ResourceId = new ResourceId
                    {
                        Kind = VideoInfo.VideoKind,
                        VideoId = playlistItem.VideoInfo.Hash
                    }
                }
            };
        }

        private void InsertPlaylistItem(PlaylistItem playlistItem)
        {
            YouTubePlayListItem youTubePlaylistItem = CreatePlaylistItem(playlistItem);
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

        private void UpdateYouTubePlayList(Playlist playlist)
        {
            var youTubePlaylist = GetYouTubePlayList(playlist.Hash);
            if (youTubePlaylist == null) return;

            youTubePlaylist.Snippet.Title = playlist.Title;
            youTubePlaylist.Status.PrivacyStatus = Enum.GetName(typeof (PrivacyStatus), playlist.PrivacyStatus).ToLower();

            try
            {
                var request = YouTubeService.Playlists.Update(youTubePlaylist, "snippet, status");
                request.ExecuteAsync(CancellationToken.None);

                OnPlaylistUpdated(playlist);
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