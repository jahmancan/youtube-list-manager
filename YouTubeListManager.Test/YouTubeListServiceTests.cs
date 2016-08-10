using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using YouTubeListAPI.Business.Service;
using YouTubeListManager.Data.Domain;
using YouTubeListManager.Test.Common;
using YouTubeListManager.Test.Common.Helpers;

using YouTubePlayList = Google.Apis.YouTube.v3.Data.Playlist;

namespace YouTubeListManager.Test
{
    [TestClass]
    public class YouTubeListServiceTests : BootStrapper
    {
        private const string TestPlayListHash = "PLFueGfLR79HBcAc9qWgtLLLkGEr8zMjO4";

        private IYouTubeListService context;

        [TestMethod]
        public void TestListOneSpecificPlayList()
        {
            PlayList dummyPlayList = new PlayList
            {
                Id = 1,
                Hash = TestPlayListHash,
                Title = "Test",
                PrivacyStatus = PrivacyStatus.Private
            };

            context = container.Resolve<IYouTubeListService>();

            IEnumerable<PlayList> playLists = context.GetPlaylists();

            Assert.AreEqual(1, playLists.Count());
            var playList = playLists.First();

            Assert.AreEqual(dummyPlayList.Hash, playList.Hash);
            Assert.AreEqual(dummyPlayList.PrivacyStatus, playList.PrivacyStatus);
        }
    }
}
