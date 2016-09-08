using System.Collections.Generic;
using System.Web.Mvc;
using YouTubeListManager.BusinessContracts.Service;
using YouTubeListManager.CrossCutting.Domain;
using YouTubeListManager.CrossCutting.Response;
using YouTubeListManager.Request;

namespace YouTubeListManager.Controllers.Api
{
    public class PlayListItemController : BaseController
    {
        public PlayListItemController(IYouTubeListManagerService youTubeListManagerService) : base(youTubeListManagerService)
        {
        }

        [HttpGet]
        public JsonResult Get(string playlistId, string requestToken)
        {
            ServiceResponse<List<PlayListItem>> response = youTubeListManagerService.GetPlayListItemsAsync(requestToken, playlistId).Result;
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public ActionResult Delete(string hash)
        {
            return new BadRequest();
        }
    }
}
