using MatchPointTennis_Crawler.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace MatchPointTennis_Crawler.Models.Crawler
{
    public class LeagueMatchCrawler : Crawler
    {
        public LeagueMatchCrawler(MainWindowViewModel viewModel)
            : base(viewModel)
        { }

        public override async Task Search()
        {
            ResetStats();

            ElapsedTimer.Restart();
            UpdateTimer.Start();

            Log = new StringBuilder();

            if (ViewModel.Rating > 0)
            {
                await RunSearch(ViewModel.Rating);
            }
            else
            {
                var rating = 2.0m;

                while (rating < 12.5m)
                {
                    await RunSearch(rating);

                    rating += 0.5m;
                }
            }

            ElapsedTimer.Stop();
            UpdateTimer.Stop();

            Log.Append("FINISHED!");
            Log = Log;
        }

        protected async Task RunSearch(decimal rating)
        {
            ItemsProcessed = 0;
            NumberOfBytes = 0;
            NumberOfRequests = 0;
            Browser.NumberOfRequests = 0;
            Browser.NumberOfBytesTransfered = 0;
            ElapsedTimer.Restart();

            var viewstate = await GetInitialViewState();

            await new ScrapeProfiles.LeagueSearch_Teams(this)
                .CreateFormDataFor_TeamSearch
                (
                    viewstate: viewstate,
                    gender: ViewModel.Gender.Substring(0, 1),
                    rating: rating.ToString("0.0")
                )
                .Post();

            Log.Append($"Finished {ViewModel.Section} | {ViewModel.District} | {ViewModel.Area} | {ViewModel.Gender} | {ViewModel.Year} | {rating}{Environment.NewLine}");
            Log.Append($"\t{Elapsed:dd\\.hh\\:mm\\:ss} | {NumberOfRequests} Requests | {NumberOfBytes}{Environment.NewLine}{Environment.NewLine}");
            Log = Log;
        }

        public async Task<string> GetInitialViewState()
        {
            string sectionId;
            string districtId;

            var postData = new Dictionary<string, string>()
            {
                {"ctl00$ScriptManager1", ""},
                {"__EVENTTARGET", ""},
                {"__EVENTARGUMENT", ""},
                {"__ASYNCPOST", "true"},
                {"ctl00$mainContent$ddlChampYear", ViewModel.Year.ToString()},
                {"__VIEWSTATE", ""},
            };

            string searchPage = "/leagues/Main/StatsAndStandings.aspx";

            // Get a blank viewstate to seed everything else.
            var response = await Browser.SendRequest(searchPage, null, true);
            postData["__VIEWSTATE"] = Parser.GetViewState(response);
            var doc = await Parser.Parse(response);



            if (!doc.Query("#ctl00_mainContent_ddlChampYear").OptionWithText(ViewModel.Year.ToString()).IsSelected)
            {
                postData["ctl00$ScriptManager1"] = "ctl00$mainContent$UpdatePanel1|ctl00$mainContent$ddlChampYear";
                postData["__EVENTTARGET"] = "ctl00$mainContent$ddlChampYear";

                response = await Browser.SendRequest(searchPage, postData, true);

                postData["__VIEWSTATE"] = Parser.GetViewState(response);

                sectionId = (await Parser.Parse(Parser.GetMainContent(response))).Query("#ctl00_mainContent_ddlSection").OptionWithText(ViewModel.Section)?.Value;
            }
            else
            {
                sectionId = doc.Query("#ctl00_mainContent_ddlSection").OptionWithText(ViewModel.Section)?.Value;
            }

            if (sectionId == null)
            {
                throw new Exception($"Section '{ViewModel.Section}' not found!");
            }

            // TODO: INSERT all sections into db.

            postData["ctl00$ScriptManager1"] = "ctl00$mainContent$UpdatePanel1|ctl00$mainContent$ddlSection";
            postData["__EVENTTARGET"] = "ctl00$mainContent$ddlSection";
            postData.Add("ctl00$mainContent$ddlSection", sectionId);

            response = await Browser.SendRequest(searchPage, postData, true);

            postData["__VIEWSTATE"] = Parser.GetViewState(response);



            districtId = (await Parser.Parse(Parser.GetMainContent(response))).Query("#ctl00_mainContent_ddlDistrict").OptionWithText(ViewModel.District)?.Value;

            if (districtId == null)
            {
                throw new Exception($"District '{ViewModel.District}' not found!");
            }
            
            // TODO: INSERT all districts into db.

            if (ViewModel.District != null)
            {
                postData["ctl00$ScriptManager1"] = "ctl00$mainContent$UpdatePanel1|ctl00$mainContent$ddlDistrict";
                postData["__EVENTTARGET"] = "ctl00$mainContent$ddlDistrict";
                postData.Add("ctl00$mainContent$ddlDistrict", districtId);

                response = await Browser.SendRequest(searchPage, postData, true);

                postData["__VIEWSTATE"] = Parser.GetViewState(response);

                // TODO: INSERT all areas into db.
                // OR do insertion on team page. We don't care about the IDs, just the name.
            }



            return postData["__VIEWSTATE"];
        }
    }
}
