﻿using System.Collections.Generic;
using System.Web.Mvc;
using YouTubeListAPI.Business.Service;
using YouTubeListManager.Data.Domain;
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
        
        [HttpPost]
        public ActionResult Delete(string hash)
        {
            return new BadRequest();
        }
    }
}
