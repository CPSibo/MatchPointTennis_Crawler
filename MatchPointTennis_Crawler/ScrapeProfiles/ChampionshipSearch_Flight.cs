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
    public class ChampionshipSearch_Flight
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

        public ChampionshipSearch_Flight(Crawler crawler)
            : base(crawler)
        {
            LoadedElementID = "#ctl00_mainContent_pnlCPDrillDownAnchor";
        }

        public ChampionshipSearch_Flight CreateFormDataFor_FlightSearch
        (
            Dictionary<string, string> postData
        )
        {
            FormData = postData;

            // Click the search button.
            FormData["ctl00$ScriptManager1"] = "ctl00$mainContent$UpdatePanel1|ctl00$mainContent$btn_Flight";
            FormData.Add("ctl00$mainContent$btn_Flight", "View Flight Championships");

            return this;
        }

        protected async override Task<object> DoParse()
        {
            if(Document.Query("#ctl00_mainContent_tblNoChampionships") != null)
            {
                Mediator.Instance.NotifyColleagues(ViewModelMessages.TeamsCollected, 0);
                Mediator.Instance.NotifyColleagues(ViewModelMessages.Finished, 0);

                return null;
            }

            var table = Document.Query("#ctl00_mainContent_divSearchResultsForTeams table") as IHtmlTableElement;

            var teamRows = table.Rows.Take(table.Rows.Count() - 1).Skip(2);

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
