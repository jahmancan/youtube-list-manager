using System.Collections.Generic;
using System.Web.Mvc;
using YouTubeListManager.BusinessContracts.Service;
using YouTubeListManager.CrossCutting.Domain;
using YouTubeListManager.CrossCutting.Request;
using YouTubeListManager.CrossCutting.Response;

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