using System.Collections.Generic;
using System.Web.Mvc;
using YouTubeListAPI.Business.Service;
using YouTubeListManager.Data.Domain;
using YouTubeListManager.Request;

namespace YouTubeListManager.Controllers.Api
{
    public class PlayListController : BaseController
    {

        public PlayListController(IYouTubeListManagerService youTubeListManagerService) : base(youTubeListManagerService)
        {
        }

        [HttpGet]
        public JsonResult GetAll(string requestToken)
        {
            ServiceResponse<List<PlayList>> response = youTubeListManagerService.GetPlaylists(requestToken);
            return Json(response, JsonRequestBehavior.AllowGet);
        } 

        [HttpGet]
        public JsonResult Get(string hash)
        {
            PlayList PlayList = youTubeListManagerService.GetPlayList(hash);
            return Json(PlayList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Save(PlayList playList)
        {
            youTubeListManagerService.UpdatePlayLists(new List<PlayList> {playList});
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
