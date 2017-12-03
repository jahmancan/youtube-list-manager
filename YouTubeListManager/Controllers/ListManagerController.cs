using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace YouTubeListManager.Controllers
{
    public class ListManagerController : Controller
    {
        // GET: ListManager
        public ActionResult Index()
        {
            return View();
        }
    }
}