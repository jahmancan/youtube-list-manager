﻿using System;
using System.Collections.Generic;
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
        private IRepositoryStore repositoryStore;
        private IYouTubeListService youTubeListService;

        public HomeController(IRepositoryStore repositoryStore, IYouTubeListService youTubeListService)
        {
            this.repositoryStore = repositoryStore;
            this.youTubeListService = youTubeListService;
        }

        public ActionResult Index()
        {
            var lists = youTubeListService.GetPlaylists();
            List<Track> tracks = repositoryStore.TrackRepository.GetAll().ToList();
            ViewBag.Title = "Home Page";

            return View(tracks);
        }
    }
}