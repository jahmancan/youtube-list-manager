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
        public async Task<JsonResult> GetAllAsync(string requestToken)
        {
            ServiceResponse<List<Playlist>> response = await youTubeListManagerService.GetPlaylistsAsync(requestToken);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAll(string requestToken)
        {
            ServiceResponse<List<Playlist>> response = youTubeListManagerService.GetPlaylists(requestToken);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetAsync(string hash)
        {
            Playlist PlayList = await youTubeListManagerService.GetPlaylistAsync(hash);
            return Json(PlayList, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult Get(string hash)
        {
            Playlist Playlist = youTubeListManagerService.GetPlaylist(hash);
            return Json(Playlist, JsonRequestBehavior.AllowGet);
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
