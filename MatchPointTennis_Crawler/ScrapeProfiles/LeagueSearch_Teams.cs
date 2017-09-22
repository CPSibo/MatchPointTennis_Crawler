using AngleSharp.Dom.Html;
using MatchPointTennis_Crawler.Models;
using MatchPointTennis_Crawler.Models.Crawler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchPointTennis_Crawler.ScrapeProfiles
{
    public class LeagueSearch_Teams
        : ScrapeProfile<object>
    {
        private struct Team
        {
            public string Name { get; set; }

            public string Section { get; set; }

            public string District { get; set; }

            public string League { get; set; }

            public string Flight { get; set; }
        }

        public LeagueSearch_Teams(LeagueMatchCrawler crawler)
            : base(crawler)
        {
            LoadedElementID = "#ctl00_mainContent_divSearchResultsForTeams";
        }

        public LeagueSearch_Teams CreateFormDataFor_TeamSearch
        (
            string viewstate, 
            string gender,
            string rating
        )
        {
            FormData = new Dictionary<string, string>()
            {
                {"ctl00$ScriptManager1", "ctl00$mainContent$UpdatePanel1|ctl00$mainContent$btnSearchTeamByName"},
                {"ctl00$mainContent$hdnSearchType", "DefaultType"},
                {"__EVENTTARGET", ""},
                {"__EVENTARGUMENT", ""},
                {"__ASYNCPOST", "true"},
                {"ctl00$mainContent$ddlNTRPLevel", rating},
                {"ctl00$mainContent$ddlGender", gender},
                {"__VIEWSTATE", viewstate},
                {"ctl00$mainContent$btnSearchTeamByName", "Find Teams" },
            };

            return this;
        }

        protected async override Task<object> DoParse()
        {
            if(Document.Query("#ctl00_mainContent_btnSearchAgain") != null)
            {
                Mediator.Instance.NotifyColleagues(ViewModelMessages.TeamsCollected, 0);
                Mediator.Instance.NotifyColleagues(ViewModelMessages.Finished, 0);

                return null;
            }

            var table = Document.Query("#ctl00_mainContent_divSearchResultsForTeams table") as IHtmlTableElement;

            var teamRows = table.Rows.Take(table.Rows.Count() - 1).Skip(2);
            //var teamCache = new HashSet<Team>();

            //foreach(var teamRow in teamRows)
            //{
            //    var cachedTeam = new Team()
            //    {
            //        Name = teamRow.Cells[0].QuerySelector("a").InnerHtml.Cleanse(),
            //        Section = teamRow.Cells[1].InnerHtml.Cleanse(),
            //        District = teamRow.Cells[2].InnerHtml.Cleanse(),
            //        Flight = teamRow.Cells[3].InnerHtml.Cleanse(),
            //    };

            //    var section = new Repository().Get<tklSection>(f => f.Section == cachedTeam.Section);
            //    var district = new Repository().Get<tklDistrict>(f => f.District == cachedTeam.Section && f.SectionID == section.SectionID);
            //    var flight = new Repository().Get<tklFlight>(f => f.FlightGender == cachedTeam.Section && f.dis);
            //    var team = new Repository().Get<tklTeam>(f => f.Section == cachedTeam.Section);
            //}

            Mediator.Instance.NotifyColleagues(ViewModelMessages.TeamsCollected, teamRows.Count());

            using (var semaphore = new System.Threading.SemaphoreSlim(1))
            {
                await Task.WhenAll(teamRows.Select(async team =>
                {
                    await semaphore.WaitAsync();

                    try
                    {
                        var item = await new ScrapeProfiles.Team(Crawler)
                            .CreateFormDataFor_FromSearch(team.Cells[0].QuerySelector("a")?.Id, ReturnedViewstate)
                            .Post();
                        
                        Mediator.Instance.NotifyColleagues(ViewModelMessages.TeamProcessed, item.TeamName);
                    }
                    catch
                    {
                        Mediator.Instance.NotifyColleagues(ViewModelMessages.TeamFailed, team.Cells[0].QuerySelector("a").InnerHtml.Cleanse());
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            return null;
        }
    }
}
