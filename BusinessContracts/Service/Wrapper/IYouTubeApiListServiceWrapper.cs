using System;
using System.Threading.Tasks;
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

        Task<PlaylistItemListResponse> ExecuteAsyncRequestPlayListItems(string requestToken, string playListId);
        Task<PlaylistListResponse> ExecuteAsyncRequestPlayList(string requestToken, string playListId);
        Task<PlaylistListResponse> ExcuteAsyncRequestPlayLists(string requestToken);
        Task<SearchListResponse> ExecuteAsyncRequestSearch(SearchRequest searchRequest);
    }
}