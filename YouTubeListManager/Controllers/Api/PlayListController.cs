using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using YouTubeListManager.BusinessContracts.Service;
using YouTubeListManager.CrossCutting.Domain;
using YouTubeListManager.CrossCutting.Response;
using YouTubeListManager.Request;

namespace YouTubeListManager.Controllers.Api
{
    public class PlaylistController : BaseController
    {

        public PlaylistController(IYouTubeListManagerService youTubeListManagerService) : base(youTubeListManagerService)
        {
        }

        [HttpGet]
        public async Task<JsonResult> GetAllAsync(string requestToken, bool isOffline = true)
        {
            ServiceResponse<List<Playlist>> response = await youTubeListManagerService.GetPlaylistsAsync(requestToken, isOffline);
            return Json(response, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public async Task<JsonResult> GetAsync(string hash, bool isOffline = true, bool withPlaylistItems = false)
        {
            Playlist PlayList = await youTubeListManagerService.GetPlaylistAsync(hash, isOffline, withPlaylistItems);
            return Json(PlayList, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Save(Playlist playlist)
        {
            youTubeListManagerService.UpdatePlaylists(new List<Playlist> {playlist});
            return Json("Playlist has been saved successfully", JsonRequestBehavior.DenyGet);
        }

        public ActionResult Put()
        {
            return new BadRequest();
        }

        public ActionResult Delete(int id)
        {
            return new BadRequest();
        }
        
    }
}
