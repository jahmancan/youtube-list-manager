using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouTubeListManager.BusinessContracts.Service.Wrapper;
using YouTubeListManager.CrossCutting.EventArgs;
using YouTubeListManager.CrossCutting.Extensions;
using YouTubeListManager.CrossCutting.Request;
using YouTubeListManager.Logger;

namespace YouTubeListAPI.Business.Service.Wrapper
{

    public class YouTubeApiListServiceWrapper : YouTubeApiServiceWrapper, IYouTubeApiListServiceWrapper
    {
        //todo: retrieve it from configuration..
        private const int MaxResults = 50;
        private const string VideoType = "video";

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


        public async void ExecuteAsyncRequestPlayListItems(string requestToken, string playListId)
        {
            if (string.IsNullOrEmpty(playListId))
                throw new ApplicationException("PlayListId can not be empty");

            PlaylistItemsResource.ListRequest request = YouTubeService.PlaylistItems.List("snippet, contentDetails, status");
            request.PlaylistId = playListId;
            request.MaxResults = MaxResults;
            request.PageToken = requestToken;
            try
            {
                await request.ExecuteAsync(CancellationToken.None).ContinueWith(taskResponse =>
                {
                    OnPlayLlistItemFetched(taskResponse);
                });
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

        public async void ExecuteAsyncRequestSearch(SearchRequest searchRequest)
        {
            SearchResource.ListRequest request = YouTubeService.Search.List("id, snippet");
            request.MaxResults = MaxResults;
            request.Q = searchRequest.SearchKey.CleanTitle();
            request.VideoDuration = searchRequest.VideoDuration;
            request.PageToken = searchRequest.NextPageRequestToken;
            request.Type = VideoType;

            try
            {
                await request.ExecuteAsync(CancellationToken.None).ContinueWith(taskResponse =>
                {
                    OnSearchResultsFetched(taskResponse);
                });
            }
            catch (Exception exception)
            {
                const string error = "Your search list request could not been served!";
                logger.LogError(error, exception);
                throw;
            }
        }

        private async void ExecuteAsyncRequestPlayLists(string requestToken, string playListId)
        {
            PlaylistsResource.ListRequest request = YouTubeService.Playlists.List("snippet, status, contentDetails");
            request.MaxResults = MaxResults;
            request.PageToken = requestToken;

            if (!string.IsNullOrEmpty(playListId))
                request.Id = playListId;
            else
                request.Mine = true;

            try
            {
                await request.ExecuteAsync(CancellationToken.None).ContinueWith(taskResponse =>
                {
                    OnPlayListFetched(taskResponse);
                });
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