using System.Collections.Generic;
using System.Web.Mvc;
using YouTubeListAPI.Business.Service;
using YouTubeListAPI.Business.Service.Response;
using YouTubeListManager.Data.Domain;

namespace YouTubeListManager.Controllers.Api
{
    public class VideoController : BaseController
    {
        public VideoController(IYouTubeListManagerService youTubeListManagerService) : base(youTubeListManagerService)
        {
        }

        [HttpPost]
        public JsonResult Post(SearchRequest searchRequest)
        {
            ServiceResponse<List<VideoInfo>> suggestions = youTubeListManagerService.SearchSuggestions(searchRequest);
            return Json(suggestions, JsonRequestBehavior.DenyGet);
        }
    }
}