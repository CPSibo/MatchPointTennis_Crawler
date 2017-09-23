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
    public class ChampionshipCrawler : Crawler
    {
        public ChampionshipCrawler(MainWindowViewModel viewModel)
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
            // Get a blank viewstate to seed everything else.
            var response = await Browser.SendRequest("/leagues/Main/StatsAndStandings.aspx", new Dictionary<string, string>(), true);
            var viewstate = Parser.GetViewState(response);
            var doc = await Parser.Parse(response);

            string sectionId;

            if (!doc.Query("#ctl00_mainContent_ddlCYear").OptionWithText(ViewModel.Year.ToString()).IsSelected)
            {
                response = await Browser.SendRequest("/leagues/Main/StatsAndStandings.aspx", new Dictionary<string, string>()
                {
                    {"ctl00$ScriptManager1", "ctl00$mainContent$UpdatePanel1|ctl00$mainContent$ddlCYear"},
                    {"__EVENTTARGET", "ctl00$mainContent$ddlCYear"},
                    {"__EVENTARGUMENT", ""},
                    {"__ASYNCPOST", "true"},
                    {"ctl00$mainContent$ddlCYear", ViewModel.Year.ToString()},
                    {"__VIEWSTATE", viewstate},
                }, true);

                viewstate = Parser.GetViewState(response);

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

            response = await Browser.SendRequest("/leagues/Main/StatsAndStandings.aspx", new Dictionary<string, string>()
            {
                {"ctl00$ScriptManager1", "ctl00$mainContent$UpdatePanel1|ctl00$mainContent$ddlSection"},
                {"__EVENTTARGET", "ctl00$mainContent$ddlSection"},
                {"__EVENTARGUMENT", ""},
                {"__ASYNCPOST", "true"},
                {"ctl00$mainContent$ddlCYear", ViewModel.Year.ToString()},
                {"ctl00$mainContent$ddlSection", sectionId},
                {"ctl00$mainContent$ddlGenderChampion", ViewModel.Gender.ToUpper()[0].ToString()},
                {"__VIEWSTATE", viewstate},
            }, true);
            viewstate = Parser.GetViewState(response);
            var districtId = (await Parser.Parse(Parser.GetMainContent(response))).Query("#ctl00_mainContent_ddlDistrict").OptionWithText(ViewModel.District)?.Value;

            if (districtId == null)
            {
                throw new Exception($"District '{ViewModel.District}' not found!");
            }

            // TODO: INSERT all districts into db.

            if (ViewModel.District != null)
            {
                response = await Browser.SendRequest("/leagues/Main/StatsAndStandings.aspx", new Dictionary<string, string>()
                {
                    {"ctl00$ScriptManager1", "ctl00$mainContent$UpdatePanel1|ctl00$mainContent$ddlDistrict"},
                    {"__EVENTTARGET", "ctl00$mainContent$ddlDistrict"},
                    {"__EVENTARGUMENT", ""},
                    {"__ASYNCPOST", "true"},
                    {"ctl00$mainContent$ddlCYear", ViewModel.Year.ToString()},
                    {"ctl00$mainContent$ddlSection", sectionId},
                    {"ctl00$mainContent$ddlDistrict", districtId},
                    {"ctl00$mainContent$ddlGenderChampion", ViewModel.Gender.ToUpper()[0].ToString()},
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
