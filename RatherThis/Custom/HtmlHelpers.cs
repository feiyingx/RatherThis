using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;
using RatherThis.Code;
using System.Web.Mvc.Html;

namespace RatherThis.Custom
{
    public static class HtmlHelpers
    {
        public static MvcHtmlString QuestionCategoryDropDownListFor<TModel, String>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, String>> expression, object htmlAttributes = null)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            List<CategoryItem> categories = Constants.QuestionCategories;

            IEnumerable<SelectListItem> items =
                categories.Select(val => new SelectListItem
                {
                    Text = val.Name,
                    Value = val.CategoryID.ToString(),
                    Selected = val.CategoryID.Equals(metadata.Model)
                });

            return htmlHelper.DropDownListFor(expression, items, htmlAttributes);
        }
    }
}