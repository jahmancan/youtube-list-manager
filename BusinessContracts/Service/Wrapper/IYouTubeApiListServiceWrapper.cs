using System;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouTubeListManager.CrossCutting.EventArgs;
using YouTubeListManager.CrossCutting.Request;

namespace YouTubeListManager.BusinessContracts.Service.Wrapper
{
    public interface IYouTubeApiListServiceWrapper : IYouTubeApiServiceWrapper
    {
        event EventHandler<ResponseEventArgs<PlaylistListResponse>> PlaylistFetched;
        event EventHandler<ResponseEventArgs<PlaylistItemListResponse>> PlaylistItemsFetched;
        event EventHandler<ResponseEventArgs<SearchListResponse>> SearchResultsFetched;

        void ExecuteAsyncRequestPlayListItems(string requestToken, string playListId);
        void ExecuteAsyncRequestPlayList(string requestToken, string playListId);
        void ExcuteAsyncRequestPlayLists(string requestToken);
        void ExecuteAsyncRequestSearch(SearchRequest searchRequest);
    }
}