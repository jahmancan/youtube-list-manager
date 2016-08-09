using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Requests;

namespace YouTubeListManager.Common.GoogleApiExtensions
{
    public interface IYouTubeRequest<TYouTubeResponse, TYouTubeObject> where TYouTubeResponse : IYouTubeApiResponse<TYouTubeObject>
        where TYouTubeObject : IYouTubeObject
    {
        string Id { get; set; }

        Task<TYouTubeResponse> ExecuteAsync(CancellationToken cancellationToken);
    }
}
