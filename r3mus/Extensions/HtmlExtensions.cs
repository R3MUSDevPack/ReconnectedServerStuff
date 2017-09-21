using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace r3mus.Extensions
{
    public static class HtmlExtensions
    {
        public static Uri UrlOriginal(this HttpRequestBase request)
        {
            string hostHeader = request.Headers["host"];

            return new Uri(string.Format("{0}://{1}{2}",
               request.Url.Scheme,
               hostHeader,
               request.RawUrl));
        }

        public static MvcHtmlString DisplayForEnum<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression)
        {
            return new MvcHtmlString(ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData).Model.ToString());
        }
    }
}