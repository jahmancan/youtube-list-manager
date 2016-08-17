using System.Web.Mvc;
using YouTubeListAPI.Business.Service;

namespace YouTubeListManager.Controllers.Api
{
    public abstract class BaseController : Controller
    {
        protected IYouTubeListManagerService youTubeListManagerService;

        protected BaseController(IYouTubeListManagerService youTubeListManagerService)
        {
            this.youTubeListManagerService = youTubeListManagerService;
        }
    }
}
