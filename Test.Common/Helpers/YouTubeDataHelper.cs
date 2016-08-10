using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.YouTube.v3.Data;
using YouTubeListManager.Data.Domain;
using YouTubeListManager.Test.Common.TestDomain;
using YouTubePlayList = Google.Apis.YouTube.v3.Data.Playlist;
using YouTubePlayListItem = Google.Apis.YouTube.v3.Data.PlaylistItem;

namespace YouTubeListManager.Test.Common.Helpers
{
    public static class YouTubeDataTestHelper
    {
        public static PlaylistListResponse CreatePlayListResponse(List<PlayList> playLists)
        {
            var playLisyListResponse = new PlaylistListResponse
            {
                Items = new List<YouTubePlayList>(playLists.Select(CreatePlayList))
            };

            return playLisyListResponse;
        }

        public static PlaylistItemListResponse CreatePlaylistItemListResponse(List<PlayListItem> playListItems)
        {
            var playlistItemListResponse = new PlaylistItemListResponse
            {
                Items = new List<YouTubePlayListItem>(playListItems.Select(CreatePlaylistItem))
            };
            return playlistItemListResponse;
        }

        public static YouTubePlayList CreatePlayList(PlayList playList)
        {
            return new YouTubePlayList
            {
                Id = playList.Hash,
                Status =
                    new PlaylistStatus {PrivacyStatus = Enum.GetName(typeof (PrivacyStatus), playList.PrivacyStatus)},
                Snippet = new PlaylistSnippet {Title = playList.Title}
            };
        }

        public static YouTubePlayListItem CreatePlaylistItem(PlayListItem playListItem)
        {
            return new YouTubePlayListItem
            {
                Id = playListItem.Hash,
                Snippet = new PlaylistItemSnippet
                {
                    Position = playListItem.Position
                },
                ContentDetails = new PlaylistItemContentDetails
                {
                    VideoId = playListItem.VideoInfo.Hash
                }
            };
        }

        public static VideoListResponse CreateVideoListResponse(List<VideoInfoTestObject> videoInfoList)
        {
            var videoListResponse = new VideoListResponse
            {
                Items = new List<Video>(videoInfoList.Select(CreateVideo))
            };
            return videoListResponse;
        }

        public static Video CreateVideo(VideoInfoTestObject videoInfo)
        {
            return new Video
            {
                Id = videoInfo.Hash,
                Snippet = new VideoSnippet
                {
                    Title = videoInfo.Title,
                    Thumbnails = CreateThumbnailDetails(videoInfo),
                    Description = videoInfo.Description
                },
                ContentDetails = new VideoContentDetails
                {
                    Duration = CreateDuration(videoInfo)
                }
            };
        }


        public static string CreateDuration(VideoInfo videoInfo)
        {
            var duration = new TimeSpan(0, 0, videoInfo.Duration);
            return string.Format("PT{0}M{1}S", duration.Minutes, duration.Seconds);
        }

        private static ThumbnailDetails CreateThumbnailDetails(VideoInfoTestObject videoInfo)
        {
            var thumbnailDetails = new ThumbnailDetails();
            switch (videoInfo.ThumbnailDetailsType)
            {
                case ThumbnailDetailsType.Default:
                    thumbnailDetails.Default__ = new Thumbnail {Url = videoInfo.ThumbnailUrl};
                    break;
                case ThumbnailDetailsType.Standard:
                    thumbnailDetails.Standard = new Thumbnail {Url = videoInfo.ThumbnailUrl};
                    break;
                default:
                    thumbnailDetails = null;
                    break;
            }
            return thumbnailDetails;
        }

        public static string CreateRandomHash()
        {
            var Characters = new List<char>
            {
                'a',
                'b',
                'c',
                'd',
                'e',
                'f',
                '1',
                '2',
                '3',
                '4',
                '5',
                '6',
                '7',
                '8',
                '9',
                '0'
            };
            var characterCount = Characters.Count;

            var hash = string.Empty;
            for (var i = 0; i < 8; i++)
            {
                hash += Characters[new Random().Next(characterCount)];
            }

            return hash;
        }
    }
}