using System.Collections.Generic;
using System.Web.Mvc;
using YouTubeListAPI.Business.Service;
using YouTubeListManager.Data.Domain;
using YouTubeListManager.Request;

namespace YouTubeListManager.Controllers.Api
{
    public class PlaylistController : BaseController
    {

        public PlaylistController(IYouTubeListManagerService youTubeListManagerService) : base(youTubeListManagerService)
        {
        }

        [HttpGet]
        public JsonResult GetAll(string requestToken)
        {
            ServiceResponse<List<Playlist>> response = youTubeListManagerService.GetPlaylists(requestToken);
            return Json(response, JsonRequestBehavior.AllowGet);
        } 

        [HttpGet]
        public JsonResult Get(string hash)
        {
            Playlist Playlist = youTubeListManagerService.GetPlayList(hash);
            return Json(Playlist, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Save(Playlist playlist)
        {
            youTubeListManagerService.UpdatePlayLists(new List<Playlist> {playlist});
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
