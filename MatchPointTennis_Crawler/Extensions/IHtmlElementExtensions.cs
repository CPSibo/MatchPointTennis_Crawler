using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchPointTennis_Crawler
{
    public static class IHtmlElementExtensions
    {
        public static IHtmlOptionElement OptionWithText(this IElement element, string text)
        {
            var cleansedText = text.Cleanse();

            return (element as IHtmlSelectElement)?.Options?.FirstOrDefault(f => f.InnerHtml.Cleanse() == cleansedText);
        }
    }
}
