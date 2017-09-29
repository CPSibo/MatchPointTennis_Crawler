using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using MatchPointTennis_Crawler.Models;
using MatchPointTennis_Crawler.Models.Crawler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MatchPointTennis_Crawler.ScrapeProfiles
{
    public class ChampionshipReport
        : ScrapeProfile<tblChampionship>
    {
        public ChampionshipReport(Crawler crawler)
            : base(crawler)
        {
            LoadedElementID = "#ctl00_mainContent_pnlCPFlightAnchor";
        }

        public ChampionshipReport CreateFormDataFor_FromFlightSearch(string linkId, string viewstate)
        {
            if (string.IsNullOrWhiteSpace(linkId))
            {
                return null;
            }

            var correctedId = linkId.Replace('_', '$');

            FormData = new Dictionary<string, string>() {
                { "ctl00$ScriptManager1", $"ctl00$mainContent$UpdatePanel1|{correctedId}" },
                { "ctl00$mainContent$hdnSearchType", "DefaultType"},
                { "__EVENTTARGET", correctedId},
                { "__EVENTARGUMENT", ""},
                { "__ASYNCPOST", "true"},
                { "__VIEWSTATE", viewstate },
                { "__VIEWSTATEGENERATOR", "FAFE42EE"},
            };

            return this;
        }

        protected async override Task<tblChampionship> DoParse()
        {
            tblChampionship championshipReport = new Repository().Get<tblChampionship>(f => f.USTAID == USTAId);

            if (championshipReport == null)
            {
                championshipReport = new tblChampionship()
                {
                    USTAID = USTAId
                };
                var detailsTable = Document.Query("#ctl00_mainContent_tblCPStandingsHeader + table + table") as IHtmlTableElement;
                var detailsRow = detailsTable.Rows[1];

                var name = detailsRow.Cells[0].QuerySelector("a:first-of-type").InnerHtml.Cleanse();
                var level = detailsRow.Cells[1].InnerHtml.Cleanse();
                var leagueType_Rating_Gender = detailsRow.Cells[2].InnerHtml.Cleanse().Split("/");

                var leagueType = leagueType_Rating_Gender[0].Cleanse();
                var rating = leagueType_Rating_Gender[1].Cleanse();
                var gender = leagueType_Rating_Gender[2].Cleanse();

                championshipReport.Name = name;
                championshipReport.Level = level;
                championshipReport.Rating = Double.Parse(rating);
                championshipReport.Gender = gender;

                championshipReport.OwnerID = 0;

                new Repository().Add(championshipReport).Save();
            }



            var roundsTable = Document.Query(".RoundTable") as IHtmlTableElement;
            var rounds = roundsTable.Rows[0].Cells;


            foreach(var round in rounds)
            {
                var links = round.QuerySelectorAll("table td div a");

                if (links == null)
                {
                    continue;
                }

                var matches = links.Cast<IHtmlAnchorElement>();

                foreach(var matchLink in matches)
                {
                    var match = await new ChampionshipMatch(Crawler)
                        .CreateFormDataFor_FromChampionshipReport(matchLink.Id, ReturnedViewstate)
                        .Post();

                    match.ChampionshipID = championshipReport.ChampionshipID;
                    new Repository().Edit(match).Save();
                }
            }

            return championshipReport;
        }
    }
}
