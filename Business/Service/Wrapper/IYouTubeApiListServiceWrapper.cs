using System;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouTubeListAPI.Business.Service.Response;

namespace YouTubeListAPI.Business.Service.Wrapper
{
    public interface IYouTubeApiListServiceWrapper : IYouTubeApiServiceWrapper
    {
        event EventHandler<ResponseEventArgs<PlaylistListResponse>> PlaylistFetched;
        event EventHandler<ResponseEventArgs<PlaylistItemListResponse>> PlaylistItemsFetched;
        event EventHandler<ResponseEventArgs<SearchListResponse>> SearchResultsFetched;

        void ExecuteAsyncRequestPlayListItems(string requestToken, string playListId);
        void ExecuteAsyncRequestPlayList(string requestToken, string playListId);
        void ExcuteAsyncRequestPlayLists(string requestToken);
        void ExecuteAsyncRequestSearch(string requestToken, string title, SearchResource.ListRequest.VideoDurationEnum videoDuration);
    }
}