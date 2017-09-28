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
            }

            var summaryTable = Document.Query("#ctl00_mainContent_pnlMatchAnchor") as IHtmlTableElement;

            return championshipReport;
        }
    }
}
