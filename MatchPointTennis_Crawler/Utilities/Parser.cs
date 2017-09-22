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

        private const string VIEWSTATE_PATTERN_1 = "(?>__VIEWSTATE\\|(\\S+?)(?>\\||$))";

        private const string VIEWSTATE_PATTERN_2 = "(?>id=\"__VIEWSTATE\"\\s*value=\"(\\S+?)\")";

        private const string MAIN_CONTENT_PATTERN = @"\|\d+?\|updatePanel\|ctl00_mainContent_UpdatePanel1\|(.+?)\|\d*?\|.*?\|";

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
            var match = Regex.Match(response, VIEWSTATE_PATTERN_1, RegexOptions.Multiline).Groups[1].Value;

            if (string.IsNullOrWhiteSpace(match))
            {
                match = Regex.Match(response, VIEWSTATE_PATTERN_2, RegexOptions.Multiline).Groups[1].Value;
            }

            return match;
        }

        public static string GetMainContent(string response)
        {
            return Regex.Match(response, MAIN_CONTENT_PATTERN, RegexOptions.Singleline | RegexOptions.IgnoreCase).Groups[1].Value;
        }
    }
}
