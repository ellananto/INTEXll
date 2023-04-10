using INTEXll.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INTEXll.Infrastructure
{
    [HtmlTargetElement("div", Attributes = "page-model")]
    public class PaginationTagHelper : TagHelper
    {
        private IUrlHelperFactory uhf;

        public PaginationTagHelper(IUrlHelperFactory temp)
        {
            uhf = temp;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext vc { get; set; }
        // page Blah and page Action refers to the stuff in the step above 
        public PageInfo PageBlah { get; set; }
        public string PageAction { get; set; }
        public string PageClass { get; set; }
        public bool PageClassEnabled { get; set; }
        public string PageClassNormal { get; set; }
        public string PageClassSelected { get; set; }
        public override void Process(TagHelperContext thc, TagHelperOutput tho)
        {
            IUrlHelper uh = uhf.GetUrlHelper(vc);

            // build the div
            TagBuilder final = new TagBuilder("div");

            // for each div:
            for (int i = 1; i <= PageBlah.TotalPages; i++)
            {
                // make an a tag 
                TagBuilder tb = new TagBuilder("a");
                // give that a tag an href from the action and page number we're on
                tb.Attributes["href"] = uh.Action(PageAction, new { pageNum = i });

                // NEW IF STATEMENT STUFF
                if (PageClassEnabled)
                {
                    // add the bootrap passed in
                    tb.AddCssClass(PageClass);
                    tb.AddCssClass(i == PageBlah.CurrentPage ? PageClassSelected : PageClassNormal);
                }

                // append that as string for the tag's inner HTML
                tb.InnerHtml.Append(i.ToString());

                // append the tagbuilder variable to the div 
                final.InnerHtml.AppendHtml(tb);
            }

            tho.Content.AppendHtml(final.InnerHtml);
        }
    }
}