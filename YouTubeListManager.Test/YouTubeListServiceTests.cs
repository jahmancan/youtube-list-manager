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
using YouTubeListAPI.Business.Service.Response;
using YouTubeListAPI.Business.Service.Wrapper;
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
        public void OnPlaylistFetchedHasBeenRaisedTest()
        {
            Mock<YouTubeApiListServiceWrapper> serviceWrapperListMock = new Mock<YouTubeApiListServiceWrapper>();

            container.RegisterInstance(serviceWrapperListMock.Object);

            var context = container.Resolve<IPlaylistResponseService>();

            serviceWrapperListMock.Raise(w => w.PlaylistFetched += null, EventArgs.Empty);

            

        }

        private void PlayListFetched(object sender, ResponseEventArgs<PlaylistListResponse> eventArgs)
        {
            if (eventArgs.Response == null) return;
        }

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

            var response = YouTubeDataTestHelper.CreatePlayListResponse(new List<PlayList> {dummyPlayList});
            //var task = new Task<PlaylistListResponse>(() => YouTubeDataTestHelper.CreatePlayListResponse(new List<PlayList> { dummyPlayList }));
            //new ResponseEventArgs<PlaylistListResponse>(Task.FromResult(response)) as EventArgs
            Task<PlaylistListResponse> task = Task.FromResult(response);

            Mock<IYouTubeApiListServiceWrapper> serviceWrapperListMock = new Mock<IYouTubeApiListServiceWrapper>();
            serviceWrapperListMock.Setup(w => w.ExcuteAsyncRequestPlayLists(It.Is<string>(s => string.IsNullOrEmpty(s))))
                .Raises(m => m.PlaylistFetched += PlayListFetched);
            container.RegisterInstance(serviceWrapperListMock.Object);

            context = container.Resolve<IYouTubeListService>();

            IEnumerable<PlayList> playLists = context.GetPlaylists(string.Empty);

            Assert.AreEqual(1, playLists.Count());
            var playList = playLists.First();

            Assert.AreEqual(dummyPlayList.Hash, playList.Hash);
            Assert.AreEqual(dummyPlayList.PrivacyStatus, playList.PrivacyStatus);
        }
    }
}
