using KoronavirusMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KoronavirusMvc.TagHelpers
{
    [HtmlTargetElement(Attributes="page-info")]
    public class PagerTagHelper : TagHelper
    {
        private readonly AppSettings appSettings;
        private readonly IUrlHelperFactory urlHelperFactory;

        public PagerTagHelper(IUrlHelperFactory helperFactory, IOptionsSnapshot<AppSettings> options)
        {
            this.urlHelperFactory = helperFactory;
            appSettings = options.Value;
        }

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public PagingInfo PageInfo { get; set; }

        public string PageTitle { get; set; }

        public string PageAction { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "nav";
            int offset = appSettings.PageOffset;
            TagBuilder paginationList = new TagBuilder("ul");
            paginationList.AddCssClass("pagination");

            if (PageInfo.CurrentPage - offset > 1)
            {
                var tag = BuildListItemForPage(1, "1..");
                paginationList.InnerHtml.AppendHtml(tag);
            }

            for(int i = Math.Max(1, PageInfo.CurrentPage - offset);
                    i <= Math.Min(PageInfo.TotalPages, PageInfo.CurrentPage + offset);
                    i++)
            {
                var tag = (i == PageInfo.CurrentPage ? BuildListItemForCurrentPage(i) : BuildListItemForPage(i));
                paginationList.InnerHtml.AppendHtml(tag);
            }

            if (PageInfo.CurrentPage + offset < PageInfo.TotalPages)
            {
                var tag = BuildListItemForPage(PageInfo.TotalPages, ".." + PageInfo.TotalPages);
                paginationList.InnerHtml.AppendHtml(tag);
            }

            output.Content.AppendHtml(paginationList);
        }

        private TagBuilder BuildListItemForPage(int i)
        {
            return BuildListItemForPage(i, i.ToString());
        }

        private TagBuilder BuildListItemForPage(int i, string text)
        {
            IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);

            TagBuilder a = new TagBuilder("a");
            a.InnerHtml.Append(text);
            a.Attributes["href"] = urlHelper.Action(PageAction, new
            {
                page = i,
                sort = PageInfo.Sort,
                ascending = PageInfo.Ascending
            });
            a.AddCssClass("page-link");

            TagBuilder li = new TagBuilder("li");
            li.AddCssClass("page-item");
            li.InnerHtml.AppendHtml(a);

            return li;
        }
        private TagBuilder BuildListItemForCurrentPage(int i)
        {
            IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);

            TagBuilder input = new TagBuilder("input");

            input.Attributes["type"] = "text";
            input.Attributes["value"] = i.ToString();

            input.Attributes["data-current"] = i.ToString();
            input.Attributes["data-min"] = "1";
            input.Attributes["data-max"] = PageInfo.TotalPages.ToString();
            input.Attributes["data-url"] = urlHelper.Action(PageAction, new
            {
                page = -1,
                sort = PageInfo.Sort,
                ascending = PageInfo.Ascending
            });
            input.AddCssClass("page-link");
            input.AddCssClass("pagebox");

            if (!string.IsNullOrWhiteSpace(PageTitle))
            {
                input.Attributes["title"] = PageTitle;
            }

            TagBuilder li = new TagBuilder("li");
            li.AddCssClass("page-item active");
            li.InnerHtml.AppendHtml(input);

            return li;
        }
    }
}
