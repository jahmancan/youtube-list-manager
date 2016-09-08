using System.Web.Mvc;
using YouTubeListManager.BusinessContracts.Service;

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
