﻿using System;
using System.Collections.Generic;
using YouTubeListManager.CrossCutting.Extensions;
using YouTubePlayList = Google.Apis.YouTube.v3.Data.Playlist;

namespace YouTubeListManager.CrossCutting.Domain
{
    public class PlayList
    {
        public int Id { get; set; }
        public string Hash { get; set; }
        public string Title { get; set; }
        public PrivacyStatus PrivacyStatus { get; set; }
        public string ThumbnailUrl { get; set; }
        public long? ItemCount { get; set; }
        public string PlayListItemsNextPageToken { get; set; }


        public int? UserId { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<PlayListItem> PlayListItems { get; set; }


        public PlayList()
        {
            PrivacyStatus = PrivacyStatus.Public;
            PlayListItems = new List<PlayListItem>();
        }

        public PlayList(YouTubePlayList youTubePlayList)
        {
            Hash = youTubePlayList.Id;
            Title = youTubePlayList.Snippet.Title;
            ItemCount = youTubePlayList.ContentDetails.ItemCount;
            ThumbnailUrl = youTubePlayList.Snippet.Thumbnails.GetThumbnailUrl();
            PrivacyStatus = (PrivacyStatus)Enum.Parse(typeof(PrivacyStatus), youTubePlayList.Status.PrivacyStatus, true);

            PlayListItems = new List<PlayListItem>();
        }
    }
}