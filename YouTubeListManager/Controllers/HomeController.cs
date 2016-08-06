using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Google;
using Microsoft.Practices.Unity;
using YouTubeListManager.Data.Domain;
using YouTubeListManager.Data.Repository;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;


namespace YouTubeListManager.Controllers
{
    public class HomeController : Controller
    {
        private IRepositoryStore repositoryStore;

        public HomeController(IRepositoryStore repositoryStore)
        {
            this.repositoryStore = repositoryStore;
        }

        public ActionResult Index()
        {
            var service = Auth();
            var searchRequest = service.Search.List("snippet");
            searchRequest.Q = "spiritual";
            //searchRequest.ForMine = true;
            searchRequest.MaxResults = 50;

            try
            {
                var results = searchRequest.Execute();
                var items = results.Items.Where(i => i.Id.Kind == "youtube#playlist").ToList();
                int index = 2;
            }
            catch
            {
            }

            PlaylistsResource.ListRequest listRequest = service.Playlists.List("snippet");
            listRequest.MaxResults = 5;
            listRequest.Mine = true;
            try
            {
                var response = listRequest.Execute();
                var results = response.Items;
            }
            catch (Exception e)
            {

            }

            List<Track> tracks = repositoryStore.TrackRepository.GetAll().ToList();
            ViewBag.Title = "Home Page";
            
            return View(tracks);
        }

        public string Authenticate(string code)
        {
            return code;
        }

        private YouTubeService Auth()
        {
            var configFilePath = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);
            configFilePath += @"\config.json";

            UserCredential credential;
            using (var stream = new FileStream(configFilePath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows for full read/write access to the
                    // authenticated user's account.
                    new[] { YouTubeService.Scope.Youtube },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(this.GetType().ToString())
                ).Result;
            }


            BaseClientService.Initializer initializer = new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "YouTubeListManager"

            }; 
            return new YouTubeService(initializer);
        }
    }
}