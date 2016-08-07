using System;

namespace YouTubeListManager.Logger
{
    public interface INlogLogger
    {
        void LogError(string error, Exception exception);
    }
}