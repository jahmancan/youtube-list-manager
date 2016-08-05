using System.Web.Mvc;
using Microsoft.Practices.Unity;
using YouTubeListManager.Data.Repository;

namespace YouTubeListManager.Controllers
{
    public class HomeController : Controller
    {
        [Dependency]
        public IRepositoryStore RepositoryStore { get; set; }

        public ActionResult Index()
        {
            var test = RepositoryStore.YouTubeTrackRepository.GetAll();
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}