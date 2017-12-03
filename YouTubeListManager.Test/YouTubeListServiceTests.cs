using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HashidsNet;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using YouTubeListManager.BusinessContracts;
using YouTubeListManager.BusinessContracts.Service;
using YouTubeListManager.BusinessContracts.Service.Response;
using YouTubeListManager.BusinessContracts.Service.Wrapper;
using YouTubeListManager.CrossCutting.Domain;
using YouTubeListManager.CrossCutting.EventArgs;
using YouTubeListManager.DataContracts.Repository;
using YouTubeListManager.Test.Common.Helpers;
using YouTubeListManager.Test.Common.TestDomain;

namespace YouTubeListManager.Test
{
    [TestClass]
    public class YouTubeListServiceTests : BootStrapper
    {
        //private const string TestPlayListHash = "PLFueGfLR79HBcAc9qWgtLLLkGEr8zMjO4";
        private IYouTubeListManagerService context;

        [TestMethod]
        public void TestListOneSpecificPlayList()
        {
            var dummyPlayList = new PlaylistTestObject
            {
                Id = 1,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(1),
                Title = "Test",
                PrivacyStatus = PrivacyStatus.Private,
                ThumbnailUrl = "dummyUrl2",
                ThumbnailDetailsType = ThumbnailDetailsType.Standard,
            };

            var response = YouTubeDataTestHelper.CreatePlayListResponse(new List<PlaylistTestObject> {dummyPlayList});
            var task = Task.FromResult(response);

            var serviceWrapperListMock = new Mock<IYouTubeApiListServiceWrapper>();
            container.RegisterInstance(serviceWrapperListMock.Object);
            var responseServiceMock = new Mock<IPlaylistResponseService>();
            responseServiceMock.Setup(m => m.GetResponse(It.IsAny<string>(), It.IsAny<string>())).Returns(task);
            container.RegisterInstance(responseServiceMock.Object);

            context = container.Resolve<IYouTubeListManagerService>();

            var playLists = context.GetPlayLists(string.Empty);

            Assert.AreEqual(1, playLists.Response.Count());
            var playList = playLists.Response.First();

            Assert.AreEqual(dummyPlayList.Hash, playList.Hash);
            Assert.AreEqual(dummyPlayList.PrivacyStatus, playList.PrivacyStatus);
        }

        [TestMethod]
        public void TestReturnNewPlayListsOnly()
        {
            var dummyPlayList1 = new PlaylistTestObject
            {
                Id = 1,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(1),
                Title = "Test",
                PrivacyStatus = PrivacyStatus.Private,
                ThumbnailUrl = "dummyUrl1",
                ThumbnailDetailsType = ThumbnailDetailsType.Standard,
            };

            var dummyPlayList2 = new PlaylistTestObject
            {
                Id = 2,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(2),
                Title = "Test",
                PrivacyStatus = PrivacyStatus.Private,
                ThumbnailUrl = "dummyUrl2",
                ThumbnailDetailsType = ThumbnailDetailsType.Standard,
            };

            var dummyPlayListExisting = new PlaylistTestObject
            {
                Id = 3,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(3),
                Title = "Test",
                PrivacyStatus = PrivacyStatus.Private,
                ThumbnailUrl = "dummyUrl3",
                ThumbnailDetailsType = ThumbnailDetailsType.Standard,
            };

            var response =
                YouTubeDataTestHelper.CreatePlayListResponse(new List<PlaylistTestObject>
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
            cache.AddPlayLists(new List<Playlist> { dummyPlayListExisting });

            context = container.Resolve<IYouTubeListManagerService>(new ParameterOverride("youTubeListManagerCache", cache));

            var playLists = context.GetPlayLists(string.Empty);

            Assert.AreEqual(2, playLists.Response.Count());
            var playList = playLists.Response.First();

            Assert.AreEqual(dummyPlayList1.Hash, playList.Hash);
            Assert.AreEqual(dummyPlayList1.PrivacyStatus, playList.PrivacyStatus);

            var playList2 = playLists.Response.Last();
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

            string expectedHash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(1);
            var dummyPlayList = new PlaylistTestObject
            {
                Id = 1,
                Hash = expectedHash,
                Title = "Test",
                PrivacyStatus = PrivacyStatus.Private,
                ThumbnailUrl = "dummyUrl",
                ThumbnailDetailsType = ThumbnailDetailsType.Standard
            };

            var dummyPlayListItem1 = new PlaylistItemTestObject
            {
                Id = 1,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(1),
                Position = 1,
                PlaylistId = 1,
                Playlist = dummyPlayList,
                VideoInfoId = 1,
                VideoInfo = dummyVideo1
            };

            var dummyPlayListItem2 = new PlaylistItemTestObject
            {
                Id = 2,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(2),
                Position = 2,
                PlaylistId = 1,
                Playlist = dummyPlayList,
                VideoInfoId = 2,
                VideoInfo = dummyVideo2
            };

            var response = YouTubeDataTestHelper.CreatePlayListResponse(new List<PlaylistTestObject> { dummyPlayList });
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
            playlistItemResponseServiceMock.Setup(m => m.GetResponse(string.Empty, expectedHash)).Returns(playlistItemListTaskResponse);
            container.RegisterInstance(playlistItemResponseServiceMock.Object);

            context = container.Resolve<IYouTubeListManagerService>();

            var playList = context.GetPlayList(expectedHash);

            Assert.IsNotNull(playList);
            Assert.AreEqual(dummyPlayList.Hash, playList.Hash);
            Assert.AreEqual(dummyPlayList.PrivacyStatus, playList.PrivacyStatus);
            Assert.AreEqual(2, playList.PlaylistItems.Count);

            var foundPlayListItem = playList.PlaylistItems.FirstOrDefault(i => i.Hash == dummyPlayListItem1.Hash);
            Assert.IsNotNull(foundPlayListItem);
            Assert.AreEqual(dummyPlayListItem1.Position, foundPlayListItem.Position);
            Assert.IsNotNull(foundPlayListItem.VideoInfo);
            Assert.AreEqual(dummyVideo1.Duration, foundPlayListItem.VideoInfo.Duration);

            foundPlayListItem = playList.PlaylistItems.FirstOrDefault(i => i.Hash == dummyPlayListItem2.Hash);
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

            var expectedHash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(1);
            var dummyPlayList = new PlaylistTestObject
            {
                Id = 1,
                Hash = expectedHash,
                Title = "Test",
                PrivacyStatus = PrivacyStatus.Private
                
            };

            var dummyPlayListItem1 = new PlaylistItemTestObject
            {
                Id = 1,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(1),
                Position = 1,
                PlaylistId = 1,
                Playlist = dummyPlayList,
                VideoInfoId = 1,
                VideoInfo = dummyVideo1,
                Title = "dummyTitle1",
                Description = VideoInfo.PrivateVideoDescription
            };

            var dummyPlayListItem2 = new PlaylistItemTestObject
            {
                Id = 2,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(2),
                Position = 2,
                PlaylistId = 1,
                Playlist = dummyPlayList,
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
                PlaylistId = 1,
                Playlist = dummyPlayList,
                VideoInfoId = 3,
                VideoInfo = dummyVideo3,
                Description = "dummyDescription3",
                Title = "dummyTitle3"
            };

            var response = YouTubeDataTestHelper.CreatePlayListResponse(new List<PlaylistTestObject> { dummyPlayList });
            var task = Task.FromResult(response);

            var serviceWrapperListMock = new Mock<IYouTubeApiListServiceWrapper>();
            serviceWrapperListMock.Setup(m => m.GetVideo(It.Is<string>(s => s == dummyVideo1.Hash)))
                .Returns(YouTubeDataTestHelper.CreateVideo(videoStore.FirstOrDefault(v => v.Hash == dummyVideo1.Hash)));
            serviceWrapperListMock.Setup(m => m.GetVideo(It.Is<string>(s => s == dummyVideo2.Hash)))
                .Returns(YouTubeDataTestHelper.CreateVideo(videoStore.FirstOrDefault(v => v.Hash == dummyVideo2.Hash)));
            serviceWrapperListMock.Setup(m => m.GetVideo(It.Is<string>(s => s == dummyVideo3.Hash)))
                .Returns(YouTubeDataTestHelper.CreateVideo(videoStore.FirstOrDefault(v => v.Hash == dummyVideo3.Hash)));
            container.RegisterInstance(serviceWrapperListMock.Object);

            var updateServiceWrapperMock = new Mock<IYouTubeApiUpdateServiceWrapper>();
            container.RegisterInstance(updateServiceWrapperMock.Object);
            
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
            playlistItemResponseServiceMock.Setup(m => m.GetResponse(string.Empty, expectedHash)).Returns(playlistItemListTaskResponse);
            container.RegisterInstance(playlistItemResponseServiceMock.Object);

            context = container.Resolve<IYouTubeListManagerService>();

            var playList = context.GetPlayList(expectedHash);

            Assert.IsNotNull(playList);
            Assert.AreEqual(dummyPlayList.Hash, playList.Hash);
            Assert.AreEqual(dummyPlayList.PrivacyStatus, playList.PrivacyStatus);
            Assert.AreEqual(1, playList.PlaylistItems.Count);

            var foundPlayListItem = playList.PlaylistItems.FirstOrDefault(i => i.Hash == dummyPlayListItem3.Hash);
            Assert.IsNotNull(foundPlayListItem);
            Assert.AreEqual(1, foundPlayListItem.Position);
            Assert.IsNotNull(foundPlayListItem.VideoInfo);
            Assert.AreEqual(dummyVideo3.Title, foundPlayListItem.VideoInfo.Title);
            Assert.AreEqual(dummyVideo3.Duration, foundPlayListItem.VideoInfo.Duration);
        }

        [TestMethod]
        public void TestReturnPlayListWithItemsOnlyWithValidTitleOrDescriptionIfTheyAreInDb()
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

            var expectedHash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(1);
            var dummyPlayList = new PlaylistTestObject
            {
                Id = 1,
                Hash = expectedHash,
                Title = "Test",
                PrivacyStatus = PrivacyStatus.Private

            };

            var dummyPlayListItem1 = new PlaylistItemTestObject
            {
                Id = 1,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(1),
                Position = 1,
                PlaylistId = 1,
                Playlist = dummyPlayList,
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
                PlaylistId = 1,
                Playlist = dummyPlayList,
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
                PlaylistId = 1,
                Playlist = dummyPlayList,
                VideoInfoId = 3,
                VideoInfo = dummyVideo3,
                Title = "dummyTitle3",
                Description = VideoInfo.PrivateVideoDescription
            };

            var response = YouTubeDataTestHelper.CreatePlayListResponse(new List<PlaylistTestObject> { dummyPlayList });
            var task = Task.FromResult(response);

            var serviceWrapperListMock = new Mock<IYouTubeApiListServiceWrapper>();
            serviceWrapperListMock.Setup(m => m.GetVideo(It.Is<string>(s => s == dummyVideo1.Hash)))
                .Returns(YouTubeDataTestHelper.CreateVideo(videoStore.FirstOrDefault(v => v.Hash == dummyVideo1.Hash)));
            serviceWrapperListMock.Setup(m => m.GetVideo(It.Is<string>(s => s == dummyVideo2.Hash)))
                .Returns(YouTubeDataTestHelper.CreateVideo(videoStore.FirstOrDefault(v => v.Hash == dummyVideo2.Hash)));
            serviceWrapperListMock.Setup(m => m.GetVideo(It.Is<string>(s => s == dummyVideo3.Hash)))
                .Returns(YouTubeDataTestHelper.CreateVideo(videoStore.FirstOrDefault(v => v.Hash == dummyVideo3.Hash)));
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
            playlistItemResponseServiceMock.Setup(m => m.GetResponse(string.Empty, expectedHash)).Returns(playlistItemListTaskResponse);
            container.RegisterInstance(playlistItemResponseServiceMock.Object);

            var repositoryStore = container.Resolve<IRepositoryStore>();
            repositoryStore.VideoInfoRepository.Insert(dummyVideo1.CreateVideoInfo());
            repositoryStore.VideoInfoRepository.Insert(dummyVideo2.CreateVideoInfo());
            repositoryStore.VideoInfoRepository.Insert(dummyVideo3.CreateVideoInfo());
            repositoryStore.SaveChanges();

            context = container.Resolve<IYouTubeListManagerService>();

            var playList = context.GetPlayList(expectedHash);

            Assert.IsNotNull(playList);
            Assert.AreEqual(dummyPlayList.Hash, playList.Hash);
            Assert.AreEqual(dummyPlayList.PrivacyStatus, playList.PrivacyStatus);
            Assert.AreEqual(3, playList.PlaylistItems.Count);

            var foundPlayListItem = playList.PlaylistItems.FirstOrDefault(i => i.Hash == dummyPlayListItem1.Hash);
            Assert.IsNotNull(foundPlayListItem);
            Assert.AreEqual(dummyPlayListItem1.Position, foundPlayListItem.Position);
            Assert.IsNotNull(foundPlayListItem.VideoInfo);
            Assert.AreEqual(dummyVideo1.Duration, foundPlayListItem.VideoInfo.Duration);

            foundPlayListItem = playList.PlaylistItems.FirstOrDefault(i => i.Hash == dummyPlayListItem2.Hash);
            Assert.IsFalse(foundPlayListItem.VideoInfo.Live);

            foundPlayListItem = playList.PlaylistItems.FirstOrDefault(i => i.Hash == dummyPlayListItem3.Hash);
            Assert.IsFalse(foundPlayListItem.VideoInfo.Live);

            var foundVideo = repositoryStore.VideoInfoRepository.FindBy(v => v.Hash == dummyVideo1.Hash).FirstOrDefault();
            repositoryStore.VideoInfoRepository.Delete(foundVideo);

            foundVideo = repositoryStore.VideoInfoRepository.FindBy(v => v.Hash == dummyVideo2.Hash).FirstOrDefault();
            repositoryStore.VideoInfoRepository.Delete(foundVideo);

            foundVideo = repositoryStore.VideoInfoRepository.FindBy(v => v.Hash == dummyVideo3.Hash).FirstOrDefault();
            repositoryStore.VideoInfoRepository.Delete(foundVideo);
            repositoryStore.SaveChanges();

        }

        [TestMethod]
        public void TestReturnSynchronizedPlayListItems()
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

            var dummyVideoHash2 = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(2);
            var dummyVideo2 = new VideoInfoTestObject
            {
                Id = 2,
                Hash = dummyVideoHash2,
                Duration = 246,
                Live = true,
                PrivacyStatus = PrivacyStatus.Private,
                ThumbnailUrl = "dummyUrl2",
                Title = VideoInfo.PrivateVideoTitle,
                ThumbnailDetailsType = ThumbnailDetailsType.Standard,
            };

            var dummyVideoHash3 = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(3);
            var dummyVideo3 = new VideoInfoTestObject
            {
                Id = 3,
                Hash = dummyVideoHash3,
                Duration = 189,
                Live = true,
                PrivacyStatus = PrivacyStatus.Public,
                ThumbnailUrl = "dummyUrl3",
                Title = VideoInfo.PrivateVideoTitle,
                Description = VideoInfo.PrivateVideoDescription,
                ThumbnailDetailsType = ThumbnailDetailsType.Standard,
            };

            var videoStore = new List<VideoInfoTestObject> { dummyVideo1, dummyVideo2, dummyVideo3 };

            var expectedHash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(1);
            var dummyPlayList = new Playlist
            {
                Id = 1,
                Hash = expectedHash,
                Title = "Test",
                PrivacyStatus = PrivacyStatus.Private
            };

            var dummyPlayListItem1 = new PlaylistItemTestObject
            {
                Id = 1,
                Hash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(1),
                Position = 1,
                PlaylistId = 1,
                Playlist = dummyPlayList,
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
                PlaylistId = 1,
                Playlist = dummyPlayList,
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
                PlaylistId = 1,
                Playlist = dummyPlayList,
                VideoInfoId = 3,
                VideoInfo = dummyVideo3,
                Title = "dummyTitle3",
                Description = VideoInfo.PrivateVideoDescription
            };

            var serviceWrapperListMock = new Mock<IYouTubeApiListServiceWrapper>();
            serviceWrapperListMock.Setup(m => m.GetVideo(It.Is<string>(s => s == dummyVideo1.Hash)))
                .Returns(YouTubeDataTestHelper.CreateVideo(videoStore.FirstOrDefault(v => v.Hash == dummyVideo1.Hash)));
            serviceWrapperListMock.Setup(m => m.GetVideo(It.Is<string>(s => s == dummyVideo2.Hash)))
                .Returns(YouTubeDataTestHelper.CreateVideo(videoStore.FirstOrDefault(v => v.Hash == dummyVideo2.Hash)));
            serviceWrapperListMock.Setup(m => m.GetVideo(It.Is<string>(s => s == dummyVideo3.Hash)))
                .Returns(YouTubeDataTestHelper.CreateVideo(videoStore.FirstOrDefault(v => v.Hash == dummyVideo3.Hash)));

            container.RegisterInstance(serviceWrapperListMock.Object);

            var playlistItemListResponse =
                YouTubeDataTestHelper.CreatePlaylistItemListResponse(new List<PlaylistItemTestObject>
                {
                    dummyPlayListItem1,
                    dummyPlayListItem2,
                    dummyPlayListItem3
                });
            var playlistItemListTaskResponse = Task.FromResult(playlistItemListResponse);
            var playlistItemResponseServiceMock = new Mock<IPlaylistItemResponseService>();
            playlistItemResponseServiceMock.Setup(m => m.GetResponse(string.Empty, expectedHash)).Returns(playlistItemListTaskResponse);
            container.RegisterInstance(playlistItemResponseServiceMock.Object);

            context = container.Resolve<IYouTubeListManagerService>();

            var repositoryStore = container.Resolve<IRepositoryStore>();

            const string expectedTitle = "TestTitle";
            repositoryStore.VideoInfoRepository.Insert(new VideoInfo
            {
                Hash = dummyVideo2.Hash,
                Title = expectedTitle,
                Duration = dummyVideo2.Duration,
                PrivacyStatus = PrivacyStatus.Public,
                ThumbnailUrl = dummyVideo2.ThumbnailUrl,
                Live = true
            });
            repositoryStore.VideoInfoRepository.Insert(new VideoInfo
            {
                Hash = dummyVideo3.Hash,
                Title = expectedTitle,
                Duration = dummyVideo3.Duration,
                PrivacyStatus = PrivacyStatus.Public,
                ThumbnailUrl = dummyVideo3.ThumbnailUrl,
                Live = true
            });
            repositoryStore.SaveChanges();

            var playListItems = context.GetPlaylistItems(string.Empty, expectedHash).Response;
            Assert.AreEqual(3, playListItems.Count);
            var foundPlayListItem = playListItems.FirstOrDefault();
            Assert.AreEqual(dummyPlayListItem1.Position, foundPlayListItem.Position);
            Assert.IsNotNull(foundPlayListItem.VideoInfo);
            Assert.AreEqual(dummyVideo1.Duration, foundPlayListItem.VideoInfo.Duration);

            foundPlayListItem = playListItems.FirstOrDefault(i => i.Hash == dummyPlayListItem2.Hash);
            Assert.AreEqual(expectedTitle, foundPlayListItem.VideoInfo.Title);

            foundPlayListItem = playListItems.FirstOrDefault(i => i.Hash == dummyPlayListItem3.Hash);
            Assert.AreEqual(expectedTitle, foundPlayListItem.VideoInfo.Title);

            var foundVideoInfoItems = repositoryStore.VideoInfoRepository.FindBy(v => v.Hash == dummyVideoHash2 || v.Hash == dummyVideoHash3);
            foreach (var videoInfo in foundVideoInfoItems)
            {
                repositoryStore.VideoInfoRepository.Delete(videoInfo);
            }
            repositoryStore.SaveChanges();
        }

        [TestMethod]
        public void TestUpdatePlayList()
        {
            var expectedHash = new Hashids(DateTime.UtcNow.Ticks.ToString(), 15).Encode(1);
            var dummyPlayList = new Playlist
            {
                Id = 1,
                Hash = expectedHash,
                Title = "Test",
                PrivacyStatus = PrivacyStatus.Private
            };

            var serviceMock = new Mock<IYouTubeApiListServiceWrapper>();
            container.RegisterInstance(serviceMock.Object);
            
            var serviceWrapperUpdateMock = new Mock<IYouTubeApiUpdateServiceWrapper>();
            serviceWrapperUpdateMock.Setup(m => m.UpdatePlaylists(It.IsAny<List<Playlist>>()))
                .Raises(m => m.PlaylistUpdated += null, new UpdatePlayListEventArgs(dummyPlayList));
            container.RegisterInstance(serviceWrapperUpdateMock.Object);

            context = container.Resolve<IYouTubeListManagerService>();

            context.UpdatePlayLists(new List<Playlist> {dummyPlayList});

            var repositoryStore = container.Resolve<IRepositoryStore>();
            var foundPlayList = repositoryStore.PlaylistRepository.FindBy(pl => pl.Hash == expectedHash).FirstOrDefault();

            Assert.IsNotNull(foundPlayList);
            Assert.AreEqual(dummyPlayList.PrivacyStatus, foundPlayList.PrivacyStatus);
            Assert.AreEqual(dummyPlayList.Title, foundPlayList.Title);

            repositoryStore.PlaylistRepository.Delete(foundPlayList);
            repositoryStore.SaveChanges();
        }
    }
}