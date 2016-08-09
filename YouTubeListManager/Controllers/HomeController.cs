using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using YouTubeListAPI.Business.Service;
using YouTubeListManager.Data.Domain;
using YouTubeListManager.Data.Repository;


namespace YouTubeListManager.Controllers
{
    public class HomeController : Controller
    {
        private IYouTubeListService youTubeListService;
        private IYouTubeUpdateService youTubeUpdateService;

        public HomeController(IYouTubeListService youTubeListService, IYouTubeUpdateService youTubeUpdateService)
        {
            this.youTubeListService = youTubeListService;
            this.youTubeUpdateService = youTubeUpdateService;
        }

        public ActionResult Index()
        {
            var lists = youTubeListService.GetPlaylists();
            var suggestions = youTubeListService.ShowSuggestions("Reece Hughes - I Mua (Nahko and Medicine for the People Cover)");
            const string playListId = "PLBCE49952BEED058B";
            var playList = new PlayList
            {
                Title = "t",
                Hash = playListId,
                PrivacyStatus = PrivacyStatus.Private,
                PlayListItems = youTubeListService.GetPlayListItems(playListId).ToList()
            };
            var list = new List<PlayList>() { playList };
            youTubeUpdateService.UpdatePlayLists(list);
            ViewBag.Title = "Home Page";

            return View(playList);
        }
    }
}