using System.Collections.Generic;
using System.Web.Mvc;
using YouTubeListAPI.Business.Service;
using YouTubeListManager.Data.Domain;

namespace YouTubeListManager.Controllers.Api
{
    public class VideoController : BaseController
    {
        public VideoController(IYouTubeListManagerService youTubeListManagerService) : base(youTubeListManagerService)
        {
        }

        [HttpGet]
        public JsonResult Get(string searchKey, string requestToken)
        {
            ServiceResponse<List<VideoInfo>> suggestions = youTubeListManagerService.ShowSuggestions(requestToken, searchKey);
            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }
    }
}