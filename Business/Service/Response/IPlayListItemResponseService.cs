﻿using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;

namespace YouTubeListAPI.Business.Service.Response
{
    public interface IPlaylistItemResponseService
    {
        Task<PlaylistItemListResponse> GetResponse(string requestToken, string playListId);
    }
}