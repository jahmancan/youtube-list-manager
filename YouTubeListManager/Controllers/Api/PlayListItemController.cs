using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using YouTubeListManager.BusinessContracts.Service;
using YouTubeListManager.CrossCutting.Domain;
using YouTubeListManager.CrossCutting.Response;
using YouTubeListManager.Request;

namespace YouTubeListManager.Controllers.Api
{
    public class PlaylistItemController : BaseController
    {
        public PlaylistItemController(IYouTubeListManagerService youTubeListManagerService) : base(youTubeListManagerService)
        {
        }

        [HttpGet]
        public JsonResult Get(string playlistId, string requestToken)
        {
            ServiceResponse<List<PlaylistItem>> response = youTubeListManagerService.GetPlaylistItems(requestToken, playlistId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetAsync(string playlistId, string requestToken)
        {
            ServiceResponse<List<PlaylistItem>> response = await youTubeListManagerService.GetPlaylistItemsAsync(requestToken, playlistId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(string hash)
        {
            return new BadRequest();
        }
    }
}
