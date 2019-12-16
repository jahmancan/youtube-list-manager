using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
//using System.Web.Http;
//using System.Web.Mvc;
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
        [Route("video/post")]
        public async Task<JsonResult> Post(SearchRequest searchRequest)
        {
            ServiceResponse<List<VideoInfo>> suggestions = await youTubeListManagerService.SearchSuggestionsAsync(searchRequest);
            return Json(suggestions, JsonRequestBehavior.DenyGet);
        }
    }
}