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
    class Flight
        : ScrapeProfile<tklFlight>
    {
        private tklLeague League { get; set; }

        public Flight(Crawler crawler, tklLeague league)
            : base(crawler)
        {
            League = league;
            LoadedElementID = "#ctl00_mainContent_pnlFlightSummary";
        }

        public Flight CreateFormDataFor_FromTeam(string linkId, string viewstate)
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

        protected async override Task<tklFlight> DoParse()
        {
            var flight = new Repository().Get<tklFlight>(f => f.USTAID == USTAId);

            if (flight != null)
            {
                return flight;
            }

            var detailsTable = Document.Query("#ctl00_mainContent_pnlFlightSummary table") as IHtmlTableElement;

            var ratingText = decimal.Parse(detailsTable.Rows[2].Cells[1].InnerHtml.Cleanse());
            //var genderText = detailsTable.Rows[2].Cells[2].InnerHtml.Cleanse().ToLower();

            flight = new tklFlight()
            {
                USTAID = USTAId,
                FlightGender = Crawler.Gender,
                FlightLevel = ratingText,
                LeagueID = League.LeagueID
            };

            new Repository().Add(flight).Save(flight);

            return flight;
        }
    }
}
