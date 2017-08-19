using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using AngleSharp;
using AngleSharp.Parser.Html;
using AngleSharp.Dom.Html;
using AngleSharp.Dom;
using System.Text.RegularExpressions;

namespace MatchPointTennis_Crawler
{
    public static class Parser
    {
        private static HtmlParser HtmlParser { get; set; } = new HtmlParser(Configuration.Default);

        public async static Task<IHtmlDocument> Parse(string markup)
        {
            return await HtmlParser.ParseAsync(markup);
        }

        public static IElement Query(this IHtmlDocument document, string selector)
        {
            return document.QuerySelector(selector);
        }

        public static IHtmlCollection<IElement> QueryAll(this IHtmlDocument document, string selector)
        {
            return document.QuerySelectorAll(selector);
        }

        public static string GetUSTAId(IHtmlDocument document)
        {
            if(document == null)
            {
                return null;
            }

            var hiddenField = document.Query("#ctl00_mainContent_hfHistoryPointParam");

            if(hiddenField == null)
            {
                return null;
            }

            var value = hiddenField.GetAttribute("value");

            if(string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            value = Uri.UnescapeDataString(value);

            var parts = value.Split(new string[] { "||" }, StringSplitOptions.None);

            if (parts == null || parts.Count() < 3)
            {
                return null;
            }

            var id = parts[2].Replace("+", "");

            return id;
        }

        public static string GetViewState(string response)
        {
            var pattern = "(?>__VIEWSTATE\\|(\\S+?)(?>\\||$))";
            var match = Regex.Match(response, pattern, RegexOptions.Multiline).Groups[1].Value;

            if (string.IsNullOrWhiteSpace(match))
            {
                pattern = "(?>id=\"__VIEWSTATE\"\\s*value=\"(\\S+?)\")";

                match = Regex.Match(response, pattern, RegexOptions.Multiline).Groups[1].Value;
            }

            return match;
        }

        public static string GetMainContent(string response)
        {
            var pattern = @"\|\d+?\|updatePanel\|ctl00_mainContent_UpdatePanel1\|(.+?)\|\d*?\|.*?\|";

            return Regex.Match(response, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase).Groups[1].Value;
        }
    }
}
