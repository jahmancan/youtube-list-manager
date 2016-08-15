using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HashidsNet;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using YouTubeListAPI.Business;
using YouTubeListAPI.Business.Service;
using YouTubeListAPI.Business.Service.Response;
using YouTubeListAPI.Business.Service.Wrapper;
using YouTubeListManager.Data.Domain;
using YouTubeListManager.Data.Repository;
using YouTubeListManager.Test.Common.Helpers;
using YouTubeListManager.Test.Common.TestDomain;

namespace YouTubeListManager.Test
{
    [TestClass]
    public class YouTubeListServiceTests : BootStrapper
    {
        private const string TestPlayListHash = "PLFueGfLR79HBcAc9qWgtLLLkGEr8zMjO4";
        private IYouTubeListManagerService context;

        private IYouTubeApiListServiceWrapper wrapperMock;

        [TestMethod]
        public void TestListOneSpecificPlayList()
        {
            var dummyPlayList = new PlayList
            {
                Id = 1,
                Hash = TestPlayListHash,
                Title = "Test",
                PrivacyStatus = PrivacyStatus.Private
            };

            var response = YouTubeDataTestHelper.CreatePlayListResponse(new List<PlayList> {dummyPlayList});
            var task = Task.FromResult(response);

            var serviceWrapperListMock = new Mock<IYouTubeApiListServiceWrapper>();
            container.RegisterInstance(serviceWrapperListMock.Object);
            var responseServiceMock = new Mock<IPlaylistResponseService>();
            responseServiceMock.Setup(m => m.GetResponse(It.IsAny<string>(), It.IsAny<string>())).Returns(task);
            container.RegisterInstance(responseServiceMock.Object);

            context = container.Resolve<IYouTubeListManagerService>();

            var playLists = context.GetPlaylists(string.Empty);

            Assert.AreEqual(1, playLists.Count());
            var playList = playLists.First();

            Assert.AreEqual(dummyPlayList.Hash, playList.Hash);
            Assert.AreEqual(dummyPlayList.PrivacyStatus, playList.PrivacyStatus);
        }

        [TestMethod]
        public void TestReturnNewPlayListsOnly()
        {
            var dummyPlayList1 = new PlayList
            {
                Id = 1,
                Hash = TestPlayListHash,
                Title = "Test",
                PrivacyStatus = PrivacyStatus.Private
            };

            var dummyPlayList2 = new PlayList
            {
                Id = 2,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(2),
                Title = "Test",
                PrivacyStatus = PrivacyStatus.Private
            };

            var dummyPlayListExisting = new PlayList
            {
                Id = 3,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(3),
                Title = "Test",
                PrivacyStatus = PrivacyStatus.Private
            };

            var response =
                YouTubeDataTestHelper.CreatePlayListResponse(new List<PlayList>
                {
                    dummyPlayList1,
                    dummyPlayList2,
                    dummyPlayListExisting
                });
            var task = Task.FromResult(response);

            var serviceWrapperListMock = new Mock<IYouTubeApiListServiceWrapper>();
            container.RegisterInstance(serviceWrapperListMock.Object);
            var responseServiceMock = new Mock<IPlaylistResponseService>();
            responseServiceMock.Setup(m => m.GetResponse(It.IsAny<string>(), It.IsAny<string>())).Returns(task);
            container.RegisterInstance(responseServiceMock.Object);

            var cache = container.Resolve<IYouTubeListManagerCache>();
            cache.PlayLists.Add(dummyPlayListExisting);

            context = container.Resolve<IYouTubeListManagerService>(new ParameterOverride("youTubeListManagerCache", cache));

            var playLists = context.GetPlaylists(string.Empty);

            Assert.AreEqual(2, playLists.Count());
            var playList = playLists.First();

            Assert.AreEqual(dummyPlayList1.Hash, playList.Hash);
            Assert.AreEqual(dummyPlayList1.PrivacyStatus, playList.PrivacyStatus);

            var playList2 = playLists.Last();
            Assert.AreEqual(dummyPlayList2.Hash, playList2.Hash);
            Assert.AreEqual(dummyPlayList2.PrivacyStatus, playList2.PrivacyStatus);
        }

        [TestMethod]
        public void TestReturnPlayListWithItems()
        {
            var dummyVideo1 = new VideoInfoTestObject
            {
                Id = 1,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(1),
                Duration = 300,
                Live = true,
                PrivacyStatus = PrivacyStatus.Public,
                ThumbnailUrl = "dummyUrl1",
                Title = "dummyTitle1",
                ThumbnailDetailsType = ThumbnailDetailsType.Standard,
            };

            var dummyVideo2 = new VideoInfoTestObject
            {
                Id = 2,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(2),
                Duration = 246,
                Live = true,
                PrivacyStatus = PrivacyStatus.Public,
                ThumbnailUrl = "dummyUrl2",
                Title = "dummyTitle2",
                ThumbnailDetailsType = ThumbnailDetailsType.Standard,
            };

            var videoStore = new List<VideoInfoTestObject> {dummyVideo1, dummyVideo2};

            var dummyPlayList = new PlayList
            {
                Id = 1,
                Hash = TestPlayListHash,
                Title = "Test",
                PrivacyStatus = PrivacyStatus.Private
            };

            var dummyPlayListItem1 = new PlaylistItemTestObject
            {
                Id = 1,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(1),
                Position = 1,
                PlayListId = 1,
                PlayList = dummyPlayList,
                VideoInfoId = 1,
                VideoInfo = dummyVideo1
            };

            var dummyPlayListItem2 = new PlaylistItemTestObject
            {
                Id = 2,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(2),
                Position = 2,
                PlayListId = 1,
                PlayList = dummyPlayList,
                VideoInfoId = 2,
                VideoInfo = dummyVideo2
            };

            var response = YouTubeDataTestHelper.CreatePlayListResponse(new List<PlayList> { dummyPlayList });
            var task = Task.FromResult(response);

            var serviceWrapperListMock = new Mock<IYouTubeApiListServiceWrapper>();
            serviceWrapperListMock.Setup(m => m.GetVideo(It.Is<string>(s => s == dummyVideo1.Hash)))
                .Returns(YouTubeDataTestHelper.CreateVideo(videoStore.FirstOrDefault(v => v.Hash == dummyVideo1.Hash)));
            serviceWrapperListMock.Setup(m => m.GetVideo(It.Is<string>(s => s == dummyVideo2.Hash)))
                .Returns(YouTubeDataTestHelper.CreateVideo(videoStore.FirstOrDefault(v => v.Hash == dummyVideo2.Hash)));
            container.RegisterInstance(serviceWrapperListMock.Object);
            
            var playlistResponseServiceMock = new Mock<IPlaylistResponseService>();
            playlistResponseServiceMock.Setup(m => m.GetResponse(It.IsAny<string>(), It.IsAny<string>())).Returns(task);
            container.RegisterInstance(playlistResponseServiceMock.Object);

            var playlistItemListResponse =
                YouTubeDataTestHelper.CreatePlaylistItemListResponse(new List<PlaylistItemTestObject>
                {
                    dummyPlayListItem1,
                    dummyPlayListItem2
                });
            var playlistItemListTaskResponse = Task.FromResult(playlistItemListResponse);
            var playlistItemResponseServiceMock = new Mock<IPlaylistItemResponseService>();
            playlistItemResponseServiceMock.Setup(m => m.GetResponse(string.Empty, TestPlayListHash)).Returns(playlistItemListTaskResponse);
            container.RegisterInstance(playlistItemResponseServiceMock.Object);

            context = container.Resolve<IYouTubeListManagerService>();

            var playList = context.GetPlayList(TestPlayListHash);

            Assert.IsNotNull(playList);
            Assert.AreEqual(dummyPlayList.Hash, playList.Hash);
            Assert.AreEqual(dummyPlayList.PrivacyStatus, playList.PrivacyStatus);
            Assert.AreEqual(2, playList.PlayListItems.Count);

            var foundPlayListItem = playList.PlayListItems.FirstOrDefault(i => i.Hash == dummyPlayListItem1.Hash);
            Assert.IsNotNull(foundPlayListItem);
            Assert.AreEqual(dummyPlayListItem1.Position, foundPlayListItem.Position);
            Assert.IsNotNull(foundPlayListItem.VideoInfo);
            Assert.AreEqual(dummyVideo1.Duration, foundPlayListItem.VideoInfo.Duration);

            foundPlayListItem = playList.PlayListItems.FirstOrDefault(i => i.Hash == dummyPlayListItem2.Hash);
            Assert.IsNotNull(foundPlayListItem);
            Assert.AreEqual(dummyPlayListItem2.Position, foundPlayListItem.Position);
            Assert.IsNotNull(foundPlayListItem.VideoInfo);
            Assert.AreEqual(dummyVideo2.Duration, foundPlayListItem.VideoInfo.Duration);
        }

        [TestMethod]
        public void TestReturnPlayListWithItemsOnlyWithValidTitleOrDescription()
        {
            var dummyVideo1 = new VideoInfoTestObject
            {
                Id = 1,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(1),
                Duration = 300,
                Live = true,
                PrivacyStatus = PrivacyStatus.Public,
                ThumbnailUrl = "dummyUrl1",
                Title = "dummyTitle1",
                ThumbnailDetailsType = ThumbnailDetailsType.Standard,
            };

            var dummyVideo2 = new VideoInfoTestObject
            {
                Id = 2,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(2),
                Duration = 246,
                Live = true,
                PrivacyStatus = PrivacyStatus.Private,
                ThumbnailUrl = "dummyUrl2",
                Title = VideoInfo.PrivateVideoTitle,
                ThumbnailDetailsType = ThumbnailDetailsType.Standard,
            };

            var dummyVideo3 = new VideoInfoTestObject
            {
                Id = 3,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(3),
                Duration = 189,
                Live = true,
                PrivacyStatus = PrivacyStatus.Public,
                ThumbnailUrl = "dummyUrl3",
                Title = "dummyTitle3",
                Description = VideoInfo.PrivateVideoDescription,
                ThumbnailDetailsType = ThumbnailDetailsType.Standard,
            };

            var videoStore = new List<VideoInfoTestObject> { dummyVideo1, dummyVideo2, dummyVideo3 };

            var dummyPlayList = new PlayList
            {
                Id = 1,
                Hash = TestPlayListHash,
                Title = "Test",
                PrivacyStatus = PrivacyStatus.Private
            };

            var dummyPlayListItem1 = new PlaylistItemTestObject
            {
                Id = 1,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(1),
                Position = 1,
                PlayListId = 1,
                PlayList = dummyPlayList,
                VideoInfoId = 1,
                VideoInfo = dummyVideo1,
                Description = "dummyDescription1",
                Title = "dummyTitle1"
            };

            var dummyPlayListItem2 = new PlaylistItemTestObject
            {
                Id = 2,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(2),
                Position = 2,
                PlayListId = 1,
                PlayList = dummyPlayList,
                VideoInfoId = 2,
                VideoInfo = dummyVideo2,
                Description = "dummyDescription2",
                Title = VideoInfo.PrivateVideoTitle
            };

            var dummyPlayListItem3 = new PlaylistItemTestObject
            {
                Id = 3,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(3),
                Position = 3,
                PlayListId = 1,
                PlayList = dummyPlayList,
                VideoInfoId = 3,
                VideoInfo = dummyVideo3,
                Title = "dummyTitle3",
                Description = VideoInfo.PrivateVideoDescription
            };

            var response = YouTubeDataTestHelper.CreatePlayListResponse(new List<PlayList> { dummyPlayList });
            var task = Task.FromResult(response);

            var serviceWrapperListMock = new Mock<IYouTubeApiListServiceWrapper>();
            serviceWrapperListMock.Setup(m => m.GetVideo(It.Is<string>(s => s == dummyVideo1.Hash)))
                .Returns(YouTubeDataTestHelper.CreateVideo(videoStore.FirstOrDefault(v => v.Hash == dummyVideo1.Hash)));
            container.RegisterInstance(serviceWrapperListMock.Object);

            var playlistResponseServiceMock = new Mock<IPlaylistResponseService>();
            playlistResponseServiceMock.Setup(m => m.GetResponse(It.IsAny<string>(), It.IsAny<string>())).Returns(task);
            container.RegisterInstance(playlistResponseServiceMock.Object);

            var playlistItemListResponse =
                YouTubeDataTestHelper.CreatePlaylistItemListResponse(new List<PlaylistItemTestObject>
                {
                    dummyPlayListItem1,
                    dummyPlayListItem2,
                    dummyPlayListItem3
                });
            var playlistItemListTaskResponse = Task.FromResult(playlistItemListResponse);
            var playlistItemResponseServiceMock = new Mock<IPlaylistItemResponseService>();
            playlistItemResponseServiceMock.Setup(m => m.GetResponse(string.Empty, TestPlayListHash)).Returns(playlistItemListTaskResponse);
            container.RegisterInstance(playlistItemResponseServiceMock.Object);

            context = container.Resolve<IYouTubeListManagerService>();

            var playList = context.GetPlayList(TestPlayListHash);

            Assert.IsNotNull(playList);
            Assert.AreEqual(dummyPlayList.Hash, playList.Hash);
            Assert.AreEqual(dummyPlayList.PrivacyStatus, playList.PrivacyStatus);
            Assert.AreEqual(1, playList.PlayListItems.Count);

            var foundPlayListItem = playList.PlayListItems.FirstOrDefault(i => i.Hash == dummyPlayListItem1.Hash);
            Assert.IsNotNull(foundPlayListItem);
            Assert.AreEqual(dummyPlayListItem1.Position, foundPlayListItem.Position);
            Assert.IsNotNull(foundPlayListItem.VideoInfo);
            Assert.AreEqual(dummyVideo1.Duration, foundPlayListItem.VideoInfo.Duration);

            foundPlayListItem = playList.PlayListItems.FirstOrDefault(i => i.Hash == dummyPlayListItem2.Hash);
            Assert.IsNull(foundPlayListItem);

            foundPlayListItem = playList.PlayListItems.FirstOrDefault(i => i.Hash == dummyPlayListItem3.Hash);
            Assert.IsNull(foundPlayListItem);
        }

        [TestMethod]
        public void TestUpdatePlayList()
        {
            var dummyPlayList = new PlayList
            {
                Id = 1,
                Hash = TestPlayListHash,
                Title = "Test",
                PrivacyStatus = PrivacyStatus.Private
            };

            var serviceMock = new Mock<IYouTubeApiListServiceWrapper>();
            container.RegisterInstance(serviceMock.Object);
            
            var serviceWrapperUpdateMock = new Mock<IYouTubeApiUpdateServiceWrapper>();
            serviceWrapperUpdateMock.Setup(m => m.UpdatePlayLists(It.IsAny<List<PlayList>>()))
                .Raises(m => m.PlaylistUpdated += null, new UpdatePlayListEventArgs(dummyPlayList));
            container.RegisterInstance(serviceWrapperUpdateMock.Object);

            context = container.Resolve<IYouTubeListManagerService>();

            context.UpdatePlayLists(new List<PlayList> {dummyPlayList});

            var repositoryStore = container.Resolve<IRepositoryStore>();
            var foundPlayList = repositoryStore.PlayListRepository.FindBy(pl => pl.Hash == dummyPlayList.Hash).FirstOrDefault();

            Assert.IsNotNull(foundPlayList);
            Assert.AreEqual(dummyPlayList.PrivacyStatus, foundPlayList.PrivacyStatus);
            Assert.AreEqual(dummyPlayList.Title, foundPlayList.Title);
        }
    }
}