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
    public class PlayerSeason
        : ScrapeProfile<tklUserList>
    {
        public PlayerSeason(LeagueMatchCrawler crawler)
            : base(crawler)
        {
            LoadedElementID = "#ctl00_mainContent_tblIndividualAnchor";
        }

        public PlayerSeason CreateFormDataFor_FromTeam(string linkId, string viewstate)
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

        protected async override Task<tklUserList> DoParse()
        {
            var player = new Repository().Get<tklUserList>(f => f.USTAID == USTAId);

            var summaryTable = Document.Query("#ctl00_mainContent_tblIndividualAnchor") as IHtmlTableElement;

            var nameText = summaryTable.Rows[1].Cells[0].QuerySelector("strong").InnerHtml.Cleanse();
            var cityText = summaryTable.Rows[1].Cells[1].QuerySelector("strong").InnerHtml.Split(',')[0].Cleanse();
            var stateText = summaryTable.Rows[1].Cells[1].QuerySelector("strong").InnerHtml.Split(',')[1].Cleanse();
            var ratingText = decimal.Parse(summaryTable.Rows[1].Cells[2].InnerHtml.Cleanse());

            var yearText = Document.Query("#ctl00_mainContent_pnlIndividualAnchor h1").InnerHtml.Cleanse().Split(' ').Last().Cleanse();

            var year = int.Parse(yearText);

            var normalizedName = nameText.DecomposeName();


            if (player == null)
            {
                player = new tklUserList()
                {
                    USTAID = USTAId,
                    FullName = normalizedName.FullName,
                    City = cityText.ToLower(),
                    State = stateText.ToUpper(),
                    InitialYear = year,
                    CurrentYear = year,
                    InitialRating = ratingText
                };

                new Repository().Add(player).Save(player);
            }
            else
            {
                if(player.InitialYear > year)
                {
                    player.InitialYear = year;
                }

                if (player.CurrentYear < year)
                {
                    player.CurrentYear = year;
                }

                new Repository().Edit(player).Save(player);
            }

            return player;
        }
    }
}
