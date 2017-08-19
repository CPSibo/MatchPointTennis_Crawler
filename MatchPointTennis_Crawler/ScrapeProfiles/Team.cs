using AngleSharp.Dom;
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
    public class Team
        : ScrapeProfile<tklTeam>
    {
        public bool ProcessMatches { get; set; } = true;

        public bool ProcessPlayers { get; set; } = true;

        public Team(Crawler crawler)
            : base(crawler)
        {
            LoadedElementID = "#ctl00_mainContent_pnlTeamAnchor";
        }

        public Team CreateFormDataFor_Direct(string ustaId, string year)
        {
            FormData = new Dictionary<string, string>()
            {
                {"ctl00$ScriptManager1", "ctl00$ScriptManager1"},
                {"ctl00$mainContent$hdnSearchType", "DefaultType"},
                {"__EVENTTARGET", "ctl00$ScriptManager1"},
                {"__EVENTARGUMENT", $"s=3||0||{ustaId}||{year}"},
                {"__ASYNCPOST", "true"},
            };

            return this;
        }

        public Team CreateFormDataFor_FromSearch(string linkId, string viewstate)
        {
            if(string.IsNullOrWhiteSpace(linkId))
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

        protected async override Task<tklTeam> DoParse()
        {
            bool isNew = false;

            var teamSummary = Document.Query(".TeamSummaryDetail");
            var summaryTable = teamSummary.QuerySelector("#ctl00_mainContent_tblTeamAnchor") as IHtmlTableElement;

            tklTeam team = new Repository().Get<tklTeam>(f => f.USTAID == USTAId);

            if (team == null)
            {
                isNew = true;

                team = new tklTeam()
                {
                    USTAID = USTAId,
                    TeamName = teamSummary.QuerySelector("h1").InnerHtml.Replace("Team:", "").Cleanse()
                };

                var section = ProcessSection(summaryTable);
                var district = ProcessDistrict(summaryTable, section);
                var area = ProcessArea(summaryTable, district);
                var league = await ProcessLeague(summaryTable, area);
                var flight = await ProcessFlight(summaryTable, league);
                var subflight = await ProcessSubFlight(summaryTable, flight);

                team.SubFlightID = subflight.SubFlightID;

                // TODO: What the hell is this TakeWhile doing?
                var captain = string.Join("", summaryTable.QuerySelector("tr:nth-child(4) td:nth-child(1)").InnerHtml.TakeWhile(f => f == '<')).Cleanse();
                var cocaptain = string.Join("", summaryTable.QuerySelector("tr:nth-child(4) td:nth-child(2)").InnerHtml.TakeWhile(f => f == '<')).Cleanse();
                var facility = summaryTable.QuerySelector("tr:nth-child(4) td:nth-child(3)").InnerHtml.Replace("<br>", Environment.NewLine).Cleanse();
                var leagueDate = summaryTable.QuerySelector("tr:nth-child(4) td:nth-child(4)").InnerHtml.Cleanse();

                team.TeamFacility = facility;

                new Repository().Add<tklTeam>(team).Save<tklTeam>(team);
            }



            if (ProcessPlayers)
            {
                var playersRows = (Document.Query("#ctl00_mainContent_tblTeamsummaryForPlayers + table") as IHtmlTableElement)?.Rows;
                var playerActions = new List<Task>();

                if (playersRows != null)
                {
                    foreach (var row in playersRows.Take(playersRows.Count() - 1).Skip(1))
                    {
                        //playerActions.Add(ProcessPlayerRow(team, row.Cells[0].QuerySelector("a") as IHtmlAnchorElement, row.Cells[1].InnerHtml.Cleanse()));
                        //playerActions.Add(ProcessPlayerRow(team, row.Cells[2].QuerySelector("a") as IHtmlAnchorElement, row.Cells[3].InnerHtml.Cleanse()));
                        //playerActions.Add(ProcessPlayerRow(team, row.Cells[4].QuerySelector("a") as IHtmlAnchorElement, row.Cells[5].InnerHtml.Cleanse()));
                        var player1 = await ProcessPlayerRow(team, row.Cells[0].QuerySelector("a") as IHtmlAnchorElement, row.Cells[1].InnerHtml.Cleanse());
                        var player2 = await ProcessPlayerRow(team, row.Cells[2].QuerySelector("a") as IHtmlAnchorElement, row.Cells[3].InnerHtml.Cleanse());
                        var player3 = await ProcessPlayerRow(team, row.Cells[4].QuerySelector("a") as IHtmlAnchorElement, row.Cells[5].InnerHtml.Cleanse());
                    }

                    var playerCount = new Repository().GetAll<tklUserTeam>(f => f.TeamID == team.TeamID);
                    team.TeamPlayerCount = Convert.ToByte(playerCount.Count());

                    new Repository().Edit<tklTeam>(team).Save<tklTeam>(team);
                }
            }



            if (ProcessMatches)
            {
                var matchesRows = (Document.Query("#TeamSummaryTeamStandings") as IHtmlTableElement)?.Rows;

                if (matchesRows != null)
                {
                    foreach (var row in matchesRows.Take(matchesRows.Count() - 1).Skip(1))
                    {
                        var match = await ProcessMatch(team, row.QuerySelector("td:nth-child(1) a") as IHtmlAnchorElement, row.Cells.Take(4).ToList());
                        var match2 = await ProcessMatch(team, row.QuerySelector("td:nth-child(5) a") as IHtmlAnchorElement, row.Cells.Skip(4).Take(4).ToList());
                    }
                }
            }

            return team;
        }

        private tklSection ProcessSection(IHtmlTableElement summaryTable)
        {
            var sectionText = summaryTable.Rows[1].Cells[0].QuerySelector("strong").InnerHtml.Cleanse();
            var section = new Repository().Get<tklSection>(f => f.Section == sectionText);
            if (section == null)
            {
                section = new tklSection()
                {
                    Section = sectionText
                };
                new Repository().Add(section).Save(section);
            }

            return section;
        }

        private tklDistrict ProcessDistrict(IHtmlTableElement summaryTable, tklSection section)
        {
            var districtText = summaryTable.Rows[1].Cells[1].QuerySelector("strong").InnerHtml.Split(new char[] { '-' }, 2)[0].Cleanse();
            var district = new Repository().Get<tklDistrict>(f => f.District == districtText && f.SectionID == section.SectionID);
            if (district == null)
            {
                district = new tklDistrict()
                {
                    District = districtText,
                    SectionID = section.SectionID
                };
                new Repository().Add(district).Save(district);
            }

            return district;
        }

        private tklArea ProcessArea(IHtmlTableElement summaryTable, tklDistrict district)
        {
            var areaText = summaryTable.Rows[1].Cells[1].QuerySelector("strong").InnerHtml.Split(new char[] { '-' }, 2)[1].Cleanse();
            var area = new Repository().Get<tklArea>(f => f.Area == areaText && f.DistrictID == district.DistrictID);
            if (area == null)
            {
                area = new tklArea()
                {
                    Area = areaText,
                    DistrictID = district.DistrictID
                };
                new Repository().Add(area).Save(area);
            }

            return area;
        }

        private async Task<tklLeague> ProcessLeague(IHtmlTableElement summaryTable, tklArea area)
        {
            var leagueLink = summaryTable.Rows[1].Cells[2].QuerySelector("a") as IHtmlAnchorElement;
            var leagueText = leagueLink.InnerHtml.Cleanse();

            var league = new Repository().Get<tklLeague>(f =>
                    f.LeagueYear == Crawler.Year &&
                    f.LeagueName == leagueText &&
                    f.AreaID == area.AreaID);

            if(league == null)
            {
                league = await new League(Crawler).CreateFormDataFor_FromTeam(leagueLink.Id, ReturnedViewstate).Post();
            }

            return league;
        }

        private async Task<tklFlight> ProcessFlight(IHtmlTableElement summaryTable, tklLeague league)
        {
            var flightLink = summaryTable.Rows[1].Cells[3].QuerySelector("a:first-child") as IHtmlAnchorElement;
            var flightText = flightLink.InnerHtml.Cleanse();
            
            var flight = new Repository().Get<tklFlight>(f =>
                f.FlightGender == Crawler.Gender &&
                f.FlightLevel == Crawler.Rating &&
                f.LeagueID == league.LeagueID);

            if (flight == null)
            {
                flight = await new Flight(Crawler, league).CreateFormDataFor_FromTeam(flightLink.Id, ReturnedViewstate).Post();
            }

            return flight;
        }

        private async Task<tklSubFlight> ProcessSubFlight(IHtmlTableElement summaryTable, tklFlight flight)
        {
            var flightLink = summaryTable.Rows[1].Cells[3].QuerySelector("a:nth-child(2)") as IHtmlAnchorElement;

            tklSubFlight subflight = null;

            if (flightLink == null)
            {
                subflight = new Repository().Get<tklSubFlight>(f =>
                    f.SubFlight == "__DEFAULT__" &&
                    f.FlightID == flight.FlightID);

                if (subflight == null)
                {
                    subflight = new tklSubFlight()
                    {
                        SubFlight = "__DEFAULT__",
                        FlightID = flight.FlightID
                    };

                    new Repository().Add(subflight).Save(subflight);
                }
            }
            else
            {
                var flightText = flightLink.InnerHtml.Split('/')[1].Cleanse();

                subflight = new Repository().Get<tklSubFlight>(f =>
                    f.SubFlight == flightText &&
                    f.FlightID == flight.FlightID);

                if (subflight == null)
                {
                    subflight = await new SubFlight(Crawler, flight).CreateFormDataFor_FromTeam(flightLink.Id, ReturnedViewstate).Post();
                }
            }

            return subflight;
        }

        private async Task<tklUserList> ProcessPlayerRow(tklTeam team, IHtmlAnchorElement link, string rating)
        {
            tklUserList player = null;
            tklUserTeam teamPlayer = null;

            if (link != null && link.Text.Length > 0)
            {
                var fullname = link.InnerHtml.Cleanse().DecomposeName().FullName;

                var possiblePlayers = new Repository().GetAll<tklUserList>(f => f.FullName.ToLower() == fullname.ToLower());
                var teamPlayers = new Repository().GetAll<tklUserTeam>(f => f.TeamID == team.TeamID);

                foreach (var possiblePlayer in possiblePlayers)
                {
                    var possibleTeamPlayer = teamPlayers.FirstOrDefault(f => f.UserID == possiblePlayer.UserID);

                    if (possibleTeamPlayer != null)
                    {
                        teamPlayer = possibleTeamPlayer;
                        player = possiblePlayer;

                        break;
                    }
                }

                if (player == null)
                {
                    player = await new PlayerSeason(Crawler).CreateFormDataFor_FromTeam(link.Id, ReturnedViewstate).Post();

                    // TODO: This needs to be handled differently in production.
                    // Maybe in one of the dynamic rating tables?
                    if(decimal.TryParse(rating, out var parsedRating))
                    {
                        player.InitialRating = parsedRating;

                        new Repository().Edit(player).Save(player);
                    }
                    else
                    {

                    }
                }

                if (teamPlayer == null)
                {
                    teamPlayer = new Repository().Get<tklUserTeam>(f => f.TeamID == team.TeamID && f.UserID == player.UserID);

                    if (teamPlayer == null)
                    {
                        teamPlayer = new tklUserTeam()
                        {
                            TeamID = team.TeamID,
                            UserID = player.UserID
                        };

                        new Repository().Add(teamPlayer).Save(teamPlayer);
                    }
                }
            }

            return player;
        }

        private async Task<tblTeamMatch> ProcessMatch(tklTeam team, IHtmlAnchorElement matchLink, List<IHtmlTableCellElement> cells)
        {
            tblTeamMatch teamMatch = null;

            if (matchLink != null && matchLink.Text.Length > 0)
            {
                var linkId = matchLink.Id;
                var matchDate = DateTime.Parse(cells[0]?.QuerySelector("a")?.InnerHtml.Cleanse()).Date;
                var opposingTeam = cells[2]?.QuerySelector("a")?.InnerHtml.Cleanse();
                var result = cells[3]?.InnerHtml.Cleanse();

                var teamMatches = new Repository().GetAll<tblTeamMatch>(f =>
                    (f.HomeTeamID == team.TeamID || f.VisitingTeamID == team.TeamID)
                    && f.DateScheduled == matchDate);

                foreach (var possibleTeamMatch in teamMatches)
                {
                    var team1 = new Repository().Get<tklTeam>(f => f.TeamID == possibleTeamMatch.HomeTeamID);
                    var team2 = new Repository().Get<tklTeam>(f => f.TeamID == possibleTeamMatch.VisitingTeamID);

                    if (team1.TeamID == team.TeamID)
                    {
                        if (team2.TeamName == opposingTeam)
                        {
                            teamMatch = possibleTeamMatch;

                            break;
                        }
                    }
                    else if (team2.TeamID == team.TeamID)
                    {
                        if (team1.TeamName == opposingTeam)
                        {
                            teamMatch = possibleTeamMatch;

                            break;
                        }
                    }
                }

                if (teamMatch == null)
                {
                    teamMatch = await new LeagueMatch(Crawler).CreateFormDataFor_FromTeam(matchLink.Id, ReturnedViewstate).Post();
                }
            }

            return teamMatch;
        }
    }
}
