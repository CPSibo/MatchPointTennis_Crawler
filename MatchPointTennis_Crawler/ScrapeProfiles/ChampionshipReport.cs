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
            var detailsTable = Document.Query("#ctl00_mainContent_tblCPStandingsHeader + table + table") as IHtmlTableElement;

            if(detailsTable == null)
            {
                detailsTable = Document.Query("#divCPSubFlightsForTeamStandings > table:nth-child(2)") as IHtmlTableElement;
            }
            
            var detailsRow = detailsTable.Rows[1];

            var name = detailsRow.Cells[0].QuerySelector("a:first-of-type").InnerHtml.Cleanse();
            var level = detailsRow.Cells[1].InnerHtml.Cleanse();
            var leagueType_Rating_Gender = detailsRow.Cells[2].InnerHtml.Cleanse().Split("/");

            var leagueType = leagueType_Rating_Gender[0].Cleanse();
            var rating = leagueType_Rating_Gender[1].Cleanse();
            var gender = leagueType_Rating_Gender[2].Cleanse();

            // TODO: Need to match by year, as well.
            tblChampionship championshipReport = new Repository().Get<tblChampionship>(f => f.Name == name);

            if (championshipReport != null)
            {
                return championshipReport;
            }

            championshipReport = new tblChampionship()
            {
                USTAID = USTAId,
                Name = name,
                Level = level,
                Rating = Double.Parse(rating),
                Gender = gender,
                OwnerID = 0,
            };

            new Repository().Add(championshipReport).Save();

            long ownerID = 0;

            FormData["ctl00$ScriptManager1"]    = "ctl00$mainContent$UpdatePanel1|ctl00$mainContent$lnkMatchSummaryForCPFlight";
            FormData["__EVENTTARGET"]           = "ctl00$mainContent$lnkMatchSummaryForCPFlight";
            FormData["__VIEWSTATE"]             = ReturnedViewstate;

            Result = await Browser.SendRequest(Path, FormData);
            ReturnedViewstate = Parser.GetViewState(Result);
            Document = await Parser.Parse(Parser.GetMainContent(Result));

            if(Document.Query("#ctl00_mainContent_tblCpMatchesHeader") == null)
            {
                throw new Exception($"Could not load Match Summary tab. ID={USTAId}");
            }

            var wrapper = Document.Query(".panes");
            var links = wrapper.QuerySelectorAll("td > .hastooltip > a");

            var matches = links.Cast<IHtmlAnchorElement>();

            foreach(var matchLink in matches)
            {
                var match = await new ChampionshipMatch(Crawler)
                    .CreateFormDataFor_FromChampionshipReport(matchLink.Id, ReturnedViewstate)
                    .Post();

                match.ChampionshipID = championshipReport.ChampionshipID;
                new Repository().Edit(match).Save();

                if (ownerID == 0)
                {
                    tklTeam team = new Repository().Get<tklTeam>(f => f.TeamID == match.HomeTeamID);
                    var subflight = new Repository().Get<tklSubFlight>(f => f.SubFlightID == team.SubFlightID);
                    var flight = new Repository().Get<tklFlight>(f => f.FlightID == subflight.FlightID);
                    var league = new Repository().Get<tklLeague>(f => f.LeagueID == flight.LeagueID);
                    var area = new Repository().Get<tklArea>(f => f.AreaID == league.AreaID);
                    var district = new Repository().Get<tklDistrict>(f => f.DistrictID == area.DistrictID);

                    switch (championshipReport.Level)
                    {
                        case "Flight":
                            if(flight.FlightID == 0)
                            {
                                throw new Exception($"Could not get flight ID. ID={USTAId}");
                            }

                            ownerID = flight.FlightID;
                            break;

                        case "District":
                            if (district.DistrictID == 0)
                            {
                                throw new Exception($"Could not get district ID. ID={USTAId}");
                            }

                            ownerID = district.DistrictID;
                            break;
                    }
                }
            }

            if(ownerID == 0)
            {
                throw new Exception($"Owner ID not found. ID={USTAId}");
            }

            championshipReport.OwnerID = ownerID;
            new Repository().Edit(championshipReport).Save();

            return championshipReport;
        }
    }
}
