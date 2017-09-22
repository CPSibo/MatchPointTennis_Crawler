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
    public class LeagueMatch
        : ScrapeProfile<tblTeamMatch>
    {
        public LeagueMatch(Crawler crawler)
            : base(crawler)
        {
            LoadedElementID = "#ctl00_mainContent_pnlMatchAnchor";
        }

        public LeagueMatch CreateFormDataFor_FromTeam(string linkId, string viewstate)
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

        protected async override Task<tblTeamMatch> DoParse()
        {
            tblTeamMatch leagueMatch = new Repository().Get<tblTeamMatch>(f => f.USTAID == USTAId);

            if (leagueMatch == null)
            {
                leagueMatch = new tblTeamMatch()
                {
                    USTAID = USTAId
                };
            }

            var summaryTable = Document.Query("#ctl00_mainContent_pnlMatchAnchor") as IHtmlTableElement;

            var scoreCardHeaderTable = Document.Query("#ctl00_mainContent_tblScoreCardHeader1") as IHtmlTableElement;
            var scoreCardHeader = scoreCardHeaderTable.QuerySelector("tr:first-child td:nth-child(1)").InnerHtml;

            var status = Regex.Match(scoreCardHeader, @"<strong>Status:.*?<\/strong>([^<&]*)[<&]", RegexOptions.Singleline | RegexOptions.IgnoreCase).Groups?[1]?.Value.Cleanse();


            
            var homeTeamLink = scoreCardHeaderTable.QuerySelector("tr:last-child td:nth-child(2) a") as IHtmlAnchorElement;
            var homeTeamText = homeTeamLink.InnerHtml.Cleanse();
            var homeTeam = await new Team(Crawler) { ProcessMatches = false }.CreateFormDataFor_FromSearch(homeTeamLink.Id, ReturnedViewstate).Post();
            leagueMatch.HomeTeamID = homeTeam.TeamID;

            var visitingTeamLink = scoreCardHeaderTable.QuerySelector("tr:last-child td:nth-child(5) a") as IHtmlAnchorElement;
            var visitingTeamText = visitingTeamLink.InnerHtml.Cleanse();
            var visitingTeam = await new Team(Crawler) { ProcessMatches = false }.CreateFormDataFor_FromSearch(visitingTeamLink.Id, ReturnedViewstate).Post();
            leagueMatch.VisitingTeamID = visitingTeam.TeamID;



            var scoreCardHeaderTable2 = Document.Query("#ctl00_mainContent_tblScoreCardHeader2") as IHtmlTableElement;

            var scheduledDate = scoreCardHeaderTable2.QuerySelector("tr:nth-child(1) td:nth-child(1) table tr:nth-child(1) td:nth-child(1) b").InnerHtml.Cleanse();
            var playedDate = scoreCardHeaderTable2.QuerySelector("tr:nth-child(1) td:nth-child(1) table tr:nth-child(1) td:nth-child(2) b").InnerHtml.Cleanse();
            var entryDate = scoreCardHeaderTable2.QuerySelector("tr:nth-child(1) td:nth-child(1) table tr:nth-child(1) td:nth-child(3) b").InnerHtml.Cleanse();
            var winCriteria = Document.Query("#ctl00_mainContent_trScoreCardMatchWinCriteria font").InnerHtml
                .Replace(Document.Query("#ctl00_mainContent_trScoreCardMatchWinCriteria font strong").OuterHtml, "").Cleanse();

            leagueMatch.DateEntered = DateTime.Parse(entryDate).Date;
            leagueMatch.DateScheduled = DateTime.Parse(scheduledDate).Date;
            leagueMatch.DatePlayed = DateTime.Parse(playedDate).Date;

            if (leagueMatch.TeamMatchID == 0)
            {
                new Repository().Add<tblTeamMatch>(leagueMatch).Save<tblTeamMatch>(leagueMatch);
            }
            else
            {
                new Repository().Edit<tblTeamMatch>(leagueMatch).Save<tblTeamMatch>(leagueMatch);
            }


            var matchesTables = Document.QueryAll("#ctl00_mainContent_divScoreCard > table").Where(f => f.Id == null).Select(f => f.QuerySelector("tr"));

            foreach (IHtmlTableRowElement row in matchesTables)
            {
                tblMatch match = new tblMatch()
                {
                    TeamMatchID = leagueMatch.TeamMatchID
                };

                match.MatchType = row.Cells[0].InnerHtml.SplitOnBr()[0].Cleanse().ToLower();

                var time = row.Cells[0].InnerHtml.SplitOnBr()[1].Cleanse();



                var homeTeamPlayers = row.Cells[1].QuerySelectorAll("a");

                var homeTeamPlayer1 = await ProcessPlayer(homeTeamPlayers[0] as IHtmlAnchorElement, homeTeam);
                match.Home_PlayerID_1 = homeTeamPlayer1?.UserID;

                var homeTeamPlayer2 = await ProcessPlayer(homeTeamPlayers[1] as IHtmlAnchorElement, homeTeam);
                match.Home_PlayerID_2 = homeTeamPlayer2?.UserID;



                var visitingTeamPlayers = row.Cells[4].QuerySelectorAll("a");

                var visitingTeamPlayer1 = await ProcessPlayer(visitingTeamPlayers[0] as IHtmlAnchorElement, visitingTeam);
                match.Visiting_PlayerID_1 = visitingTeamPlayer1?.UserID;

                var visitingTeamPlayer2 = await ProcessPlayer(visitingTeamPlayers[1] as IHtmlAnchorElement, visitingTeam);
                match.Visiting_PlayerID_2 = visitingTeamPlayer2?.UserID;



                tklTeam winningTeam = null;

                if (row.Cells[2].QuerySelector("img") != null)
                {
                    winningTeam = homeTeam;
                }
                else if (row.Cells[5].QuerySelector("img") != null)
                {
                    winningTeam = visitingTeam;
                }

                match.WinningTeamID = winningTeam?.TeamID;


                var setScores = row.Cells[6].InnerHtml.SplitOnBr();

                if (setScores.Count() >= 1)
                {
                    var set = ProcessSet(setScores[0].Cleanse(), winningTeam == homeTeam);

                    match.Set1_HomeScore = set[0];
                    match.Set1_VisitingScore = set[1];
                }
                if (setScores.Count() >= 2)
                {
                    var set = ProcessSet(setScores[1].Cleanse(), winningTeam == homeTeam);

                    match.Set2_HomeScore = set[0];
                    match.Set2_VisitingScore = set[1];
                }
                if (setScores.Count() >= 3)
                {
                    var set = ProcessSet(setScores[2].Cleanse(), winningTeam == homeTeam);

                    match.Set3_HomeScore = set[0];
                    match.Set3_VisitingScore = set[1];
                }

                new Repository().Add<tblMatch>(match).Save<tblMatch>(match);
            }

            return leagueMatch;
        }

        private int[] ProcessSet(string setScore, bool homeWon)
        {
            setScore = setScore.Cleanse();

            int[] scores = new int[2];

            if (string.IsNullOrWhiteSpace(setScore))
            {
                setScore = "0-0";
            }

            scores[homeWon ? 0 : 1] = int.Parse(setScore.Split('-')[0]);
            scores[homeWon ? 1 : 0] = int.Parse(setScore.Split('-')[1]);

            return scores;
        }

        private async Task<tklUserList> ProcessPlayer(IHtmlAnchorElement link, tklTeam team)
        {
            var text = link.InnerHtml.Cleanse();

            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var name = text.DecomposeName().FullName;

            var possiblePlayers = new Repository().GetAll<tklUserList>(f => f.FullName.ToLower() == name.ToLower()).Select(f => f.UserID).ToList();

            if (possiblePlayers.Count() > 0)
            {
                var possibleTeamPlayers = new Repository().GetAll<tklUserTeam>(f => f.TeamID == team.TeamID && possiblePlayers.Contains(f.UserID)).Select(f => f.UserID).ToList();

                if (possibleTeamPlayers.Count() > 0)
                {
                    return new Repository().Get<tklUserList>(f => f.UserID == possibleTeamPlayers.FirstOrDefault());
                }
            }

            return await new PlayerSeason(Crawler).CreateFormDataFor_FromTeam(link.Id, ReturnedViewstate).Post();
        }
    }
}
