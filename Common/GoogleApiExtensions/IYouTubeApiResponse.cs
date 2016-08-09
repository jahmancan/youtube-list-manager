using System.Collections.Generic;
using Google.Apis.Requests;

namespace YouTubeListManager.Common.GoogleApiExtensions
{
    public interface IYouTubeApiResponse<TYouTubeObject> : IDirectResponseSchema where TYouTubeObject : IYouTubeObject
    {
        string ETag { get; set; }
        IList<TYouTubeObject> Items { get; set; }
    }
}