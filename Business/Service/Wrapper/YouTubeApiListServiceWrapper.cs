using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouTubeListAPI.Business.Extensions;
using YouTubeListAPI.Business.Service.Response;
using YouTubeListManager.Logger;

namespace YouTubeListAPI.Business.Service.Wrapper
{

    public class YouTubeApiListServiceWrapper : YouTubeApiServiceWrapper, IYouTubeApiListServiceWrapper
    {
        //todo: retrieve it from configuration..
        private const int MaxResults = 50;

        public event EventHandler<ResponseEventArgs<PlaylistListResponse>> PlaylistFetched;
        private void OnPlayListFetched(Task<PlaylistListResponse> response)
        {
            //if (PlaylistFetched == null) return;

            //PlaylistFetched(this, new ResponseEventArgs<PlaylistListResponse>(response));

            //shorter version of above
            PlaylistFetched?.Invoke(this, new ResponseEventArgs<PlaylistListResponse>(response));
        }

        public event EventHandler<ResponseEventArgs<PlaylistItemListResponse>> PlaylistItemsFetched;
        private void OnPlayLlistItemFetched(Task<PlaylistItemListResponse> response)
        {
            PlaylistItemsFetched?.Invoke(this, new ResponseEventArgs<PlaylistItemListResponse>(response));
        }

        public event EventHandler<ResponseEventArgs<SearchListResponse>> SearchResultsFetched;

        private void OnSearchResultsFetched(Task<SearchListResponse> response)
        {
            SearchResultsFetched?.Invoke(this, new ResponseEventArgs<SearchListResponse>(response));
        }

        public YouTubeApiListServiceWrapper(INlogLogger logger) : base(logger)
        {
        }


        public void ExecuteAsyncRequestPlayListItems(string requestToken, string playListId)
        {
            if (string.IsNullOrEmpty(playListId))
                throw new ApplicationException("PlayListId can not be empty");

            PlaylistItemsResource.ListRequest request = YouTubeService.PlaylistItems.List("snippet, contentDetails");
            request.PlaylistId = playListId;
            request.MaxResults = MaxResults;
            request.PageToken = requestToken;
            try
            {
                Task<PlaylistItemListResponse> taskResponse = request.ExecuteAsync(CancellationToken.None);

                OnPlayLlistItemFetched(taskResponse);
            }
            catch (Exception exception)
            {
                const string error = "Your playListItem list request could not been served!";
                logger.LogError(error, exception);
                throw;
            }
        }

        public void ExecuteAsyncRequestPlayList(string requestToken, string playListId)
        {
            ExecuteAsyncRequestPlayLists(requestToken, playListId);
        }

        public void ExcuteAsyncRequestPlayLists(string requestToken)
        {
            ExecuteAsyncRequestPlayLists(requestToken, string.Empty);
        }

        public void ExecuteAsyncRequestSearch(string requestToken, string title)
        {
            SearchResource.ListRequest request = YouTubeService.Search.List("id, snippet");
            request.MaxResults = MaxResults;
            request.Q = title.CleanTitle();
            request.PageToken = requestToken;

            try
            {
                Task<SearchListResponse> taskResponse = request.ExecuteAsync(CancellationToken.None);
                
                OnSearchResultsFetched(taskResponse);
            }
            catch (Exception exception)
            {
                const string error = "Your search list request could not been served!";
                logger.LogError(error, exception);
                throw;
            }
        }

        private void ExecuteAsyncRequestPlayLists(string requestToken, string playListId)
        {
            PlaylistsResource.ListRequest request = YouTubeService.Playlists.List("snippet");
            request.MaxResults = MaxResults;
            request.PageToken = requestToken;

            if (!string.IsNullOrEmpty(playListId))
                request.Id = playListId;
            else
                request.Mine = true;

            try
            {
                Task<PlaylistListResponse> taskResponse = request.ExecuteAsync(CancellationToken.None);
                OnPlayListFetched(taskResponse);
            }
            catch (Exception exception)
            {
                const string error = "Your playlist list request could not been served!";
                logger.LogError(error, exception);
                throw;
            }
        }
    }
}