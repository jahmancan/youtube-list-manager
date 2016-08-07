using System;
using NLog;

namespace YouTubeListManager.Logger
{
    public class NLogLogger : INlogLogger
    {
        private static readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();
        
        public void LogError(string error, Exception exception)
        {
            logger.Log(LogLevel.Error, exception, error);
        }
    }
}
