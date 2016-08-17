using System.Web.Mvc;

namespace YouTubeListManager.Request
{
    public class BadRequest : JsonResult
    {
        private const string DefaultErrorMessage = "Bad Request";

        public BadRequest()
        {
            Data = DefaultErrorMessage;
        }

        public BadRequest(string message)
        {
            if (string.IsNullOrEmpty(message))
                message = DefaultErrorMessage;

            Data = message;
        }

        public BadRequest(object data)
        {
            Data = data;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.RequestContext.HttpContext.Response.StatusCode = 400;
            base.ExecuteResult(context);
        }

    }
}