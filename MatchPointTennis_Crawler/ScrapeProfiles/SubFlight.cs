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
    public class SubFlight
        : ScrapeProfile<tklSubFlight>
    {
        private tklFlight Flight { get; set; }

        public SubFlight(Crawler crawler, tklFlight flight)
            : base(crawler)
        {
            Flight = flight;
            LoadedElementID = "#ctl00_mainContent_tblSubFlightAnchor";
        }

        public SubFlight CreateFormDataFor_FromTeam(string linkId, string viewstate)
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

        protected async override Task<tklSubFlight> DoParse()
        {
            var subflight = new Repository().Get<tklSubFlight>(f => f.USTAID == USTAId && Flight.FlightID == Flight.FlightID);

            if(subflight != null)
            {
                return subflight;
            }

            var summaryTable = Document.Query("#ctl00_mainContent_tblSubFlightAnchor") as IHtmlTableElement;

            var subflightText = summaryTable.Rows[1].Cells[0].QuerySelector("strong").InnerHtml.Cleanse();

            subflight = new tklSubFlight()
            {
                USTAID = USTAId,
                SubFlight = subflightText,
                FlightID = Flight.FlightID
            };

            new Repository().Add(subflight).Save(subflight);

            return subflight;
        }
    }
}
