using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MatchPointTennis_Crawler
{
    public struct Name
    {
        public string First { get; set; }

        public string Middle { get; set; }

        public string Last { get; set; }

        public string FullName => First + " " + (string.IsNullOrWhiteSpace(Middle) ? "" : Middle + " ") + Last;
    }

    public static class StringExtensions
    {
        public static string Cleanse(this string me)
        {
            return me
                .Replace("<br>", "")
                .Replace("<br />", "")
                .Replace("&nbsp;", " ")
                .Replace("&amp;", "&")
                .Trim();
        }

        public static string[] SplitOnBr(this string me)
        {
            return me.Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] SplitOnNewline(this string me)
        {
            return me.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string TitleCase(this string me)
        {
            return new CultureInfo("en-US", false).TextInfo.ToTitleCase(me);
        }

        public static Name DecomposeName(this string me)
        {
            me = me.Cleanse();

            Name name = new Name();

            string pattern = null;

            // Last, M First
            string pattern_LastFirstMiddle = @"^(?<LastName>.+),\s*(?<FirstName>\S+)(?:\s+(?<MiddleName>\S+))?$";
            // First M Last
            string pattern_FirstMiddleLast = @"^(?<FirstName>\S+)(?:\s+(?<MiddleName>\S+))?[\s|\.]+(?<LastName>.+)$";

            if (Regex.IsMatch(me, pattern_LastFirstMiddle))
            {
                pattern = pattern_LastFirstMiddle;
            }
            else if (Regex.IsMatch(me, pattern_FirstMiddleLast))
            {
                pattern = pattern_FirstMiddleLast;
            }
            else
            {
                throw new Exception($"Cannot parse name: {me}");
            }

            var matches = Regex.Match(me, pattern).Groups;

            if (matches.Count > 1)
            {
                name.First = matches["FirstName"].Value.Trim();

                if (matches["MiddleName"].Value.Length > 0)
                {
                    name.Middle = matches["MiddleName"].Value.Replace(".", "").Trim();
                }

                name.Last = matches["LastName"].Value.Trim();
            }

            return name;
        }

        public static string[] Split(this string haystack, string token)
        {
            return haystack.Split(new string[] { token }, StringSplitOptions.None);
        }
    }
}
