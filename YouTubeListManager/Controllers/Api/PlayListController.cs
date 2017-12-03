using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using YouTubeListManager.BusinessContracts.Service;
using YouTubeListManager.CrossCutting.Domain;
using YouTubeListManager.CrossCutting.Response;
using YouTubeListManager.Request;

namespace YouTubeListManager.Controllers.Api
{
    public class PlayListController : BaseController
    {

        public PlayListController(IYouTubeListManagerService youTubeListManagerService) : base(youTubeListManagerService)
        {
        }

        [HttpGet]
        public async Task<JsonResult> GetAllAsync(string requestToken)
        {
            ServiceResponse<List<Playlist>> response = await youTubeListManagerService.GetPlayListsAsync(requestToken);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAll(string requestToken)
        {
            ServiceResponse<List<Playlist>> response = youTubeListManagerService.GetPlayLists(requestToken);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetAsync(string hash)
        {
            Playlist PlayList = await youTubeListManagerService.GetPlayListAsync(hash);
            return Json(PlayList, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult Get(string hash)
        {
            Playlist PlayList = youTubeListManagerService.GetPlayList(hash);
            return Json(PlayList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Save(Playlist playList)
        {
            youTubeListManagerService.UpdatePlayLists(new List<Playlist> {playList});
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
