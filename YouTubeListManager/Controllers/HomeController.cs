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
        private IYouTubeListManagerService youTubeListManagerService;

        public HomeController(IYouTubeListManagerService youTubeListManagerService)
        {
            this.youTubeListManagerService = youTubeListManagerService;
        }

        public ActionResult Index()
        {
            var lists = youTubeListManagerService.GetPlaylists(string.Empty);
            var suggestions = youTubeListManagerService.ShowSuggestions(string.Empty, "Reece Hughes - I Mua (Nahko and Medicine for the People Cover)");
            const string playListId = "PLBCE49952BEED058B";
            var playList = new PlayList
            {
                Title = "t",
                Hash = playListId,
                PrivacyStatus = PrivacyStatus.Private,
                PlayListItems = youTubeListManagerService.GetPlayListItems(string.Empty, playListId).ToList()
            };
            var list = new List<PlayList>() { playList };
            youTubeListManagerService.UpdatePlayLists(list);
            ViewBag.Title = "Home Page";

            return View(playList);
        }
    }
}