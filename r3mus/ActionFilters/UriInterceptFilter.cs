using r3mus.Extensions;
using System;
using System.Web.Mvc;

namespace r3mus.ActionFilters
{
    public class UriInterceptFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var s = filterContext.HttpContext.Request.UrlOriginal();
            filterContext.HttpContext.Session.Add("filteredUri", s.AbsoluteUri);
            filterContext.HttpContext.Session.Add("originalUri", filterContext.HttpContext.Request.Url);
            base.OnActionExecuting(filterContext);
        }
    }
}