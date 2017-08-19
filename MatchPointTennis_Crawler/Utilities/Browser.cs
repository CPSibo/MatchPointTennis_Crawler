using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using AngleSharp;
using AngleSharp.Dom.Html;

namespace MatchPointTennis_Crawler
{
    public static class Browser
    {
        public static HttpClient Client { get; private set; }

        private const long DESIRED_REQUEST_DURATION = 2000;

        private const int MAX_CONCURRENT_REQUESTS = 1;

        private const int REQUEST_TIMEOUT = 30;

        private const int MAX_RETRIES = 3;

        private const int RETRY_DELAY = 5;

        public static UInt64 NumberOfRequests { get; set; }

        public static long NumberOfBytesTransfered { get; set; }

        static Browser()
        {
            Client = CreateClient();
        }

        private static HttpClient CreateClient()
        {
            HttpClient client = new HttpClient()
            {
                BaseAddress = new Uri("http://tennislink.usta.com"),
                Timeout = TimeSpan.FromSeconds(REQUEST_TIMEOUT),
            };

            client.DefaultRequestHeaders.Referrer = new Uri("http://tennislink.usta.com/leagues/Main/StatsAndStandings.aspx?SearchType=3");

            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            client.DefaultRequestHeaders.Add("X-MicrosoftAjax", "Delta=true");
            //client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 MatchPointTennisBot/0.1 (+https://match-point.tennis/bot)");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.91 Safari/537.36");
            client.DefaultRequestHeaders.Add("Origin", "http://tennislink.usta.com");
            client.DefaultRequestHeaders.Add("Host", "tennislink.usta.com");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");

            return client;
        }

        public static string CreatePostData(Dictionary<string, string> content)
        {
            var sb = new StringBuilder();
            foreach (var item in content)
            {
                if (sb.Length > 0)
                {
                    sb.Append("&");
                }

                sb.Append(item.Key);
                sb.Append("=");
                sb.Append(System.Web.HttpUtility.UrlEncode(item.Value));
            }

            return sb.ToString();
        }

        public static HttpRequestMessage CreateRequest(string path, string content)
        {
            return new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded")
            };
        }

        public static async Task<string> SendRequest(HttpRequestMessage request, bool bypassThrottle = false)
        {
            var watch = new System.Diagnostics.Stopwatch();

            HttpResponseMessage response = null;

            int currentTry = -1;

            while (currentTry <= MAX_RETRIES)
            {
                currentTry++;

                try
                {
                    response = await Client.SendAsync(request);

                    break;
                }
                catch(Exception e)
                {
                    await Task.Delay((int)Math.Pow(RETRY_DELAY, currentTry) * 1000);
                }
            }

            if(response == null)
            {
                throw new Exception("Response is null. Maximum retries reached.");
            }

            NumberOfRequests++;
            Mediator.Instance.NotifyColleagues(ViewModelMessages.RequestSent, NumberOfRequests);

            var content = await response?.Content.ReadAsStringAsync();

            NumberOfBytesTransfered += System.Text.ASCIIEncoding.ASCII.GetByteCount(content);
            Mediator.Instance.NotifyColleagues(ViewModelMessages.RequestReceived, NumberOfBytesTransfered);

            watch.Stop();

            if (!bypassThrottle && watch.ElapsedMilliseconds < DESIRED_REQUEST_DURATION)
            {
                var junk = TimeSpan.FromMilliseconds(DESIRED_REQUEST_DURATION - watch.ElapsedMilliseconds);
                await Task.Delay(TimeSpan.FromMilliseconds(DESIRED_REQUEST_DURATION - watch.ElapsedMilliseconds));
            }

            return content;
        }

        public static async Task<string> SendRequest(string path, Dictionary<string, string> content, bool bypassThrottle = false)
        {
            var stringContent = CreatePostData(content);

            var request = CreateRequest(path, stringContent);

            return await SendRequest(request, bypassThrottle);
        }

        public static async Task<string> GetInitialViewState(string year, string section, string district = null)
        {
            // Get a blank viewstate to seed everything else.
            var response = await SendRequest("/leagues/Main/StatsAndStandings.aspx", new Dictionary<string, string>(), true);
            var viewstate = Parser.GetViewState(response);

            response = await SendRequest("/leagues/Main/StatsAndStandings.aspx", new Dictionary<string, string>()
            {
                {"ctl00$ScriptManager1", "ctl00$mainContent$UpdatePanel1|ctl00$mainContent$ddlChampYear"},
                {"__EVENTTARGET", "ctl00$mainContent$ddlChampYear"},
                {"__EVENTARGUMENT", ""},
                {"__ASYNCPOST", "true"},
                {"ctl00$mainContent$ddlChampYear", year},
                {"__VIEWSTATE", viewstate},
            }, true);
            viewstate = Parser.GetViewState(response);
            var sectionId = (await Parser.Parse(Parser.GetMainContent(response))).Query("#ctl00_mainContent_ddlSection").OptionWithText(section)?.Value;

            if(sectionId == null)
            {
                throw new Exception($"Section '{section}' not found!");
            }

            // TODO: INSERT all sections into db.

            response = await SendRequest("/leagues/Main/StatsAndStandings.aspx", new Dictionary<string, string>()
            {
                {"ctl00$ScriptManager1", "ctl00$mainContent$UpdatePanel1|ctl00$mainContent$ddlSection"},
                {"__EVENTTARGET", "ctl00$mainContent$ddlSection"},
                {"__EVENTARGUMENT", ""},
                {"__ASYNCPOST", "true"},
                {"ctl00$mainContent$ddlChampYear", year},
                {"ctl00$mainContent$ddlSection", sectionId},
                {"__VIEWSTATE", viewstate},
            }, true);
            viewstate = Parser.GetViewState(response);
            var districtId = (await Parser.Parse(Parser.GetMainContent(response))).Query("#ctl00_mainContent_ddlDistrict").OptionWithText(district)?.Value;

            if (districtId == null)
            {
                throw new Exception($"District '{district}' not found!");
            }

            // TODO: INSERT all districts into db.

            if (district != null)
            {
                response = await SendRequest("/leagues/Main/StatsAndStandings.aspx", new Dictionary<string, string>()
                {
                    {"ctl00$ScriptManager1", "ctl00$mainContent$UpdatePanel1|ctl00$mainContent$ddlDistrict"},
                    {"__EVENTTARGET", "ctl00$mainContent$ddlDistrict"},
                    {"__EVENTARGUMENT", ""},
                    {"__ASYNCPOST", "true"},
                    {"ctl00$mainContent$ddlChampYear", year},
                    {"ctl00$mainContent$ddlSection", sectionId},
                    {"ctl00$mainContent$ddlDistrict", districtId},
                    {"__VIEWSTATE", viewstate},
                }, true);
                viewstate = Parser.GetViewState(response);

                // TODO: INSERT all areas into db.
                // OR do insertion on team page. We don't care about the IDs, just the name.
            }

            return viewstate;
        }
    }
}
